using System.Threading.Tasks;
using Acs7;
using AElf.CrossChain.Cache;
using Google.Protobuf;

namespace AElf.CrossChain.Communication.Kafka
{
    public interface IKafkaCrossChainProducer
    {
        Task ProduceChainInitializationDataAsync(ChainInitializationData chainInitializationData);
        Task ProduceCrossChainBlockDataAsync<T>(T t) where T : IMessage, IBlockCacheEntity;
    }
}