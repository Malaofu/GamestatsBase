using GamestatsBase;
using Microsoft.AspNetCore.Mvc;

namespace SampleCore.Controllers;

[Route("session")]
[ApiController]
public class SessionController : ControllerBase
{
    private readonly GamestatsSessionManager _sessionManager;

    public SessionController(GamestatsSessionManager sessionManager)
    {
        _sessionManager = sessionManager;
    }

    [HttpGet]
    public IActionResult GetAll()
    {
        return Ok(_sessionManager.Sessions);
    }

    [HttpGet("{gameId}")]
    public IActionResult Get(string gameId)
    {
        return Ok(_sessionManager.Sessions.Where(s => s.Value.GameId == gameId));
    }
}
