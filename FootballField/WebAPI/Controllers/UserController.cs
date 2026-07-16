using Business.Abstract;
using Core.Extensions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;

        public UserController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpGet("myProfile")]
        public IActionResult GetMyProfile()
        {
            var email = User.GetUserEmail();

            var result = _userService.GetUserProfileByEmail(email);

            if (!result.Success)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }
        [HttpGet("profile/{rowGuid}")]
        public IActionResult GetUserProfile([FromRoute] Guid rowGuid)
        {
            
            var result = _userService.GetUserProfileByGuid(rowGuid);
            if (!result.Success) return NotFound(result);
            return Ok(result);
        }
    }
}
