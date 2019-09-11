namespace AElf.Contracts.Consensus.AEDPoS
{
    // ReSharper disable once InconsistentNaming
    public static class AEDPoSContractConstants
    {
        public const int MaximumTinyBlocksCount = 8;
        public const int TotalTinySlots = MaximumTinyBlocksCount;
        public const int LimitBlockExecutionTimeTotalShares = 5;
        public const int LimitBlockExecutionTimeWeight = 3;
        public const int RandomNumberDueRoundCount = 1024;
        public const int MinimumIntervalOfProducingBlocks = 100;
        public const long InitialMiningRewardPerBlock = 12500000;
        public const long TimeToReduceMiningRewardByHalf = 126144000;// 60 * 60 * 24 * 365 * 4
        public const int InitialMinersCount = 9;
        public const int KeepRounds = 32;
        public const long MaximumMissedBlocksCount = 34560;
    }
}