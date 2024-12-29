using AutoMapper;
using BE_AuctionAOT.Controllers.Personal_Account_Management;
using BE_AuctionAOT.Models;

namespace BE_AuctionAOT.DAO.Personal_Account_Management.Personal_Account
{
	public class PersonalConversion : Profile
	{
		public PersonalConversion()
		{
			CreateMap<User, UserDto>()
			.ForMember(dest => dest.UserProfile, opt => opt.MapFrom(src => src.UserProfile)).ReverseMap();

			CreateMap<UserProfile, UserProfileDto>().ReverseMap();
		}


	}
}
