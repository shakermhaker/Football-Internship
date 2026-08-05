using Business.Abstract;
using Core.Extensions;
using Entities.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Microsoft.AspNetCore.RateLimiting;

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

        [HttpGet("getheldids")]
        public async Task<IActionResult> GetHeldScheduleIdsByDate(int businessId, [FromQuery] DateOnly date)
        {
            var result = await _reservationService.GetHeldScheduleIdsByDateAsync(businessId, date);
            if (result.Success)
            {
                return Ok(result);
            }
            return BadRequest(result);
        }

        [HttpPost("hold-slot")]
        [EnableRateLimiting("ReservationLimit")] // Hold işlemi de spama karşı korunsun
        public async Task<IActionResult> HoldSlot([FromQuery] int businessId, [FromQuery] DateOnly date, [FromQuery] int scheduleId)
        {
            var userId = User.GetUserId(); // Token içinden UserId'yi güvenle alıyoruz
            if (userId == null) return Unauthorized("Kullanıcı kimliği doğrulanamadı.");

            var result = await _reservationService.HoldReservationSlotAsync(businessId, date, scheduleId, userId);

            if (result.Success) return Ok(result);
            return BadRequest(result);
        }


        [HttpPost("cancel-hold")]
        public async Task<IActionResult> CancelHold([FromQuery] int businessId, [FromQuery] DateOnly date, [FromQuery] int scheduleId)
        {
            var userId = User.GetUserId();
            if (userId == null) return Unauthorized("Kullanıcı kimliği doğrulanamadı.");

            var result = await _reservationService.CancelHoldSlotAsync(businessId, date, scheduleId, userId);

            if (result.Success) return Ok(result);
            return BadRequest(result);
        }

        [HttpPost("create")]
        [EnableRateLimiting("ReservationLimit")]
        public async Task<IActionResult> CreateReservation([FromBody] CreateReservationDto reservationDto) // 🚀 Task yapıldı
        {
            var userId = User.GetUserId(); // Token içinden UserId'yi güvenle alıyoruz
            if (userId == null) return Unauthorized("Kullanıcı kimliği doğrulanamadı.");

            // 🚀 async/await yapısına uyarlandı
            var result = await _reservationService.CreateReservationAsync(reservationDto, userId);

            if (result.Success) return Ok(result);
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

        [HttpGet("getdailyreservations")]
        public IActionResult GetDailyReservations(int businessId, DateTime date)
            {
            var result = _reservationService.GetDailyReservations(businessId, date);
            if (result.Success)
            {
                return Ok(result);
            }
            return BadRequest(result);
        }

        [HttpPost("cancelbybusiness")]
        public IActionResult CancelByBusiness([FromBody] int reservationId)
        {
            var result = _reservationService.CancelReservationByBusiness(reservationId);
            if (result.Success)
            {
                return Ok(result);
            }
            return BadRequest(result);
        }
    }
}
