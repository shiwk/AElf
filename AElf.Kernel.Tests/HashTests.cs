using System.Collections.Generic;
using Xunit;
using AElf.Common;

namespace AElf.Kernel.Tests
{
    public class HashTests
    {
//        [Fact]
//        public void EqualTest()
//        {
//            var hash1 = new Hash(new byte[] {10, 14, 1, 15});
//            var hash2 = new Hash(new byte[] {10, 14, 1, 15});
//            var hash3 = new Hash(new byte[] {15, 1, 14, 10});
//            Assert.True(hash1 == hash2);
//            Assert.False(hash1 == hash3);
//        }
//
//        [Fact]
//        public void CompareTest()
//        {
//            var hash1 = new Hash(new byte[] {10, 14, 1, 15});
//            var hash2 = new Hash(new byte[] {15, 1, 14, 10});
//            
//            Assert.True(new Hash().Compare(hash1, hash2) == -1);
//        }
//
//        [Fact]
//        public void DictionaryTest()
//        {
//            var dict = new Dictionary<Hash, string>();
//            var hash = new Hash(new byte[] {10, 14, 1, 15});
//            dict[hash] = "test";
//            
//            var anotherHash = new Hash(new byte[] {10, 14, 1, 15});
//            
//            Assert.True(dict.TryGetValue(anotherHash, out var test));
//            Assert.Equal("test", test);
//        }

        [Fact]
        public void RandomHashTest()
        {
            var hash1 = Hash.Generate();
            var hash2 = Hash.Generate();
            
            Assert.False(hash1 == hash2);
        }
    }
}