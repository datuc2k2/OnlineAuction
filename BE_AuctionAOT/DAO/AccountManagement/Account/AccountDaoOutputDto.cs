using BE_AuctionAOT.Common.Base.Entity;

namespace BE_AuctionAOT.DAO.AccountManagement.Account
{
	public class AccountDaoOutputDto : BaseOutputDto
	{
		public List<UserFromDto> Users { get; set; }
		public int allRecords { get; set; }
	}

	public class AccountInviteDaoOutputDto : BaseOutputDto
	{
		public List<AccountInvite> AccountInvites { get; set; }
	}
	public class OtherUserInforOutputDto
	{
		public string Username { get; set; }
		public string Avatar { get; set; }
		public DateTime? CreatedAt { get; set; }
		public DateTime? LastLogin { get; set; }
		public bool? IsEkyc { get; set; }
		public bool? IsActive { get; set; }
	}

	public class AccountInvite
	{
		public long UserId { get; set; }
		public string Username { get; set; } = null!;
		public string Email { get; set; }
	}
	public class AccountGetForAuction
	{
		public long UserId { get; set; }
		public string Username { get; set; } = null!;
		public string Email { get; set; }
		public string Avatar { get; set; }
	}
	public class UserFromDto
	{
		public long UserId { get; set; }
		public string Username { get; set; } = null!;
		public bool? IsActive { get; set; }
		public DateTime? CreatedAt { get; set; }
		public DateTime? UpdatedAt { get; set; }
		public UserProfileDto UserProfile { get; set; }
	}
	public class UserDto
	{
		public long UserId { get; set; }
		public string Username { get; set; } = null!;
		public bool? IsActive { get; set; }
		public bool? EmailVerified { get; set; }
		public DateTime? CreatedAt { get; set; }
		public DateTime? UpdatedAt { get; set; }

		public virtual UserProfileDto? UserProfile { get; set; }
	}
	public partial class UserProfileDto
	{
		public string? FullName { get; set; }
		public string Email { get; set; } = null!;
		public string? PhoneNumber { get; set; }
		public string? Address { get; set; }
		public string? Avatar { get; set; }
		public string? Cccd { get; set; }
		public DateTime? Dob { get; set; }
		public DateTime? CreatedAt { get; set; }
		public DateTime? UpdatedAt { get; set; }
	}
}
