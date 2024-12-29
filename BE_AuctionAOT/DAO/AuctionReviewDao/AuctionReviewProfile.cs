using AutoMapper;
using BE_AuctionAOT.Models;

namespace BE_AuctionAOT.DAO.AuctionReviewDao
{
    public class AuctionReviewProfile : Profile
    {
        public AuctionReviewProfile() 
        {
            CreateMap<AuctionReviewImage, AuctionReviewImageDto>();

            CreateMap<User, AuctionReviewUserDto>()
                      .ForMember(dest => dest.Avatar, opt => opt.MapFrom(src => src.UserProfile.Avatar));

            CreateMap<AuctionReview, AuctionReviewDto>()
                  .ForMember(dest => dest.Content, opt => opt.MapFrom(src => src.Comment))
                  .ForMember(dest => dest.Owner, opt => opt.MapFrom(src => src.User));
            CreateMap<AuctionReview, AuctionReviewCommentDto>()
                     .ForMember(dest => dest.Content, opt => opt.MapFrom(src => src.Comment))
                     .ForMember(dest => dest.Owner, opt => opt.MapFrom(src => src.User));
        }

    }
}
