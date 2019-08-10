using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Acs7;
using AElf.CrossChain.Cache;

namespace AElf.CrossChain.Communication.Kafka
{
    public interface IKafkaCrossChainConsumer
    {
        Task SubscribeCrossChainBlockDataAsync(int chainId);

        Task ConsumeCrossChainBlockDataAsync(long targetHeight, CancellationTokenSource cts,
            Func<IBlockCacheEntity, bool> consumerHandler);

        Task<ChainInitializationData> ConsumeCrossChainInitializationData(int chainId);
        
        void Close();
    }
}