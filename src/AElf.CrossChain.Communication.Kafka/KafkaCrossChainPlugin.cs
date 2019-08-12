using System.Threading.Tasks;
using AElf.CrossChain.Communication.Application;
using Volo.Abp.DependencyInjection;

namespace AElf.CrossChain.Communication.Kafka
{
    public class KafkaCrossChainPlugin : IKafKaCrossChainPlugin, ITransientDependency
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