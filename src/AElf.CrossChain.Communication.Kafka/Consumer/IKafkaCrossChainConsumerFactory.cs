using AElf.CrossChain.Cache;
using Confluent.Kafka;
using Google.Protobuf;
using Microsoft.Extensions.Options;
using Volo.Abp.DependencyInjection;

namespace AElf.CrossChain.Communication.Kafka
{
    public interface IKafkaCrossChainConsumerFactory
    {
        IKafkaCrossChainConsumer Create<T>(string groupId, string broker, bool isAutoCommit) where T : IMessage<T>, IBlockCacheEntity, new();
    }
    
    public class KafkaCrossChainConsumerFactory : IKafkaCrossChainConsumerFactory, ITransientDependency
    {
        private readonly KafkaCrossChainConfigOptions _kafkaCrossChainConfigOptions;

        public KafkaCrossChainConsumerFactory(
            IOptionsSnapshot<KafkaCrossChainConfigOptions> kafkaCrossChainConfigOptionsSnapshot)
        {
            _kafkaCrossChainConfigOptions = kafkaCrossChainConfigOptionsSnapshot.Value;
        }
        
        public IKafkaCrossChainConsumer Create<T>(string groupId, string broker, bool isAutoCommit) where T : IMessage<T>, IBlockCacheEntity, new()
        {
            var config = new ConsumerConfig
            {
                BootstrapServers = broker,
                GroupId = groupId,
                EnableAutoCommit = isAutoCommit,
                StatisticsIntervalMs = _kafkaCrossChainConfigOptions.StatisticsIntervalMilliSeconds,
                SessionTimeoutMs = _kafkaCrossChainConfigOptions.SessionTimeoutMilliSeconds,
                AutoOffsetReset = AutoOffsetReset.,
                EnablePartitionEof = true,
                EnableAutoOffsetStore = true
            };

            return new KafkaCrossChainConsumer<T>(config);
        }
    }
}