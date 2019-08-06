using System.Threading.Tasks;
using Acs7;
using AElf.CrossChain.Communication.Infrastructure;

namespace AElf.CrossChain.Communication.Kafka.Client
{
    public class Consumer : ICrossChainClient
    {
        public int RemoteChainId { get; }
        public string TargetUriString { get; }
        public bool IsConnected { get; }
        
        public Task RequestCrossChainDataAsync(long targetHeight)
        {
            throw new System.NotImplementedException();
        }

        public Task<ChainInitializationData> RequestChainInitializationDataAsync(int chainId)
        {
            throw new System.NotImplementedException();
        }

        public Task ConnectAsync()
        {
            throw new System.NotImplementedException();
        }

        public Task CloseAsync()
        {
            throw new System.NotImplementedException();
        }
    }
}