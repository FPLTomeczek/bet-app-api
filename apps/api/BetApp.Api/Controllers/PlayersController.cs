using BetApp.Api.Data;
using BetApp.Api.Dtos;
using BetApp.Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BetApp.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PlayersController : ControllerBase
{
    private readonly BetAppContext _context;

    public PlayersController(BetAppContext context)
    {
        _context = context;
    }

    [HttpGet]
    [ProducesResponseType<IEnumerable<PlayerResponse>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<PlayerResponse>>> GetAll()
    {
        var players = await _context.Players
            .AsNoTracking()
            .Select(p => new PlayerResponse(p.Id, p.Name, p.TeamId, p.Nationality, p.Position, p.BirthDate))
            .ToListAsync();

        return Ok(players);
    }

    [HttpGet("{id:int}")]
    [ProducesResponseType<PlayerResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PlayerResponse>> GetById(int id)
    {
        var player = await _context.Players.FindAsync(id);

        if (player is null)
            return NotFound();

        return Ok(new PlayerResponse(player.Id, player.Name, player.TeamId, player.Nationality, player.Position, player.BirthDate));
    }

    [HttpPost]
    [ProducesResponseType<PlayerResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<PlayerResponse>> Create(CreatePlayerRequest request)
    {
        if (request.TeamId is int teamId && !await _context.Teams.AnyAsync(t => t.Id == teamId))
        {
            ModelState.AddModelError(nameof(request.TeamId), ErrorCodes.TeamNotFound);
            return ValidationProblem(ModelState);
        }

        var player = new Player
        {
            Name = request.Name,
            TeamId = request.TeamId,
            Nationality = request.Nationality,
            Position = request.Position,
            BirthDate = request.BirthDate
        };

        _context.Players.Add(player);
        await _context.SaveChangesAsync();

        var response = new PlayerResponse(player.Id, player.Name, player.TeamId, player.Nationality, player.Position, player.BirthDate);
        return CreatedAtAction(nameof(GetById), new { id = player.Id }, response);
    }

    [HttpPut("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(int id, UpdatePlayerRequest request)
    {
        var player = await _context.Players.FindAsync(id);

        if (player is null)
            return NotFound();

        if (request.TeamId is int teamId && !await _context.Teams.AnyAsync(t => t.Id == teamId))
        {
            ModelState.AddModelError(nameof(request.TeamId), ErrorCodes.TeamNotFound);
            return ValidationProblem(ModelState);
        }

        player.Name = request.Name;
        player.TeamId = request.TeamId;
        player.Nationality = request.Nationality;
        player.Position = request.Position;
        player.BirthDate = request.BirthDate;
        await _context.SaveChangesAsync();

        return NoContent();
    }

    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id)
    {
        var player = await _context.Players.FindAsync(id);

        if (player is null)
            return NotFound();

        _context.Players.Remove(player);
        await _context.SaveChangesAsync();

        return NoContent();
    }
}
