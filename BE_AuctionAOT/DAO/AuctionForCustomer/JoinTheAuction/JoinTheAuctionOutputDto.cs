using BE_AuctionAOT.Common.Base.Entity;
using BE_AuctionAOT.Models;

namespace BE_AuctionAOT.DAO.AuctionForCustomer.JoinTheAuction
{
    public class JoinTheAuctionOutputDto : BaseOutputDto
    {
        public long NewBidId { get; set; }
    }
}
