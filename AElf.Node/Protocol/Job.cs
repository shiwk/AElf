﻿using System.Collections.Generic;
using AElf.Kernel;
using AElf.Network;
using AElf.Network.Peers;

namespace AElf.Node.Protocol
{
    public class Job
    {
        public Block Block { get; set; }
        public List<byte[]> Transactions { get; set; }
        public AElfProtocolMsgType MsgType { get; set; }

        public IPeer Peer { get; set; }
    }
}