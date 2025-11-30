using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;

namespace University.Tuition.Api.Controllers
{
    [ApiController]
    [ApiVersion("1.0")]
    [ApiVersion("2.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    public class PingController : ControllerBase
    {
        [HttpGet]
        [MapToApiVersion("1.0")]
        public IActionResult GetV1()
            => Ok(new { status = "ok", version = "v1" });

        [HttpGet]
        [MapToApiVersion("2.0")]
        public IActionResult GetV2()
            => Ok(new { status = "ok", version = "v2", note = "API is healthy" });
    }
}
