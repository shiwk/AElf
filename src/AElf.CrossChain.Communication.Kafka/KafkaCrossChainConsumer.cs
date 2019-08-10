using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Acs7;
using AElf.CrossChain.Cache;
using Confluent.Kafka;

namespace AElf.CrossChain.Communication.Kafka
{
    public class KafkaCrossChainConsumer : IKafkaCrossChainConsumer
    {
        private ConsumerConfig _consumerConfig;
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

        public void SubscribeCrossChainBlockData(int chainId)
        {
            _consumer.
        }

        public Task<List<IBlockCacheEntity>> ConsumeCrossChainBlockDataAsync(long targetHeight)
        {
            throw new System.NotImplementedException();
        }

        public Task<ChainInitializationData> ConsumeCrossChainInitializationData(int chainId)
        {
            throw new System.NotImplementedException();
        }

        public Task CloseAsync()
        {
            throw new System.NotImplementedException();
        }
    }
}