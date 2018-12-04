﻿using System;
using System.Threading.Tasks;
using AElf.Common;
using AElf.Configuration;
using AElf.Configuration.Config.Chain;
using AElf.Configuration.Config.Consensus;
using AElf.Kernel.Storages;
using AElf.SmartContract;
using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;
using NLog;
using NLog.Targets.Wrappers;

namespace AElf.Kernel.Consensus
{
    // ReSharper disable InconsistentNaming
    public class DPoSInfoProvider
    {
        private readonly IStateStore _stateStore;

        private readonly ILogger _logger = LogManager.GetLogger(nameof(DPoSInfoProvider));

        public DPoSInfoProvider(IStateStore stateStore)
        {
            _stateStore = stateStore;
        }

        public Hash ChainId => Hash.LoadHex(ChainConfig.Instance.ChainId);
        public Address ContractAddress => AddressHelpers.GetSystemContractAddress(
            Hash.LoadHex(ChainConfig.Instance.ChainId),
            SmartContractType.AElfDPoS.ToString());
        
        private DataProvider DataProvider
        {
            get
            {
                var dp = DataProvider.GetRootDataProvider(ChainId, ContractAddress);
                dp.StateStore = _stateStore;
                return dp;
            }
        }
        
        /// <summary>
        /// Assert: Related value has surely exists in database.
        /// </summary>
        /// <param name="keyHash"></param>
        /// <param name="resourceStr"></param>
        /// <returns></returns>
        private async Task<byte[]> GetBytes<T>(Hash keyHash, string resourceStr = "") where T : IMessage, new()
        {
            return await (resourceStr != ""
                ? DataProvider.GetChild(resourceStr).GetAsync<T>(keyHash)
                : DataProvider.GetAsync<T>(keyHash));
        }
        
        public async Task<Miners> GetMiners()
        {
            try
            {
                var miners =
                    Miners.Parser.ParseFrom(
                        await GetBytes<Miners>(Hash.FromString(GlobalConfig.AElfDPoSBlockProducerString)));
                return miners;
            }
            catch (Exception ex)
            {
                _logger?.Trace(ex, "Failed to get miners list.");
                return new Miners();
            }
        }

        public async Task<ulong> GetCurrentRoundNumber()
        {
            try
            {
                var number = UInt64Value.Parser.ParseFrom(
                    await GetBytes<UInt64Value>(Hash.FromString(GlobalConfig.AElfDPoSCurrentRoundNumber)));
                return number.Value;
            }
            catch (Exception ex)
            {
                _logger?.Trace(ex, "Failed to current round number.");
                return 0;
            }
        }

        public async Task<Round> GetCurrentRoundInfo()
        {
            var currentRoundNumber = await GetCurrentRoundNumber();
            try
            {
                var bytes = await GetBytes<Round>(Hash.FromMessage(new UInt64Value {Value = currentRoundNumber}),
                    GlobalConfig.AElfDPoSInformationString);
                var round = Round.Parser.ParseFrom(bytes);
                return round;
            }
            catch (Exception e)
            {
                _logger.Error(e,
                    $"Failed to get Round information of provided round number. - {currentRoundNumber}\n");
                return null;
            }
        }

        public async Task<BlockProducer> GetBPInfo(string addressToHex = null)
        {
            if (addressToHex == null)
            {
                addressToHex = NodeConfig.Instance.NodeAccount;
            }
            
            var round = await GetCurrentRoundInfo();
            return round.BlockProducers[addressToHex.RemoveHexPrefix()];
        }

        public async Task<Timestamp> GetTimeSlot(string addressToHex = null)
        {
            if (addressToHex == null)
            {
                addressToHex = NodeConfig.Instance.NodeAccount;
            }

            var info = await GetBPInfo(addressToHex);
            return info.TimeSlot;
        }

        public async Task<double> GetDistanceToTimeSlot(string addressToHex = null)
        {
            if (addressToHex == null)
            {
                addressToHex = NodeConfig.Instance.NodeAccount;
            }

            var timeSlot = await GetTimeSlot(addressToHex);
            var distance = timeSlot - DateTime.UtcNow.ToTimestamp();
            return distance.ToTimeSpan().TotalMilliseconds;
        }

        public async Task<double> GetDistanceToTimeSlotEnd(string addressToHex = null)
        {
            var distance = (double) ConsensusConfig.Instance.DPoSMiningInterval;
            var currentRoundNumber = await GetCurrentRoundNumber();
            if (currentRoundNumber != 0)
            {
                var info = await GetBPInfo(addressToHex);

                var now = DateTime.UtcNow.ToTimestamp();
                distance += (info.TimeSlot - now).ToTimeSpan().TotalMilliseconds;
                if (info.IsEBP && distance < 0)
                {
                    distance += (GlobalConfig.BlockProducerNumber - info.Order + 2) * ConsensusConfig.Instance.DPoSMiningInterval;
                }
            }
            // Todo the time slot of dpos is not exact
            return (distance < 1000 || distance > (double) ConsensusConfig.Instance.DPoSMiningInterval) ? ConsensusConfig.Instance.DPoSMiningInterval : distance;
        }
    }
}