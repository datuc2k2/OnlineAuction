using System.ComponentModel.DataAnnotations;

namespace BE_AuctionAOT.Controllers.Personal_Account_Management
{
	public class PersonalDaoInputDto
	{
		[Required]
		public string Username { get; set; } = null!;
		[Required]
		public string? FullName { get; set; }
		[Required]
		public string Email { get; set; } = null!;
		[Required]
		public string? PhoneNumber { get; set; }
		[Required]
		public string? Address { get; set; }
		public IFormFile? Avatar { get; set; }
		[Required]
		public string? Cccd { get; set; }
		[Required]
		public IFormFile? FrontIdCard { get; set; }
		[Required]
		public IFormFile? BackIdCard { get; set; }
		[Required]
		public DateTime? Dob { get; set; }
	}

	public class EkycInputDto
	{
		[Required]
		public IFormFile portrait_img { get; set; }
		[Required]
		public string clientSession { get; set; }
	}
}
