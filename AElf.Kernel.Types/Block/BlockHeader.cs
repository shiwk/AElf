﻿using AElf.Cryptography.ECDSA;
using AElf.Common;
using Google.Protobuf;

// ReSharper disable once CheckNamespace
namespace AElf.Kernel
{
    public partial class BlockHeader: IBlockHeader
    {
        private Hash _blockHash;
        
        public BlockHeader(Hash preBlockHash)
        {
            PreviousBlockHash = preBlockHash;
        }

        public Hash GetHash()
        {
            if (_blockHash == null)
            {
                _blockHash = Hash.FromRawBytes(GetSignatureData());
            }

            return _blockHash;
        }

        public byte[] GetHashBytes()
        {
            if (_blockHash == null)
                _blockHash = Hash.FromRawBytes(GetSignatureData());

            return _blockHash.DumpByteArray();
        }
        
        public ECSignature GetSignature()
        {
            return new ECSignature(R.ToByteArray(), S.ToByteArray());
        }

        private byte[] GetSignatureData()
        {
            var rawBlock = new BlockHeader
            {
                ChainId = ChainId?.Clone(),
                Index = Index,
                PreviousBlockHash = PreviousBlockHash?.Clone(),
                MerkleTreeRootOfTransactions = MerkleTreeRootOfTransactions?.Clone(),
                MerkleTreeRootOfWorldState = MerkleTreeRootOfWorldState?.Clone(),
                Bloom = Bloom,
                SideChainBlockHeadersRoot = SideChainBlockHeadersRoot?.Clone(),
                SideChainTransactionsRoot = MerkleTreeRootOfTransactions?.Clone()
            };
            if (Index > GlobalConfig.GenesisBlockHeight)
                rawBlock.Time = Time?.Clone();

            return rawBlock.ToByteArray();
        }

        public Hash GetDisambiguationHash()
        {
            return HashHelpers.GetDisambiguationHash(Index, Address.FromRawBytes(P.ToByteArray()));
        }
    }
}