using System.Linq.Expressions;
using BetApp.Api.Data;
using BetApp.Api.Dtos;
using BetApp.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace BetApp.Api.Services;

/// <summary>
/// Domain logic for the coupon aggregate. A coupon is placed together with its
/// selections in a single operation; its financial figures (total odds, potential
/// payout) are computed here, not supplied by the client. This class knows nothing
/// about HTTP — it takes/returns DTOs and reports failures via <see cref="Result{T}"/>,
/// so the controller stays a thin translator between HTTP and the domain.
/// </summary>
public class CouponService
{
    private readonly BetAppContext _context;

    public CouponService(BetAppContext context)
    {
        _context = context;
    }

    public async Task<List<CouponResponse>> GetAllAsync()
    {
        return await _context.Coupons
            .AsNoTracking()
            .Select(MapToResponse)
            .ToListAsync();
    }

    public async Task<CouponResponse?> GetByIdAsync(int id)
    {
        return await _context.Coupons
            .AsNoTracking()
            .Where(c => c.Id == id)
            .Select(MapToResponse)
            .FirstOrDefaultAsync();
    }

    public async Task<Result<CouponResponse>> PlaceCouponAsync(CreateCouponRequest request)
    {
        // --- Referential validation (does everything the coupon points at exist?) ---
        var errors = new List<ValidationError>();

        if (!await _context.AppUsers.AnyAsync(u => u.Id == request.UserId))
            errors.Add(new ValidationError(nameof(request.UserId), ErrorCodes.UserNotFound));

        if (request.BonusId is int bonusId && !await _context.Bonuses.AnyAsync(b => b.Id == bonusId))
            errors.Add(new ValidationError(nameof(request.BonusId), ErrorCodes.BonusNotFound));

        var eventIds = request.Selections.Select(s => s.EventId).Distinct().ToList();
        var existingEvents = await _context.Events.CountAsync(e => eventIds.Contains(e.Id));
        if (existingEvents != eventIds.Count)
            errors.Add(new ValidationError(nameof(request.Selections), ErrorCodes.SelectionEventNotFound));

        if (errors.Count > 0)
            return Result<CouponResponse>.Invalid(errors);

        // --- Business calculation. Delegated to CouponCalculator so the maths can be
        //     tested without a database standing in the way. ---
        var totalOdds = CouponCalculator.CalculateTotalOdds(
            request.Selections.Select(s => s.Odds).ToList());
        var potentialPayout = CouponCalculator.CalculatePotentialPayout(request.Stake, totalOdds);

        var coupon = new Coupon
        {
            UserId = request.UserId,
            BonusId = request.BonusId,
            Stake = request.Stake,
            TotalOdds = totalOdds,
            PotentialPayout = potentialPayout,
            // Status defaults to Placed; PlacedAt is set by the database (now()).
            Selections = request.Selections
                .Select(s => new CouponSelection
                {
                    EventId = s.EventId,
                    Market = s.Market,
                    Selection = s.Selection,
                    Odds = s.Odds
                    // Result defaults to Open.
                })
                .ToList()
        };

        _context.Coupons.Add(coupon);
        await _context.SaveChangesAsync();

        var response = await _context.Coupons
            .AsNoTracking()
            .Where(c => c.Id == coupon.Id)
            .Select(MapToResponse)
            .FirstAsync();

        return Result<CouponResponse>.Success(response);
    }

    // Reused projection so list, detail and create responses stay identical.
    private static readonly Expression<Func<Coupon, CouponResponse>> MapToResponse =
        c => new CouponResponse(
            c.Id,
            c.UserId,
            c.BonusId,
            c.Stake,
            c.TotalOdds,
            c.PotentialPayout,
            c.Status,
            c.PlacedAt,
            c.Selections
                .Select(s => new CouponSelectionResponse(s.Id, s.EventId, s.Market, s.Selection, s.Odds, s.Result))
                .ToList());
}
