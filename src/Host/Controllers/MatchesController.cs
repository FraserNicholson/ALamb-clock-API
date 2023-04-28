using Microsoft.AspNetCore.Mvc;
using Shared.Clients;

namespace ALamb_clock_API.Controllers
{
    [ApiController]
    [Route("matches")]
    public class MatchesController : Controller
    {
        private readonly IDbClient _dbClient;
        public MatchesController(IDbClient dbClient)
        {
            _dbClient = dbClient;
        }

        [HttpGet("matches-list")]
        public async Task<IActionResult> GetMatchesList()
        {
            var matchesResponse = await _dbClient.GetCricketMatches();
            
            return Ok(matchesResponse);
        }
    }
}