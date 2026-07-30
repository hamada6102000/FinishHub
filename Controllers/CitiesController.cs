using Microsoft.AspNetCore.Mvc;
using test.DTOs;
using test.Helpers;
using test.Services;

namespace test.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CitiesController : ControllerBase
{
    private readonly CityService _cities;

    public CitiesController(CityService cities) => _cities = cities;

    private string GetLang() =>
        Request.Headers.AcceptLanguage.ToString().StartsWith("ar", StringComparison.OrdinalIgnoreCase) ? "ar" : "en";

    /// <summary>Add a new city.</summary>
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateCityRequest req)
    {
        var lang = GetLang();
        var (result, message, data) = await _cities.CreateAsync(req, lang);
        return result switch
        {
            CityResult.Success => CreatedAtAction(nameof(GetById), new { id = data!.Id }, ApiResponse.Success(data, message)),
            CityResult.DuplicateName => Conflict(ApiResponse.Fail(message)),
            _ => BadRequest(ApiResponse.Fail(message)),
        };
    }

    /// <summary>Get all cities ordered alphabetically.</summary>
    [HttpGet]
    public async Task<IActionResult> GetAll() =>
        Ok(ApiResponse.Success(await _cities.GetAllAsync(GetLang())));

    /// <summary>Get a single city by id.</summary>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var dto = await _cities.GetByIdAsync(id, GetLang());
        if (dto == null) return NotFound(ApiResponse.Fail("City not found."));
        return Ok(ApiResponse.Success(dto));
    }

    /// <summary>Update a city.</summary>
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateCityRequest req)
    {
        var (result, message, data) = await _cities.UpdateAsync(id, req, GetLang());
        return result switch
        {
            CityResult.Success => Ok(ApiResponse.Success(data, message)),
            CityResult.NotFound => NotFound(ApiResponse.Fail(message)),
            CityResult.DuplicateName => Conflict(ApiResponse.Fail(message)),
            _ => BadRequest(ApiResponse.Fail(message)),
        };
    }

    /// <summary>Pin or unpin a city (admin use).</summary>
    [HttpPatch("{id}/pinned")]
    public async Task<IActionResult> SetPinned(int id, [FromBody] SetCityPinnedRequest req)
    {
        var (result, message, data) = await _cities.SetPinnedAsync(id, req.IsPinned, GetLang());
        return result switch
        {
            CityResult.Success => Ok(ApiResponse.Success(data, message)),
            CityResult.NotFound => NotFound(ApiResponse.Fail(message)),
            _ => BadRequest(ApiResponse.Fail(message)),
        };
    }

    /// <summary>Delete a city.</summary>
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var (result, message) = await _cities.DeleteAsync(id);
        return result switch
        {
            CityResult.Success => Ok(ApiResponse.Success(null, message)),
            CityResult.NotFound => NotFound(ApiResponse.Fail(message)),
            CityResult.InUse => Conflict(ApiResponse.Fail(message)),
            _ => BadRequest(ApiResponse.Fail(message)),
        };
    }
}
