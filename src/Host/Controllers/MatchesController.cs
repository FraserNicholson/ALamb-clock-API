using Microsoft.AspNetCore.Mvc;

namespace ALamb_clock_API.Controllers
{
    [ApiController]
    [Route("matches")]
    public class MatchesController : Controller
    {
        public MatchesController()
        {
        }

        [HttpGet("matches-list")]
        public async Task<IActionResult> GetMatchesList()
        {
            return Ok();
        }
    }
}