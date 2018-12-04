﻿using AElf.ChainController.TxMemPool;
using Autofac;

namespace AElf.Kernel.Modules.AutofacModule
{
    public class TxPoolServiceModule : Module
    {
        public ITxPoolConfig PoolConfig { get; set; }

        public TxPoolServiceModule(ITxPoolConfig poolConfig)
        {
            PoolConfig = poolConfig;
        }
        
        protected override void Load(ContainerBuilder builder)
        {
            if (PoolConfig != null)
                builder.RegisterInstance(PoolConfig).As<ITxPoolConfig>();
            else
                builder.RegisterInstance(TxPoolConfig.Default).As<ITxPoolConfig>();
            
            builder.RegisterType<ContractTxPool>().As<IContractTxPool>().SingleInstance();
            builder.RegisterType<TxValidator>().As<ITxValidator>();
            builder.RegisterType<TxPoolServiceBM>().As<ITxPoolService>().SingleInstance();
        }
    }
}