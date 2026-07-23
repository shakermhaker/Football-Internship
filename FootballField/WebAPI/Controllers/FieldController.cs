using Business.Abstract;
using Core.Extensions;
using DataAccess.Abstract;
using Entities.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize] // Token'ı olmayan buraya giremez
    public class FieldsController : ControllerBase
    {
        private readonly IFieldService _fieldService;

        public FieldsController(IFieldService fieldService)
        {
            _fieldService = fieldService;
        }

        [HttpPost("addwithschedules")]
        public IActionResult AddWithSchedules([FromBody] FootballFieldAddDTO fieldDto)
        {
            // 1. ADIM: Senin yazdığın o kral metodu kullanarak ID'yi çekiyoruz
            var userId = User.GetUserId();

            // 2. ADIM: Angular'dan gelen DTO'nun boş olan BusinessId kısmını dolduruyoruz.
            // (Eğer senin GetUserId() metodu string dönüyorsa burayı int.Parse(userId) yaparsın)
            fieldDto.BusinessId = userId;

            // 3. ADIM: Güvenli ve dolgun veriyi Manager'ın şefkatli kollarına bırakıyoruz
            var result = _fieldService.AddWithDetails(fieldDto);

            if (result.Success)
            {
                return Ok(result);
            }

            return BadRequest(result);
        }
    }
}