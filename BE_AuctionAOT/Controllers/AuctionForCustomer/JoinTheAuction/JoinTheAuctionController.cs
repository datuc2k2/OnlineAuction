using BE_AuctionAOT.Common.Utility;
using BE_AuctionAOT.Controllers.AuctionManagement.Auction;
using BE_AuctionAOT.DAO.AccountManagement.Account;
using BE_AuctionAOT.DAO.AuctionForCustomer.JoinTheAuction;
using BE_AuctionAOT.DAO.AuctionManagement.Auction;
using BE_AuctionAOT.Models;
using BE_AuctionAOT.RabbitMQ.BidQueue.Publishers;
using BE_AuctionAOT.RabbitMQ.BidQueue.Services;
using BE_AuctionAOT.Realtime;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text.Json;
using System.Threading.Channels;

namespace BE_AuctionAOT.Controllers.AuctionForCustomer.JoinTheAuction
{
    [Route("api/[controller]")]
    [ApiController]
    public class JoinTheAuctionController : ControllerBase
    {
        private readonly IBidPublisher _bidPublisher;
        private readonly AuctionDao _auctionDao;
        private readonly AuthUtility _authUtility;
        private readonly JoinTheAuctionDao _joinTheAuctionDao;
        private readonly HttpClient _httpClient;
        private readonly IHubContext<BidRealtimeHub> _hubContext;
        private readonly AccountDao _accountDao;

        public JoinTheAuctionController(AccountDao accountDao, IHubContext<BidRealtimeHub> hubContext, HttpClient httpClient, IBidPublisher bidPublisher, AuctionDao auctionDao, AuthUtility authUtility, JoinTheAuctionDao joinTheAuctionDao)
        {
            _bidPublisher = bidPublisher;
            _auctionDao = auctionDao;
            _authUtility = authUtility;
            _joinTheAuctionDao = joinTheAuctionDao;
            _httpClient = httpClient;
            _hubContext = hubContext;
            _accountDao = accountDao;
        }

        [HttpPost("bidToAuction")]
        public async Task<IActionResult> BidToAuction(int AuctionId, int BidAmount, int DepositeAmount)
        {
            try
            {
                var token = HttpContext.Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
                var uId = _authUtility.GetIdInHeader(token);

                //Check user first bid | 
                var listAuctionBidByAuctionIdAndUserId = _joinTheAuctionDao.GetAllAuctionBidByAuctionIdAndUserId(AuctionId, Int32.Parse(uId + ""));
                if(listAuctionBidByAuctionIdAndUserId.Count == 0)
                {
                    //Minus deposite amount
                    var userCurrentPoint = _joinTheAuctionDao.GetUserPoint(Int32.Parse(uId + ""));
                    if(userCurrentPoint < DepositeAmount)
                    {
                        return BadRequest("Bạn không đủ tiền cọc");
                    } 
                    else
                    {
                        _joinTheAuctionDao.UpdateUserPoint(Int32.Parse(uId + ""), userCurrentPoint - DepositeAmount);
                    }
                }

                var auctionById = _auctionDao.GetAuctionDetails(AuctionId);
                if(auctionById.Auction == null) {
                    return NotFound("Auction id is not valid");
                }

                if(BidAmount <= 0)
                {
                    return BadRequest("Bid amount is not valid");
                }

                

                //Handle Bid first time
                var listAuctionBidByAuctionId = _joinTheAuctionDao.GetAllAuctionBidByAuctionId(AuctionId);
                if(listAuctionBidByAuctionId == null || listAuctionBidByAuctionId.Count == 0)
                {
                    //create auctionbid and declare a new queue
                    AuctionBid auctionBidCreate = new AuctionBid()
                    {
                        AuctionId = AuctionId,
                        UserId = uId,
                        BidAmount = BidAmount,
                        Currency = "VND",
                        BidTime = DateTime.Now,
                    };
                    auctionBidCreate = _joinTheAuctionDao.CreateNewAuctionBid(auctionBidCreate);
                    if(auctionBidCreate == null || auctionBidCreate.BidId <= 0)
                    {
                        return BadRequest("Can not bid");
                    }
                    string queueName = $"bidsQueue_{auctionBidCreate.AuctionId}";
                    _bidPublisher.CreateANewQueue(queueName);
                    //Notify to group
                    string groupName = $"auction_{AuctionId}";
                    await _hubContext.Clients.Group(groupName).SendAsync("ReceiveNotification", "New bid, let re-fetch api");

                    return Ok("Bid first time successfully");
                }
                else
                {
                    Bid bid = new Bid()
                    {
                        AuctionId = AuctionId,
                        UserId = Int32.Parse(uId + ""),
                        BidAmount = BidAmount,
                        Timestamp = DateTime.Now,
                    };
                    _bidPublisher.PublishBid(bid);
                }
                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("createAutoBid")]
        public async Task<IActionResult> CreateAutoBid(int AuctionId, int UserId, int MaxBidAmount)
        {
            try
            {
                AutoBid autoBid = new AutoBid()
                {
                    AuctionId = AuctionId,
                    UserId = UserId,
                    MaxBidAmount = MaxBidAmount,
                    CurrentBidAmount = 0,
                    Currency = "VND",
                    CreatedAt = DateTime.Now,
                };
                _joinTheAuctionDao.CreateAutoBidBid(autoBid);
                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("createAuctionReview")]
        public async Task<IActionResult> CreateAuctionReview(AuctionReviewInputDto auctionReviewInputDto)
        {
            try
            {
                AuctionReview auctionReview = new AuctionReview()
                {
                    AuctionId = auctionReviewInputDto.AuctionId,
                    UserId = auctionReviewInputDto.UserId,
                    Rating = auctionReviewInputDto.Rating,
                    Comment = auctionReviewInputDto.Comment,
                };

                var result = _joinTheAuctionDao.CreateAuctionReview(auctionReview);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("getAllAuctionReviewByAuctionId/{AuctionId}")]
        public async Task<IActionResult> GetAllAuctionReviewByAuctionId(int AuctionId)
        {
            try
            {
                var auctionReviewByAuctionId = _joinTheAuctionDao.GetAllAuctionReviewByAuctionId(AuctionId);
                return Ok(auctionReviewByAuctionId);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("getAllAuctionInvitationByAuctionId/{AuctionId}")]
        public async Task<IActionResult> GetAllAuctionInvitationByAuctionId(int AuctionId)
        {
            try
            {
                var auctionInvitationByAuctionId = _joinTheAuctionDao.GetAllAuctionInvitationByAuctionId(AuctionId);
                return Ok(auctionInvitationByAuctionId);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("getAllAuctionInvitationByUserId/{UserId}")]
        public async Task<IActionResult> GetAllAuctionInvitationByUserId(int UserId)
        {
            try
            {
                var auctionInvitationByUserId = _joinTheAuctionDao.GetAllAuctionInvitationByUserId(UserId);
                return Ok(auctionInvitationByUserId);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("acceptOrRejectAuctionInvitation")]
        public async Task<IActionResult> AcceptOrRejectAuctionInvitation(int InvitationId, bool AcceptOrReject)
        {
            try
            {
                var result = _joinTheAuctionDao.AcceptOrRejectAuctionInvitation(InvitationId, AcceptOrReject);
                if (result == null)
                {
                    if(AcceptOrReject)
                    {
                        return BadRequest("Không thể đồng ý lời mời");
                    } 
                    else
                    {
                        return BadRequest("Không thể từ chối lời mời");
                    }
                }
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }


        [HttpPost("createChatMessage")]
        public async Task<IActionResult> CreateChatMessage(int AuctionId, string Message, int SenderId)
        {
            try
            {
                int chatId = _joinTheAuctionDao.CreateChat(AuctionId);
                if (chatId == 0) return BadRequest("Tạo tin nhắn lỗi");
                ChatMessage chatMessage = new ChatMessage()
                {
                    ChatId = chatId,
                    SenderId = SenderId,
                    Content = Message,
                    SentAt = DateTime.Now,
                    IsDeleted = false,
                };
                ChatMessage chatmess = _joinTheAuctionDao.CreateChatMessage(chatMessage);
                if(chatmess != null)
                {
                    AccountGetForAuction accountByUserId = _accountDao.GetAccountByUserId((int)chatMessage.SenderId);
                    //Notify new message to room => AuctionId
                    string groupName = $"auction_{AuctionId}";
                    await _hubContext.Clients.Group(groupName).SendAsync("SendMessage", Message, SenderId, accountByUserId?.Username, accountByUserId?.Avatar);
                }
                return Ok(chatmess);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("getAllChatMessageByAuctionId/{AuctionId}")]
        public async Task<IActionResult> GetAllChatMessageByAuctionId(int AuctionId)
        {
            try
            {
                var chatMessageByAuctionId = _joinTheAuctionDao.GetAllChatMessageByAuctionId(AuctionId);
                List<ChatMessageDto> listChatMessageDto = new List<ChatMessageDto>();
                foreach(var chatMessage in  chatMessageByAuctionId)
                {
                    AccountGetForAuction accountByUserId = _accountDao.GetAccountByUserId((int) chatMessage.SenderId);
                    ChatMessageDto chatMessageDto = new ChatMessageDto()
                    {
                        MessageId = chatMessage.MessageId,
                        ChatId = chatMessage.ChatId,
                        SenderId = chatMessage.SenderId,
                        Content = chatMessage.Content,
                        SentAt = chatMessage.SentAt,
                        IsDeleted = chatMessage.IsDeleted,
                        AuctionId = AuctionId,
                        SenderName = accountByUserId.Username,
                        Avatar = chatMessage.Sender.UserProfile.Avatar,
                    };
                    listChatMessageDto.Add(chatMessageDto);
                }
                return Ok(listChatMessageDto);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpDelete("deleteAutoBid/{Id}")]
        public async Task<IActionResult> DeleteAutoBid(int Id)
        {
            try
            {
                _joinTheAuctionDao.DeleteAutoBidBid(Id);
                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("getAutoBid/{AuctionId}/{UserId}")]
        public async Task<IActionResult> GetAutoBid(int AuctionId, int UserId)
        {
            try
            {
                var autoBid = _joinTheAuctionDao.GetAutoBidByAuctionIdAndUserId(AuctionId, UserId);
                return Ok(autoBid);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [NonAction]
        private async Task<List<string>> GetQueueNamesFromRabbitMqAsync()
        {
        string RabbitMqManagementApiUrl = "http://localhost:15672/api/queues";
        var requestMessage = new HttpRequestMessage(HttpMethod.Get, RabbitMqManagementApiUrl);

            // Add the RabbitMQ management API credentials (guest:guest)
            var byteArray = new System.Text.ASCIIEncoding().GetBytes("guest:guest");
            requestMessage.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", Convert.ToBase64String(byteArray));

            var response = await _httpClient.SendAsync(requestMessage);

            if (response.IsSuccessStatusCode)
            {
                var responseData = await response.Content.ReadAsStringAsync();
                var queues = JsonSerializer.Deserialize<List<RabbitMqQueue>>(responseData);

                // Extract queue names
                var queueNames = new List<string>();
                foreach (var queue in queues)
                {
                    queueNames.Add(queue.name);
                }
                return queueNames;
            }
            else
            {
                // Handle error
                return new List<string>();
            }
        }

        [HttpGet("getAllAuctionBidByAuctionId/{AuctionId}")]
        public async Task<IActionResult> GetAllAuctionBidByAuctionId(int AuctionId)
        {
            try
            {
                var listAuctionBidByAuctionId = _joinTheAuctionDao.GetAllAuctionBidByAuctionId(AuctionId);
                var listAuctionBidOutput = new List<AuctionBidDto>();
                foreach (var auctionBid in listAuctionBidByAuctionId)
                {
                    AuctionBidDto auctionBidDto = new AuctionBidDto()
                    {
                        BidId = auctionBid.BidId,
                        AuctionId = auctionBid.AuctionId,
                        UserId = auctionBid.UserId,
                        BidAmount = auctionBid.BidAmount,
                        Currency = auctionBid.Currency,
                        BidTime = auctionBid.BidTime,
                        UserName = auctionBid?.User?.Username,
                        UserAvatar = auctionBid?.User?.UserProfile?.Avatar,
                    };
                    listAuctionBidOutput.Add(auctionBidDto);    
                }
                return Ok(listAuctionBidOutput);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

    }
}
