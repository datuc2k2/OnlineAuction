namespace BE_AuctionAOT.Models
{
    public partial class AuctionReviewImage
    {
        public long ImageId { get; set; } // ID của hình ảnh
        public long ReviewId { get; set; } // Liên kết tới AuctionReview
        public string ImageUrl { get; set; } = null!; // Đường dẫn hoặc tên file

        public virtual AuctionReview AuctionReview { get; set; } = null!;
    }
}
