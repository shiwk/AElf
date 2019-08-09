using System.Collections.Generic;
using System.Threading.Tasks;
using Acs7;
using AElf.CrossChain.Cache;

namespace AElf.CrossChain.Communication.Kafka
{
    public interface IKafkaCrossChainConsumer
    {
        void SubscribeCrossChainBlockData(int chainId);

        Task<List<IBlockCacheEntity>> ConsumeCrossChainBlockDataAsync(long targetHeight);

        Task<ChainInitializationData> ConsumeCrossChainInitializationData(int chainId);
        
        Task CloseAsync();
    }
}