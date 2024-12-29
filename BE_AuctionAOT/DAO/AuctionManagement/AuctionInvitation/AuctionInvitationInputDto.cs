using BE_AuctionAOT.Common.Base.Entity;

namespace BE_AuctionAOT.DAO.AuctionManagement.AuctionInvitation
{
    public class AuctionInvitationInputDto : BaseInputDto
    {
        public string searchName {  get; set; }

		public DateOnly? startDate { get; set; }
		public DateOnly? endDate { get; set; }

    }
    public class IsAcceptedInvitation
    {
		public string auctionId { get; set; }
		public bool isAccept { get; set; }
	}

    public class CreateInvitationInputDto
    {
        public long AuctionId { get; set; }
        public List<long> InvitedIds { get; set; }
    }
}
