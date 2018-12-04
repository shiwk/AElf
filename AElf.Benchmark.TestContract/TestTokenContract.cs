﻿using System;
using AElf.Kernel;
using AElf.Sdk.CSharp.Types;
using AElf.Types.CSharp.MetadataAttribute;
using Google.Protobuf.WellKnownTypes;
using CSharpSmartContract = AElf.Sdk.CSharp.CSharpSmartContract;
using Api = AElf.Sdk.CSharp.Api;
using AElf.Common;

namespace AElf.Benchmark.TestContract
{
    public class TestTokenContract : CSharpSmartContract
    {
        [SmartContractFieldData("${this}.Balances", DataAccessMode.AccountSpecific)]
        public MapToUInt64<Address> Balances = new MapToUInt64<Address>("Balances");
        [SmartContractFieldData("${this}.TokenContractName", DataAccessMode.ReadOnlyAccountSharing)]
        public StringField TokenContractName;
        
        
        [SmartContractFunction("${this}.InitBalance", new string[]{}, new []{"${this}.Balances"})]
        public bool InitBalance(Address addr)
        {
            // Console.WriteLine("InitBalance " + addr);
            ulong initBalance = ulong.MaxValue - 1;
            Balances.SetValue(addr, initBalance);
            var fromBal = Balances.GetValue(addr);
            // Console.WriteLine("Read from db of account " + addr + " with balance " + fromBal);
            return true;
        }
        
        [SmartContractFunction("${this}.Transfer", new string[]{}, new []{"${this}.Balances"})]
        public bool Transfer(Address from, Address to, ulong qty)
        {
            var fromBal = Balances.GetValue(from);
            // Console.WriteLine("from pass");

            var toBal = Balances.GetValue(to);
            // Console.WriteLine("to pass");
            var newFromBal = fromBal - qty;
            Api.Assert(fromBal > qty, $"Insufficient balance, {qty} is required but there is only {fromBal}.");
            
            var newToBal = toBal + qty;

            Balances.SetValue(from, newFromBal);
            // Console.WriteLine("set from pass");

            Balances.SetValue(to, newToBal);
            // Console.WriteLine("set to pass");
            // Console.WriteLine($"After transfer: {from.DumpHex()} - {newFromBal} || {to.DumpHex()} - {newToBal}");
            return true;
        }

        [SmartContractFunction("${this}.GetBalance", new string[]{}, new []{"${this}.Balances"})]
        public ulong GetBalance(Address account)
        {
            // Console.WriteLine("Getting balance.");
            return Balances.GetValue(account);
        }
    }
}