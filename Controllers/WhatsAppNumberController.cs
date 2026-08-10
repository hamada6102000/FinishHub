using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using test.DTOs;
using test.Helpers;
using test.Services;

namespace test.Controllers;

/// <summary>
/// Global WhatsApp number configuration. The solution holds exactly zero or one number:
/// it can be added once and then updated, but never deleted — so there is deliberately
/// no DELETE endpoint and no list endpoint here.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class WhatsAppNumberController : ControllerBase
{
    private readonly WhatsAppNumberService _whatsAppNumbers;

    public WhatsAppNumberController(WhatsAppNumberService whatsAppNumbers) => _whatsAppNumbers = whatsAppNumbers;

    /// <summary>Get the configured WhatsApp number.</summary>
    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> Get()
    {
        var dto = await _whatsAppNumbers.GetAsync();
        if (dto == null) return NotFound(ApiResponse.Fail("No WhatsApp number has been configured yet."));
        return Ok(ApiResponse.Success(dto));
    }

    /// <summary>Add the WhatsApp number. Only allowed when none exists.</summary>
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateWhatsAppNumberRequest req)
    {
        var (result, message, data) = await _whatsAppNumbers.CreateAsync(req);
        return result switch
        {
            WhatsAppNumberResult.Success       => CreatedAtAction(nameof(Get), null, ApiResponse.Success(data, message)),
            WhatsAppNumberResult.AlreadyExists => Conflict(ApiResponse.Fail(message)),
            _ => BadRequest(ApiResponse.Fail(message)),
        };
    }

    /// <summary>Update the existing WhatsApp number.</summary>
    [HttpPut]
    public async Task<IActionResult> Update([FromBody] UpdateWhatsAppNumberRequest req)
    {
        var (result, message, data) = await _whatsAppNumbers.UpdateAsync(req);
        return result switch
        {
            WhatsAppNumberResult.Success         => Ok(ApiResponse.Success(data, message)),
            WhatsAppNumberResult.NotFound        => NotFound(ApiResponse.Fail(message)),
            WhatsAppNumberResult.DuplicateNumber => Conflict(ApiResponse.Fail(message)),
            _ => BadRequest(ApiResponse.Fail(message)),
        };
    }
}
