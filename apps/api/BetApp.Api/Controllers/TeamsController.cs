using BetApp.Api.Data;
using BetApp.Api.Dtos;
using BetApp.Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BetApp.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TeamsController : ControllerBase
{
    private readonly BetAppContext _context;

    public TeamsController(BetAppContext context)
    {
        _context = context;
    }

    [HttpGet]
    [ProducesResponseType<IEnumerable<TeamResponse>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<TeamResponse>>> GetAll()
    {
        var teams = await _context.Teams
            .AsNoTracking()
            .Select(t => new TeamResponse(t.Id, t.Name, t.SportCategoryId, t.Country, t.ImageUrl))
            .ToListAsync();

        return Ok(teams);
    }

    [HttpGet("{id:int}")]
    [ProducesResponseType<TeamResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<TeamResponse>> GetById(int id)
    {
        var team = await _context.Teams.FindAsync(id);

        if (team is null)
            return NotFound();

        return Ok(new TeamResponse(team.Id, team.Name, team.SportCategoryId, team.Country, team.ImageUrl));
    }

    [HttpPost]
    [ProducesResponseType<TeamResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<TeamResponse>> Create(CreateTeamRequest request)
    {
        if (!await _context.SportCategories.AnyAsync(c => c.Id == request.SportCategoryId))
        {
            ModelState.AddModelError(nameof(request.SportCategoryId), ErrorCodes.SportCategoryNotFound);
            return ValidationProblem(ModelState);
        }

        var team = new Team
        {
            Name = request.Name,
            SportCategoryId = request.SportCategoryId,
            Country = request.Country,
            ImageUrl = request.ImageUrl
        };

        _context.Teams.Add(team);
        await _context.SaveChangesAsync();

        var response = new TeamResponse(team.Id, team.Name, team.SportCategoryId, team.Country, team.ImageUrl);
        return CreatedAtAction(nameof(GetById), new { id = team.Id }, response);
    }

    [HttpPut("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(int id, UpdateTeamRequest request)
    {
        var team = await _context.Teams.FindAsync(id);

        if (team is null)
            return NotFound();

        if (!await _context.SportCategories.AnyAsync(c => c.Id == request.SportCategoryId))
        {
            ModelState.AddModelError(nameof(request.SportCategoryId), ErrorCodes.SportCategoryNotFound);
            return ValidationProblem(ModelState);
        }

        team.Name = request.Name;
        team.SportCategoryId = request.SportCategoryId;
        team.Country = request.Country;
        team.ImageUrl = request.ImageUrl;
        await _context.SaveChangesAsync();

        return NoContent();
    }

    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id)
    {
        var team = await _context.Teams.FindAsync(id);

        if (team is null)
            return NotFound();

        _context.Teams.Remove(team);
        await _context.SaveChangesAsync();

        return NoContent();
    }
}
