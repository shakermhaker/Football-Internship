using Business.Abstract;
using Core.Extensions; // User.GetUserId() için gerekli
using Core.Utilities.Results;
using Entities.DTOs;
using Entities.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc;

namespace WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BusinessController : ControllerBase
    {
        private readonly IBusinessService _businessService;
        private readonly IBusinessImageService _businessImageService;
        // Sadece BusinessService'i enjekte etmemiz yeterli
        public BusinessController(IBusinessService businessService, IBusinessImageService businessImageService)
        {
            _businessService = businessService;
            _businessImageService = businessImageService;

        }

        [HttpPost("add")]
        [Authorize]
        public IActionResult Add([FromBody] BusinessForRegisterDTO businessDto)
        {
            var userId = User.GetUserId();

            var result = _businessService.Add(businessDto, userId);

            if (!result.Success) return BadRequest(result);

            return Ok(result);
        }
        [HttpGet("getall")]
        public IActionResult GetAll([FromQuery] int? cityId, [FromQuery] int? districtId, [FromQuery] string? search)
        {
            var result = _businessService.GetFilteredBusinesses(cityId, districtId, search);

            if (!result.Success)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }

        [HttpGet("getallfields")]
        public IActionResult GetAllFields([FromQuery] int businessId) // Metot ismini de URL'e uygun yapabilirsin
        {
            // 1. Token'dan giriş yapan kişinin UserId'sini al
            

            // 2. Servise BusinessId değil, UserId gönderiyoruz
            var result = _businessService.GetFieldsByUserId(businessId);

            if (!result.Success)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }
        [HttpGet("getdetailsbyid")]
        public IActionResult GetDetailsById([FromQuery] int businessId)
        {
            // BusinessManager içindeki GetBusinessDetails metodunu çağırıyoruz
            var result = _businessService.GetBusinessDetails(businessId);

            if (!result.Success)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }


        // Dosya yükleme olduğu için FromBody değil FromForm kullanıyoruz!
        [HttpPost("addimage")]
        public async Task<IActionResult> AddImage([FromForm] int businessId, IFormFile file, [FromForm] bool isCover)
        {
            var result = await _businessImageService.AddImageAsync(businessId, file, isCover);

            if (!result.Success)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }

        [HttpPut("update")]
        [Authorize] // Sadece giriş yapmış yetkili kullanıcılar güncelleyebilsin
        public IActionResult Update([FromBody] BusinessUpdateDto businessUpdateDto)
        {
            var result = _businessService.Update(businessUpdateDto);

            if (!result.Success)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }
        [HttpDelete("deleteimage")]
        [Authorize]
        public async Task<IActionResult> DeleteImage([FromQuery] int imageId)
        {
            var result = await _businessImageService.DeleteImageAsync(imageId);

            if (!result.Success)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }
        [HttpGet("dashboard-summary")]
        public IActionResult GetDashboardSummary([FromQuery] int businessId, [FromQuery] int? year)
        {
            int targetYear = year ?? DateTime.Now.Year;

            // BusinessService üzerinden (içine bağladığın ReservationDal sayesinde) verileri çekiyoruz
            var result = _businessService.GetBusinessDashboardStats(businessId, targetYear);

            if (result.Success)
            {
                return Ok(result);
            }

            return BadRequest(result);
        }


    }
}