using System.Collections.Generic;
using System.Threading.Tasks;
using Acs7;
using AElf.CrossChain.Cache;

namespace AElf.CrossChain.Communication.Kafka
{
    public class KafkaCrossChainConsumer : IKafkaCrossChainConsumer
    {
        public KafkaCrossChainConsumer(string broker)
        {
            throw new System.NotImplementedException();
        }

        public void SubscribeCrossChainBlockData(int chainId)
        {
            throw new System.NotImplementedException();
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