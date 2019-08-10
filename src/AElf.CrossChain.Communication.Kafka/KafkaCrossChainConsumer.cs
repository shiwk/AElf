using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Acs7;
using AElf.CrossChain.Cache;
using Confluent.Kafka;

namespace AElf.CrossChain.Communication.Kafka
{
    public class KafkaCrossChainConsumer : IKafkaCrossChainConsumer
    {
        private readonly ConsumerConfig _consumerConfig;
        private IConsumer<Ignore, string> _consumer;
        
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
            _consumer = new ConsumerBuilder<Ignore, string>(_consumerConfig).Build();
        }

        public Task SubscribeCrossChainBlockDataAsync(int chainId)
        {
            DoRequest(() =>
            {
                _consumer = new ConsumerBuilder<Ignore, string>(_consumerConfig).Build();
                _consumer.Subscribe(ChainHelper.ConvertChainIdToBase58(chainId));
            });
            return Task.CompletedTask;
        }

        public Task ConsumeCrossChainBlockDataAsync(long targetHeight, CancellationTokenSource cts, 
            Func<IBlockCacheEntity, bool> consumerHandler)
        {
            var res = new List<IBlockCacheEntity>();
            DoRequest(() =>
            {
                while (res.Count < CrossChainCommunicationConstants.MaximalIndexingCount)
                {
                    var consumeResult = _consumer.Consume(cts.Token);
                    if (consumeResult.IsPartitionEOF)
                    {
                        break;
                    }

                    if (!consumerHandler(consumeResult.Value))
                        break;
                }
            });

            return Task.FromResult(res);
        }

        public Task<ChainInitializationData> ConsumeCrossChainInitializationData(int chainId)
        {
            throw new System.NotImplementedException();
        }

        public void Close()
        {
            _consumer?.Close();
        }
        
        private void DoRequest(Action request)
        {
            try
            {
                request();
            }
            catch (KafkaException)
            {
                Close();
                throw;
            }
        }
    }
}