using AutoMapper;
using AutoMapper.QueryableExtensions;
using Azure.Core;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Blobs;
using BE_AuctionAOT.DAO.Search;
using BE_AuctionAOT.Models;
using Microsoft.EntityFrameworkCore;
using BE_AuctionAOT.Controllers.Users;

namespace BE_AuctionAOT.DAO.AuctionReviewDao
{
    public class AuctionReviewDao
    {

        private readonly DB_AuctionAOTContext _context;
        private readonly IMapper _mapper;
        private readonly BlobServiceClient _blobServiceClient;
        public AuctionReviewDao(DB_AuctionAOTContext context, IMapper mapper, BlobServiceClient blobServiceClient)
        {
            _mapper = mapper;
            _context = context;
            _blobServiceClient = blobServiceClient;
        }
        public async Task<bool> ToggleLikeAsync(long userId,  long auctionReviewId)
        {
            var existingLike = await _context.Likes
                .Where(l => l.UserId == userId &&
                            l.AuctionReviewId == auctionReviewId  &&
                            l.DeletedAt == null)
                .FirstOrDefaultAsync();
            var auctionReview = await _context.AuctionReviews
                 .Where(ar => ar.ReviewId == auctionReviewId)
                 .FirstOrDefaultAsync();

            if (auctionReview == null)
            {
                return false; 
            }

            if (existingLike != null)
            {
                // Nếu đã có like, thực hiện xóa
                existingLike.DeletedAt = DateTime.UtcNow;
                _context.Likes.Update(existingLike);

                auctionReview.LikesCount = Math.Max(0, auctionReview.LikesCount - 1);
            }
            else
            {
                // Nếu chưa có like, tạo mới like
                var newLike = new Like
                {
                    UserId = userId,
                    AuctionReviewId = auctionReviewId,
                    CreatedAt = DateTime.UtcNow
                };

                await _context.Likes.AddAsync(newLike);

                auctionReview.LikesCount += 1;
            }

            _context.AuctionReviews.Update(auctionReview);
            await _context.SaveChangesAsync();
            return existingLike == null; // Trả về true nếu like mới được thêm, false nếu like bị xóa
        }
        public async Task<bool> UpdateComment(long reviewId, UpdateCommentRequest request)
        {
            using (var transaction = _context.Database.BeginTransaction())
            {
                try
                {
                    var comment = _context.AuctionReviews
                        .FirstOrDefault(ar => ar.ReviewId == reviewId);

                    if (comment == null)
                    {
                        return false;
                    }

                    comment.Comment = request.Content;
                    comment.UpdatedAt = DateTime.UtcNow;

                    if (request.Images != null && request.Images.Any())
                    {
                        var oldImages = _context.AuctionReviewImages.Where(ir => ir.ReviewId == reviewId).ToList();
                        _context.AuctionReviewImages.RemoveRange(oldImages);

                        var addedUrls = new List<AuctionReviewImage>();
                        string containerName = "productimg";
                        BlobContainerClient containerClient = _blobServiceClient.GetBlobContainerClient(containerName);

                        foreach (var file in request.Images)
                        {
                            if (file != null && file.Length > 0)
                            {
                                var blobName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);

                                BlobClient blobClient = containerClient.GetBlobClient(blobName);

                                var blobHttpHeaders = new BlobHttpHeaders
                                {
                                    ContentType = file.ContentType
                                };

                                using (var stream = file.OpenReadStream())
                                {
                                    await blobClient.UploadAsync(stream, new BlobUploadOptions { HttpHeaders = blobHttpHeaders });
                                }

                                string fileUrl = blobClient.Uri.ToString();
                                addedUrls.Add(new AuctionReviewImage()
                                {
                                    ReviewId = reviewId,
                                    ImageUrl = fileUrl,
                                });
                            }
                        }

                        _context.AuctionReviewImages.AddRange(addedUrls);
                    }

                    _context.SaveChanges();

                    transaction.Commit();

                    return true;
                }
                catch (Exception)
                {
                    transaction.Rollback();
                    throw;
                }
            }
        }
        public bool DeleteComment(long reviewId)
        {
            var comment = _context.AuctionReviews
                .Include(ar => ar.SubComments)
                .FirstOrDefault(ar => ar.ReviewId == reviewId);

            if (comment == null)
            {
                return false;
            }

            DeleteSubComments(comment.SubComments.ToList());


            _context.AuctionReviews.Remove(comment);
            _context.SaveChanges();

            return true;
        }
        public AuctionReviewDto CreateReview(long userId, long currentUserId, byte rating, string comment)
        {
            var user = _context.Users.Include(u => u.UserProfile).FirstOrDefault(u => u.UserId == userId);
            if (user == null)
            {
                throw new Exception("User not found.");
            }

            var currentUser = _context.Users.Include(u => u.UserProfile).FirstOrDefault(u => u.UserId == currentUserId);
            if (currentUser == null)
            {
                throw new Exception("User not found.");
            }

            var date = DateTime.Now;
            var auctionReview = new AuctionReview
            {
                UserId = currentUserId,
                User = currentUser,
                ToUserId = userId,
                Rating = rating,
                Comment = comment,
                CreatedAt = date,
                UpdatedAt = date,
            };

            _context.AuctionReviews.Add(auctionReview);
            _context.SaveChanges();


            var auctionDto = new AuctionReviewDto
            {
                ReviewId = auctionReview.ReviewId,
                AuctionId = auctionReview.AuctionId,
                UserId = auctionReview.UserId,
                Rating = auctionReview.Rating,
                Content = auctionReview.Comment,
                CreatedAt = auctionReview.CreatedAt,
                UpdatedAt = auctionReview.UpdatedAt,
                LikesCount = auctionReview.LikesCount,
                IsLiked =  IsLikedAsync(currentUserId, auctionReview.ReviewId),
                SubCommentsCount = GetAllNestedComments(auctionReview.SubComments.ToList()).Count,
                Owner = new AuctionReviewUserDto
                {
                    UserId = auctionReview.UserId,
                    Avatar = auctionReview.User?.UserProfile?.Avatar,
                    Username = auctionReview.User?.Username
                }
            };
            return auctionDto;
        }
		public List<AuctionReviewDto> GetReviewsByUserId(long userId, long currentUserId)
		{
			var reviews = _context.AuctionReviews
				.Include(ar => ar.SubComments)
				.Include(ar => ar.Auction)
				.Include(ar => ar.User)
					.ThenInclude(u => u.UserProfile)
				.Where(ar =>
				/*    (ar.Auction != null && ar.Auction.UserId == userId) 
					|| */
				(ar.ToUser != null && ar.ToUser.UserId == userId)
				)
				.OrderByDescending(ar => ar.CreatedAt)
				.ToList();
			if (!reviews.Any()) return new List<AuctionReviewDto>();
			if (reviews != null)
			{
				LoadSubComments(reviews); // Tải toàn bộ sub-comments
			}

			var reviewDtos = reviews?.Select(ar => new AuctionReviewDto
			{
				ReviewId = ar.ReviewId,
				AuctionId = ar.AuctionId,
				UserId = ar.UserId,
				Rating = ar.Rating,
				Content = ar.Comment,
				CreatedAt = ar.CreatedAt,
				UpdatedAt = ar.UpdatedAt,
				LikesCount = ar.LikesCount,
				IsLiked = IsLikedAsync(currentUserId, ar.ReviewId),
				SubCommentsCount = GetAllNestedComments(ar.SubComments.ToList()).Count,
				Owner = new AuctionReviewUserDto
				{
					UserId = ar.UserId,
					Avatar = ar.User?.UserProfile?.Avatar,
					Username = ar.User?.Username
				}
			}).ToList();

			return reviewDtos;

		}

		public List<AuctionReviewCommentDto> GetCommentsByReviewId(long reviewId, long userId)
        {
            var comments = _context.AuctionReviews
                .Include(ar => ar.SubComments)
                .Include(ar => ar.Images)
                .Include(sc => sc.User)
                    .ThenInclude(u => u.UserProfile)
                .Where(ar => ar.ParentId == reviewId)
                .ToList();

            if (comments != null)
            {
                LoadSubComments(comments); // Tải toàn bộ sub-comments
            }

            // Flatten các bình luận với độ sâu tùy chỉnh
            var result = FlattenSubCommentsToDepth(comments, userId, 1, 1);

            return result.OrderBy(ar => ar.CreatedAt).ToList();
        }
		public async Task<AuctionReviewCommentDto> AddComment(AddCommentRequest request)
		{
			using var transaction = await _context.Database.BeginTransactionAsync();
			try
			{
				var user = _context.Users.Include(u => u.UserProfile).FirstOrDefault(u => u.UserId == request.UserId);
				if (user == null)
				{
					throw new Exception("User not found.");
				}

				var date = DateTime.Now;
				var auctionReview = new AuctionReview
				{
					UserId = request.UserId,
					User = user,
					Comment = request.Comment,
					ParentId = request.ParentId,
					LikesCount = 0,
					CreatedAt = date,
					UpdatedAt = date
				};

				_context.AuctionReviews.Add(auctionReview);
				await _context.SaveChangesAsync();

				if (request.Images != null && request.Images.Any())
				{
					// Upload ảnh lên Azure Blob Storage và lưu URL vào DB
					var addedUrls = new List<AuctionReviewImage>();
					string containerName = "productimg";
					BlobContainerClient containerClient = _blobServiceClient.GetBlobContainerClient(containerName);

					foreach (var file in request.Images)
					{
						if (file != null && file.Length > 0)
						{
							var blobName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);

							BlobClient blobClient = containerClient.GetBlobClient(blobName);

							var blobHttpHeaders = new BlobHttpHeaders
							{
								ContentType = file.ContentType
							};

							// Upload file lên Azure Blob Storage
							using (var stream = file.OpenReadStream())
							{
								await blobClient.UploadAsync(stream, new BlobUploadOptions { HttpHeaders = blobHttpHeaders });
							}

							// Lưu URL ảnh vào DB
							string fileUrl = blobClient.Uri.ToString();
							addedUrls.Add(new AuctionReviewImage()
							{
								ReviewId = auctionReview.ReviewId,
								ImageUrl = fileUrl,
							});
						}
					}
					_context.AuctionReviewImages.AddRange(addedUrls);
					await _context.SaveChangesAsync();

				}

				// Commit transaction
				await transaction.CommitAsync();

				// Trả về DTO của AuctionReview
				return _mapper.Map<AuctionReviewCommentDto>(auctionReview);
			}
			catch (Exception ex)
			{
				// Rollback nếu có lỗi
				await transaction.RollbackAsync();
				throw new Exception($"Error while adding comment: {ex.Message}", ex);
			}
		}


		// INNER FUNCTION //
		private bool IsLikedAsync(long userId, long auctionReviewId)
        {
            var like = _context.Likes
                .Where(l => l.UserId == userId &&
                            l.AuctionReviewId == auctionReviewId &&
                            l.DeletedAt == null)
                .FirstOrDefault();

            return like != null;
        }
        private void DeleteSubComments(List<AuctionReview> subComments)
        {
            foreach (var subComment in subComments)
            {
                if (subComment.SubComments != null && subComment.SubComments.Any())
                {
                    DeleteSubComments(subComment.SubComments.ToList());
                }

                _context.AuctionReviews.Remove(subComment);
            }
        }
        private void LoadSubComments(List<AuctionReview> comments)
        {
            foreach (var comment in comments)
            {
                // Tải SubComments cho từng review
                _context.Entry(comment)
                    .Collection(r => r.SubComments)
                    .Query()
                    .Include(ar => ar.Images)
                    .Include(sc => sc.User) // Bao gồm User
                        .ThenInclude(u => u.UserProfile) // Bao gồm UserProfile
                    .Load();

                // Đệ quy: Gọi LoadSubComments với SubComments của review hiện tại
                if (comment.SubComments != null && comment.SubComments.Any())
                {
                    LoadSubComments(comment.SubComments.ToList());
                }
            }
        }
        private List<AuctionReviewCommentDto> FlattenSubCommentsToDepth(List<AuctionReview> comments, long userId, int currentDepth, int maxDepth)
        {
            var flattenedComments = new List<AuctionReviewCommentDto>();

            foreach (var comment in comments)
            {
                // Chuyển đổi bình luận hiện tại thành DTO
                var dto = new AuctionReviewCommentDto
                {
                    ReviewId = comment.ReviewId,
                    AuctionId = comment.AuctionId,
                    UserId = comment.UserId,
                    Rating = comment.Rating,
                    Content = comment.Comment,
                    CreatedAt = comment.CreatedAt,
                    UpdatedAt = comment.UpdatedAt,
                    LikesCount = comment.LikesCount,
                    IsReviewAboutAuction = comment.IsReviewAboutAuction,
                    IsCommentAboutReview = comment.IsCommentAboutReview,
                    IsReplyComment = comment.IsReplyComment,
                    ParentId = comment.ParentId,
                    IsLiked = IsLikedAsync(userId, comment.ReviewId),
                    Images = _mapper.Map<List<AuctionReviewImageDto>>(comment.Images),
                    Owner = new AuctionReviewUserDto
                    {
                        UserId = comment.User.UserId,
                        Avatar = comment.User.UserProfile.Avatar,
                        Username = comment.User.Username
                    },
                    SubComments = new List<AuctionReviewCommentDto>() // Khởi tạo danh sách rỗng
                };

                // Nếu chưa vượt quá độ sâu tối đa, tiếp tục đệ quy
                if (currentDepth < maxDepth)
                {
                    dto.SubComments = FlattenSubCommentsToDepth(comment.SubComments?.ToList() ?? new List<AuctionReview>(), userId, currentDepth + 1, maxDepth);
                }
                else
                {
                    // Khi đạt maxDepth, gom tất cả sub-comments vào một danh sách phẳng
                    var allSubComments = GetAllNestedComments(comment.SubComments?.ToList() ?? new List<AuctionReview>());

                    dto.SubComments = allSubComments.Select(sc => new AuctionReviewCommentDto
                    {
                        ReviewId = sc.ReviewId,
                        AuctionId = sc.AuctionId,
                        UserId = sc.UserId,
                        Rating = sc.Rating,
                        Content = sc.Comment,
                        CreatedAt = sc.CreatedAt,
                        UpdatedAt = sc.UpdatedAt,
                        LikesCount = sc.LikesCount,
                        IsReviewAboutAuction = sc.IsReviewAboutAuction,
                        IsCommentAboutReview = sc.IsCommentAboutReview,
                        IsReplyComment = sc.IsReplyComment,
                        ParentId = sc.ParentId,
                        IsLiked = IsLikedAsync(userId, sc.ReviewId),
                        Images = _mapper.Map<List<AuctionReviewImageDto>>(comment.Images),
                        Owner = new AuctionReviewUserDto
                        {
                            UserId = sc.User.UserId,
                            Avatar = sc.User.UserProfile.Avatar,
                            Username = sc.User.Username
                        }
                    }).ToList();
                }

                flattenedComments.Add(dto);
            }

            return flattenedComments;
        }
        private List<AuctionReview> GetAllNestedComments(List<AuctionReview> comments)
        {
            var allComments = new List<AuctionReview>();

            foreach (var comment in comments)
            {
                allComments.Add(comment);

                // Đệ quy để thêm tất cả các cấp bình luận con
                allComments.AddRange(GetAllNestedComments(comment.SubComments?.ToList() ?? new List<AuctionReview>()));
            }

            return allComments;
        }
    }
}
