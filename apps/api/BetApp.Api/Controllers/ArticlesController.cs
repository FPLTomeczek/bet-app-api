using BetApp.Api.Data;
using BetApp.Api.Dtos;
using BetApp.Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BetApp.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ArticlesController : ControllerBase
{
    private readonly BetAppContext _context;

    public ArticlesController(BetAppContext context)
    {
        _context = context;
    }

    [HttpGet]
    [ProducesResponseType<IEnumerable<ArticleResponse>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<ArticleResponse>>> GetAll()
    {
        var articles = await _context.Articles
            .AsNoTracking()
            .Select(a => new ArticleResponse(a.Id, a.Title, a.Content, a.Author, a.SportCategoryId, a.PublishedAt))
            .ToListAsync();

        return Ok(articles);
    }

    [HttpGet("{id:int}")]
    [ProducesResponseType<ArticleResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ArticleResponse>> GetById(int id)
    {
        var article = await _context.Articles.FindAsync(id);

        if (article is null)
            return NotFound();

        return Ok(new ArticleResponse(article.Id, article.Title, article.Content, article.Author, article.SportCategoryId, article.PublishedAt));
    }

    [HttpPost]
    [ProducesResponseType<ArticleResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ArticleResponse>> Create(CreateArticleRequest request)
    {
        if (request.SportCategoryId is int categoryId && !await _context.SportCategories.AnyAsync(c => c.Id == categoryId))
        {
            ModelState.AddModelError(nameof(request.SportCategoryId), ErrorCodes.SportCategoryNotFound);
            return ValidationProblem(ModelState);
        }

        var article = new Article
        {
            Title = request.Title,
            Content = request.Content,
            Author = request.Author,
            SportCategoryId = request.SportCategoryId,
            PublishedAt = request.PublishedAt
        };

        _context.Articles.Add(article);
        await _context.SaveChangesAsync();

        var response = new ArticleResponse(article.Id, article.Title, article.Content, article.Author, article.SportCategoryId, article.PublishedAt);
        return CreatedAtAction(nameof(GetById), new { id = article.Id }, response);
    }

    [HttpPut("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(int id, UpdateArticleRequest request)
    {
        var article = await _context.Articles.FindAsync(id);

        if (article is null)
            return NotFound();

        if (request.SportCategoryId is int categoryId && !await _context.SportCategories.AnyAsync(c => c.Id == categoryId))
        {
            ModelState.AddModelError(nameof(request.SportCategoryId), ErrorCodes.SportCategoryNotFound);
            return ValidationProblem(ModelState);
        }

        article.Title = request.Title;
        article.Content = request.Content;
        article.Author = request.Author;
        article.SportCategoryId = request.SportCategoryId;
        article.PublishedAt = request.PublishedAt;
        await _context.SaveChangesAsync();

        return NoContent();
    }

    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id)
    {
        var article = await _context.Articles.FindAsync(id);

        if (article is null)
            return NotFound();

        _context.Articles.Remove(article);
        await _context.SaveChangesAsync();

        return NoContent();
    }
}
