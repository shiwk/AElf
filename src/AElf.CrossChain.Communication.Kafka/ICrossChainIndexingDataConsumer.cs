using System.Collections.Generic;
using System.Threading.Tasks;

namespace AElf.CrossChain.Communication.Kafka
{
    public interface ICrossChainIndexingDataConsumer
    {
        Task SubscribeAsync(IEnumerable<string> topics);
        Task CloseAsync();

        Task ConsumeAsync(string topic);
    }
}