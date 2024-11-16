using GamestatsBase;
using Microsoft.AspNetCore.Mvc;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace SampleCore.Controllers;

[Route("dummy.asp")]
[ApiController]
[GamestatsConfig("00000000000000000000", 0, 0, 0, 0, "dummy", GamestatsRequestVersions.Version1, GamestatsResponseVersions.Version1, encryptedRequest: false, requireSession: false)]
public class DummyController : ControllerBase
{
    [HttpGet]
    public IActionResult Get(string data)
    {
        return Ok();
    }
}
