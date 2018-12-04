﻿using System;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using AElf.ABI.CSharp;
using AElf.Kernel.Managers;
using AElf.Kernel.Types;
using Google.Protobuf;
using AElf.Kernel;
using AElf.Configuration;
using AElf.Types.CSharp;
using Type = System.Type;
using AElf.Common;
using AElf.Kernel.Storages;

namespace AElf.SmartContract
{
    public class SmartContractService : ISmartContractService
    {
        private readonly ISmartContractManager _smartContractManager;
        private readonly ISmartContractRunnerFactory _smartContractRunnerFactory;
        private readonly ConcurrentDictionary<Address, ConcurrentBag<IExecutive>> _executivePools = new ConcurrentDictionary<Address, ConcurrentBag<IExecutive>>();
        private readonly IStateStore _stateStore;
        private readonly IFunctionMetadataService _functionMetadataService;

        public SmartContractService(ISmartContractManager smartContractManager, ISmartContractRunnerFactory smartContractRunnerFactory, IStateStore stateStore,
            IFunctionMetadataService functionMetadataService)
        {
            _smartContractManager = smartContractManager;
            _smartContractRunnerFactory = smartContractRunnerFactory;
            _stateStore = stateStore;
            _functionMetadataService = functionMetadataService;
        }

        private ConcurrentBag<IExecutive> GetPoolFor(Address account)
        {
            if (!_executivePools.TryGetValue(account, out var pool))
            {
                pool = new ConcurrentBag<IExecutive>();
                _executivePools[account] = pool;
            }
            return pool;
        }

        public async Task<IExecutive> GetExecutiveAsync(Address contractAddress, Hash chainId)
        {
            var pool = GetPoolFor(contractAddress);
            if (pool.TryTake(out var executive))
                return executive;

            // get registration
            var reg = await _smartContractManager.GetAsync(contractAddress);

            // get runner
            var runner = _smartContractRunnerFactory.GetRunner(reg.Category);

            if (runner == null)
            {
                throw new NotSupportedException($"Runner for category {reg.Category} is not registered.");
            }

            // get account dataprovider
            var dataProvider = DataProvider.GetRootDataProvider(chainId, contractAddress);
            dataProvider.StateStore = _stateStore;
            // run smartcontract executive info and return executive

            executive = await runner.RunAsync(reg);

            executive.SetStateStore(_stateStore);
            
            executive.SetSmartContractContext(new SmartContractContext()
            {
                ChainId = chainId,
                ContractAddress = contractAddress,
                DataProvider = dataProvider,
                SmartContractService = this
            });

            return executive;
        }

        public async Task PutExecutiveAsync(Address account, IExecutive executive)
        {
            executive.SetTransactionContext(new TransactionContext());
            executive.SetDataCache(new Dictionary<DataPath, StateCache>());
            GetPoolFor(account).Add(executive);
            await Task.CompletedTask;
        }

        private Type GetContractType(SmartContractRegistration registration)
        {
            var runner = _smartContractRunnerFactory.GetRunner(registration.Category);
            if (runner == null)
            {
                throw new NotSupportedException($"Runner for category {registration.Category} is not registered.");
            }
            return runner.GetContractType(registration);
        }
        
        /// <inheritdoc/>
        public async Task DeployContractAsync(Hash chainId, Address contractAddress, SmartContractRegistration registration, bool isPrivileged)
        {
            // get runnner
            var runner = _smartContractRunnerFactory.GetRunner(registration.Category);
            runner.CodeCheck(registration.ContractBytes.ToByteArray(), isPrivileged);

            if (ParallelConfig.Instance.IsParallelEnable)
            {
                var contractType = runner.GetContractType(registration);
                var contractTemplate = runner.ExtractMetadata(contractType);
                await _functionMetadataService.DeployContract(chainId, contractAddress, contractTemplate);
            }
            
            await _smartContractManager.InsertAsync(contractAddress, registration);
        }

        public async Task<IMessage> GetAbiAsync(Address account, string name = null)
        {
            var reg = await _smartContractManager.GetAsync(account);
            return GetAbiAsync(reg, name);
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<string>> GetInvokingParams(Transaction transaction)
        {
            var reg = await _smartContractManager.GetAsync(transaction.To);
            var abi = (Module) GetAbiAsync(reg);
            
            // method info 
            var methodInfo = GetContractType(reg).GetMethod(transaction.MethodName);
            var parameters = ParamsPacker.Unpack(transaction.Params.ToByteArray(),
                methodInfo.GetParameters().Select(y => y.ParameterType).ToArray());
            // get method in abi
            var method = abi.Methods.First(m => m.Name.Equals(transaction.MethodName));
            
            // deserialize
            return method.DeserializeParams(parameters);
        }

        private IMessage GetAbiAsync(SmartContractRegistration reg, string name = null)
        {
            var runner = _smartContractRunnerFactory.GetRunner(reg.Category);
            return runner.GetAbi(reg, name);
        }
    }
}
