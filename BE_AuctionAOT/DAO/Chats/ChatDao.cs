using BE_AuctionAOT.Controllers.Chats;
using BE_AuctionAOT.Models;
using Microsoft.EntityFrameworkCore;

namespace BE_AuctionAOT.DAO.Chats
{
    public class ChatDao
    {
        private readonly DB_AuctionAOTContext _context;
        public ChatDao(DB_AuctionAOTContext context)
        {
            _context = context;
        }

        public List<UserChatWithDto> GetAllUserChatWithByUserId(int UserId)
        {
            try
            {
                var userChatWithList = (from chat in _context.Chats
                                        where chat.ChatParticipants.Any(cp => cp.UserId == UserId)
                                        from participant in chat.ChatParticipants
                                        where participant.UserId != UserId
                                        select new UserChatWithDto
                                        {
                                            UserIdWith = (int)participant.UserId,
                                            UserNameWith = participant.User.Username,
                                            UserAvatarWidth = participant.User.UserProfile.Avatar,
                                            ChatId = (int)chat.ChatId,
                                            IsAdmin = !participant.User.UserRoles.Any(r => r.RoleId == 2),
                                            ChatMessages = (from message in _context.ChatMessages
                                                            where message.ChatId == chat.ChatId
                                                            select new ChatMessageDto
                                                            {
                                                                MessageID = (int)message.MessageId,
                                                                ChatID = (int)message.ChatId,
                                                                SenderID = (int)message.Sender.UserId,
                                                                SenderName = message.Sender.Username,
                                                                SenderAvatar = message.Sender.UserProfile.Avatar,
                                                                Content = message.Content,
                                                                SendAt = message.SentAt,
                                                                IsDeleted = message.IsDeleted,
                                                                IsRead = message.IsRead,
                                                            })
                                                            .OrderBy(m => m.SendAt)
                                                            .ToList()
                                        })
                        .OrderByDescending(c => c.ChatMessages.FirstOrDefault().SendAt)
                        .ToList();


                return userChatWithList;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public void SetIsReadForUser(int SenderId, int ChatId)
        {
            try
            {
                var messageBySenderId = _context.ChatMessages.Where(cm => cm.SenderId == SenderId && cm.ChatId == ChatId).ToList();
                foreach(var message in messageBySenderId)
                {
                    message.IsRead = true;
                }
                _context.UpdateRange(messageBySenderId);
                _context.SaveChanges();
            }
            catch (Exception ex)
            {
                return;
            }
        }

        public bool CheckChatExist(int SenderId, int ReceiverId)
        {
            try
            {
                var chatExists = _context.ChatParticipants
                .Where(cp => cp.UserId == SenderId || cp.UserId == ReceiverId)
                .GroupBy(cp => cp.ChatId)
                .Any(g => g.Count() == 2);
                return chatExists;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public UserOutputDto GetUserInChatExceptMe(int MeId, int ChatId)
        {
            try
            {
                var user = _context.ChatParticipants
                                .Include(cp => cp.User)
                                .Where(cp => cp.UserId != MeId && cp.ChatId == ChatId)
                                .Select(cp => new UserOutputDto()
                                {
                                    UserId = (int) cp.UserId,
                                    UserName = cp.User.Username,
                                    UserEmail = cp.User.Username,
                                })
                                .FirstOrDefault();

                return user;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public ChatMessage AddChatMessage(ChatMessage chatMessage)
        {
            try
            {
                _context.ChatMessages.Add(chatMessage);
                _context.SaveChanges();
                return chatMessage;
            }
            catch(Exception ex)
            {
                return null;
            }
        }

        public Chat AddChat(Chat chat)
        {
            try
            {
                _context.Chats.Add(chat);
                _context.SaveChanges();
                return chat;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public ChatParticipant AddChatParticipant(ChatParticipant chatParticipant)
        {
            try
            {
                _context.ChatParticipants.Add(chatParticipant);
                _context.SaveChanges();
                return chatParticipant;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public List<UserOutputDto> GetUserExceptMe(int UserId)
        {
            try
            {
                var users = (from u in _context.Users.Include(u => u.UserProfile).Include(u => u.UserRoles)
                             where u.UserId != UserId && u.UserRoles.Any(role => role.RoleId == 2)
                             select new UserOutputDto
                             {
                                 UserId = (int) u.UserId,
                                 UserName = u.Username,
                                 UserEmail = u.UserProfile.Email,
                                 UserAvatar = u.UserProfile.Avatar,
                             }).ToList();

                return users;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public List<UserOutputDto> GetAdminExceptMe(int UserId)
        {
            try
            {
                var admins = (from u in _context.Users.Include(u => u.UserProfile).Include(u => u.UserRoles)
                             where u.UserId != UserId && u.UserRoles.Any(role => role.RoleId != 2)
                             select new UserOutputDto
                             {
                                 UserId = (int)u.UserId,
                                 UserName = u.Username,
                                 UserEmail = u.UserProfile.Email,
                                 UserAvatar = u.UserProfile.Avatar,
                             }).ToList();

                return admins;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

    }
}
