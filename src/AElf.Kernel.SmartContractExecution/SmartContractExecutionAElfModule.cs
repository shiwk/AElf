﻿using AElf.Kernel.SmartContract;
using AElf.Kernel.SmartContract.ExecutionPluginForAcs1;
using AElf.Kernel.SmartContract.ExecutionPluginForAcs5;
using AElf.Kernel.SmartContract.ExecutionPluginForAcs8;
using AElf.Modularity;
using Volo.Abp;
using Volo.Abp.Modularity;

namespace AElf.Kernel.SmartContractExecution
{
    [DependsOn(typeof(SmartContractAElfModule),
        typeof(ExecutionPluginForAcs8Module),
        typeof(ExecutionPluginForAcs5Module),
        typeof(ExecutionPluginForAcs1Module))]
    public class SmartContractExecutionAElfModule : AElfModule
    {
        public override void ConfigureServices(ServiceConfigurationContext context)
        {

        }

        public override void OnApplicationInitialization(ApplicationInitializationContext context)
        {
            //var executorType = context.ServiceProvider.GetService<IOptionsSnapshot<ExecutionOptions>>().Value.ExecutorType;
        }
    }
}