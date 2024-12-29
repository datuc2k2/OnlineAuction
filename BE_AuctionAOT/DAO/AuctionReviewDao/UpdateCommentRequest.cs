namespace BE_AuctionAOT.DAO.AuctionReviewDao
{
    public class UpdateCommentRequest
    {
        public string Content { get; set; }
        public List<IFormFile>? Images { get; set; }
    }
}