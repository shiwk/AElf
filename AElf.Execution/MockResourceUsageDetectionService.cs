﻿using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Google.Protobuf;
using AElf.Kernel;
using AElf.Common;
using GlobalConfig = AElf.Common.GlobalConfig;

namespace AElf.Execution
{
    public class MockResourceUsageDetectionService : IResourceUsageDetectionService
    {
        public async Task<IEnumerable<string>> GetResources(Hash chainId, Transaction transaction)
        {
            //var hashes = ECParameters.Parser.ParseFrom(transaction.Params).Params.Select(p => p.HashVal);
            List<Address> addresses = new List<Address>();
            using (MemoryStream mm = new MemoryStream(transaction.Params.ToByteArray()))
            using (CodedInputStream input = new CodedInputStream(mm))
            {
                uint tag;
                while ((tag = input.ReadTag()) != 0)
                {
                    switch (WireFormat.GetTagWireType(tag))
                    {
                        case WireFormat.WireType.Varint:
                            input.ReadUInt64();
                            break;
                        case WireFormat.WireType.LengthDelimited:
                            var bytes = input.ReadBytes();
                            if (bytes.Length == GlobalConfig.AddressLength + 2)
                            {
                                var h = new Address();
                                h.MergeFrom(bytes);
                                addresses.Add(h);
                            }

                            break;
                    }
                }
            }

            addresses.Add(transaction.From);

            return await Task.FromResult(addresses.Select(a=>a.DumpHex()));
        }
    }
}