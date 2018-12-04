﻿using System;
using AElf.Sdk.CSharp.Types;
using AElf.Types.CSharp.MetadataAttribute;
using CSharpSmartContract = AElf.Sdk.CSharp.CSharpSmartContract;
using Api = AElf.Sdk.CSharp.Api;
using AElf.Common;
using Google.Protobuf.WellKnownTypes;

namespace AElf.Kernel.Tests.TestContract
{
    public class TestContract : CSharpSmartContract
    {
        
        [SmartContractFieldData("${this}.Balances", DataAccessMode.AccountSpecific)]
        public MapToUInt64<Address> Balances = new MapToUInt64<Address>("Balances");
        
        
        [SmartContractFieldData("${this}.TransactionStartTimes", DataAccessMode.AccountSpecific)]
        public MapToString<Hash> TransactionStartTimes = new MapToString<Hash>("TransactionStartTimes");
        
        
        [SmartContractFieldData("${this}.TransactionEndTimes", DataAccessMode.AccountSpecific)]
        public MapToString<Hash> TransactionEndTimes = new MapToString<Hash>("TransactionEndTimes");

        [SmartContractFunction("${this}.Initialize", new string[]{}, new []{"${this}.Balances"})]
        public bool Initialize(Address account, UInt64Value qty)
        {
            Console.WriteLine($"Initialize {account.DumpHex()} to {qty.Value}");
            Balances.SetValue(account, qty.Value);
            return true;
        }

        public void SleepMilliseconds(int milliSeconds)
        {
            // Used to test timeout
            Api.Sleep(milliSeconds);
        }

        public string NoAction()
        {
            // Don't delete, this is needed to test placeholder transactions
            var str = "NoAction";
            Console.WriteLine("NoAction");
            return str;
        }
        
        [SmartContractFunction("${this}.Transfer", new string[]{}, new []{"${this}.Balances", "${this}.TransactionStartTimes", "${this}.TransactionEndTimes"})]
        public bool Transfer(Address from, Address to, UInt64Value qty)
        {
            Console.WriteLine("From: " + from.DumpHex());
            Console.WriteLine("To: " + to.DumpHex());

            // This is for testing batched transaction sequence
            TransactionStartTimes.SetValue(Api.GetTransaction().GetHash(), Now());
            var fromBal = Balances.GetValue(from);
            Console.WriteLine("Old From Balance: " + fromBal);

            var toBal = Balances.GetValue(to);
            Console.WriteLine("Old To Balance: " + toBal);

            Console.WriteLine("Assertion: " + (fromBal >= qty.Value));
            Api.Assert(fromBal >= qty.Value, $"Insufficient balance, {qty.Value} is required but there is only {fromBal}.");
            
            var newFromBal = fromBal - qty.Value;
            var newToBal = toBal + qty.Value;
            Console.WriteLine("New From Balance: " + newFromBal);
            Console.WriteLine("New To Balance: " + newToBal);
            Balances.SetValue(from, newFromBal);
            Balances.SetValue(to, newToBal);

            // This is for testing batched transaction sequence
            TransactionEndTimes.SetValue(Api.GetTransaction().GetHash(), Now());
            return true;
        }

        [SmartContractFunction("${this}.GetBalance", new string[]{}, new []{"${this}.Balances"})]
        public ulong GetBalance(Address account)
        {
            var b = Balances.GetValue(account);
            //Console.WriteLine(b);
            return b;
        }

        [SmartContractFunction("${this}.GetTransactionStartTime", new string[]{}, new []{"${this}.TransactionStartTimes"})]
        public string GetTransactionStartTime(Hash transactionHash)
        {
            var startTime = TransactionStartTimes.GetValue(transactionHash);
            return startTime;
        }

        [SmartContractFunction("${this}.GetTransactionEndTime", new string[]{}, new []{"${this}.TransactionEndTimes"})]
        public string GetTransactionEndTime(Hash transactionHash)
        {
            var endTime = TransactionEndTimes.GetValue(transactionHash);
            return endTime;
        }

        private string Now()
        {
            var dtStr = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.ffffff");
            return dtStr;
        }

        public void Print(string name)
        {
            Console.WriteLine("Hello, {0}", name);
        }

        public void InlineTxnBackToSelf(int recurseCount)
        {
            if (recurseCount > 0)
            {
                Api.SendInline(Api.GetContractAddress(), "InlineTxnBackToSelf", recurseCount - 1);                
            }
        }
    }
}