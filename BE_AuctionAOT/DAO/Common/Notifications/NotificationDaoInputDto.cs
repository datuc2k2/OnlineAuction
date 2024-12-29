using BE_AuctionAOT.Common.Base.Entity;

namespace BE_AuctionAOT.DAO.Common.Notifications
{
	public class NotificationDaoInputDto
	{
	}

	public class CreateNotiInputDto
	{
		public string title { get; set; }
		public int type { get; set; }
		public long userId { get; set; }
		public string description { get; set; }
	}

	public class ListNotiAmindInputDto : BaseInputDto
	{
		public int? type { get; set; }
		public DateOnly? startDate { get; set; }
		public DateOnly? endDate { get; set; }
	}
	public class ListNotiUserInputDto : BaseInputDto
	{
	}
	public class NotiDetailInputDto
	{
		public long notiId { get; set; }
	}
}
