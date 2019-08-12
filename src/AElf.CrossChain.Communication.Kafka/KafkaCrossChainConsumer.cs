using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Acs7;
using AElf.CrossChain.Cache;
using Confluent.Kafka;
using Google.Protobuf;

namespace AElf.CrossChain.Communication.Kafka
{
    public class KafkaCrossChainConsumer<T> : IKafkaCrossChainConsumer where T : IMessage<T>, IBlockCacheEntity, new()
    {
        private readonly ConsumerConfig _consumerConfig;
        private IConsumer<Ignore, T> _crossChainBlockDataConsumer;
        private IConsumer<Ignore, ChainInitializationData> _chainInitializationDataConsumer;
        public bool IsAlive { get; private set; }
        
        public KafkaCrossChainConsumer(string broker)
        {
            _consumerConfig = new ConsumerConfig
            {
                BootstrapServers = broker,
                GroupId = "cross-chain",
                EnableAutoCommit = false,
                StatisticsIntervalMs = 5000,
                SessionTimeoutMs = 6000,
                AutoOffsetReset = AutoOffsetReset.Earliest,
                EnablePartitionEof = true
            };
        }

        public async Task SubscribeCrossChainBlockDataTopicAsync(int chainId)
        {
            await DoRequestAsync(() =>
            {
                _crossChainBlockDataConsumer = new ConsumerBuilder<Ignore, T>(_consumerConfig).SetValueDeserializer(new ProtobufKafkaDeserializer<T>()).Build();
                _crossChainBlockDataConsumer.Subscribe(ChainHelper.ConvertChainIdToBase58(chainId));
            });
            IsAlive = true;
        }

        public async Task SubscribeChainInitializationDataTopicAsync(int chainId)
        {
            await DoRequestAsync(() =>
            {
                _chainInitializationDataConsumer = new ConsumerBuilder<Ignore, ChainInitializationData>(_consumerConfig)
                    .SetValueDeserializer(new ProtobufKafkaDeserializer<ChainInitializationData>()).Build();
                _chainInitializationDataConsumer.Subscribe(ChainHelper.ConvertChainIdToBase58(chainId));
            });
            IsAlive = true;
        }
        
        public async Task ConsumeCrossChainBlockDataAsync(long targetHeight, CancellationTokenSource cts, 
            Func<IBlockCacheEntity, bool> consumerHandler)
        {
            void RequestFunc()
            {
                var i = 0;
                while (i++ < CrossChainCommunicationConstants.MaximalIndexingCount)
                {
                    var consumeResult = _crossChainBlockDataConsumer.Consume(cts.Token);
                    if (consumeResult.IsPartitionEOF)
                    {
                        break;
                    }

                    if (!consumerHandler(consumeResult.Value)) break;
                }
            }

            _ = DoRequestAsync(RequestFunc);
        }

        public async Task<ChainInitializationData> ConsumeCrossChainInitializationData(int chainId,
            CancellationTokenSource cts)
        {
            ChainInitializationData chainInitializationData = null;

            void RequestFunc()
            {
                var consumeResult = _chainInitializationDataConsumer.Consume(cts.Token);
                if (consumeResult.IsPartitionEOF)
                {
                    chainInitializationData = null;
                }

                chainInitializationData = consumeResult.Value;
            }

            _ = DoRequestAsync(RequestFunc);
            return chainInitializationData;
        }

        public void Close()
        {
            IsAlive = false;
            _crossChainBlockDataConsumer?.Close();
        }
        
        private async Task DoRequestAsync(Action request)
        {
            try
            {
                await Task.Run(request);
            }
            catch (KafkaException)
            {
                Close();
                throw;
            }
        }
    }

    internal class ProtobufKafkaDeserializer<T> : IDeserializer<T> where T : IMessage<T>, new()
    {
        public T Deserialize(ReadOnlySpan<byte> data, bool isNull, SerializationContext context)
        {
            var obj = new T();
            obj.MergeFrom(data.ToArray());
            return obj;
        }
    }
}