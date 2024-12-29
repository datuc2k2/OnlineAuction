using AutoMapper;
using BE_AuctionAOT.Common.Base.Entity;
using BE_AuctionAOT.Common.Constants;
using BE_AuctionAOT.Controllers.Common.Auth;
using BE_AuctionAOT.Models;
using Microsoft.EntityFrameworkCore;

namespace BE_AuctionAOT.DAO.Common.User
{
    public class UserDao
    {
        private readonly DB_AuctionAOTContext _context;
        private readonly IMapper _mapper;
        public UserDao(DB_AuctionAOTContext context, IMapper mapper)
        {
            _mapper = mapper;
            _context = context;
        }

        public async Task<UserDaoOutputDto?> GetAccountLogin(LoginControllerInputDto inputDto)
        {
            try
            {
                var output = this.Output(ResultCd.SUCCESS).Create<UserDaoOutputDto>();
                var user = await (from ur in _context.UserRoles
                                  join u in _context.Users on ur.UserId equals u.UserId
                                  join r in _context.Roles on ur.RoleId equals r.RoleId
                                  join up in _context.UserProfiles on ur.UserId equals up.UserId
                                  where (up.Email == inputDto.Email || u.Username == inputDto.Email)
                                  && u.IsActive == Status.ACTIVE && u.EmailVerified == true
                                  select new UserRole
                                  {
                                      User = u,
                                      Role = r,
                                  }).FirstOrDefaultAsync();

                if (user != null)
                {
                    if (BCrypt.Net.BCrypt.Verify(inputDto.Password, user.User.Password))
                    {
                        output.user = user;
                    }
                }
                return output;
            }
            catch (Exception ex)
            {
                return this.Output(ResultCd.FAILURE).WithException(ex).Create<UserDaoOutputDto>();
            }
        }

        public async Task<BaseOutputDto> updatelastLogin(long? userId)
        {
            try
            {
                var output = this.Output(ResultCd.SUCCESS).Create<BaseOutputDto>();
                var user = await _context.Users.FirstOrDefaultAsync(x => x.UserId == userId);
                if (user == null)
                {
                    output.ResultCd = ResultCd.FAILURE;
                    return output;
                }

                user.LastLogin = DateTime.Now;
                _context.Update(user);
                await _context.SaveChangesAsync();

                return output;

            }
            catch (Exception ex)
            {
                return this.Output(ResultCd.FAILURE).WithException(ex).Create<BaseOutputDto>();
            }
        }

        public async Task<CheckAccountAlredyExistsOutputDto> CheckAccountAlredyExists(RegisterControllerInputDto inputDto)
        {
            try
            {
                var output = this.Output(ResultCd.SUCCESS).Create<CheckAccountAlredyExistsOutputDto>();
                var user = await (from u in _context.Users
                                  join up in _context.UserProfiles on u.UserId equals up.UserId
                                  where u.Username == inputDto.UserName || up.Email == inputDto.Email
                                  select u).FirstOrDefaultAsync();

                if (user != null)
                {
                    output.isExist = true;
                    return output;
                }
                output.isExist = false;
                return output;
            }
            catch (Exception ex)
            {
                return this.Output(ResultCd.FAILURE).WithException(ex).Create<CheckAccountAlredyExistsOutputDto>();
            }
        }

        public async Task<BaseOutputDto> RegisterAccount(RegisterControllerInputDto inputDto, string? verifyCode, string? isVerify)
        {
            try
            {
                var output = this.Output(ResultCd.SUCCESS).Create<BaseOutputDto>();

                var minutes = await _context.SystemConfigurations.Where(x => x.KeyId == "0001").Select(x => x.Value).FirstOrDefaultAsync();

                var User = new Models.User
                {
                    Username = inputDto.UserName,
                    Password = inputDto.Password,
                    VerifyToken = verifyCode,
                    VerifyTokenExpiresAt = DateTime.Now.AddMinutes(minutes != null ? int.Parse(minutes) : 3)
                };

                if (isVerify == "0")
                {
                    User.EmailVerified = true;
                    User.VerifyTokenExpiresAt = null;
                }

                await _context.Users.AddAsync(User);
                await _context.SaveChangesAsync();

                var newUserId = await GetLastUserId();

                if (newUserId == -1)
                {
                    output.ResultCd = ResultCd.FAILURE;
                    return output;
                }

                var userProfile = new Models.UserProfile
                {
                    UserId = newUserId,
                    Email = inputDto.Email,
                };

                await _context.UserProfiles.AddAsync(userProfile);

                var userRole = new Models.UserRole
                {
                    UserId = newUserId,
                    RoleId = 2,
                };
                await _context.UserRoles.AddAsync(userRole);
                await _context.SaveChangesAsync();

                return output;
            }
            catch (Exception ex)
            {
                return this.Output(ResultCd.FAILURE).WithException(ex).Create<BaseOutputDto>();
            }
        }

        public async Task<long> GetLastUserId()
        {
            try
            {
                long? id = await _context.Users.OrderByDescending(x => x.UserId).Select(x => x.UserId).FirstOrDefaultAsync();
                return id ?? 1;
            }
            catch (Exception ex)
            {
                return -1;
            }
        }

        public async Task<ConfirmOutputDto> CheckConfirmCode(ConfirmInputDto inputDto)
        {
            try
            {
                var output = this.Output(ResultCd.SUCCESS).Create<ConfirmOutputDto>();
                output.isSuccess = false;
                var acc = await (from up in _context.UserProfiles
                                 join u in _context.Users on up.UserId equals u.UserId
                                 where up.Email == inputDto.Email
                                 select u).FirstOrDefaultAsync();

                if (acc != null)
                {
                    if (acc.VerifyTokenExpiresAt >= DateTime.Now)
                    {
                        if (BCrypt.Net.BCrypt.Verify(inputDto.Code, acc.VerifyToken))
                        {
                            acc.EmailVerified = true;
                            acc.VerifyToken = null;
                            acc.VerifyTokenExpiresAt = null;
                            await _context.SaveChangesAsync();
                            output.isSuccess = true;
                            return output;
                        }
                        output = this.Output(ResultCd.SUCCESS).CommonMessageWithInfo("wrong code", "0004").Create<ConfirmOutputDto>();
                        output.isSuccess = false;
                        return output;
                    }
                    return this.Output(ResultCd.SUCCESS).CommonMessageWithInfo("Out Date", "0003").Create<ConfirmOutputDto>();
                    
                }
                output = this.Output(ResultCd.SUCCESS).CommonMessageWithInfo("wrong email", "0005").Create<ConfirmOutputDto>();
                output.isSuccess = false;
                return output;
            }
            catch (Exception ex)
            {
                return this.Output(ResultCd.FAILURE).WithException(ex).Create<ConfirmOutputDto>();
            }
        }

        public async Task<ConfirmOutputDto> CheckConfirmCodeResetPassword(ConfirmInputDto inputDto)
        {
            try
            {
                var output = this.Output(ResultCd.SUCCESS).Create<ConfirmOutputDto>();
                output.isSuccess = false;
                var acc = await (from p in _context.PasswordResetRequests
                                 where p.Email == inputDto.Email && p.IsUsed == false
                                 select p).OrderByDescending(x => x.CreatedAt).FirstOrDefaultAsync();

                if (acc != null)
                {
                    if (acc.ExpiresAt >= DateTime.Now)
                    {
                        if (BCrypt.Net.BCrypt.Verify(inputDto.Code, acc.ResetToken))
                        {
                            acc.IsUsed = true;
                            await _context.SaveChangesAsync();
                            output.isSuccess = true;
                            return output;
                        }
                        output = this.Output(ResultCd.FAILURE).CommonMessageWithInfo("wrong code", "0004").Create<ConfirmOutputDto>();
                        output.isSuccess = false;
                        return output;
                    }
                    return this.Output(ResultCd.FAILURE).CommonMessageWithInfo("Out Date", "0003").Create<ConfirmOutputDto>();

                }
                output = this.Output(ResultCd.FAILURE).CommonMessageWithInfo("wrong email", "0005").Create<ConfirmOutputDto>();
                output.isSuccess = false;
                return output;
            }
            catch (Exception ex)
            {
                return this.Output(ResultCd.FAILURE).WithException(ex).Create<ConfirmOutputDto>();
            }
        }

        public async Task<GetUserOutputDto> GetAccountByEmial(ResetPasswordInputDto inputDto)
        {
            try
            {
                var output = this.Output(ResultCd.SUCCESS).Create<GetUserOutputDto>();
                var user = await (from up in _context.UserProfiles
                                  where up.Email == inputDto.Email
                                  select up).FirstOrDefaultAsync();

                output.userProfile = user;
                return output;
            }
            catch (Exception ex)
            {
                return this.Output(ResultCd.FAILURE).WithException(ex).Create<GetUserOutputDto>();
            }
        }

        public async Task<BaseOutputDto> RequestResetPassword(GetUserOutputDto user, string verifyCode)
        {
            try
            {
                var output = this.Output(ResultCd.SUCCESS).Create<GetUserOutputDto>();
                var minutes = await _context.SystemConfigurations.Where(x => x.KeyId == "0001").Select(x => x.Value).FirstOrDefaultAsync();
                var p = new PasswordResetRequest
                {
                    UserId = user.userProfile.UserId,
                    Email = user.userProfile.Email,
                    ExpiresAt = DateTime.Now.AddMinutes(minutes != null ? int.Parse(minutes) : 3),
                    ResetToken = verifyCode,
                };

                await _context.AddAsync(p);
                await _context.SaveChangesAsync();
                return output;

            }
            catch (Exception ex)
            {
                return this.Output(ResultCd.FAILURE).WithException(ex).Create<GetUserOutputDto>();
            }
        }

        public async Task<BaseOutputDto> CheckResetPassword(CreateNewPasswordInputDto inputDto)
        {
            try
            {
                var output = this.Output(ResultCd.SUCCESS).Create<BaseOutputDto>();

                var p = await _context.PasswordResetRequests.Where(x => x.Email == inputDto.Email && x.IsUsed == true && x.ExpiresAt >= DateTime.Now).FirstOrDefaultAsync();

                if(p == null)
                {
                    return this.Output(ResultCd.FAILURE).Create<BaseOutputDto>();
                }

                return output;

            }
            catch (Exception ex)
            {
                return this.Output(ResultCd.FAILURE).WithException(ex).Create<BaseOutputDto>();
            }
        }

        public async Task<BaseOutputDto> UpdaetPassword(CreateNewPasswordInputDto inputDto)
        {
            try
            {
                var output = this.Output(ResultCd.SUCCESS).Create<BaseOutputDto>();
                var acc =  await(from u in _context.Users
                                 join up in _context.UserProfiles on u.UserId equals up.UserId
                                 where up.Email == inputDto.Email
                                 select u).FirstOrDefaultAsync();

                if(acc == null)
                {
                    return this.Output(ResultCd.FAILURE).Create<BaseOutputDto>();
                }

                acc.Password = inputDto.Password;
                _context.Update(acc);
                _context.SaveChanges();

                return output;

            }
            catch (Exception ex)
            {
                return this.Output(ResultCd.FAILURE).WithException(ex).Create<BaseOutputDto>();
            }
        }

        public async Task<BaseOutputDto> UpdateVerifyCode(ReSendVerifyCodeInputDto inputDto, string? code)
        {
            try
            {
                var output = this.Output(ResultCd.SUCCESS).Create<BaseOutputDto>();
                var acc = await (from u in _context.Users
                                 join up in _context.UserProfiles on u.UserId equals up.UserId
                                 where up.Email == inputDto.Email
                                 select u).FirstOrDefaultAsync();

                if (acc == null)
                {
                    return this.Output(ResultCd.FAILURE).Create<BaseOutputDto>();
                }
                var minutes = await _context.SystemConfigurations.Where(x => x.KeyId == "0001").Select(x => x.Value).FirstOrDefaultAsync();
                acc.VerifyToken = code;
                acc.VerifyTokenExpiresAt = DateTime.Now.AddMinutes(minutes != null ? int.Parse(minutes) : 3);
                _context.Update(acc);
                _context.SaveChanges();

                return output;

            }
            catch (Exception ex)
            {
                return this.Output(ResultCd.FAILURE).WithException(ex).Create<BaseOutputDto>();
            }
        }

        public async Task<BaseOutputDto> CheckPassword(long id, ChangePasswordInputDto inputDto)
        {
            try
            {
                var acc = await (from u in _context.Users
                                 where u.UserId == id
                                 select u).FirstOrDefaultAsync();
                if (acc != null)
                {
                    if (!BCrypt.Net.BCrypt.Verify(inputDto.OldPassword, acc.Password))
                    {
                        return this.Output(ResultCd.FAILURE).CommonMessageWithInfo("wrong password", "0007").Create<BaseOutputDto>();
                    }
                    return this.Output(ResultCd.SUCCESS).CommonMessageWithInfo("Success", "0008").Create<BaseOutputDto>();
                }
                return this.Output(ResultCd.FAILURE).Create<BaseOutputDto>();
            }
            catch (Exception ex)
            {
                return this.Output(ResultCd.FAILURE).WithException(ex).Create<BaseOutputDto>();
            }
        }

        public async Task<BaseOutputDto> UpdaetPasswordById(long id, ChangePasswordInputDto inputDto)
        {
            try
            {
                var output = this.Output(ResultCd.SUCCESS).Create<BaseOutputDto>();
                var acc = await (from u in _context.Users
                                 where u.UserId == id
                                 select u).FirstOrDefaultAsync();

                if (acc == null)
                {
                    return this.Output(ResultCd.FAILURE).Create<BaseOutputDto>();
                }

                acc.Password = inputDto.NewPassword;
                _context.Update(acc);
                _context.SaveChanges();

                return this.Output(ResultCd.SUCCESS).CommonMessageWithInfo("Success", "0008").Create<BaseOutputDto>();

            }
            catch (Exception ex)
            {
                return this.Output(ResultCd.FAILURE).WithException(ex).Create<BaseOutputDto>();
            }
        }
    }
}
