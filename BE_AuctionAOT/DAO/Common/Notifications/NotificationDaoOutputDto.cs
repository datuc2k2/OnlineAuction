using BE_AuctionAOT.Common.Base.Entity;

namespace BE_AuctionAOT.DAO.Common.Notifications
{
	public class NotificationDaoOutputDto
	{
	}

	public class CreateNotiOutputDto : BaseOutputDto
	{
	}
	public class GetTypeOutputDto : BaseOutputDto
	{
		public List<Dropdown>? Type { get; set; }
	}

	public class ListNotiAdminOutputDto : BaseOutputDto
	{
		public List<Models.Notification> Notifications { get; set; }
		public int allRecords { get; set; }
	}
	public class ListNotiUserOutputDto : BaseOutputDto
	{
		public List<Models.Notification> Notifications { get; set; }
	}
	public class NotiDetailOutputDto : BaseOutputDto
	{
		public Models.Notification Notification { get; set; }
	}
}
