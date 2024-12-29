using Azure.Storage.Blobs.Models;
using Azure.Storage.Blobs;
using BE_AuctionAOT.Common.Base.Entity;
using BE_AuctionAOT.Common.Constants;
using BE_AuctionAOT.Common.Utility;
using BE_AuctionAOT.DAO.AuctionManagement.Auction;
using BE_AuctionAOT.DAO.AuctionManagement.Dispute;
using BE_AuctionAOT.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using BE_AuctionAOT.DAO.AuctionManagement.EvidenceFile;

namespace BE_AuctionAOT.Controllers.AuctionManagement.PaymentConfirmation
{
	[Route("api/[controller]")]
	[ApiController]
	public class PaymentConfirmationController : ControllerBase
	{
		private readonly DisputeDao _disputeDao;
		private readonly EvidenceFileDao _evidenceFileDao;
		private readonly AuthUtility _authUtility;
        private readonly BlobServiceClient _blobServiceClient;

        public PaymentConfirmationController(DisputeDao disputeDao, EvidenceFileDao evidenceFileDao, AuthUtility authUtility, BlobServiceClient blobServiceClient)
		{
			_disputeDao = disputeDao;
            _evidenceFileDao = evidenceFileDao;
			_authUtility = authUtility;
            _blobServiceClient = blobServiceClient;
        }


		[Authorize]
		[HttpPost("ListWinnedAuc")]
		public async Task<IActionResult> GetListWinnedAuctionUser(WinnerAuctionInputDto inputDto)
		{
			try
			{
				var token = HttpContext.Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
				var uId = _authUtility.GetIdInHeader(token);
				var winnedList = new JoinedAuctionOutputDto();
				winnedList = await _disputeDao.GetWinnedAuctionForUser(inputDto, uId);
				if (winnedList.ResultCd != ResultCd.SUCCESS)
				{
					return BadRequest(winnedList);
				}
				return Ok(winnedList);
			}
			catch (Exception ex)
			{
				return BadRequest();
			}
		}
		[Authorize]
		[HttpPost("ListBiddedAuc")]
		public async Task<IActionResult> GetListBidedAuctionUser(ListAuctionBiddedInputDto inputDto)
		{
			try
			{
				var token = HttpContext.Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
				var uId = _authUtility.GetIdInHeader(token);
				var bidedList = new ListBidedOutputDto();
				bidedList = await _disputeDao.GetAuctionListWasBided(inputDto, uId);
				if (bidedList.ResultCd != ResultCd.SUCCESS)
				{
					return BadRequest(bidedList);
				}
				return Ok(bidedList);
			}
			catch (Exception ex)
			{
				return BadRequest();
			}
		}
		[Authorize]
		[HttpPost("ListDispute")]
		public async Task<IActionResult> GetListDisputeAdmin(ListDisputeInputDto inputDto)
		{
			try
			{
				var disputeList = new ListDisputeOutputDto();
				disputeList = await _disputeDao.GetDisputeListAdmin(inputDto);
				if (disputeList.ResultCd != ResultCd.SUCCESS)
				{
					return BadRequest(disputeList);
				}
				return Ok(disputeList);
			}
			catch (Exception ex)
			{
				return BadRequest();
			}
		}

		[Authorize]
		[HttpPut("ConfirmedTrueCreator/{AuctionId}")]
		public async Task<IActionResult> ConfirmedtrueCreator(int AuctionId)
		{
			try
			{
				var confirmtrue = new BaseOutputDto();
				confirmtrue = await _disputeDao.TrueConfirmedCreator(AuctionId);
				if (confirmtrue.ResultCd != ResultCd.SUCCESS)
				{
					return BadRequest(confirmtrue);
				}
				return Ok(confirmtrue);
			}
			catch (Exception ex)
			{
				return BadRequest();
			}

		}


        [Authorize]
        [HttpPost("confirmpayment")]
        public async Task<IActionResult> CustomerPaymentConfirmation([FromForm]CustomerPaymentConfirmationInputDto inputDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }
                var token = HttpContext.Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
                var uId = _authUtility.GetIdInHeader(token);
                var output = this.Output(ResultCd.SUCCESS).Create<BaseOutputDto>();
                var disputeDao = await _disputeDao.GetDisputeById(inputDto.AuctionId);

				if (disputeDao == null)
				{
					return Ok(disputeDao);
				}

				if (disputeDao.Dispute.CreatorId == uId)
				{
					disputeDao.Dispute.CreatorConfirmed = inputDto.IsPaid;
                    
                    if (!inputDto.IsPaid)
                    {
						if(disputeDao.Dispute?.WinnerConfirmed == true)
						{
                            disputeDao.Dispute.DisputeStatusId = AuctionConst.DISPUTE.PENDING;
                        }
                        disputeDao.Dispute.CreatorEvidence = inputDto.CreatorEvidence;

                        string containerName = "evidenceofpayment";
                        BlobContainerClient containerClient = _blobServiceClient.GetBlobContainerClient(containerName);
                        var addedUrls = new List<EvidenceFile>();
						var file = inputDto.FileEvidence;
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
                            addedUrls.Add(new EvidenceFile()
                            {
                                DisputeId = disputeDao.Dispute.DisputeId,
								FileUrl = fileUrl,
								FileType = Path.GetExtension(file.FileName),
								UploadedBy = uId
                            });
                        }
                        if (addedUrls.Count > 0)
                        {
                            _evidenceFileDao.SaveEvidenceFile(addedUrls);
                        }
                    }

                    var confirm = await _disputeDao.Confirm(disputeDao.Dispute);
                    if (confirm.ResultCd != ResultCd.SUCCESS)
                    {
                        return Ok(confirm);
                    }
                }
				else
				{
					if(disputeDao.Dispute.WinnerId == uId)
					{
						if (inputDto.IsPaid) 
						{
                            if (disputeDao.Dispute?.CreatorConfirmed == false)
                            {
                                disputeDao.Dispute.DisputeStatusId = AuctionConst.DISPUTE.PENDING;
                            }

                            disputeDao.Dispute.WinnerConfirmed = true;
							disputeDao.Dispute.WinnerEvidence = inputDto.WinnerEvidence;

                            var confirm = await _disputeDao.Confirm(disputeDao.Dispute);
                            if (confirm.ResultCd != ResultCd.SUCCESS)
                            {
                                return Ok(confirm);
                            }

                            string containerName = "evidenceofpayment";
                            BlobContainerClient containerClient = _blobServiceClient.GetBlobContainerClient(containerName);
                            var addedUrls = new List<EvidenceFile>();
                            var file = inputDto.FileEvidence;
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
                                addedUrls.Add(new EvidenceFile()
                                {
                                    DisputeId = disputeDao.Dispute.DisputeId,
                                    FileUrl = fileUrl,
                                    FileType = Path.GetExtension(file.FileName),
                                    UploadedBy = uId
                                });
                            }
                            if (addedUrls.Count > 0)
                            {
                                _evidenceFileDao.SaveEvidenceFile(addedUrls);
                            }
                        }
					}
				}
				return Ok(output);

            }
            catch (Exception ex)
            {
                return BadRequest(ex);
            }

        }

		[Authorize]
		[HttpPost("AdminConfirmed")]
		public async Task<IActionResult> AdminConfirmed([FromForm] AdminDecisionInputDto inputDto)
		{
			try
			{
				var confirmtrue = new BaseOutputDto();
				confirmtrue = await _disputeDao.AdminConfirm(inputDto);
				if (confirmtrue.ResultCd != ResultCd.SUCCESS)
				{
					return BadRequest(confirmtrue);
				}
				return Ok(confirmtrue);
			}
			catch (Exception ex)
			{
				return BadRequest();
			}

		}
		[Authorize]
		[HttpGet("DisputeDetail/{DisputeId}")]
		public async Task<IActionResult> GetDisputeDetail(long DisputeId)
		{
			try
			{
				var disputeDetail = new DisputeDetailDto();
				disputeDetail = await _disputeDao.GetDisputeDetail(DisputeId);
				if (disputeDetail.ResultCd != ResultCd.SUCCESS)
				{
					return BadRequest(disputeDetail);
				}
				return Ok(disputeDetail);
			}
			catch (Exception ex)
			{
				return BadRequest();
			}

		}
	}
}
