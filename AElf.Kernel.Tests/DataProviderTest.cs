using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AElf.Common;
using AElf.Database;
using AElf.Kernel;
using AElf.Kernel.Storages;
using AElf.SmartContract;
using Google.Protobuf;
using Google.Protobuf.Collections;
using Google.Protobuf.WellKnownTypes;
using Xunit;

namespace AElf.Kernel.Tests
{
    public class DataProviderTest
    {
        [Fact]
        public async Task Test()
        {
            var db = new InMemoryDatabase();
            var chainId = Hash.FromString("chain1");
            var address = Address.FromRawBytes(Hash.FromString("contract1").ToByteArray());
            var root = DataProvider.GetRootDataProvider(chainId, address);
            root.StateStore = new StateStore(db);
            var s = "test";
            var sb = Encoding.UTF8.GetBytes(s);
            var statePath = new StatePath()
            {
                Path = {ByteString.CopyFrom(address.DumpByteArray()), ByteString.CopyFromUtf8(s)}
            };
            await root.SetAsync(s, sb);

            // Value correctly set
            var retrievedChanges = root.GetChanges();
            Assert.Equal(1, retrievedChanges.Count);

            var bytes = retrievedChanges[statePath].CurrentValue.ToByteArray();
            Assert.Equal(s, Encoding.UTF8.GetString(bytes));

            // Commit changes to store
            var toCommit = retrievedChanges.ToDictionary(kv => kv.Key, kv => kv.Value.CurrentValue.ToByteArray());
            await root.StateStore.PipelineSetDataAsync(toCommit);

            // Setting the same value again in another DataProvider, no change will be returned
            var root2 = DataProvider.GetRootDataProvider(chainId, address);
            root2.StateStore = new StateStore(db);
            await root2.SetAsync(s, sb);
            var changes2 = root2.GetChanges();
            Assert.Equal(0, changes2.Count);

            // Test path
            var sub = root.GetChild("sub");
            await sub.SetAsync("dummy", new byte[] {0x01});
            var subPath = sub.GetChanges().Keys.First();
            var path = new RepeatedField<ByteString>
            {
                ByteString.CopyFrom(address.DumpByteArray()), ByteString.Empty, ByteString.CopyFromUtf8("sub"),
                ByteString.CopyFromUtf8("dummy")
            };
            Assert.Equal(subPath.Path, path);
        }
    }
}