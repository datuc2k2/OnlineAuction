using BE_AuctionAOT.Common.Base.Entity;
using BE_AuctionAOT.Common.Constants;
using BE_AuctionAOT.Controllers.AuctionForCustomer.JoinTheAuction;
using BE_AuctionAOT.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;

namespace BE_AuctionAOT.DAO.AuctionForCustomer.JoinTheAuction
{
    public class JoinTheAuctionDao
    {
        private readonly DB_AuctionAOTContext _context;
        public JoinTheAuctionDao(DB_AuctionAOTContext context)
        {
            _context = context;
        }

        public JoinTheAuctionOutputDto BidToAution(Models.AuctionBid inputCreateAuctionBid)
        {
            try
            {
                var output = this.Output(ResultCd.SUCCESS).Create<JoinTheAuctionOutputDto>();
                _context.Add(inputCreateAuctionBid);
                _context.SaveChanges();
                var newId = inputCreateAuctionBid.BidId;
                output.NewBidId = newId;
                return output;
            }
            catch (Exception ex)
            {
                return this.Output(ResultCd.FAILURE).WithException(ex).Create<JoinTheAuctionOutputDto>();
            }
        }

        public List<AuctionBid> GetAllAuctionBidByAuctionId (int auctionId)
        {
            try
            {
                var listAuctionBidByAuctionId = _context.AuctionBids.Include(ab => ab.User).ThenInclude(u => u.UserProfile).Where(b => b.AuctionId == auctionId).OrderBy(x => x.BidAmount).ToList();
                return listAuctionBidByAuctionId;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public List<AuctionInvitationDto> GetAllAuctionInvitationByUserId(int UserId)
        {
            try
            {
                var listAuctionInvitationByUserId = (from ai in _context.AuctionInvitations
                                                     join a in _context.Auctions
                                                     on ai.AuctionId equals a.AuctionId
                                                     join u in _context.Users
                                                     on a.UserId equals u.UserId
                                                     join up in _context.UserProfiles
                                                     on u.UserId equals up.UserId
                                                     where ai.InvitedUserId == UserId
                                                     select new AuctionInvitationDto
                                                     {
                                                         InvitationId = ai.InvitationId,
                                                         AuctionId = a.AuctionId,
                                                         InvitedUserId = ai.InvitedUserId,
                                                         IsAccepted = ai.IsAccepted,
                                                         InvitedAt = ai.InvitedAt,
                                                         AcceptedAt = ai.AcceptedAt,
                                                         InviterId = u.UserId,
                                                         InviterAvatar = up.Avatar,
                                                         InviterName = u.Username,
                                                     })
                                    .OrderByDescending(ai => ai.InvitedAt)
                                    .ToList();
                return listAuctionInvitationByUserId;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public List<AuctionBid> GetAllAuctionBidByAuctionIdAndUserId(int auctionId, int userId)
        {
            try
            {
                var listAuctionBidByAuctionId = _context.AuctionBids.Include(ab => ab.User).ThenInclude(u => u.UserProfile).Where(b => b.AuctionId == auctionId && b.UserId == userId).ToList();
                return listAuctionBidByAuctionId;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public int GetUserPoint(int userId)
        {
            
            try
            {
                var point = _context.Points.FirstOrDefault(o => o.UserId == userId);
                if(point == null)
                {
                    return 0;
                }
                return (int)Math.Floor(point.PointsAmount);
            }
            catch (Exception ex)
            {
                return 0;
            }
        }

        public void UpdateUserPoint(int userId, int newPoint)
        {
            try
            {
                var point = _context.Points.FirstOrDefault(o => o.UserId == userId);
                if (point == null)
                {
                    return;
                }
                point.PointsAmount = newPoint;
                _context.Points.Update(point);
                _context.SaveChanges();
            }
            catch (Exception ex){}
        }

        public AuctionBid CreateNewAuctionBid(AuctionBid auctionBid)
        {
            try
            {
                _context.Add(auctionBid);
                _context.SaveChanges();
                return auctionBid;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public int CreateChat(int auctionID)
        {
            try
            {
                var getChatByAuctionId = _context.Chats.Where(c => c.AuctionId == auctionID).FirstOrDefault();
                if (getChatByAuctionId == null)
                {
                    Chat chat = new Chat()
                    {
                        AuctionId = auctionID,
                        IsGroupChat = false,
                        CreatedAt = DateTime.Now,
                        CreatedBy = 0000,
                    };
                    _context.Add(chat);
                    _context.SaveChanges();
                    return (int)chat.ChatId;
                }
                return (int)getChatByAuctionId.ChatId;
            }
            catch (Exception ex)
            {
                return 0;
            }
        }

        public ChatMessage CreateChatMessage(ChatMessage chatMessage)
        {
            try
            {
                _context.Add(chatMessage);
                _context.SaveChanges();
                return chatMessage;
            }
            catch (Exception ex)
            {
                return null;
            }
        }
        public List<ChatMessage> GetAllChatMessageByAuctionId(int auctionID)
        {
            try
            {
                var chatMessageByAuctionId = _context.ChatMessages.Include(c => c.Chat).Include(c => c.Sender).ThenInclude(s => s.UserProfile).Where(c => c.Chat.AuctionId == auctionID).ToList();
                return chatMessageByAuctionId;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public List<AuctionReviewDto> GetAllAuctionReviewByAuctionId(int auctionID)
        {
            try
            {
                var auctionReviewByAuctionId = _context.AuctionReviews
                    .Include(a => a.User)
                    .ThenInclude(u => u.UserProfile)
                    .Where(a => a.AuctionId == auctionID)
                    .OrderByDescending(a => a.CreatedAt)
                    .ToList();
                List<AuctionReviewDto> result = new List<AuctionReviewDto>();
                foreach(var auctionReview in auctionReviewByAuctionId)
                {
                    AuctionReviewDto auctionReviewDto = new AuctionReviewDto()
                    {
                        ReviewId = auctionReview.ReviewId,
                        AuctionId = auctionReview.AuctionId,
                        UserId = auctionReview.UserId,
                        Rating = auctionReview.Rating,
                        Comment = auctionReview.Comment,
                        CreatedAt = auctionReview.CreatedAt,
                        UpdatedAt = auctionReview.UpdatedAt,
                        UserName = auctionReview.User.Username,
                        UserAvatar = auctionReview.User.UserProfile.Avatar,
                    };
                    result.Add(auctionReviewDto);
                }

                return result;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public List<AuctionInvitation> GetAllAuctionInvitationByAuctionId(int auctionID)
        {
            try
            {
                var auctionInvitationByAuctionId = _context.AuctionInvitations.Where(a => a.AuctionId == auctionID).ToList();
                return auctionInvitationByAuctionId;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public AuctionInvitation AcceptOrRejectAuctionInvitation(int auctionInvitationId, bool acceptOrReject)
        {
            try
            {
                AuctionInvitation auctionInvitation = _context.AuctionInvitations.Where(a => a.InvitationId == auctionInvitationId).FirstOrDefault();
                if(auctionInvitation == null)
                {
                    return null;
                }
                auctionInvitation.IsAccepted = acceptOrReject;
                auctionInvitation.AcceptedAt = DateTime.Now;
                _context.Update(auctionInvitation);
                _context.SaveChanges();
                return auctionInvitation;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public AuctionReview CreateAuctionReview(AuctionReview auctionReview)
        {
            try
            {
                _context.Add(auctionReview);
                _context.SaveChanges();
                return auctionReview;
            }
            catch (Exception ex)
            {
                return null;
            }
        }



        public AutoBid CreateAutoBidBid(AutoBid autoBid)
        {
            try
            {
                _context.Add(autoBid);
                _context.SaveChanges();
                return autoBid;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public AutoBid GetAutoBidByAuctionIdAndUserId(int autionId, int userId)
        {
            try
            {
                var autoBid = _context.AutoBids.Where(a => a.AuctionId == autionId && a.UserId == userId).FirstOrDefault();
                return autoBid;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public int DeleteAutoBidBid(int Id)
        {
            try
            {
                var autoBidById = _context.AutoBids.Where(a => a.AutoBidId == Id).FirstOrDefault();
                _context.Remove(autoBidById);
                return _context.SaveChanges();
            }
            catch (Exception ex)
            {
                return 0;
            }
        }


    }   
}

public class AuctionReviewDto
{
    public long ReviewId { get; set; }
    public long? AuctionId { get; set; }
    public long UserId { get; set; }
    public byte? Rating { get; set; }
    public string? Comment { get; set; }
    public DateTime? CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? UserName { get; set;}
    public string? UserAvatar { get; set;}
}
