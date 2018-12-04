﻿using System;
using System.IO;
using AElf.ChainController;
using AElf.Common;
using AElf.Configuration.Config.Contract;
using AElf.Database;
using AElf.Execution;
using AElf.Execution.Execution;
using AElf.Kernel;
using AElf.Miner;
using AElf.Network;
using AElf.Runtime.CSharp;
using AElf.SmartContract;
using Autofac;
using NLog;

namespace AElf.Concurrency.Worker
{
    class Program
    {
        private static ILogger _logger = LogManager.GetCurrentClassLogger();
        
        static void Main(string[] args)
        {
            var confParser = new ConfigParser();
            bool parsed;
            try
            {
                parsed = confParser.Parse(args);
            }
            catch (Exception e)
            {
                _logger.Error(e, "Exception while parse config.");
                throw;
            }

            if (!parsed)
                return;

            RunnerConfig.Instance.SdkDir = Path.GetDirectoryName(typeof(RunnerAElfModule).Assembly.Location);
            var runner = new SmartContractRunner();
            var smartContractRunnerFactory = new SmartContractRunnerFactory();
            smartContractRunnerFactory.AddRunner(0, runner);
            smartContractRunnerFactory.AddRunner(1, runner);

            // Setup ioc 
            var container = SetupIocContainer(true, smartContractRunnerFactory);
            if (container == null)
            {
                _logger.Error("IoC setup failed.");
                return;
            }

            if (!CheckDBConnect(container))
            {
                _logger.Error("Database connection failed.");
                return;
            }

            using (var scope = container.BeginLifetimeScope())
            {
                var service = scope.Resolve<ActorEnvironment>();
                service.InitWorkActorSystem();
                Console.WriteLine("Press Control + C to terminate.");
                Console.CancelKeyPress += async (sender, eventArgs) => { await service.StopAsync(); };
                service.TerminationHandle.Wait();
            }
        }

        private static IContainer SetupIocContainer(bool isMiner, SmartContractRunnerFactory smartContractRunnerFactory)
        {
            var builder = new ContainerBuilder();

            //builder.RegisterModule(new MainModule()); // todo : eventually we won't need this

            // Module registrations
            builder.RegisterModule(new LoggerAutofacModule());
            builder.RegisterModule(new DatabaseAutofacModule());
            builder.RegisterModule(new NetworkAutofacModule());
            builder.RegisterModule(new MinerAutofacModule(null));
            builder.RegisterModule(new ChainAutofacModule());
            builder.RegisterModule(new KernelAutofacModule());
            builder.RegisterModule(new SmartContractAutofacModule());

            builder.RegisterInstance(smartContractRunnerFactory).As<ISmartContractRunnerFactory>().SingleInstance();
            builder.RegisterType<ServicePack>().PropertiesAutowired();
            builder.RegisterType<ActorEnvironment>().SingleInstance();
            IContainer container;
            try
            {
                container = builder.Build();
            }
            catch (Exception e)
            {
                _logger.Error(e);
                return null;
            }
            return container;
        }

        private static bool CheckDBConnect(IComponentContext container)
        {
            var db = container.Resolve<IKeyValueDatabase>();
            try
            {
                return db.IsConnected();
            }
            catch (Exception e)
            {
                _logger.Error(e);
                return false;
            }
        }
    }
}