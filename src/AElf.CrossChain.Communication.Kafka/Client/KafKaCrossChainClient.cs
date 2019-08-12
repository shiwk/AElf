using System;
using System.Threading;
using System.Threading.Tasks;
using Acs7;
using AElf.CrossChain.Cache;
using AElf.CrossChain.Communication.Infrastructure;
using Google.Protobuf;

namespace AElf.CrossChain.Communication.Kafka.Client
{
    public abstract class KafKaCrossChainClient<T> : ICrossChainClient where T : IMessage<T>, IBlockCacheEntity, new()
    {
        public int RemoteChainId { get; }
        public string TargetUriString { get; }
        public bool IsConnected => KafkaCrossChainConsumer.IsAlive;

        private Func<IBlockCacheEntity, bool> _crossChainBlockDataEntityHandler;

        protected readonly IKafkaCrossChainConsumer KafkaCrossChainConsumer;

        protected readonly int ConsumeTimeoutInMilliSeconds;

        protected KafKaCrossChainClient(string host, int port, int chainId, int consumeTimeoutInMilliSeconds)
        {
            TargetUriString = string.Join(":", host, port);
            RemoteChainId = chainId;
            KafkaCrossChainConsumer = new KafkaCrossChainConsumer<T>(TargetUriString);
            ConsumeTimeoutInMilliSeconds = consumeTimeoutInMilliSeconds;
        }
        
        public void SetCrossChainBlockDataEntityHandler(Func<IBlockCacheEntity, bool> crossChainBlockDataEntityHandler)
        {
            _crossChainBlockDataEntityHandler = crossChainBlockDataEntityHandler;
        }

        public async Task RequestCrossChainDataAsync(long targetHeight)
        {
            using (var cts = new CancellationTokenSource(ConsumeTimeoutInMilliSeconds))
            {
                await KafkaCrossChainConsumer.ConsumeCrossChainBlockDataAsync(targetHeight, cts,
                    _crossChainBlockDataEntityHandler);
            }
        }

        public abstract Task<ChainInitializationData> RequestChainInitializationDataAsync(int chainId);

        protected virtual async Task SubscribeTopicsAsync()
        {
            await KafkaCrossChainConsumer.SubscribeCrossChainBlockDataTopicAsync(RemoteChainId);
        }

        public async Task ConnectAsync()
        {
            await SubscribeTopicsAsync();
        }

        public Task CloseAsync()
        {
            KafkaCrossChainConsumer.Close();
            return Task.CompletedTask;
        }
    }

    public sealed class KafkaClientForParentChain : KafKaCrossChainClient<ParentChainBlockData>
    {
        public KafkaClientForParentChain(string host, int port, int chainId, int consumeTimeoutInMilliSeconds) 
            : base(host, port, chainId, consumeTimeoutInMilliSeconds)
        {
        }
        
        public override async Task<ChainInitializationData> RequestChainInitializationDataAsync(int chainId)
        {
            using (var cts = new CancellationTokenSource(ConsumeTimeoutInMilliSeconds))
            {
                return await KafkaCrossChainConsumer.ConsumeCrossChainInitializationData(chainId, cts);
            }
        }

        protected override async Task SubscribeTopicsAsync()
        {
            await base.SubscribeTopicsAsync();
            await KafkaCrossChainConsumer.SubscribeChainInitializationDataTopicAsync(RemoteChainId);
        }
    }

    public sealed class KafkaClientForSideChain : KafKaCrossChainClient<SideChainBlockData>
    {
        public KafkaClientForSideChain(string host, int port, int chainId, int consumeTimeoutInMilliSeconds) 
            : base(host, port, chainId, consumeTimeoutInMilliSeconds)
        {
        }

        public override Task<ChainInitializationData> RequestChainInitializationDataAsync(int chainId)
        {
            throw new NotImplementedException();
        }
    }
}