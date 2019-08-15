using System.Threading.Tasks;
using Acs7;
using AElf.CrossChain.Cache;
using Google.Protobuf;

namespace AElf.CrossChain.Communication.Kafka
{
    public class KafkaCrossChainProducer : IKafkaCrossChainProducer
    {
        public Task ProduceChainInitializationDataAsync(ChainInitializationData chainInitializationData)
        {
            throw new System.NotImplementedException();
        }

        public Task ProduceCrossChainBlockDataAsync<T>(T t) where T : IMessage, IBlockCacheEntity
        {
            throw new System.NotImplementedException();
        }
    }
}