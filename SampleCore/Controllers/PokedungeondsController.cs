using GamestatsBase;
using Microsoft.AspNetCore.Mvc;

namespace SampleCore.Controllers;

[Route("pokedungeonds/web")]
[ApiController]
[GamestatsConfig("TXqjDDOLhPySKSztgBHY", 114069, 32153, 512, 1631340900, "pokedungeonds", GamestatsRequestVersions.Version2, GamestatsResponseVersions.Version1, encryptedRequest: true, requireSession: true)]
public class PokedungeondsController : ControllerBase
{
    [HttpGet("common/setProfile.asp")]
    public IActionResult SetProfile()
    {
        Response.Body.Write([0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00]);

        return Ok();
    }

    [HttpGet("rescue/rescueExist.asp")]
    public IActionResult RescueExist()
    {
        return Ok();
    }

    [HttpGet("rescue/rescueList.asp")]
    public IActionResult RescueList()
    {
        return Ok();
    }
}
