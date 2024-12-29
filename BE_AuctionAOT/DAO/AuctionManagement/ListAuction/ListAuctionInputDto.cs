using BE_AuctionAOT.Common.Base.Entity;

namespace BE_AuctionAOT.DAO.AuctionManagement.ListAuction
{

	public class ListAuctionInputDto : BaseInputDto
	{
		public string searchText { get; set; }
		public DateOnly? startDate { get; set; }
		public DateOnly? endDate { get; set; }
		public long? status { get; set; }
	}
	public class ListOtherUserAuctionInputDto : BaseInputDto
	{
		public String userId { get; set; }
		public string searchText { get; set; }
		public DateOnly? startDate { get; set; }
		public DateOnly? endDate { get; set; }
		public long? status { get; set; }
	}
	public class JoinedAuctionInputDto : BaseInputDto
	{
		public string searchText { get; set; }
		public DateOnly? startDate { get; set; }
		public DateOnly? endDate { get; set; }
		public bool? isWinner { get; set; }
	}

	public class FillterAuctionInputDto
	{
		public int? CategoryId { get; set; }
		public int? Status { get; set; }
		public DateTime? StartDate { get; set; }
		public DateTime? EndDate { get; set; }
		public DateTime? StartTime { get; set; }
		public DateTime? EndTime { get; set; }
		public decimal? StartPrice { get; set; }
		public decimal? EndPrice { get; set; }
		public int pageIndex { get; set; }
		public int pageSize { get; set; }
    }
    public class FillterViewAuctionInputDto
    {
        public int? UserId { get; set; }
        public int? Status { get; set; }
        public int pageIndex { get; set; }
        public int pageSize { get; set; }
    }

}
