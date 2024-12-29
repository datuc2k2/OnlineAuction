using BE_AuctionAOT.Common.Base.Entity;
using BE_AuctionAOT.Models;

namespace BE_AuctionAOT.Controllers.Personal_Account_Management
{
	public class PersonalDaoOutputDto : BaseOutputDto

	{
		public UserDto User { get; set; }
	}
	public class GetFrontIdCardOutputDto : BaseOutputDto
	{
		public string FrontIdCard { get; set; }
	}

	public class UserDto
	{
		public string UserRoleId { get; set; }
		public string UserRoleName { get; set; }
		public decimal Point {  get; set; }
		public string Currency { get; set; }
		public long UserId { get; set; }
		public string Username { get; set; } = null!;
		public bool? IsActive { get; set; }
		public bool? IsEkyc { get; set; }

		public virtual UserProfileDto? UserProfile { get; set; }
	}
	public class UserProfileDto
	{
		public long UserId { get; set; }
		public string? FullName { get; set; }
		public string Email { get; set; } = null!;
		public string? PhoneNumber { get; set; }
		public string? Address { get; set; }
		public string? Avatar { get; set; }
		public string? Cccd { get; set; }
		public string? FrontIdCard { get; set; }
		public string? BackIdCard { get; set; }
		public DateTime? Dob { get; set; }
		public DateTime? CreatedAt { get; set; }
		public DateTime? UpdatedAt { get; set; }
		public long? UpdatedBy { get; set; }
	}
}
