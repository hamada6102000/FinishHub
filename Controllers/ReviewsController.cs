using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using test.DTOs;
using test.Helpers;
using test.Services;

namespace test.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ReviewsController : ControllerBase
{
    private readonly ReviewService _reviews;

    public ReviewsController(ReviewService reviews) => _reviews = reviews;

    private int CurrentUserId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)
        ?? User.FindFirstValue("sub")!);

    /// <summary>Get all reviews for the current user.</summary>
    [HttpGet("user")]
    [Authorize]
    public async Task<IActionResult> GetUserReviews() =>
        Ok(ApiResponse.Success(await _reviews.GetUserReviewsAsync(CurrentUserId)));

    /// <summary>Get a review by id.</summary>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var dto = await _reviews.GetByIdAsync(id);
        if (dto == null) return NotFound(ApiResponse.Fail("Review not found."));
        return Ok(ApiResponse.Success(dto));
    }

    /// <summary>Add a review to a user.</summary>
    [HttpPost("user/{userId}")]
    [Authorize]
    public async Task<IActionResult> Add(int userId, [FromBody] AddReviewRequest req)
    {
        var (success, message, dto) = await _reviews.AddAsync(userId, CurrentUserId, req);
        if (!success) return BadRequest(ApiResponse.Fail(message));
        return CreatedAtAction(nameof(GetById), new { id = dto!.Id }, ApiResponse.Success(dto, message));
    }

    /// <summary>Update a review.</summary>
    [HttpPut("{id}")]
    [Authorize]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateReviewRequest req)
    {
        var (success, dto) = await _reviews.UpdateAsync(id, req);
        if (!success) return NotFound(ApiResponse.Fail("Review not found or invalid rate."));
        return Ok(ApiResponse.Success(dto, "Review updated."));
    }

    /// <summary>Delete a review.</summary>
    [HttpDelete("{id}")]
    [Authorize]
    public async Task<IActionResult> Delete(int id)
    {
        var success = await _reviews.DeleteAsync(id);
        if (!success) return NotFound(ApiResponse.Fail("Review not found."));
        return Ok(ApiResponse.Success(null, "Review deleted."));
    }
}
