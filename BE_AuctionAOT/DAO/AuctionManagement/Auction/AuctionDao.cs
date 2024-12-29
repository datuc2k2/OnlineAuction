using AutoMapper;
using BE_AuctionAOT.Common.Base.Entity;
using BE_AuctionAOT.Common.Constants;
using BE_AuctionAOT.Controllers.AuctionManagement.Auction;
using BE_AuctionAOT.DAO.Common.User;
using BE_AuctionAOT.Models;
using Microsoft.AspNetCore.Components.Forms;

namespace BE_AuctionAOT.DAO.AuctionManagement.Auction
{
    public class AuctionDao
    {
        private readonly DB_AuctionAOTContext _context;
        public AuctionDao(DB_AuctionAOTContext context)
        {
            _context = context;
        }

        public async Task<CreateAuctionOutputDto> CreateAution(Models.Auction inputCreateAuc)
        {
            try
            {
                var output = this.Output(ResultCd.SUCCESS).Create<CreateAuctionOutputDto>();
                await _context.AddAsync(inputCreateAuc);
                await _context.SaveChangesAsync();
                var newId = inputCreateAuc.AuctionId;
                output.NewAucId = newId;
                return output;
            }
            catch (Exception ex)
            {
                return this.Output(ResultCd.FAILURE).WithException(ex).Create<CreateAuctionOutputDto>();
            }
        }

        public GetAuctionCategoryOutputDto GetCategory(int type)
        {
            try
            {
                var output = this.Output(ResultCd.SUCCESS).Create<GetAuctionCategoryOutputDto>();

                var cate = _context.Categories.Where(x => x.Type == type).Select(x => new Dropdown()
                {
                    Id = x.CategoryId,
                    Name = x.CategoryName,
                    Value = x.Value+""
                }).ToList();
                output.Categories = cate;
                return output;
            }
            catch (Exception ex)
            {
                return this.Output(ResultCd.FAILURE).WithException(ex).Create<GetAuctionCategoryOutputDto>();
            }
        }

        public BaseOutputDto UpdateAution(Models.Auction inputUpdateAuc, long? id)
        {
            try
            {
                var output = this.Output(ResultCd.SUCCESS).Create<BaseOutputDto>();
                var auc = _context.Auctions.Where(x => x.AuctionId == inputUpdateAuc.AuctionId).FirstOrDefault();
                if (auc != null)
                {
                    auc.UserId = inputUpdateAuc.UserId;
                    auc.ProductName = inputUpdateAuc.ProductName;
                    auc.CategoryId = inputUpdateAuc.CategoryId;
                    auc.StartingPrice = inputUpdateAuc.StartingPrice;
                    auc.Currency = inputUpdateAuc.Currency;
                    auc.Description = inputUpdateAuc.Description;
                    auc.StepPrice = inputUpdateAuc.StepPrice;
                    auc.Mode = inputUpdateAuc.Mode;
                    auc.IsPrivate = inputUpdateAuc.IsPrivate;
                    auc.DepositAmount = inputUpdateAuc.DepositAmount;
                    auc.DepositDeadline = inputUpdateAuc.DepositDeadline;
                    auc.StartTime = inputUpdateAuc.StartTime;
                    auc.EndTime = inputUpdateAuc.EndTime;
                    auc.UpdatedAt = DateTime.Now;
                    auc.UpdatedBy = id;

                    _context.Update(auc);
                    _context.SaveChanges();
                }

                return output;
            }
            catch (Exception ex)
            {
                return this.Output(ResultCd.FAILURE).WithException(ex).Create<BaseOutputDto>();
            }
        }

        public GetCurrentBlobNamesFromDatabaseOutputDto GetCurrentBlobNamesFromDatabase(long id)
        {
            try
            {
                var output = this.Output(ResultCd.SUCCESS).Create<GetCurrentBlobNamesFromDatabaseOutputDto>();
                var names = _context.AuctionImages.Where(x => x.AuctionId == id).Select(x => x.ImageUrl.Substring(x.ImageUrl.LastIndexOf('/') + 1)).ToList();
                output.Names = names;
                return output;
            }
            catch (Exception ex)
            {
                return this.Output(ResultCd.FAILURE).WithException(ex).Create<GetCurrentBlobNamesFromDatabaseOutputDto>();
            }
        }

        public AuctionDetailsOutputDto GetAuctionDetails(long id)
        {
            try
            {
                var output = this.Output(ResultCd.SUCCESS).Create<AuctionDetailsOutputDto>();

                var auction = (from auc in _context.Auctions
                               join u in _context.Users on auc.UserId equals u.UserId
                               join up in _context.UserProfiles on auc.UserId equals up.UserId
                               where auc.AuctionId == id
                               select new AuctionDetailsOutputDto
                               {
                                   Auction = new Auction
                                   {
                                       UserID = auc.UserId,
                                       ProductName = auc.ProductName,
                                       CategoryId = auc.CategoryId,
                                       StartingPrice = auc.StartingPrice,
                                       Currency = auc.Currency,
                                       StepPrice = auc.StepPrice,
                                       Mode = auc.Mode,
                                       AucStatus = new Dropdown { Id = auc.Status, Name = _context.Categories.Where(x => x.CategoryId == auc.Status).Select(x => x.CategoryName).First() },
                                       AucStatusPayment = new Dropdown { Id = auc.PaymentStatus, Name = _context.Categories.Where(x => x.CategoryId == auc.PaymentStatus).Select(x => x.CategoryName).First() },
                                       Description = auc.Description,
                                       IsPrivate = auc.IsPrivate,
                                       InvitedIds = _context.AuctionInvitations
                                           .Where(x => x.AuctionId == id)
                                           .Select(x => x.InvitedUserId).ToList(),
                                       DepositAmount = auc.DepositAmount,
                                       DepositDeadline = auc.DepositDeadline,
                                       StartTime = auc.StartTime,
                                       EndTime = auc.EndTime,
                                       Images = _context.AuctionImages
                                           .Where(x => x.AuctionId == id)
                                           .Select(x => x.ImageUrl).ToList(),
                                   },
                                   createrProfile = new CreaterProfile
                                   {
                                       Avata = up.Avatar,
                                       Name = u.Username,
                                       Email = up.Email,
                                       Phone = up.PhoneNumber,
                                       Location = up.Address,
                                   }
                               }).FirstOrDefault();

                output.Auction = auction.Auction;
                output.createrProfile = auction.createrProfile;

                return output;
            }
            catch (Exception ex)
            {
                return this.Output(ResultCd.FAILURE).WithException(ex).Create<AuctionDetailsOutputDto>();
            }
        }
    }
}
