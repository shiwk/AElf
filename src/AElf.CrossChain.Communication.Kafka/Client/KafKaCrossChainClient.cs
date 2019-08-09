using System;
using System.Threading.Tasks;
using Acs7;
using AElf.CrossChain.Cache;
using AElf.CrossChain.Communication.Infrastructure;

namespace AElf.CrossChain.Communication.Kafka.Client
{
    public class KafKaCrossChainClient : ICrossChainClient
    {
        public int RemoteChainId { get; }
        public string TargetUriString { get; }
        public bool IsConnected { get; private set; }

        private Func<IBlockCacheEntity, bool> _crossChainBlockDataEntityHandler;

        private readonly IKafkaCrossChainConsumer _kafkaCrossChainConsumer;

        public KafKaCrossChainClient(string host, int port, int chainId)
        {
            TargetUriString = string.Join(":", host, port);
            RemoteChainId = chainId;
            _kafkaCrossChainConsumer = new KafkaCrossChainConsumer(TargetUriString);
            _kafkaCrossChainConsumer.SubscribeCrossChainBlockData(RemoteChainId);
        }
        
        public void SetCrossChainBlockDataEntityHandler(Func<IBlockCacheEntity, bool> crossChainBlockDataEntityHandler)
        {
            _crossChainBlockDataEntityHandler = crossChainBlockDataEntityHandler;
        }

        public async Task RequestCrossChainDataAsync(long targetHeight)
        {
            var crossChainBlockDataList = await _kafkaCrossChainConsumer.ConsumeCrossChainBlockDataAsync(targetHeight);
            foreach (var blockCacheEntity in crossChainBlockDataList)
            {
                _crossChainBlockDataEntityHandler(blockCacheEntity);
            }
        }

        public async Task<ChainInitializationData> RequestChainInitializationDataAsync(int chainId)
        {
            return await _kafkaCrossChainConsumer.ConsumeCrossChainInitializationData(chainId);
        }

        public Task ConnectAsync()
        {
            _kafkaCrossChainConsumer.SubscribeCrossChainBlockData(RemoteChainId);
            IsConnected = true;
            return Task.CompletedTask;
        }

        public async Task CloseAsync()
        {
            IsConnected = false;
            await _kafkaCrossChainConsumer.CloseAsync();
        }
    }
}