using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using test.DTOs;
using test.Helpers;
using test.Services;

namespace test.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class DesignConversationsController : ControllerBase
{
    private readonly DesignConversationService _conversations;

    public DesignConversationsController(DesignConversationService conversations) => _conversations = conversations;

    private int CurrentUserId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)
        ?? User.FindFirstValue("sub")!);

    /// <summary>Book a private design conversation with an engineer.</summary>
    [HttpPost("engineer/{engineerId}")]
    public async Task<IActionResult> Book(int engineerId, [FromBody] BookDesignConversationRequest req)
    {
        var (result, message, dto) = await _conversations.BookAsync(CurrentUserId, engineerId, req);
        return result switch
        {
            DesignConversationResult.Success => CreatedAtAction(nameof(GetById), new { id = dto!.Id }, ApiResponse.Success(dto, message)),
            DesignConversationResult.EngineerNotFound => NotFound(ApiResponse.Fail(message)),
            DesignConversationResult.CityNotFound => NotFound(ApiResponse.Fail(message)),
            _ => BadRequest(ApiResponse.Fail(message)),
        };
    }

    /// <summary>Get a design conversation request by id (only the client or the engineer can see it).</summary>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var dto = await _conversations.GetByIdAsync(id);
        if (dto == null) return NotFound(ApiResponse.Fail("Design conversation request not found."));
        if (dto.UserId != CurrentUserId && dto.EngineerId != CurrentUserId)
            return StatusCode(StatusCodes.Status403Forbidden, ApiResponse.Fail("You are not allowed to view this request."));
        return Ok(ApiResponse.Success(dto));
    }

    /// <summary>Get all design conversation requests the logged-in user booked.</summary>
    [HttpGet("my")]
    public async Task<IActionResult> GetMine() =>
        Ok(ApiResponse.Success(await _conversations.GetMyRequestsAsync(CurrentUserId)));

    /// <summary>Get all design conversation requests sent to the logged-in engineer.</summary>
    [HttpGet("received")]
    public async Task<IActionResult> GetReceived() =>
        Ok(ApiResponse.Success(await _conversations.GetReceivedRequestsAsync(CurrentUserId)));
}
