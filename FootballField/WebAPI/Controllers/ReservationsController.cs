using Business.Abstract;
using Core.Extensions;
using Entities.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReservationsController : ControllerBase
    {
        private readonly IReservationService _reservationService;

        public ReservationsController(IReservationService reservationService)
        {
            _reservationService = reservationService;
        }

        [HttpGet("getbusinessfieldschedules")]
        public IActionResult GetBusinessFieldSchedules(int businessId, [FromQuery] DateOnly date) // 🚀 Tarih parametresi eklendi
        {
            var result = _reservationService.GetBusinessFieldSchedules(businessId, date);
            if (result.Success) return Ok(result);
            return BadRequest(result);
        }

        [HttpGet("getbookedids")]
        public IActionResult GetBookedScheduleIdsByDate(int businessId, [FromQuery] DateOnly date)
        {
            var result = _reservationService.GetBookedScheduleIdsByDate(businessId, date);
            if (result.Success)
            {
                return Ok(result);
            }
            return BadRequest(result);
        }

        [HttpPost("create")]
        public IActionResult CreateReservation([FromBody] CreateReservationDto reservationDto)
        {
            var userId = User.GetUserId(); // Kullanıcı kimliğini almak içi
            if (userId == null)
            {
                return Unauthorized("Kullanıcı kimliği doğrulanamadı. Lütfen tekrar giriş yapın.");
            }

            var result = _reservationService.CreateReservation(reservationDto, userId);


            // Sonuç başarılıysa 200 (Ok), hata varsa (örn: slot dolmuşsa) 400 (BadRequest) dönüyoruz.
            if (result.Success)
            {
                return Ok(result);
            }
            return BadRequest(result);
        }

        [HttpGet("my-reservations")]
        
        public IActionResult GetMyReservations()
        {
            // Token içinden UserId'yi güvenle alıyoruz
            var userId = User.GetUserId();

            if (userId == null)
            {
                return Unauthorized("Kullanıcı kimliği doğrulanamadı. Lütfen giriş yapın.");
            }

            var result = _reservationService.GetUserReservations(userId);

            if (result.Success)
            {
                return Ok(result);
            }

            return BadRequest(result);
        }

        [HttpPut("cancel/{id}")]
        public IActionResult CancelReservation(int id)
        {
            // Token'dan giriş yapan kullanıcının ID'sini alıyoruz
            var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (!int.TryParse(userIdString, out int userId) || userId <= 0)
            {
                return Unauthorized("Kullanıcı kimliği doğrulanamadı.");
            }

            // İptal işlemini servise gönderiyoruz
            var result = _reservationService.CancelReservation(id, userId);

            if (result.Success)
            {
                return Ok(result); // Başarılıysa 200 döner
            }

            return BadRequest(result); // Kurallara uymazsa (Başkasına aitse vb.) 400 döner
        }
    }
}
