using Microsoft.AspNetCore.Mvc;
using test.DTOs;
using test.Helpers;
using test.Services;

namespace test.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UserTypesController : ControllerBase
{
    private readonly UserTypeService _userTypes;

    public UserTypesController(UserTypeService userTypes) => _userTypes = userTypes;

    private string GetLang() =>
        Request.Headers.AcceptLanguage.ToString().StartsWith("ar", StringComparison.OrdinalIgnoreCase) ? "ar" : "en";

    /// <summary>Add a new user type.</summary>
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateUserTypeRequest req)
    {
        var (result, message, data) = await _userTypes.CreateAsync(req, GetLang());
        return result switch
        {
            UserTypeResult.Success => CreatedAtAction(nameof(GetById), new { id = data!.Id }, ApiResponse.Success(data, message)),
            UserTypeResult.DuplicateName => Conflict(ApiResponse.Fail(message)),
            _ => BadRequest(ApiResponse.Fail(message)),
        };
    }

    /// <summary>
    /// Get a page of user types. Returns active types only — this is what the signup screen
    /// calls; the Dashboard passes includeInactive=true to also get deactivated types.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] PaginationQuery pagination) =>
        Ok(ApiResponse.Success(await _userTypes.GetAllAsync(pagination, GetLang())));

    /// <summary>Get a single user type by id.</summary>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var dto = await _userTypes.GetByIdAsync(id, GetLang());
        if (dto == null) return NotFound(ApiResponse.Fail("User type not found."));
        return Ok(ApiResponse.Success(dto));
    }

    /// <summary>Update a user type.</summary>
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateUserTypeRequest req)
    {
        var (result, message, data) = await _userTypes.UpdateAsync(id, req, GetLang());
        return result switch
        {
            UserTypeResult.Success => Ok(ApiResponse.Success(data, message)),
            UserTypeResult.NotFound => NotFound(ApiResponse.Fail(message)),
            UserTypeResult.DuplicateName => Conflict(ApiResponse.Fail(message)),
            UserTypeResult.SystemType => Conflict(ApiResponse.Fail(message)),
            _ => BadRequest(ApiResponse.Fail(message)),
        };
    }

    /// <summary>
    /// Activate or deactivate a user type (admin use). Deactivating leaves the users already
    /// assigned to the type untouched; it only removes it from signup and from future assignment.
    /// </summary>
    [HttpPatch("{id}/active")]
    public async Task<IActionResult> SetActive(int id, [FromBody] SetUserTypeActiveRequest req)
    {
        var (result, message, data) = await _userTypes.SetActiveAsync(id, req.IsActive, GetLang());
        return result switch
        {
            UserTypeResult.Success => Ok(ApiResponse.Success(data, message)),
            UserTypeResult.NotFound => NotFound(ApiResponse.Fail(message)),
            UserTypeResult.SystemType => Conflict(ApiResponse.Fail(message)),
            _ => BadRequest(ApiResponse.Fail(message)),
        };
    }

    /// <summary>Delete a user type. Types that are in use, and the built-in types, cannot be deleted.</summary>
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var (result, message) = await _userTypes.DeleteAsync(id);
        return result switch
        {
            UserTypeResult.Success => Ok(ApiResponse.Success(null, message)),
            UserTypeResult.NotFound => NotFound(ApiResponse.Fail(message)),
            UserTypeResult.InUse => Conflict(ApiResponse.Fail(message)),
            UserTypeResult.SystemType => Conflict(ApiResponse.Fail(message)),
            _ => BadRequest(ApiResponse.Fail(message)),
        };
    }
}
