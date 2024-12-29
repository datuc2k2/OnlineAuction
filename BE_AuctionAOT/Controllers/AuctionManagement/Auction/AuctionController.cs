using AutoMapper;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using BE_AuctionAOT.Common.Base.Entity;
using BE_AuctionAOT.Common.Constants;
using BE_AuctionAOT.Common.Utility;
using BE_AuctionAOT.DAO.AuctionManagement.Auction;
using BE_AuctionAOT.DAO.AuctionManagement.AuctionImage;
using BE_AuctionAOT.DAO.AuctionManagement.AuctionInvitation;
using BE_AuctionAOT.DAO.AuctionManagement.AuctionRequest;
using BE_AuctionAOT.DAO.AuctionManagement.Payment;
using BE_AuctionAOT.DAO.Common.SystemConfiguration;
using BE_AuctionAOT.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Mvc;

namespace BE_AuctionAOT.Controllers.AuctionManagement.Auction
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuctionController : ControllerBase
    {
        private readonly AuctionDao _auctionDao;
        private readonly AuctionRequestDao _auctionRequestDao;
        private readonly PaymentDao _paymentDao;
        private readonly AuctionImageDao _auctionImageDao;
        private readonly AuctionInvitationDao _auctionInvitationDao;
        private readonly SystemConfigurationDao _systemConfigurationDao;
        private readonly BlobServiceClient _blobServiceClient;
        private readonly IMapper _mapper;
        //private readonly Number _numberUtility;
        //private readonly Mail _mailUtility;
        private readonly AuthUtility _authUtility;
        private readonly IConfiguration _configuration;
        public AuctionController(PaymentDao paymentDao, AuctionDao auctionDao, AuctionRequestDao auctionRequestDao, AuctionImageDao auctionImageDao, AuctionInvitationDao auctionInvitationDao, SystemConfigurationDao systemConfigurationDao, BlobServiceClient blobServiceClient, IMapper mapper, IConfiguration configuration, Number numberUtility, Mail mailUtility, AuthUtility authUtility)
        {
            _systemConfigurationDao = systemConfigurationDao;
            _auctionDao = auctionDao;
            _auctionInvitationDao = auctionInvitationDao;
            _mapper = mapper;
            _authUtility = authUtility;
            _configuration = configuration;
            _auctionImageDao = auctionImageDao;
            _blobServiceClient = blobServiceClient;
            _auctionRequestDao = auctionRequestDao;
            //_numberUtility = numberUtility;
            //_mailUtility = mailUtility;
            _paymentDao = paymentDao;
        }

        [Authorize]
        [HttpGet("getSysConfig")]
        public IActionResult GetSysConfig()
        {
            try
            {
                var output = this.Output(ResultCd.SUCCESS).Create<ListSysConfigOutputDto>();

                var listCode = new List<string>()
                {
                    "0007"
                };

                var getValuesOutput = _systemConfigurationDao.GetValues(listCode);

                if (getValuesOutput.ResultCd != ResultCd.SUCCESS)
                {
                    return BadRequest(getValuesOutput);
                }
                output.sysConfig = getValuesOutput.sysConfig;
                return Ok(output);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        //[Authorize]
        [HttpGet("getAuctionCategory")]
        public async Task<IActionResult> GetAuctionCategory()
        {
            try
            {
                var output = this.Output(ResultCd.SUCCESS).Create<GetAuctionCategoryOutputDto>();

                var type = await _systemConfigurationDao.GetValue("0005");
                var cate = _auctionDao.GetCategory(int.Parse(type));
                if (cate.ResultCd != ResultCd.SUCCESS)
                {
                    return BadRequest(cate);
                }
                output.Categories = cate.Categories;

                type = await _systemConfigurationDao.GetValue("0006");
                var mode = _auctionDao.GetCategory(int.Parse(type));
                if (mode.ResultCd != ResultCd.SUCCESS)
                {
                    return BadRequest(mode);
                }
                output.Modes = mode.Categories;

                return Ok(output);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        //[Authorize]
        [HttpGet("getAuctionDetails")]
        public async Task<IActionResult> GetAuctionDetails(long aucId)
        {
            try
            {
                var output = this.Output(ResultCd.SUCCESS).Create<AuctionDetailsOutputDto>();
                FeeCaculationInputDto inputDto = new FeeCaculationInputDto()
                {
                    auctionId = aucId
                };
                var returnfee = await _paymentDao.FeeCaculation(inputDto);

                output = _auctionDao.GetAuctionDetails(aucId);
                output.Fee = returnfee.Fee;

                if (output.ResultCd != ResultCd.SUCCESS)
                {
                    return BadRequest(output);
                }
                return Ok(output);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [Authorize(Roles = "1")]
        [HttpPost("createAuction")]
        public async Task<IActionResult> CreateAuction([FromForm] CreateAuctionInputDto inputDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }
                var token = HttpContext.Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
                var uId = _authUtility.GetIdInHeader(token);

                var InputCreateAuc = _mapper.Map<Models.Auction>(inputDto.Auction);
                InputCreateAuc.IsActive = true;
                InputCreateAuc.UserId = inputDto.Auction.UserID;
                InputCreateAuc.CreatedBy = uId;
                InputCreateAuc.Status = AuctionConst.Category.COMINGSOON;
                InputCreateAuc.PaymentStatus = AuctionConst.Category.UNPAID;

                var createAucion = await _auctionDao.CreateAution(InputCreateAuc);

                if (createAucion.ResultCd != ResultCd.SUCCESS)
                {
                    return BadRequest(createAucion);
                }

                string containerName = "productimg";
                BlobContainerClient containerClient = _blobServiceClient.GetBlobContainerClient(containerName);
                var addedUrls = new List<AuctionImage>();
                foreach (var file in inputDto.Images)
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
                        addedUrls.Add(new AuctionImage()
                        {
                            AuctionId = createAucion.NewAucId,
                            ImageUrl = fileUrl,
                        });
                    }
                }
                if (addedUrls.Count > 0)
                {
                    _auctionImageDao.SaveImgProduct(addedUrls);
                }
                if (inputDto.Auction.IsPrivate == true)
                {
                    var inputInv = new CreateInvitationInputDto
                    {
                        AuctionId = createAucion.NewAucId,
                        InvitedIds = inputDto.Auction.InvitedIds,
                    };

                    var InputInv = _auctionInvitationDao.CreateInvitation(inputInv);

                    if (InputInv.ResultCd != ResultCd.SUCCESS)
                    {
                        return BadRequest(InputInv);
                    }

                }


                var output = this.Output(ResultCd.SUCCESS).Create<CreateAuctionOutputDto>();

                return Ok(output);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [Authorize]
        [HttpPost("requestCreateAuction")]
        public async Task<IActionResult> RequestCreateAuction([FromForm] CreateAuctionInputDto inputDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }
                var token = HttpContext.Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
                var uId = _authUtility.GetIdInHeader(token);

                var InputCreateAuc = _mapper.Map<Models.Auction>(inputDto.Auction);
                InputCreateAuc.IsActive = true;
                InputCreateAuc.UserId = uId;
                InputCreateAuc.CreatedBy = uId;
                InputCreateAuc.Status = AuctionConst.Category.INPROCESS;
                InputCreateAuc.PaymentStatus = AuctionConst.Category.UNPAID;

                var createAucion = await _auctionDao.CreateAution(InputCreateAuc);

                if (createAucion.ResultCd != ResultCd.SUCCESS)
                {
                    return BadRequest(createAucion);
                }

                string containerName = "productimg";
                BlobContainerClient containerClient = _blobServiceClient.GetBlobContainerClient(containerName);

                var addedUrls = new List<AuctionImage>();
                foreach (var file in inputDto.Images)
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

                        addedUrls.Add(new AuctionImage()
                        {
                            AuctionId = createAucion.NewAucId,
                            ImageUrl = fileUrl,
                        });
                    }
                }
                if (addedUrls.Count > 0)
                {
                    _auctionImageDao.SaveImgProduct(addedUrls);
                }
                if (inputDto.Auction.IsPrivate == true)
                {
                    var inputInv = new CreateInvitationInputDto
                    {
                        AuctionId = createAucion.NewAucId,
                        InvitedIds = inputDto.Auction.InvitedIds,
                    };

                    var InputInv = _auctionInvitationDao.CreateInvitation(inputInv);

                    if (InputInv.ResultCd != ResultCd.SUCCESS)
                    {
                        return BadRequest(InputInv);
                    }

                }

                var isCraete = await _systemConfigurationDao.GetValue("0003");

                var inputCreateRequest = new CreateAuctionRequestInputDto()
                {
                    AuctionId = createAucion.NewAucId,
                    UserId = uId,
                    Type = isCraete == "0" ? false : true,
                };

                var createReq = _auctionRequestDao.CreateAuctionRequest(inputCreateRequest);

                if (createReq.ResultCd != ResultCd.SUCCESS)
                {
                    return BadRequest(createReq);
                }

                var output = this.Output(ResultCd.SUCCESS).Create<CreateAuctionOutputDto>();

                return Ok(output);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [Authorize(Roles = "1")]
        [HttpPost("updateAuction")]
        public async Task<IActionResult> UpdateAuction([FromForm] UpdateAuctionInputDto inputDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }
                var token = HttpContext.Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
                var uId = _authUtility.GetIdInHeader(token);

                var InputCreateAuc = _mapper.Map<Models.Auction>(inputDto.UpdateAuction);


                var updateAucion = _auctionDao.UpdateAution(InputCreateAuc, uId);

                if (updateAucion.ResultCd != ResultCd.SUCCESS)
                {
                    return BadRequest(updateAucion);
                }


                string containerName = "productimg";
                BlobContainerClient containerClient = _blobServiceClient.GetBlobContainerClient(containerName);
                var addedUrls = new List<AuctionImage>();
                var deletedUrls = new List<AuctionImage>();

                // Lấy danh sách blob name hiện có trong hệ thống từ cơ sở dữ liệu
                var currentBlobNames = _auctionDao.GetCurrentBlobNamesFromDatabase(inputDto.UpdateAuction.AuctionID);

                // **Thêm ảnh mới** nếu ảnh chưa tồn tại
                if (inputDto.FilesToAdd != null)
                {
                    foreach (var file in inputDto.FilesToAdd)
                    {
                        if (file != null && file.Length > 0)
                        {
                            // Tạo tên blob duy nhất (hoặc dùng file.FileName)
                            var blobName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);

                            // Nếu blob chưa tồn tại trong hệ thống, mới tiến hành upload
                            if (!currentBlobNames.Names.Contains(blobName))
                            {
                                // Lấy BlobClient từ container
                                BlobClient blobClient = containerClient.GetBlobClient(blobName);

                                // Đặt Content-Type tương ứng với định dạng ảnh
                                var blobHttpHeaders = new BlobHttpHeaders
                                {
                                    ContentType = file.ContentType
                                };

                                // Upload ảnh lên Blob Storage
                                using (var stream = file.OpenReadStream())
                                {
                                    await blobClient.UploadAsync(stream, new BlobUploadOptions { HttpHeaders = blobHttpHeaders });
                                }

                                // Lấy URL của ảnh sau khi upload
                                string fileUrl = blobClient.Uri.ToString();

                                // Thêm URL vào danh sách đã thêm để lưu vào DB
                                addedUrls.Add(new AuctionImage()
                                {
                                    AuctionId = inputDto.UpdateAuction.AuctionID,
                                    ImageUrl = fileUrl,
                                });
                            }
                        }
                    }
                }

                // **Xóa ảnh cũ**: Xóa những ảnh không còn trong danh sách hiện có từ client
                foreach (var currentBlobName in currentBlobNames.Names)
                {
                    // Nếu ảnh hiện có không nằm trong danh sách hiện tại của client, tiến hành xóa
                    if (!inputDto.ExistingBlobNames.Contains(currentBlobName))
                    {
                        // Lấy BlobClient từ container để xóa ảnh
                        BlobClient blobClient = containerClient.GetBlobClient(currentBlobName);

                        // Xóa blob từ Azure Blob Storage
                        await blobClient.DeleteIfExistsAsync();

                        // Thêm URL đã xóa vào danh sách xóa để cập nhật DB
                        string fileUrl = blobClient.Uri.ToString();
                        deletedUrls.Add(new AuctionImage()
                        {
                            AuctionId = inputDto.UpdateAuction.AuctionID,
                            ImageUrl = fileUrl,
                        });
                    }
                }

                // Cập nhật cơ sở dữ liệu sau khi thêm và xóa
                if (addedUrls.Count > 0)
                {
                    var addImg = _auctionImageDao.SaveImgProduct(addedUrls);
                    if (addImg.ResultCd != ResultCd.SUCCESS)
                    {
                        return BadRequest(addImg);
                    }
                }
                if (deletedUrls.Count > 0)
                {
                    var deleteImg = _auctionImageDao.DeleteImgProduct(deletedUrls);
                    if (deleteImg.ResultCd != ResultCd.SUCCESS)
                    {
                        return BadRequest(deleteImg);
                    }
                }


                if (inputDto.UpdateAuction.IsPrivate == true)
                {
                    var inputInv = new CreateInvitationInputDto
                    {
                        AuctionId = inputDto.UpdateAuction.AuctionID,
                        InvitedIds = inputDto.UpdateAuction.InvitedIds,
                    };

                    var curInv = _auctionInvitationDao.GetCurInvitation(inputDto.UpdateAuction.AuctionID);

                    var addedIds = new List<long>();

                    foreach (var item in inputInv.InvitedIds)
                    {
                        if (!curInv.Invs.Contains(item))
                        {
                            addedIds.Add(item);
                        }
                        else
                        {
                            curInv.Invs.Remove(item);
                        }
                    }

                    inputInv.InvitedIds = addedIds;
                    if (addedIds.Count > 0)
                    {
                        var InputInv = _auctionInvitationDao.CreateInvitation(inputInv);
                        if (InputInv.ResultCd != ResultCd.SUCCESS)
                        {
                            return BadRequest(InputInv);
                        }
                    }

                    if (curInv.Invs.Count > 0)
                    {
                        var deleteInv = new CreateInvitationInputDto
                        {
                            AuctionId = inputDto.UpdateAuction.AuctionID,
                            InvitedIds = curInv.Invs,
                        };
                        var deleteIds = _auctionInvitationDao.DeleteInvitation(deleteInv);
                        if (deleteIds.ResultCd != ResultCd.SUCCESS)
                        {
                            return BadRequest(deleteIds);
                        }
                    }
                }


                var output = this.Output(ResultCd.SUCCESS).Create<CreateAuctionOutputDto>();

                return Ok(output);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [Authorize]
        [HttpPost("requestUpdateAuction")]
        public async Task<IActionResult> RequestUpdateAuction([FromForm] UpdateAuctionInputDto inputDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }
                var token = HttpContext.Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
                var uId = _authUtility.GetIdInHeader(token);

                var InputCreateAuc = _mapper.Map<Models.Auction>(inputDto.UpdateAuction);

                var updateAucion = _auctionDao.UpdateAution(InputCreateAuc, uId);

                if (updateAucion.ResultCd != ResultCd.SUCCESS)
                {
                    return BadRequest(updateAucion);
                }


                string containerName = "productimg";
                BlobContainerClient containerClient = _blobServiceClient.GetBlobContainerClient(containerName);
                var addedUrls = new List<AuctionImage>();
                var deletedUrls = new List<AuctionImage>();

                // Lấy danh sách blob name hiện có trong hệ thống từ cơ sở dữ liệu
                var currentBlobNames = _auctionDao.GetCurrentBlobNamesFromDatabase(inputDto.UpdateAuction.AuctionID);

                // **Thêm ảnh mới** nếu ảnh chưa tồn tại
                if (inputDto.FilesToAdd != null)
                {
                    foreach (var file in inputDto.FilesToAdd)
                    {
                        if (file != null && file.Length > 0)
                        {
                            // Tạo tên blob duy nhất (hoặc dùng file.FileName)
                            var blobName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);

                            // Nếu blob chưa tồn tại trong hệ thống, mới tiến hành upload
                            if (!currentBlobNames.Names.Contains(blobName))
                            {
                                // Lấy BlobClient từ container
                                BlobClient blobClient = containerClient.GetBlobClient(blobName);

                                // Đặt Content-Type tương ứng với định dạng ảnh
                                var blobHttpHeaders = new BlobHttpHeaders
                                {
                                    ContentType = file.ContentType
                                };

                                // Upload ảnh lên Blob Storage
                                using (var stream = file.OpenReadStream())
                                {
                                    await blobClient.UploadAsync(stream, new BlobUploadOptions { HttpHeaders = blobHttpHeaders });
                                }

                                // Lấy URL của ảnh sau khi upload
                                string fileUrl = blobClient.Uri.ToString();

                                // Thêm URL vào danh sách đã thêm để lưu vào DB
                                addedUrls.Add(new AuctionImage()
                                {
                                    AuctionId = inputDto.UpdateAuction.AuctionID,
                                    ImageUrl = fileUrl,
                                });
                            }
                        }
                    }
                }

                // **Xóa ảnh cũ**: Xóa những ảnh không còn trong danh sách hiện có từ client
                foreach (var currentBlobName in currentBlobNames.Names)
                {
                    // Nếu ảnh hiện có không nằm trong danh sách hiện tại của client, tiến hành xóa
                    if (!inputDto.ExistingBlobNames.Contains(currentBlobName))
                    {
                        // Lấy BlobClient từ container để xóa ảnh
                        BlobClient blobClient = containerClient.GetBlobClient(currentBlobName);

                        // Xóa blob từ Azure Blob Storage
                        await blobClient.DeleteIfExistsAsync();

                        // Thêm URL đã xóa vào danh sách xóa để cập nhật DB
                        string fileUrl = blobClient.Uri.ToString();
                        deletedUrls.Add(new AuctionImage()
                        {
                            AuctionId = inputDto.UpdateAuction.AuctionID,
                            ImageUrl = fileUrl,
                        });
                    }
                }

                // Cập nhật cơ sở dữ liệu sau khi thêm và xóa
                if (addedUrls.Count > 0)
                {
                    var addImg = _auctionImageDao.SaveImgProduct(addedUrls);
                    if (addImg.ResultCd != ResultCd.SUCCESS)
                    {
                        return BadRequest(addImg);
                    }
                }
                if (deletedUrls.Count > 0)
                {
                    var deleteImg = _auctionImageDao.DeleteImgProduct(deletedUrls);
                    if (deleteImg.ResultCd != ResultCd.SUCCESS)
                    {
                        return BadRequest(deleteImg);
                    }
                }


                if (inputDto.UpdateAuction.IsPrivate == true)
                {
                    var inputInv = new CreateInvitationInputDto
                    {
                        AuctionId = inputDto.UpdateAuction.AuctionID,
                        InvitedIds = inputDto.UpdateAuction.InvitedIds,
                    };

                    var curInv = _auctionInvitationDao.GetCurInvitation(inputDto.UpdateAuction.AuctionID);

                    var addedIds = new List<long>();

                    foreach (var item in inputInv.InvitedIds)
                    {
                        if (!curInv.Invs.Contains(item))
                        {
                            addedIds.Add(item);
                        }
                        else
                        {
                            curInv.Invs.Remove(item);
                        }
                    }

                    inputInv.InvitedIds = addedIds;
                    if (addedIds.Count > 0)
                    {
                        var InputInv = _auctionInvitationDao.CreateInvitation(inputInv);
                        if (InputInv.ResultCd != ResultCd.SUCCESS)
                        {
                            return BadRequest(InputInv);
                        }
                    }

                    if (curInv.Invs.Count > 0)
                    {
                        var deleteInv = new CreateInvitationInputDto
                        {
                            AuctionId = inputDto.UpdateAuction.AuctionID,
                            InvitedIds = curInv.Invs,
                        };
                        var deleteIds = _auctionInvitationDao.DeleteInvitation(deleteInv);
                        if (deleteIds.ResultCd != ResultCd.SUCCESS)
                        {
                            return BadRequest(deleteIds);
                        }
                    }
                }

                var isUpdate = await _systemConfigurationDao.GetValue("0004");

                var inputCreateRequest = new CreateAuctionRequestInputDto()
                {
                    AuctionId = inputDto.UpdateAuction.AuctionID,
                    UserId = uId,
                    Type = isUpdate == "0" ? false : true,
                };

                var createReq = _auctionRequestDao.CreateAuctionRequest(inputCreateRequest);

                if (createReq.ResultCd != ResultCd.SUCCESS)
                {
                    return BadRequest(createReq);
                }

                var output = this.Output(ResultCd.SUCCESS).Create<CreateAuctionOutputDto>();

                return Ok(output);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }


}
