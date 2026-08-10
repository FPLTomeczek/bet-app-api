using BetApp.Api.Data;
using BetApp.Api.Dtos;
using BetApp.Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BetApp.Api.Controllers;

// Append-only ledger: transactions can be read and created, but never updated or
// deleted through the API. Corrections happen by posting a compensating entry,
// which preserves the audit trail.
[ApiController]
[Route("api/[controller]")]
public class TransactionsController : ControllerBase
{
    private readonly BetAppContext _context;

    public TransactionsController(BetAppContext context)
    {
        _context = context;
    }

    [HttpGet]
    [ProducesResponseType<IEnumerable<TransactionResponse>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<TransactionResponse>>> GetAll()
    {
        var transactions = await _context.Transactions
            .AsNoTracking()
            .Select(t => new TransactionResponse(t.Id, t.UserId, t.Type, t.Amount, t.Status, t.Method, t.CreatedAt))
            .ToListAsync();

        return Ok(transactions);
    }

    [HttpGet("{id:int}")]
    [ProducesResponseType<TransactionResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<TransactionResponse>> GetById(int id)
    {
        var transaction = await _context.Transactions.FindAsync(id);

        if (transaction is null)
            return NotFound();

        return Ok(new TransactionResponse(transaction.Id, transaction.UserId, transaction.Type, transaction.Amount, transaction.Status, transaction.Method, transaction.CreatedAt));
    }

    [HttpPost]
    [ProducesResponseType<TransactionResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<TransactionResponse>> Create(CreateTransactionRequest request)
    {
        if (!await _context.AppUsers.AnyAsync(u => u.Id == request.UserId))
        {
            ModelState.AddModelError(nameof(request.UserId), ErrorCodes.UserNotFound);
            return ValidationProblem(ModelState);
        }

        var transaction = new Transaction
        {
            UserId = request.UserId,
            Type = request.Type,
            Amount = request.Amount,
            Method = request.Method
            // Status defaults to Pending; CreatedAt is set by the database (now()).
            // NOTE: reconciling the user's balance on completion is domain logic
            // that belongs here in a real implementation.
        };

        _context.Transactions.Add(transaction);
        await _context.SaveChangesAsync();

        var response = new TransactionResponse(transaction.Id, transaction.UserId, transaction.Type, transaction.Amount, transaction.Status, transaction.Method, transaction.CreatedAt);
        return CreatedAtAction(nameof(GetById), new { id = transaction.Id }, response);
    }
}
