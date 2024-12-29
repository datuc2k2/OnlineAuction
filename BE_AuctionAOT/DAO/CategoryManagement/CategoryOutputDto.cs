using BE_AuctionAOT.Common.Base.Entity;
using BE_AuctionAOT.Models;
namespace BE_AuctionAOT.DAO.CategoryManagement
{
    public class CategoryOutputDto : BaseOutputDto
    {
        public List<Category> Categories { get; set; } = new List<Category>();
    }
}
