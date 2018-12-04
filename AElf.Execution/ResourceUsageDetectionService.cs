﻿using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Google.Protobuf;
using AElf.Kernel;
using AElf.SmartContract;
using AElf.Common;
using GlobalConfig = AElf.Common.GlobalConfig;

namespace AElf.Execution
{
    public class ResourceUsageDetectionService : IResourceUsageDetectionService
    {
        private IFunctionMetadataService _functionMetadataService;
        public ResourceUsageDetectionService(IFunctionMetadataService functionMetadataService)
        {
            _functionMetadataService = functionMetadataService;
        }

        public async Task<IEnumerable<string>> GetResources(Hash chainId, Transaction transaction)
        {

            var addrs = GetRelatedAccount(transaction).ToImmutableHashSet()
                .Select(addr => addr.DumpHex()).ToList();

            var results = new List<string>();
            var functionMetadata = await _functionMetadataService.GetFunctionMetadata(chainId, GetFunctionName(transaction));
            foreach (var resource in functionMetadata.FullResourceSet)
            {
                switch (resource.DataAccessMode)
                {
                    case DataAccessMode.AccountSpecific:
                        foreach (var addr in addrs)
                        {
                            results.Add(resource.Name + "." + addr);
                        }
                        break;
                    
                    case DataAccessMode.ReadWriteAccountSharing:
                        results.Add(resource.Name);
                        break;
                }
            }

            return results;
        }

        private string GetFunctionName(Transaction tx)
        {
            return tx.To.DumpHex() + "." + tx.MethodName;
        }

        private List<Address> GetRelatedAccount(Transaction transaction)
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
                                // TODO: Ignore if parsing failed, which means our guess is wrong - the bytes is not an address
                                var h = new Address();
                                h.MergeFrom(bytes);
                                addresses.Add(h);
                            }
                            break;
                    }
                }
            }

            addresses.Add(transaction.From);

            return addresses;
        }
    }
}