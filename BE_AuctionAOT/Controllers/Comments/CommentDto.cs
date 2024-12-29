namespace BE_AuctionAOT.Controllers.Comments;

public class CommentDto
{
    public int CommentId { get; set; }
    public int? ParentId { get; set; }
    public int TotalLike { get; set; }
    public List<CommentDto> SubComments { get; set; } = new List<CommentDto>();
    // For nested comments
}


public class PostCommentsDto
{
    public int PostId { get; set; }
    public List<CommentDto> Comments { get; set; } = new List<CommentDto>();
}