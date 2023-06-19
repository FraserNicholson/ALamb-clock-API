using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Shared.Clients;
using Shared.Contracts;
using Shared.Models.Database;

namespace ALamb_clock_API.Controllers;

[ApiController]
[Route("matches")]
[EnableRateLimiting("Fixed")]
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
    [ProducesResponseType(typeof(IEnumerable<MatchDbModel>), 200)]
    public async Task<IActionResult> GetMatchesList()
    {
        var matchesResponse = await _dbClient.GetAllMatches();
            
        return Ok(matchesResponse);
    }

    /// <summary>
    /// Queries currently stored matches
    /// </summary>
    /// <param name="request"></param>
    /// <returns>All matches that match the request criteria</returns>
    [HttpPost("query-matches")]
    public async Task<IActionResult> QueryMatches([FromBody] QueryMatchesRequest request)
    {
        var (isValid, errorMessage) = request.IsValid();
        
        if (!isValid)
        {
            return BadRequest(errorMessage);
        }

        var matches = await _dbClient.QueryMatches(request);

        return Ok(matches);
    }
    

    /// <summary>
    /// Sets up a notification
    /// </summary>
    /// <returns>Sets up a notification that will be sent to a device when the notification criteria is met</returns>
    [HttpPost("setup-notification")]
    public async Task<IActionResult> SetupNotification([FromBody] AddNotificationRequest request)
    {
        var createdNotification = await _dbClient.AddOrUpdateNotification(request);

        var response = new NotificationCreatedResponse
        {
            Id = createdNotification.Id,
            MatchId = createdNotification.MatchId
        };

        return Created(response.Id, response);
    }

    [HttpGet("get-notifications")]
    public async Task<IActionResult> GetNotifications([FromQuery] string registrationToken)
    {
        if (string.IsNullOrWhiteSpace(registrationToken))
        {
            return BadRequest($"Query parameter {nameof(registrationToken)} must be provided");
        }

        var notifications = await _dbClient.GetAllNotificationsForRegistrationToken(registrationToken);

        if (notifications is null || !notifications.Any())
        {
            return NotFound();
        }
        
        return Ok(notifications);
    }

    [HttpDelete("delete-notification")]
    public async Task<IActionResult> DeleteNotification(
        [FromQuery] string notificationId,
        [FromQuery] string registrationToken)
    {
        if (string.IsNullOrWhiteSpace(registrationToken))
        {
            return BadRequest($"Query parameter {nameof(registrationToken)} must be provided");
        }
        
        if (string.IsNullOrWhiteSpace(notificationId))
        {
            return BadRequest($"Query parameter {nameof(notificationId)} must be provided");
        }

        await _dbClient.DeleteNotification(notificationId, registrationToken);

        return Ok();
    }
}