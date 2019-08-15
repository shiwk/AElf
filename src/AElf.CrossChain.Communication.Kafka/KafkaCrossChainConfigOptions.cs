namespace AElf.CrossChain.Communication.Kafka
{
    public class KafkaCrossChainConfigOptions
    {
        public string BrokerHost { get; set; }
        public int BrokerPort { get; set; }
        public int StatisticsIntervalMilliSeconds { get; set; } = KafkaCrossChainConstants.DefaultSessionTimeoutMilliSeconds;
        public int SessionTimeoutMilliSeconds { get; set; } = KafkaCrossChainConstants.DefaultSessionTimeoutMilliSeconds;
    }
}