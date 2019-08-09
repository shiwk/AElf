using System.Collections.Generic;
using System.Threading.Tasks;

namespace AElf.CrossChain.Communication.Kafka
{
    public interface IKafkaCrossChainBlockDataConsumerProvider
    {
        IKafkaCrossChainConsumer GetConsumer();
    }
}