using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using BE_AuctionAOT.DAO.AccountManagement.Account;
using BE_AuctionAOT.DAO.Chats;
using BE_AuctionAOT.Models;
using BE_AuctionAOT.Realtime;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using System.ComponentModel.DataAnnotations;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace BE_AuctionAOT.Controllers.Chats
{
    [Route("api/[controller]")]
    [ApiController]
    public class ChatController : ControllerBase
    {
        private readonly ChatDao _chatDao;
        private readonly BlobServiceClient _blobServiceClient;
        private readonly IHubContext<ChatHub> _hubContext;
        private readonly AccountDao _accountDao;

        public ChatController(ChatDao chatDao, BlobServiceClient blobServiceClient, IHubContext<ChatHub> hubContext, AccountDao accountDao)
        {
            _chatDao = chatDao;
            _blobServiceClient = blobServiceClient;
            _hubContext = hubContext;
            _accountDao = accountDao;
        }

        [HttpGet("getAllUserChatWithByUserId/{UserId}")]
        public async Task<IActionResult> GetAllUserChatWithByUserId(int UserId)
        {
            try
            {
                var allUserChatWithByUserId = _chatDao.GetAllUserChatWithByUserId(UserId);
                return Ok(allUserChatWithByUserId);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("setIsReadForUser")]
        public async Task<IActionResult> SetIsReadForUser([FromQuery, Required] int UserId, [FromQuery, Required] int ChatId, [FromQuery, Required] int UserToNotiId)
        {
            try
            {
                _chatDao.SetIsReadForUser(UserId, ChatId);
                //Notify IsRead
                try
                {
                    var userToNotiConnect = ChatHub.userConnections.ContainsKey(UserToNotiId + "")
                                ? ChatHub.userConnections[UserToNotiId + ""]
                                : null;
                    await _hubContext.Clients.Client(userToNotiConnect).SendAsync("IsRead", ChatId);
                }
                catch (Exception ex) { }
                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("getUserExceptMe/{UserId}")]
        public async Task<IActionResult> GetUserExceptMe(int UserId)
        {
            try
            {
                var userExceptMe = _chatDao.GetUserExceptMe(UserId);
                return Ok(userExceptMe);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("getAdminExceptMe/{UserId}")]
        public async Task<IActionResult> GetAdminExceptMe(int UserId)
        {
            try
            {
                var adminExceptMe = _chatDao.GetAdminExceptMe(UserId);
                return Ok(adminExceptMe);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("addChatMessage")]
        public async Task<IActionResult> AddChatMessage([FromForm] AddChatMessageDto addChatMessageDto)
        {
            try
            {
                string containerName = "chatimg";
                BlobContainerClient containerClient = _blobServiceClient.GetBlobContainerClient(containerName);
                string fileUrl = "";

                if (addChatMessageDto.ContentImage != null)
                {
                    var blobName = Guid.NewGuid().ToString() + Path.GetExtension(addChatMessageDto.ContentImage.FileName);

                    BlobClient blobClient = containerClient.GetBlobClient(blobName);

                    var blobHttpHeaders = new BlobHttpHeaders
                    {
                        ContentType = addChatMessageDto.ContentImage.ContentType
                    };

                    using (var stream = addChatMessageDto.ContentImage.OpenReadStream())
                    {
                        await blobClient.UploadAsync(stream, new BlobUploadOptions { HttpHeaders = blobHttpHeaders });
                    }
                    fileUrl = blobClient.Uri.ToString();
                }

                string contentToSave = addChatMessageDto.ContentText;
                if(fileUrl != "")
                {
                    contentToSave += ("|/|" + fileUrl);
                }

                ChatMessage chatMessage = new ChatMessage()
                {
                    ChatId = addChatMessageDto.ChatId,
                    SenderId = addChatMessageDto.SenderId,
                    Content = contentToSave,
                    SentAt = DateTime.Now,
                    IsDeleted = false,
                };
                var chatMessageReturn = _chatDao.AddChatMessage(chatMessage);

                if(chatMessageReturn == null)
                {
                    return BadRequest("Không thể thêm tin nhắn");
                }

                //Get sender
                AccountGetForAuction accountByUserId = _accountDao.GetAccountByUserId((int)chatMessage.SenderId);

                //Notify to receive user
                try
                {
                    var userInChatExceptMe = _chatDao.GetUserInChatExceptMe((int)addChatMessageDto.SenderId, (int)addChatMessageDto.ChatId);
                    if (userInChatExceptMe != null)
                    {
                        var connectionIdReceiver = ChatHub.userConnections.ContainsKey(userInChatExceptMe.UserId + "")
                                    ? ChatHub.userConnections[userInChatExceptMe.UserId + ""]
                                    : null;
                        await _hubContext.Clients.Client(connectionIdReceiver).SendAsync("ReceiveNotifyNewMessage", addChatMessageDto.ChatId, accountByUserId.Username, accountByUserId.Avatar, contentToSave, addChatMessageDto.SenderId, false); //IsSender = false
                    }
                }
                catch (Exception ex){}


                //Notify to sender user
                try
                {
                    var connectionIdSender = ChatHub.userConnections.ContainsKey(addChatMessageDto.SenderId + "")
                                ? ChatHub.userConnections[addChatMessageDto.SenderId + ""]
                                : null;
                    await _hubContext.Clients.Client(connectionIdSender).SendAsync("ReceiveNotifyNewMessage", addChatMessageDto.ChatId, accountByUserId.Username, accountByUserId.Avatar, contentToSave, addChatMessageDto.SenderId, true); //IsSender = true
                }
                catch (Exception ex) { }
                
                return Ok(chatMessageReturn);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("addChat")]
        public async Task<IActionResult> AddChat([FromQuery, Required] long CreatorId, [FromQuery, Required] long ReceiverId)
        {
            try
            {
                var chatExist = _chatDao.CheckChatExist((int) CreatorId, (int) ReceiverId);
                
                if(chatExist)
                {
                    return Ok(chatExist);
                }

                Chat chatCreate = new Chat()
                {
                    CreatedBy = CreatorId,
                };

                var chat = _chatDao.AddChat(chatCreate);
                if(chat == null)
                {
                    return BadRequest("Không thể tạo chat");
                }
                ChatParticipant chatParticipantCreator = new ChatParticipant()
                {
                    ChatId = chat.ChatId,
                    UserId = CreatorId,
                };
                _chatDao.AddChatParticipant(chatParticipantCreator);

                ChatParticipant chatParticipantReceiver = new ChatParticipant()
                {
                    ChatId = chat.ChatId,
                    UserId = ReceiverId,
                };
                _chatDao.AddChatParticipant(chatParticipantReceiver);
                return Ok(chat);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

    }
}
