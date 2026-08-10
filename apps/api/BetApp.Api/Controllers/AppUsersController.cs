using BetApp.Api.Data;
using BetApp.Api.Dtos;
using BetApp.Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BetApp.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AppUsersController : ControllerBase
{
    private readonly BetAppContext _context;

    public AppUsersController(BetAppContext context)
    {
        _context = context;
    }

    [HttpGet]
    [ProducesResponseType<IEnumerable<AppUserResponse>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<AppUserResponse>>> GetAll()
    {
        // PasswordHash is deliberately never projected into the response.
        var users = await _context.AppUsers
            .AsNoTracking()
            .Select(u => new AppUserResponse(u.Id, u.Username, u.Email, u.Balance, u.Status, u.CreatedAt))
            .ToListAsync();

        return Ok(users);
    }

    [HttpGet("{id:int}")]
    [ProducesResponseType<AppUserResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<AppUserResponse>> GetById(int id)
    {
        var user = await _context.AppUsers.FindAsync(id);

        if (user is null)
            return NotFound();

        return Ok(new AppUserResponse(user.Id, user.Username, user.Email, user.Balance, user.Status, user.CreatedAt));
    }

    [HttpPost]
    [ProducesResponseType<AppUserResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<AppUserResponse>> Create(CreateAppUserRequest request)
    {
        if (await _context.AppUsers.AnyAsync(u => u.Username == request.Username))
            ModelState.AddModelError(nameof(request.Username), ErrorCodes.UsernameTaken);

        if (await _context.AppUsers.AnyAsync(u => u.Email == request.Email))
            ModelState.AddModelError(nameof(request.Email), ErrorCodes.EmailTaken);

        if (!ModelState.IsValid)
            return ValidationProblem(ModelState);

        var user = new AppUser
        {
            Username = request.Username,
            Email = request.Email,
            // TODO: hash the password with a real hasher (e.g. ASP.NET Identity
            // PasswordHasher or BCrypt). Storing the plaintext here is a placeholder.
            PasswordHash = request.Password
            // Balance defaults to 0 and Status to Active — both server-controlled.
        };

        _context.AppUsers.Add(user);
        await _context.SaveChangesAsync();

        var response = new AppUserResponse(user.Id, user.Username, user.Email, user.Balance, user.Status, user.CreatedAt);
        return CreatedAtAction(nameof(GetById), new { id = user.Id }, response);
    }

    [HttpPut("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(int id, UpdateAppUserRequest request)
    {
        var user = await _context.AppUsers.FindAsync(id);

        if (user is null)
            return NotFound();

        if (await _context.AppUsers.AnyAsync(u => u.Username == request.Username && u.Id != id))
            ModelState.AddModelError(nameof(request.Username), ErrorCodes.UsernameTaken);

        if (await _context.AppUsers.AnyAsync(u => u.Email == request.Email && u.Id != id))
            ModelState.AddModelError(nameof(request.Email), ErrorCodes.EmailTaken);

        if (!ModelState.IsValid)
            return ValidationProblem(ModelState);

        user.Username = request.Username;
        user.Email = request.Email;
        await _context.SaveChangesAsync();

        return NoContent();
    }

    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id)
    {
        var user = await _context.AppUsers.FindAsync(id);

        if (user is null)
            return NotFound();

        _context.AppUsers.Remove(user);
        await _context.SaveChangesAsync();

        return NoContent();
    }
}
