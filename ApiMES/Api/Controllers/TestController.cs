using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ApiMES.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TestController : ControllerBase
    {
        [HttpGet("secure")]
        [Authorize]
        public IActionResult GetSecureData()
        {
            return Ok(new { message = "Bạn đã truy cập API bảo vệ thành công!" });
        }

        [HttpGet("check-token")]
        public IActionResult CheckTokenFromCookie()
        {
            var token = Request.Cookies["AccessToken"];
            return Ok(new { token });
        }

        [HttpDelete("delete-token")]
        public IActionResult DeleteTokenFromCookie()
        {
            Response.Cookies.Delete("AccessToken");

            return Ok(new { message = "Token đã được xoá khỏi cookie." });
        }
    }
}
