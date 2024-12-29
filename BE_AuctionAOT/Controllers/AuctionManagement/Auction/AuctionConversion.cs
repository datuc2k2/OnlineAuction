using AutoMapper;

namespace BE_AuctionAOT.Controllers.AuctionManagement.Auction
{
    public class AuctionConversion : Profile
    {
        public AuctionConversion() {
            CreateMap<Models.Auction, Auction>().ReverseMap();
            CreateMap<Models.Auction, UpdateAuction>().ReverseMap();
        }
    }
}
