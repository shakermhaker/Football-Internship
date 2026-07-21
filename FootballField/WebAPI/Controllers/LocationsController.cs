using Business.Abstract;
using Microsoft.AspNetCore.Mvc;

namespace WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LocationsController : ControllerBase
    {
        private readonly ILocationService _locationService;

        // Constructor Injection (UserController ile birebir aynı mantık)
        public LocationsController(ILocationService locationService)
        {
            _locationService = locationService;
        }

        // Tüm illeri getiren endpoint
        [HttpGet("cities")]
        public IActionResult GetCities()
        {
            var result = _locationService.GetAllCities();

            if (!result.Success)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }

        // Seçilen ilin ID'sine (Plaka) göre ilçeleri getiren endpoint
        [HttpGet("cities/{cityId}/districts")]
        public IActionResult GetDistrictsByCity([FromRoute] int cityId)
        {
            var result = _locationService.GetDistrictsByCityId(cityId);

            if (!result.Success)
            {
                // İlçe bulunamazsa veya hata olursa NotFound veya BadRequest dönüyoruz
                return NotFound(result);
            }

            return Ok(result);
        }
    }
}