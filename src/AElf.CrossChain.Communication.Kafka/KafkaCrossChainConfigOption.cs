namespace AElf.CrossChain.Communication.Kafka
{
    public class KafkaCrossChainConfigOption
    {
        public string BrokerHost { get; set; }
        public int BrokerPort { get; set; }
        public int ConsumeTimeout { get; set; } = 500;
    }
}