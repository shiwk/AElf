using System.Collections.Generic;
using AElf.CrossChain.Cache.Application;
using AElf.CrossChain.Communication.Infrastructure;
using Volo.Abp.DependencyInjection;

namespace AElf.CrossChain.Communication.Kafka
{
    public class KafkaCrossChainClientProvider : ICrossChainClientProvider, ISingletonDependency
    {

        public ICrossChainClient AddOrUpdateClient(CrossChainClientDto crossChainClientDto)
        {
            throw new System.NotImplementedException();
        }

        public bool TryGetClient(int chainId, out ICrossChainClient client)
        {
            throw new System.NotImplementedException();
        }

        public ICrossChainClient CreateCrossChainClient(CrossChainClientDto crossChainClientDto)
        {
            throw new System.NotImplementedException();
        }

        public List<ICrossChainClient> GetAllClients()
        {
            throw new System.NotImplementedException();
        }
    }
}