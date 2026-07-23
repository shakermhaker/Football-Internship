using Business.Abstract;
using Core.Extensions;
using Entities.DTOs;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

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

        [HttpGet("TeamAvatars")]
        public async Task<IActionResult> GetAvatars()
        {
            
            var avatars = _userService.GetAvatars();
            return Ok(new { success = true, data = avatars, message = "Takım listesi başarıyla getirildi." });
        }

        [HttpPut("updateProfile")]
        public async Task<IActionResult> UpdateProfile([FromBody] UserProfileDto updateDto)
        {
            var userEmail = User.GetUserEmail();

            // Tüm iş kuralı ve güncelleme mantığı Business katmanına devredildi
            var result = await _userService.UpdateProfileAsync(userEmail, updateDto);

            if (result.Success)
            {
                return Ok(result);
            }

            return BadRequest(result);
        }
    }
}
