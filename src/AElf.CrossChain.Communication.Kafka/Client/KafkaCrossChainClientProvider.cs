using System.Collections.Generic;
using AElf.CrossChain.Cache.Application;
using AElf.CrossChain.Communication.Infrastructure;
using Volo.Abp.DependencyInjection;

namespace AElf.CrossChain.Communication.Kafka
{
    public class KafkaCrossChainClientProvider : ICrossChainClientProvider, ISingletonDependency
    {
        private readonly Dictionary<int, ICrossChainClient> _clients = new Dictionary<int, ICrossChainClient>();
        private readonly IBlockCacheEntityProducer _blockCacheEntityProducer;

        public KafkaCrossChainClientProvider(IBlockCacheEntityProducer blockCacheEntityProducer)
        {
            _blockCacheEntityProducer = blockCacheEntityProducer;
        }

        public ICrossChainClient CreateAndCacheClient(CrossChainClientDto crossChainClientDto)
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