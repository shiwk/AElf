using System;
using System.Threading.Tasks;
using AElf.Common;
using AElf.Configuration;
using AElf.Configuration.Config.Chain;
using AElf.Kernel;
using AElf.Kernel.Storages;
using Google.Protobuf.WellKnownTypes;

namespace AElf.ChainController.CrossChain
{
    public class CrossChainInfo : ICrossChainInfo
    {
        private readonly CrossChainHelper _crossChainHelper;

        public CrossChainInfo(IStateStore stateStore)
        {
            var chainId = Hash.LoadHex(ChainConfig.Instance.ChainId);
            _crossChainHelper = new CrossChainHelper(chainId, stateStore);
        }

        /// <summary>
        /// Get merkle path of transaction root in parent chain.
        /// </summary>
        /// <param name="blockHeight">Child chain block height.</param>
        /// <returns></returns>
        public MerklePath GetTxRootMerklePathInParentChain(ulong blockHeight)
        {
            var bytes = _crossChainHelper.GetBytes<MerklePath>(
                Hash.FromMessage(new UInt64Value {Value = blockHeight}), GlobalConfig.AElfTxRootMerklePathInParentChain);
            return MerklePath.Parser.ParseFrom(bytes);
        }

        /// <summary>
        /// Get height of parent chain block which indexed the local chain block at <see cref="localChainHeight"/>
        /// </summary>
        /// <param name="localChainHeight"></param>
        /// <returns></returns>
        public ulong GetBoundParentChainHeight(ulong localChainHeight)
        {
            var bytes = _crossChainHelper.GetBytes<UInt64Value>(
                Hash.FromMessage(new UInt64Value {Value = localChainHeight}), GlobalConfig.AElfBoundParentChainHeight);
            return UInt64Value.Parser.ParseFrom(bytes).Value;
        }

        /// <summary>
        /// Get current height of parent chain block stored locally
        /// </summary>
        /// <returns></returns>
        public ulong GetParentChainCurrentHeight()
        {
            var bytes = _crossChainHelper.GetBytes<UInt64Value>(
                Hash.FromString(GlobalConfig.AElfCurrentParentChainHeight));
            return bytes == null ? 0 : UInt64Value.Parser.ParseFrom(bytes).Value;
        }

        /// <summary>
        /// Get info of parent chain block which indexes the local chain block at <see cref="localChainHeight"/>
        /// </summary>
        /// <param name="localChainHeight">Local chain height</param>
        /// <returns></returns>
        public ParentChainBlockInfo GetBoundParentChainBlockInfo(ulong localChainHeight)
        {
            var bytes = _crossChainHelper.GetBytes<ParentChainBlockInfo>(
                Hash.FromMessage(new UInt64Value {Value = localChainHeight}), GlobalConfig.AElfParentChainBlockInfo);
            return ParentChainBlockInfo.Parser.ParseFrom(bytes);
        }
    }
}