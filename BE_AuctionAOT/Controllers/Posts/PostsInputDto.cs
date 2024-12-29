using BE_AuctionAOT.Models;
using System.ComponentModel.DataAnnotations;

namespace BE_AuctionAOT.Controllers.Posts
{
    public class CreatePostInputDto
    {
        [Required]
        public Post? Post { get; set; }
        [Required]
        public List<IFormFile>? Images { get; set; }
    }

    public class Post
    {
        [Required]
        public string Title { get; set; } = null!;
        [Required]
        public string Content { get; set; } = string.Empty;
        public long? CategoryId { get; set; }
    }
}
