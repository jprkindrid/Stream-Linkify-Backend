using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;

namespace Stream_Linkify_Backend.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class HealthController : ControllerBase
    {
        [HttpGet]
        [DisableCors]
        public IActionResult Get() => Ok();
    }
}
