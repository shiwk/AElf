using System.Threading.Tasks;
using AElf.Kernel.Blockchain.Events;
using Volo.Abp.EventBus;

namespace AElf.CrossChain.Communication.Kafka
{
    public class KafkaCrossChainEventHandler : ILocalEventHandler<NewIrreversibleBlockFoundEvent>
    {
        private readonly IKafkaCrossChainProducer _kafkaCrossChainProducer;

        public KafkaCrossChainEventHandler(IKafkaCrossChainProducer kafkaCrossChainProducer)
        {
            _kafkaCrossChainProducer = kafkaCrossChainProducer;
        }

        public Task HandleEventAsync(NewIrreversibleBlockFoundEvent eventData)
        {
            throw new System.NotImplementedException();
        }
    }
}