﻿using AElf.Common;
using AElf.Cryptography.ECDSA;

namespace AElf.Configuration
{
    [ConfigFile(FileName = "node.json")]
    public class NodeConfig : ConfigBase<NodeConfig>
    {
        public bool IsMiner { get; set; }
        public bool IsChainCreator { get; set; }
        public bool ConsensusInfoGenerator { get; set; }
        public string ExecutorType { get; set; }
        public string NodeName { get; set; }
        public string NodeAccount { get; set; }
        public string NodeAccountPassword { get; set; }
        public ECKeyPair ECKeyPair { get; set; }
        public ConsensusKind ConsensusKind { get; set; }
        public float RatioMine { get; set; }
        public float RatioSynchronize { get; set; }
    }
}