﻿using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Shared.Clients;
using Shared.Contracts;
using Shared.Models;
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
    /// Save notification
    /// </summary>
    /// <returns>Adds or updates notification that will be sent to a device when the notification criteria is met</returns>
    [HttpPost("notification")]
    public async Task<IActionResult> SaveNotification(
        [FromBody] SaveNotificationRequest request,
        [FromHeader] string registrationToken)
    {
        if (string.IsNullOrWhiteSpace(registrationToken))
        {
            return BadRequest($"Query parameter {nameof(registrationToken)} must be provided");
        }
        
        var (isValid, errorMessage) = request.IsValid();
        
        if (!isValid)
        {
            return BadRequest(errorMessage);
        }

        var addNotificationDbRequest = new SaveNotificationDbRequest
        {
            NotificationId = request.NotificationId,
            MatchId = request.MatchId,
            RegistrationToken = registrationToken,
            TeamInQuestion = request.TeamInQuestion,
            NotificationType = Enum.Parse<NotificationType>(request.NotificationType, ignoreCase: true),
            NumberOfWickets = request.NumberOfWickets
        };
        
        var savedNotification = await _dbClient.AddOrUpdateNotification(addNotificationDbRequest);

        var response = new NotificationSavedResponse
        {
            Id = savedNotification.Id,
            MatchId = savedNotification.MatchId
        };

        if (request.NotificationId != null)
        {
            return Ok();
        }

        return Created(response.Id, response);
    }

    [HttpGet("notifications")]
    public async Task<IActionResult> GetNotifications([FromHeader] string registrationToken)
    {
        if (string.IsNullOrWhiteSpace(registrationToken))
        {
            return BadRequest($"Query parameter {nameof(registrationToken)} must be provided");
        }

        var notifications = await _dbClient.GetAllNotificationsForRegistrationToken(registrationToken);

        if (notifications is null || !notifications.Any())
        {
            return Ok(Enumerable.Empty<NotificationResponse>());
        }
        
        return Ok(notifications);
    }

    [HttpDelete("notification/{notificationId}")]
    public async Task<IActionResult> DeleteNotification(
        [FromRoute] string notificationId,
        [FromHeader] string registrationToken)
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