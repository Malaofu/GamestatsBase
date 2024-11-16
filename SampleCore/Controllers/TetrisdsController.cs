using GamestatsBase;
using Microsoft.AspNetCore.Mvc;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace SampleCore.Controllers;

[Route("tetrisds")]
[ApiController]
[GamestatsConfig("Wo3vqrDoL56sAdveYeC1", 0, 0, 0, 0, "tetrisds", GamestatsRequestVersions.Version1, GamestatsResponseVersions.Version1, encryptedRequest: false, requireSession: true)]
public class TetrisdsController : ControllerBase
{
    [HttpGet("store.asp")]
    public IActionResult Store(string name, string? region)
    {
        byte[] nameBytes = Common.FromUrlSafeBase64String(name);
        char[] nameChars = new char[nameBytes.Length >> 1];

        Buffer.BlockCopy(nameBytes, 0, nameChars, 0, nameBytes.Length);
        string _name = new string(nameChars);

        string _region = region;

        // todo: Figure out what the data contains and how to parse it, so
        // we can have a leaderboard.

        // todo: Write name, pid, region, and the mysteries contained
        // within the data blob to a database.

        // Since the correct response is actually blank, we don't need to
        // write anything to it here.

        return Ok(_name);
    }
}
