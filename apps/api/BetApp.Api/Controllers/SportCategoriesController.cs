using BetApp.Api.Data;
using BetApp.Api.Dtos;
using BetApp.Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BetApp.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SportCategoriesController : ControllerBase
{
    private readonly BetAppContext _context;

    public SportCategoriesController(BetAppContext context)
    {
        _context = context;
    }

    [HttpGet]
    [ProducesResponseType<IEnumerable<SportCategoryResponse>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<SportCategoryResponse>>> GetAll()
    {
        var categories = await _context.SportCategories
            .AsNoTracking()
            .Select(c => new SportCategoryResponse(c.Id, c.Name))
            .ToListAsync();

        return Ok(categories);
    }

    [HttpGet("{id:int}")]
    [ProducesResponseType<SportCategoryResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<SportCategoryResponse>> GetById(int id)
    {
        var category = await _context.SportCategories.FindAsync(id);

        if (category is null)
            return NotFound();

        return Ok(new SportCategoryResponse(category.Id, category.Name));
    }

    [HttpPost]
    [ProducesResponseType<SportCategoryResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<SportCategoryResponse>> Create(CreateSportCategoryRequest request)
    {
        // Name has a unique index — pre-check to return a clean 400 instead of
        // letting the DB constraint surface as a raw 500.
        if (await _context.SportCategories.AnyAsync(c => c.Name == request.Name))
        {
            ModelState.AddModelError(nameof(request.Name), ErrorCodes.CategoryNameTaken);
            return ValidationProblem(ModelState);
        }

        var category = new SportCategory { Name = request.Name };

        _context.SportCategories.Add(category);
        await _context.SaveChangesAsync();

        var response = new SportCategoryResponse(category.Id, category.Name);

        return CreatedAtAction(nameof(GetById), new { id = category.Id }, response);
    }

    [HttpPut("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(int id, UpdateSportCategoryRequest request)
    {
        var category = await _context.SportCategories.FindAsync(id);

        if (category is null)
            return NotFound();

        if (await _context.SportCategories.AnyAsync(c => c.Name == request.Name && c.Id != id))
        {
            ModelState.AddModelError(nameof(request.Name), ErrorCodes.CategoryNameTaken);
            return ValidationProblem(ModelState);
        }

        category.Name = request.Name;
        await _context.SaveChangesAsync();

        return NoContent();
    }

    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id)
    {
        var category = await _context.SportCategories.FindAsync(id);

        if (category is null)
            return NotFound();

        _context.SportCategories.Remove(category);
        await _context.SaveChangesAsync();

        return NoContent();
    }
}
