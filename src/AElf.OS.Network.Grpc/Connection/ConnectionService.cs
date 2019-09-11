using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using AElf.OS.Network.Events;
using AElf.OS.Network.Infrastructure;
using AElf.OS.Network.Protocol;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Volo.Abp.EventBus.Local;

namespace AElf.OS.Network.Grpc.Connection
{
    public class ConnectionService : IConnectionService
    {
        private readonly IPeerPool _peerPool;
        private readonly IPeerDialer _peerDialer;
        private readonly IHandshakeProvider _handshakeProvider;
        public ILocalEventBus EventBus { get; set; }
        public ILogger<GrpcNetworkServer> Logger { get; set; }

        public ConnectionService(IPeerPool peerPool, IPeerDialer peerDialer,
            IHandshakeProvider handshakeProvider)
        {
            _peerPool = peerPool;
            _peerDialer = peerDialer;
            _handshakeProvider = handshakeProvider;

            Logger = NullLogger<GrpcNetworkServer>.Instance;
            EventBus = NullLocalEventBus.Instance;
        }

        public async Task DisconnectAsync(IPeer peer, bool sendDisconnect = false)
        {
            if (peer == null)
                throw new ArgumentNullException(nameof(peer));

            // clean the pool
            if (_peerPool.RemovePeer(peer.Info.Pubkey) == null)
                Logger.LogWarning($"{peer} was not found in pool.");

            // clean the peer
            await peer.DisconnectAsync(sendDisconnect);

            Logger.LogDebug($"Removed peer {peer}");
        }

        public GrpcPeer GetPeerByPubkey(string pubkey)
        {
            return _peerPool.FindPeerByPublicKey(pubkey) as GrpcPeer;
        }

        /// <summary>
        /// Connects to a node with the given ip address and adds it to the node's peer pool.
        /// </summary>
        /// <param name="endpoint">the ip address of the distant node</param>
        /// <returns>True if the connection was successful, false otherwise</returns>
        public async Task<bool> ConnectAsync(IPEndPoint endpoint)
        {
            Logger.LogTrace($"Attempting to reach {endpoint}.");

            if (_peerPool.FindPeerByEndpoint(endpoint) != null)
            {
                Logger.LogWarning($"Peer {endpoint} is already in the pool.");
                return false;
            }

            var peer = await _peerDialer.DialPeerAsync(endpoint);

            if (peer == null)
                return false;

            if (!_peerPool.TryAddPeer(peer))
            {
                Logger.LogWarning($"Peer {peer.Info.Pubkey} is already in the pool.");
                await peer.DisconnectAsync(false);
                return false;
            }

            try
            {
                await peer.ConfirmHandshakeAsync();
            }
            catch (Exception e)
            {
                Logger.LogError(e, $"Confirm handshake error. Peer: {peer.Info.Pubkey}.");
                _peerPool.RemovePeer(peer.Info.Pubkey);
                await peer.DisconnectAsync(false);
                throw;
            }

            peer.IsConnected = true;

            Logger.LogTrace($"Connected to {peer} - LIB height {peer.LastKnownLibHeight}, " +
                            $"best chain [{peer.CurrentBlockHeight}, {peer.CurrentBlockHash}].");

            FireConnectionEvent(peer);

            return true;
        }

        private void FireConnectionEvent(GrpcPeer peer)
        {
            var nodeInfo = new NodeInfo {Endpoint = peer.RemoteEndpoint.ToString(), Pubkey = peer.Info.Pubkey.ToByteString()};
            var bestChainHash = peer.CurrentBlockHash;
            var bestChainHeight = peer.CurrentBlockHeight;

            _ = EventBus.PublishAsync(new PeerConnectedEventData(nodeInfo, bestChainHash, bestChainHeight));
        }

        public async Task<HandshakeReply> DoHandshakeAsync(IPEndPoint endpoint, Handshake handshake)
        {
            var handshakeValidationResult = await _handshakeProvider.ValidateHandshakeAsync(handshake);
            if (handshakeValidationResult != HandshakeValidationResult.Ok)
            {
                var handshakeError = GetHandshakeError(handshakeValidationResult);
                return new HandshakeReply {Error = handshakeError};
            }

            var pubkey = handshake.HandshakeData.Pubkey.ToHex();
            var currentPeer = _peerPool.FindPeerByPublicKey(pubkey);
            if (currentPeer != null)
            {
                _peerPool.RemovePeer(pubkey);
                await currentPeer.DisconnectAsync(false);
            }

            if (_peerPool.IsFull())
            {
                Logger.LogWarning("Peer pool is full.");
                return new HandshakeReply {Error = HandshakeError.ConnectionRefused};
            }

            var peerAddress = new IPEndPoint(endpoint.Address, handshake.HandshakeData.ListeningPort);
            var grpcPeer = await _peerDialer.DialBackPeerAsync(peerAddress, handshake);

            if (!_peerPool.TryAddPeer(grpcPeer))
            {
                Logger.LogWarning($"Stopping connection, peer already in the pool {grpcPeer.Info.Pubkey}.");
                await grpcPeer.DisconnectAsync(false);
                return new HandshakeReply {Error = HandshakeError.RepeatedConnection};
            }

            Logger.LogDebug($"Added to pool {grpcPeer.Info.Pubkey}.");

            var replyHandshake = await _handshakeProvider.GetHandshakeAsync();
            return new HandshakeReply
            {
                Handshake = replyHandshake, 
                Error = HandshakeError.HandshakeOk
            };
        }

        private HandshakeError GetHandshakeError(HandshakeValidationResult handshakeValidationResult)
        {
            HandshakeError handshakeError;

            switch (handshakeValidationResult)
            {
                case HandshakeValidationResult.InvalidChainId:
                    handshakeError = HandshakeError.ChainMismatch;
                    break;
                case HandshakeValidationResult.InvalidVersion:
                    handshakeError = HandshakeError.ProtocolMismatch;
                    break;
                case HandshakeValidationResult.HandshakeTimeout:
                case HandshakeValidationResult.InvalidSignature:
                    handshakeError = HandshakeError.WrongSignature;
                    break;
                case HandshakeValidationResult.Unauthorized:
                    handshakeError = HandshakeError.ConnectionRefused;
                    break;
                default:
                    throw new ArgumentException($"Unable to process handshake validation result: {handshakeValidationResult}");
            }

            return handshakeError;
        }

        public void ConfirmHandshake(string peerPubkey)
        {
            var peer = _peerPool.FindPeerByPublicKey(peerPubkey) as GrpcPeer;
            if (peer == null)
            {
                Logger.LogWarning($"Cannot find Peer {peerPubkey} in the pool.");
                return;
            }

            peer.IsConnected = true;
        }

        public async Task DisconnectPeersAsync(bool gracefulDisconnect)
        {
            var peers = _peerPool.GetPeers(true);
            foreach (var peer in peers)
            {
                await peer.DisconnectAsync(gracefulDisconnect);
            }
        }

        public void RemovePeer(string pubkey)
        {
            _peerPool.RemovePeer(pubkey);
        }
    }
}