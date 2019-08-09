using System.Threading.Tasks;
using Acs7;
using AElf.CrossChain.Communication.Application;

namespace AElf.CrossChain.Communication.Kafka
{
    public class KafkaCrossChainPlugin : IKafKaCrossChainPlugin
    {
        private readonly ICrossChainClientService _crossChainClientService;

        public KafkaCrossChainPlugin(ICrossChainClientService crossChainClientService)
        {
            _crossChainClientService = crossChainClientService;
        }

        public async Task StartAsync(int chainId)
        {
            await _crossChainClientService.CreateClientAsync(new CrossChainClientDto
            {
                IsClientToParentChain = true,
                LocalChainId = chainId,
            });
        }

        public async Task ShutdownAsync()
        {
            await _crossChainClientService.CloseClientsAsync();
        }
    }
}