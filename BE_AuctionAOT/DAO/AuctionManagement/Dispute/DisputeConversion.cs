using AutoMapper;
using BE_AuctionAOT.Controllers.Personal_Account_Management;
using BE_AuctionAOT.Models;

namespace BE_AuctionAOT.DAO.AuctionManagement.Dispute
{
	public class DisputeConversion: Profile
	{
		public DisputeConversion() {
			CreateMap<Models.Auction, AuctionDto>()
				.ForMember(dest => dest.AuctionRequests, opt => opt.MapFrom(src => src.AuctionRequests)).ReverseMap();
			CreateMap<Models.AuctionRequest, AuctionRequestDto>().ReverseMap();

			CreateMap<Models.AuctionBid, AuctionBidDto>()
				.ForMember(dest => dest.Auction, opt => opt.MapFrom(src => src.Auction)).ReverseMap();
			CreateMap<Models.Auction, JoinedAuctionDto>().ReverseMap();

			CreateMap<Models.Auction, AuctionBidedDto>().ReverseMap();
			CreateMap<Models.Dispute, DisputeDto>()
				.ForMember(dest => dest.Auction, opt => opt.MapFrom(src => src.Auction)).ReverseMap();

			CreateMap<Models.EvidenceFile, EvidenceFileDto>().ReverseMap();
		}
	}
}
