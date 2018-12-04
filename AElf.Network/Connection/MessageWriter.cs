﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using AElf.Common;
using NLog;

[assembly: InternalsVisibleTo("AElf.Network.Tests")]
namespace AElf.Network.Connection
{

    public class WriteJob
    {
        public Message Message { get; set; }
        public Action<Message> SuccessCallback { get; set; }
    }
    /// <summary>
    /// This class performs writes to the underlying tcp stream.
    /// </summary>
    public class MessageWriter : IMessageWriter
    {
        private const int DefaultMaxOutboundPacketSize = 20148;

        private readonly ILogger _logger;
        private readonly NetworkStream _stream;

        private BlockingCollection<WriteJob> _outboundMessages;

        internal bool IsDisposed { get; private set; }

        /// <summary>
        /// This configuration property determines the maximum size an
        /// outgoing messages payload. If the payloads size is larger
        /// than this value, this message will be send in multiple sub
        /// packets.
        /// </summary>
        public int MaxOutboundPacketSize { get; set; } = DefaultMaxOutboundPacketSize;

        public MessageWriter(NetworkStream stream)
        {
            _outboundMessages = new BlockingCollection<WriteJob>();
            _stream = stream;

            _logger = LogManager.GetLogger(nameof(MessageWriter));
        }

        /// <summary>
        /// Starts the dequing of outgoing messages.
        /// </summary>
        public void Start()
        {
            Task.Run(() => DequeueOutgoingLoop()).ConfigureAwait(false);
        }

        public void EnqueueMessage(Message p, Action<Message> successCallback = null)
        {
            if (IsDisposed || _outboundMessages == null || _outboundMessages.IsAddingCompleted)
                return;

            try
            {
                _outboundMessages.Add(new WriteJob { Message = p, SuccessCallback = successCallback});
            }
            catch (Exception e)
            {
                _logger.Trace(e, "Exception while enqueue for outgoing message.");
            }
        }

        /// <summary>
        /// The main loop that sends queud up messages from the message queue.
        /// </summary>
        internal void DequeueOutgoingLoop()
        {
            while (!IsDisposed && _outboundMessages != null)
            {
                WriteJob job;

                try
                {
                    job = _outboundMessages.Take();
                }
                catch (Exception)
                {
                    Dispose(); // if already disposed will do nothing 
                    break;
                }

                var p = job.Message;

                if (p == null)
                {
                    _logger?.Warn("Cannot write a null message.");
                    continue;
                }
                
                try
                {
                    if (p.Payload.Length > MaxOutboundPacketSize)
                    {
                        var partials = PayloadToPartials(p.Type, p.Payload, MaxOutboundPacketSize);

                        _logger?.Trace($"Message split into {partials.Count} packets.");

                        foreach (var msg in partials)
                        {
                            SendPartialPacket(msg);
                        }
                    }
                    else
                    {
                        // Send without splitting
                        SendPacketFromMessage(p);
                    }
                    
                    job.SuccessCallback?.Invoke(p);
                }
                catch (Exception e) when (e is IOException || e is ObjectDisposedException)
                {
                    _logger?.Trace("Exception with the underlying socket or stream closed.");
                    Dispose();
                }
                catch (Exception e)
                {
                    _logger?.Trace(e, "Exception while dequeing message.");
                }
            }

            _logger?.Trace("Finished writting messages.");
        }

        internal List<PartialPacket> PayloadToPartials(int msgType, byte[] arrayToSplit, int chunckSize)
        {
            List<PartialPacket> splitted = new List<PartialPacket>();

            int sourceArrayLength = arrayToSplit.Length; 
            int wholePacketCount = sourceArrayLength / chunckSize;
            int lastPacketSize = sourceArrayLength % chunckSize;

            if (wholePacketCount == 0 && lastPacketSize <= 0)
                return null;

            for (int i = 0; i < wholePacketCount; i++)
            {
                byte[] slice = new byte[chunckSize];
                Array.Copy(arrayToSplit, i*chunckSize, slice, 0, MaxOutboundPacketSize);
                
                var partial = new PartialPacket {
                    Type = msgType, Position = i, TotalDataSize = sourceArrayLength, Data = slice
                };
                
                splitted.Add(partial);
            }
            
            if (lastPacketSize != 0)
            {
                byte[] slice = new byte[lastPacketSize];
                Array.Copy(arrayToSplit, wholePacketCount*chunckSize, slice, 0, lastPacketSize);
                
                var partial = new PartialPacket {
                    Type = msgType, Position = wholePacketCount, TotalDataSize = sourceArrayLength, Data = slice
                };
                
                // Set last packet flag to this packet
                partial.IsEnd = true;
                
                splitted.Add(partial);
            }
            else
            {
                splitted.Last().IsEnd = true;
            }

            return splitted;
        }

        internal void SendPacketFromMessage(Message p)
        {
            byte[] type = {(byte) p.Type};
            byte[] hasId = {p.HasId ? (byte) 1 : (byte) 0};
            byte[] isbuffered = {0};
            byte[] length = BitConverter.GetBytes(p.Length);
            byte[] arrData = p.Payload;

            byte[] b;

            if (p.HasId)
            {
                b = ByteArrayHelpers.Combine(type, hasId, p.Id, isbuffered, length, arrData);
            }
            else
            {
                b = ByteArrayHelpers.Combine(type, hasId, isbuffered, length, arrData);
            }

            _stream.Write(b, 0, b.Length);
        }

        internal void SendPartialPacket(PartialPacket p)
        {
            byte[] type = {(byte) p.Type};
            byte[] hasId = {p.HasId ? (byte) 1 : (byte) 0};
            byte[] isbuffered = {1};
            byte[] length = BitConverter.GetBytes(p.Data.Length);

            byte[] posBytes = BitConverter.GetBytes(p.Position);
            byte[] isEndBytes = p.IsEnd ? new byte[] {1} : new byte[] {0};
            byte[] totalLengthBytes = BitConverter.GetBytes(p.TotalDataSize);

            byte[] arrData = p.Data;

            byte[] b;
            if (p.HasId)
            {
                b = ByteArrayHelpers.Combine(type, hasId, p.Id, isbuffered, length, posBytes, isEndBytes, totalLengthBytes, arrData);
            }
            else
            {
                b = ByteArrayHelpers.Combine(type, hasId, isbuffered, length, posBytes, isEndBytes, totalLengthBytes, arrData);
            }

            _stream.Write(b, 0, b.Length);
        }

        #region Closing and disposing

        public void Close()
        {
            Dispose();
        }

        public void Dispose()
        {
            if (IsDisposed)
                return;

            // Note that This will cause an IOException in the read loop.
            _stream?.Close();

            _outboundMessages?.CompleteAdding();
            _outboundMessages?.Dispose();
            _outboundMessages = null;

            IsDisposed = true;
        }

        #endregion
    }
}