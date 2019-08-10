using System;
using System.Threading;
using System.Threading.Tasks;
using Acs7;
using AElf.CrossChain.Cache;
using AElf.CrossChain.Communication.Infrastructure;
using Confluent.Kafka;

namespace AElf.CrossChain.Communication.Kafka.Client
{
    public class KafKaCrossChainClient : ICrossChainClient
    {
        public int RemoteChainId { get; }
        public string TargetUriString { get; }
        public bool IsConnected { get; private set; }

        private Func<IBlockCacheEntity, bool> _crossChainBlockDataEntityHandler;

        private readonly IKafkaCrossChainConsumer _kafkaCrossChainConsumer;

        private readonly int _consumeTimeoutInMilliSeconds;
        
        public KafKaCrossChainClient(string host, int port, int chainId, int consumeTimeoutInMilliSeconds)
        {
            TargetUriString = string.Join(":", host, port);
            RemoteChainId = chainId;
            _kafkaCrossChainConsumer = new KafkaCrossChainConsumer(TargetUriString);
            _consumeTimeoutInMilliSeconds = consumeTimeoutInMilliSeconds;
        }
        
        public void SetCrossChainBlockDataEntityHandler(Func<IBlockCacheEntity, bool> crossChainBlockDataEntityHandler)
        {
            _crossChainBlockDataEntityHandler = crossChainBlockDataEntityHandler;
        }

        public async Task RequestCrossChainDataAsync(long targetHeight)
        {
            using (var cts = new CancellationTokenSource(_consumeTimeoutInMilliSeconds))
            {
                await _kafkaCrossChainConsumer.ConsumeCrossChainBlockDataAsync(targetHeight, cts,
                    _crossChainBlockDataEntityHandler);
            }
        }

        public async Task<ChainInitializationData> RequestChainInitializationDataAsync(int chainId)
        {
            return await _kafkaCrossChainConsumer.ConsumeCrossChainInitializationData(chainId);
        }

        public async Task ConnectAsync()
        {
            await _kafkaCrossChainConsumer.SubscribeCrossChainBlockDataAsync(RemoteChainId);
            IsConnected = true;
        }

        public Task CloseAsync()
        {
            IsConnected = false;
            _kafkaCrossChainConsumer.Close();
            return Task.CompletedTask;
        }
    }
}