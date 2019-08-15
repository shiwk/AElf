using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Acs7;
using AElf.CrossChain.Communication.Infrastructure;
using AElf.CrossChain.Communication.Kafka.Client;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Volo.Abp.DependencyInjection;

namespace AElf.CrossChain.Communication.Kafka
{
    public class KafkaCrossChainClientProvider : ICrossChainClientProvider, ISingletonDependency
    {
        private readonly ConcurrentDictionary<int, ICrossChainClient> _kafkaCrossChainClients =
            new ConcurrentDictionary<int, ICrossChainClient>();

        private readonly KafkaCrossChainConfigOptions _kafkaCrossChainConfigOptions;
        private readonly IKafkaCrossChainConsumerFactory _kafkaCrossChainConsumerFactory;

        private string KafkaCrossChainBroker { get; }
        public ILogger<KafkaCrossChainClientProvider> Logger { get; set; }
        
        public KafkaCrossChainClientProvider(IOptionsSnapshot<KafkaCrossChainConfigOptions> kafkaCrossChainConfigOptionsSnapshot, 
            IKafkaCrossChainConsumerFactory kafkaCrossChainConsumerFactory)
        {
            _kafkaCrossChainConsumerFactory = kafkaCrossChainConsumerFactory;
            _kafkaCrossChainConfigOptions = kafkaCrossChainConfigOptionsSnapshot.Value;
            KafkaCrossChainBroker = string.Join(":", _kafkaCrossChainConfigOptions.BrokerHost,
                _kafkaCrossChainConfigOptions.BrokerPort);
        }

        public ICrossChainClient AddOrUpdateClient(CrossChainClientDto crossChainClientDto)
        {
            var chainId = crossChainClientDto.RemoteChainId;
            if (TryGetClient(chainId, out var client))
                return client; // client already cached
            
            client = CreateCrossChainClient(crossChainClientDto);
            _kafkaCrossChainClients.TryAdd(chainId, client);
            Logger.LogTrace("Create client finished.");
            return client;
        }

        public bool TryGetClient(int chainId, out ICrossChainClient client)
        {
            return _kafkaCrossChainClients.TryGetValue(chainId, out client);
        }

        public ICrossChainClient CreateCrossChainClient(CrossChainClientDto crossChainClientDto)
        {
            IKafkaCrossChainConsumer consumer;
            if (crossChainClientDto.IsClientToParentChain)
            {
                consumer = _kafkaCrossChainConsumerFactory.Create<ParentChainBlockData>(
                    ChainHelper.ConvertChainIdToBase58(crossChainClientDto.RemoteChainId), KafkaCrossChainBroker,
                    false);
                return new KafkaClientForParentChain(consumer, crossChainClientDto.RemoteChainId);
            }
            
            consumer = _kafkaCrossChainConsumerFactory.Create<SideChainBlockData>(
                ChainHelper.ConvertChainIdToBase58(crossChainClientDto.RemoteChainId), KafkaCrossChainBroker,
                false);
            return new KafkaClientForSideChain(consumer, crossChainClientDto.RemoteChainId);
        }
        
        public List<ICrossChainClient> GetAllClients()
        {
            return _kafkaCrossChainClients.Values.ToList();
        }
    }
}