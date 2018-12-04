﻿using AElf.Contracts.Genesis;
using AElf.Kernel;
using AElf.Sdk.CSharp;
using AElf.Sdk.CSharp.Types;
using AElf.Types.CSharp.MetadataAttribute;
using Api = AElf.Sdk.CSharp.Api;
using AElf.Common;

namespace AElf.Contracts.Token
{
    #region Events

    public class Transfered : Event
    {
        [Indexed] public Address From { get; set; }
        [Indexed] public Address To { get; set; }
        [Indexed] public ulong Amount { get; set; }
    }

    public class Approved : Event
    {
        [Indexed] public Address Owner { get; set; }
        [Indexed] public Address Spender { get; set; }
        [Indexed] public ulong Amount { get; set; }
    }


    public class UnApproved : Event
    {
        [Indexed] public Address Owner { get; set; }
        [Indexed] public Address Spender { get; set; }
        [Indexed] public ulong Amount { get; set; }
    }

    #endregion Events

    public class  TokenContract : CSharpSmartContract
    {
        [SmartContractFieldData("${this}._initialized", DataAccessMode.ReadWriteAccountSharing)]
        private readonly BoolField _initialized = new BoolField("_Initialized_");
        [SmartContractFieldData("${this}._symbol", DataAccessMode.ReadOnlyAccountSharing)]
        private readonly StringField _symbol = new StringField("_Symbol_");
        [SmartContractFieldData("${this}._tokenName", DataAccessMode.ReadOnlyAccountSharing)]
        private readonly StringField _tokenName = new StringField("_TokenName_");
        [SmartContractFieldData("${this}._totalSupply", DataAccessMode.ReadOnlyAccountSharing)]
        private readonly UInt64Field _totalSupply = new UInt64Field("_TotalSupply_");
        [SmartContractFieldData("${this}._decimals", DataAccessMode.ReadOnlyAccountSharing)]
        private readonly UInt32Field _decimals = new UInt32Field("_Decimals_");
        [SmartContractFieldData("${this}._balances", DataAccessMode.AccountSpecific)]
        private readonly MapToUInt64<Address> _balances = new MapToUInt64<Address>("_Balances_");

        [SmartContractFieldData("${this}._allowancePlaceHolder", DataAccessMode.AccountSpecific)]
        private readonly object _allowancePlaceHolder;

        #region ABI (Public) Methods

        #region View Only Methods

        [SmartContractFunction("${this}.Symbol", new string[]{}, new []{"${this}._symbol"})]
        [View]
        public string Symbol()
        {
            return _symbol.GetValue();
        }

        [SmartContractFunction("${this}.TokenName", new string[]{}, new []{"${this}._tokenName"})]
        [View]
        public string TokenName()
        {
            return _tokenName.GetValue();
        }

        [SmartContractFunction("${this}.TotalSupply", new string[]{}, new []{"${this}._totalSupply"})]
        [View]
        public ulong TotalSupply()
        {
            return _totalSupply.GetValue();
        }

        [SmartContractFunction("${this}.Decimals", new string[]{}, new []{"${this}._decimals"})]
        [View]
        public uint Decimals()
        {
            return _decimals.GetValue();
        }

        [SmartContractFunction("${this}.BalanceOf", new string[]{}, new []{"${this}._balances"})]
        [View]
        public ulong BalanceOf(Address owner)
        {
            return _balances[owner];
        }

        [SmartContractFunction("${this}.Allowance", new string[]{}, new []{"${this}._allowancePlaceHolder"})]
        [View]
        public ulong Allowance(Address owner, Address spender)
        {
            return Allowances.GetAllowance(owner, spender);
        }

        #endregion View Only Methods


        #region Actions

        [SmartContractFunction("${this}.Initialize", new string[]{}, new []{"${this}._initialized", "${this}._symbol", "${this}._tokenName", "${this}._totalSupply", "${this}._decimals", "${this}._balances"})]
        public void Initialize(string symbol, string tokenName, ulong totalSupply, uint decimals)
        {
            Api.Assert(!_initialized.GetValue(), "Already initialized.");
            // Api.Assert(Api.GetContractOwner().Equals(Api.GetTransaction().From), "Only owner can initialize the contract state.");
            _symbol.SetValue(symbol);
            _tokenName.SetValue(tokenName);
            _totalSupply.SetValue(totalSupply);
            _decimals.SetValue(decimals);
            _balances[Api.GetTransaction().From] = totalSupply;
            _initialized.SetValue(true);
        }

        [SmartContractFunction("${this}.Transfer", new string[]{"${this}.DoTransfer"}, new string[]{})]
        public void Transfer(Address to, ulong amount)
        {
            var from = Api.GetTransaction().From;
            DoTransfer(from, to, amount);
        }

        [SmartContractFunction("${this}.TransferFrom", new string[]{"${this}.DoTransfer"}, new string[]{"${this}._allowancePlaceHolder"})]
        public void TransferFrom(Address from, Address to, ulong amount)
        {
            var allowance = Allowances.GetAllowance(from, Api.GetTransaction().From);
            Api.Assert(allowance > amount, "Insufficient allowance.");

            DoTransfer(from, to, amount);
            Allowances.Reduce(from, amount);
        }

        [SmartContractFunction("${this}.Approve", new string[]{}, new string[]{"${this}._allowancePlaceHolder"})]
        public void Approve(Address spender, ulong amount)
        {
            Allowances.Approve(spender, amount);
            new Approved()
            {
                Owner = Api.GetTransaction().From,
                Spender = spender,
                Amount = amount
            }.Fire();
        }

        [SmartContractFunction("${this}.UnApprove", new string[]{}, new string[]{"${this}._allowancePlaceHolder"})]
        public void UnApprove(Address spender, ulong amount)
        {
            Allowances.Reduce(spender, amount);
            new UnApproved()
            {
                Owner = Api.GetTransaction().From,
                Spender = spender,
                Amount = amount
            }.Fire();
        }

        #endregion Actions

        #endregion ABI (Public) Methods


        #region Private Methods

        [SmartContractFunction("${this}.DoTransfer", new string[]{}, new string[]{"${this}._balances"})]
        private void DoTransfer(Address from, Address to, ulong amount)
        {
            var balSender = _balances[from];
            Api.Assert(balSender >= amount, "Insufficient balance.");
            var balReceiver = _balances[to];
            balSender = balSender.Sub(amount);
            balReceiver = balReceiver.Add(amount);
            _balances[from] = balSender;
            _balances[to] = balReceiver;
            new Transfered()
            {
                From = from,
                To = to,
                Amount = amount
            }.Fire();
        }

        #endregion Private Methods
    }

    #region Helper Type

    internal class Allowances
    {
        private static MapToUInt64<AddressPair> _allowances = new MapToUInt64<AddressPair>("_Allowances_");

        public static ulong GetAllowance(Address owner, Address spender)
        {
            return _allowances.GetValue(new AddressPair() {First = owner, Second = spender});
        }

        public static void Approve(Address spender, ulong amount)
        {
            var pair = new AddressPair() {First = Api.GetTransaction().From, Second = spender};
            _allowances[pair] = _allowances[pair].Add(amount);
        }

        public static void Reduce(Address owner, ulong amount)
        {
            var pair = new AddressPair() {First = owner, Second = Api.GetTransaction().From};
            _allowances[pair] = _allowances[pair].Sub(amount);
        }
    }

    #endregion Helper Type
}