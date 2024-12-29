using BE_AuctionAOT.Common.Base.Entity;

namespace BE_AuctionAOT.Controllers.Posts
{
    public class CreatePostOutputDto : BaseOutputDto
    {
        public int Id {  get; set; }
    }
    public class GetPostCategoryOutputDto : BaseOutputDto
    {
        public List<Dropdown>? Categories { get; set; }
    }
}
