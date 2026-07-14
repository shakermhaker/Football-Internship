using Business.Abstract;
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

        [HttpPost("login")]
        public ActionResult Login(UserForLoginDto userForLoginDto)
        {
            var userToLogin = _authService.Login(userForLoginDto);
            if (!userToLogin.Success) return BadRequest(userToLogin.Message);

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
        public ActionResult Register(UserForRegisterDto userForRegisterDto)
        {
            // Kayıt işlemini başlatıyoruz
            var registerResult = _authService.Register(userForRegisterDto, userForRegisterDto.Password);

            // KRİTİK KONTROL: Kayıt işlemi başarılı oldu mu? (Örn: Bu e-posta zaten kullanılıyor olabilir)
            if (!registerResult.Success)
            {
                // Başarısızsa (kullanıcı varsa vb.) token üretmeye geçmeden işlemi durdur ve hata mesajını dön
                return BadRequest(registerResult.Message);
            }

            // Kayıt başarılıysa, kullanıcıya direkt giriş yetkisi vermek için Token üretiyoruz
            var result = _authService.CreateAccessToken(registerResult.Data);
            if (!result.Success)
            {
                return BadRequest(result.Message);
            }

            // Her şey kusursuzsa token'ı (ve dolayısıyla başarı mesajını) Angular'a gönderiyoruz
            return Ok(result.Data);
        }
    }
}
