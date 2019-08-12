using AElf.Kernel.Node.Infrastructure;
using AElf.Modularity;
using Microsoft.Extensions.DependencyInjection;
using Volo.Abp.Modularity;

namespace AElf.CrossChain.Communication.Kafka
{
    [DependsOn(typeof(CrossChainCommunicationModule))]
    public class KafkaCrossChainModule : AElfModule
    {
        public override void ConfigureServices(ServiceConfigurationContext context)
        {
            var services = context.Services;
            services.AddSingleton<INodePlugin, KafkaCrossChainPlugin>();
            
            var grpcCrossChainConfiguration = services.GetConfiguration().GetSection("CrossChain");
            Configure<KafkaCrossChainConfigOption>(grpcCrossChainConfiguration.GetSection("Kafka"));
        }
    }
}