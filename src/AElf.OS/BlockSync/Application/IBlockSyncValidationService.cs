using System.Threading.Tasks;
using AElf.Kernel;
using AElf.OS.Network;

namespace AElf.OS.BlockSync.Application
{
    public interface IBlockSyncValidationService
    {
        Task<bool> ValidateAnnouncementAsync(Chain chain, BlockAnnouncement blockAnnouncement, string senderPubKey);

        Task<bool> ValidateBlockAsync(Chain chain, BlockWithTransactions blockWithTransactions, string senderPubKey);

        Task<bool> ValidateTransactionAsync(BlockWithTransactions blockWithTransactions);

        Task<bool> ValidateBlockBeforeAttachAsync(BlockWithTransactions blockWithTransactions);
    }
}