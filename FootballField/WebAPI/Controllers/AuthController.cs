using Business.Abstract;
using Core.Utilities.Results;
using Entities.Concrete;
using Entities.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace WebAPI.Controllers
{
    [Route("api/[controller]")] // 1. EKLENEN SATIR: Adresi api/Auth olarak ayarlar
    [ApiController]             // 2. EKLENEN SATIR: Bunun bir API olduğunu belirtir
    public class AuthController : ControllerBase // 3. DEĞİŞEN KISIM: Controller -> ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("resendVerificationEmail")]
        public async Task<IActionResult> ResendVerificationEmail([FromBody] string email)
        {
            // E-posta boş gelmiş mi kontrolü
            if (string.IsNullOrWhiteSpace(email))
            {
                return BadRequest(new ErrorResult("E-posta adresi boş olamaz."));
            }

            // Business katmanına işi devrediyoruz
            var result = await _authService.ResendVerificationEmailAsync(email);

            // Eğer kullanıcı yoksa veya zaten onaylıysa BadRequest dönüyoruz
            if (!result.Success)
            {
                return BadRequest(result);
            }

            // Başarılıysa 200 OK ile mesajı dönüyoruz
            return Ok(result);
        }

        [HttpGet("verifyUserAccount")]
        public async Task<IActionResult> VerifyUserAccount([FromQuery] int userId, [FromQuery] string token)
        {
            // Parametreler eksik mi diye güvenlik kontrolü
            if (userId <= 0 || string.IsNullOrEmpty(token))
            {
                return BadRequest("Geçersiz veya eksik doğrulama bağlantısı.");
            }

            
            var result = await _authService.VerifyUserAccountAsync(userId, token);

            if (!result.Success)
            {
                
                return BadRequest(result.Message);
            }

            

            return Redirect("http://localhost:4200/auth/login");
        }


        [HttpPost("login")]
        public ActionResult Login(UserForLoginDto userForLoginDto)
        {
            var userToLogin = _authService.Login(userForLoginDto);
            if (!userToLogin.Success)
            {
                return BadRequest(userToLogin);
            }

            var result = _authService.CreateAccessToken(userToLogin.Data);
            if (result.Success)
            {
                // Token'ı Cookie'ye ekle
                Response.Cookies.Append("auth_token", result.Data.Token, new CookieOptions
                {
                    HttpOnly = true,    // JS erişemez (GÜVENLİ!)
                    Secure = true,      // Sadece HTTPS üzerinden gider
                    SameSite = SameSiteMode.Strict, // CSRF koruması
                    Expires = DateTime.Now.AddDays(1)
                });

                return Ok(new { message = "Giriş başarılı" });
            }
            return BadRequest(result.Message);
        }

        [HttpPost("register")]
        public async Task<ActionResult> Register(UserForRegisterDto userForRegisterDto)
        {
            var registerResult = await _authService.Register(userForRegisterDto, userForRegisterDto.Password);
            var result =_authService.CreateAccessToken(registerResult.Data);
            if (!result.Success)
            {
                return BadRequest(result.Message);
            }
            return Ok(result.Data);
        }
    }
}
