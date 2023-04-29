using Microsoft.AspNetCore.Mvc;
using Shared.Clients;
using Shared.Models;

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

        /// <summary>
        /// Get most recent matches list
        /// </summary>
        /// <returns>List of current and upcoming cricket matches</returns>
        [HttpGet("matches-list")]
        [ProducesResponseType(typeof(MatchesDbModel), 200)]
        public async Task<IActionResult> GetMatchesList()
        {
            var matchesResponse = await _dbClient.GetMostRecentlyStoredCricketMatches();
            
            return Ok(matchesResponse);
        }
    }
}