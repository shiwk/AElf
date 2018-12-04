﻿using System.Threading.Tasks;
using AElf.Kernel;
using AElf.SmartContract;
using Google.Protobuf;
using AElf.Types.CSharp;
using AElf.Common;

namespace AElf.Runtime.CSharp.Tests
{
    public class TestContractShim
    {
        private MockSetup _mock;
        private Address ContractAddress
        {
            get
            {
                if (!_second)
                {
                    return _mock.ContractAddress1;
                }
                return _mock.ContractAddress2;
            }
        }
        private IExecutive Executive { get; set; }

        private Hash ChainId
        {
            get
            {
                if (!_second)
                {
                    return _mock.ChainId1;
                }
                return _mock.ChainId2;
            }
        }
        
        private bool _second = false;
        public TestContractShim(MockSetup mock, bool second = false)
        {
            _mock = mock;
            _second = second;
            SetExecutive();
        }

        private void SetExecutive()
        {
            Task<IExecutive> task = null;
            if (!_second)
            {
                task = _mock.SmartContractService.GetExecutiveAsync(_mock.ContractAddress1, _mock.ChainId1);
                task.Wait();
                Executive = task.Result;
                Executive.SetSmartContractContext(new SmartContractContext()
                {
                    ChainId = _mock.ChainId1,
                    ContractAddress = _mock.ContractAddress1,
                    DataProvider = _mock.DataProvider1,
                    SmartContractService = _mock.SmartContractService
                });
            }
            else
            {
                task = _mock.SmartContractService.GetExecutiveAsync(_mock.ContractAddress2, _mock.ChainId2);
                task.Wait();
                Executive = task.Result;
                Executive.SetSmartContractContext(new SmartContractContext()
                {
                    ChainId = _mock.ChainId2,
                    ContractAddress = _mock.ContractAddress2,
                    DataProvider = _mock.DataProvider2,
                    SmartContractService = _mock.SmartContractService
                });
            }
        }

        public bool Initialize(Address account, ulong qty)
        {
            var tx = new Transaction
            {
                From = Address.Zero,
                To = ContractAddress,
                IncrementId = _mock.NewIncrementId(),
                MethodName = "Initialize",
                Params = ByteString.CopyFrom(ParamsPacker.Pack(account, qty))
            };
            var tc = new TransactionContext()
            {
                Transaction = tx
            };
            Executive.SetTransactionContext(tc).Apply().Wait();
            tc.Trace.CommitChangesAsync(_mock.StateStore).Wait();

            return true;
        }

        public void InvokeAsync()
        {
        }

        public bool Transfer(Address from, Address to, ulong qty)
        {
            var tx = new Transaction
            {
                From = Address.Zero,
                To = ContractAddress,
                IncrementId = _mock.NewIncrementId(),
                MethodName = "Transfer",
                Params = ByteString.CopyFrom(ParamsPacker.Pack(from, to, qty))
            };
            var tc = new TransactionContext()
            {
                Transaction = tx
            };
            
            Executive.SetTransactionContext(tc).Apply().Wait();
            tc.Trace.CommitChangesAsync(_mock.StateStore).Wait();
            
            return tc.Trace.RetVal.Data.DeserializeToBool();
        }

        public ulong GetBalance(Address account)
        {
            var tx = new Transaction
            {
                From = Address.Zero,
                To = ContractAddress,
                IncrementId = _mock.NewIncrementId(),
                MethodName = "GetBalance",
                Params = ByteString.CopyFrom(ParamsPacker.Pack(account))
            };
            var tc = new TransactionContext()
            {
                Transaction = tx
            };
            Executive.SetTransactionContext(tc).Apply().Wait();
            tc.Trace.CommitChangesAsync(_mock.StateStore).Wait();
            return tc.Trace.RetVal.Data.DeserializeToUInt64();
        }

        public string GetTransactionStartTime(Hash transactionHash)
        {
            var tx = new Transaction
            {
                From = Address.Zero,
                To = ContractAddress,
                IncrementId = _mock.NewIncrementId(),
                MethodName = "GetTransactionStartTime",
                Params = ByteString.CopyFrom(ParamsPacker.Pack(transactionHash))
            };
            var tc = new TransactionContext()
            {
                Transaction = tx
            };
            Executive.SetTransactionContext(tc).Apply().Wait();
            tc.Trace.CommitChangesAsync(_mock.StateStore).Wait();
            return tc.Trace.RetVal.Data.DeserializeToString();
        }

        public string GetTransactionEndTime(Hash transactionHash)
        {
            var tx = new Transaction
            {
                From = Address.Zero,
                To = ContractAddress,
                IncrementId = _mock.NewIncrementId(),
                MethodName = "GetTransactionEndTime",
                Params = ByteString.CopyFrom(ParamsPacker.Pack(transactionHash))
            };
            var tc = new TransactionContext()
            {
                Transaction = tx
            };
            Executive.SetTransactionContext(tc).Apply().Wait();
            tc.Trace.CommitChangesAsync(_mock.StateStore).Wait();
            return tc.Trace.RetVal.Data.DeserializeToString();
        }
    }
}
