using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
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

        private readonly KafkaCrossChainConfigOption _kafkaCrossChainConfigOption;

        public ILogger<KafkaCrossChainClientProvider> Logger { get; set; }
        
        public KafkaCrossChainClientProvider(IOptionsSnapshot<KafkaCrossChainConfigOption> kafkaCrossChainConfigOption)
        {
            _kafkaCrossChainConfigOption = kafkaCrossChainConfigOption.Value;
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
            if (crossChainClientDto.IsClientToParentChain)
                return new KafkaClientForParentChain(_kafkaCrossChainConfigOption.BrokerHost,
                    _kafkaCrossChainConfigOption.BrokerPort, crossChainClientDto.RemoteChainId,
                    _kafkaCrossChainConfigOption.ConsumeTimeout);
            return new KafkaClientForSideChain(_kafkaCrossChainConfigOption.BrokerHost,
                _kafkaCrossChainConfigOption.BrokerPort, crossChainClientDto.RemoteChainId,
                _kafkaCrossChainConfigOption.ConsumeTimeout);
        }
        
        public List<ICrossChainClient> GetAllClients()
        {
            return _kafkaCrossChainClients.Values.ToList();
        }
    }
}