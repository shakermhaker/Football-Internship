using Entities.DTOs;
using Microsoft.AspNetCore.Mvc;
using Business.Abstract;
using Core.Extensions; // User.GetUserId() için gerekli
using Entities.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BusinessController : ControllerBase
    {
        private readonly IBusinessService _businessService;

        // Sadece BusinessService'i enjekte etmemiz yeterli
        public BusinessController(IBusinessService businessService)
        {
            _businessService = businessService;
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
    }
}