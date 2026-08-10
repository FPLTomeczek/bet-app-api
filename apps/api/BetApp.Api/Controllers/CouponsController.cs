using BetApp.Api.Dtos;
using BetApp.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace BetApp.Api.Controllers;

// A coupon is the aggregate root: placed together with its selections in one request,
// with its financial figures computed server-side — hence no standalone selection
// controller. PUT/DELETE are intentionally omitted: a placed coupon changes only
// through settlement, a dedicated state transition rather than a free-form edit.
[ApiController]
[Route("api/[controller]")]
public class CouponsController : ControllerBase
{
    private readonly CouponService _couponService;

    public CouponsController(CouponService couponService)
    {
        _couponService = couponService;
    }

    [HttpGet]
    [ProducesResponseType<IEnumerable<CouponResponse>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<CouponResponse>>> GetAll()
    {
        return Ok(await _couponService.GetAllAsync());
    }

    [HttpGet("{id:int}")]
    [ProducesResponseType<CouponResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CouponResponse>> GetById(int id)
    {
        var coupon = await _couponService.GetByIdAsync(id);

        if (coupon is null)
            return NotFound();

        return Ok(coupon);
    }

    [HttpPost]
    [ProducesResponseType<CouponResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<CouponResponse>> Create(CreateCouponRequest request)
    {
        var result = await _couponService.PlaceCouponAsync(request);

        if (!result.IsSuccess)
        {
            foreach (var error in result.Errors)
                ModelState.AddModelError(error.Field, error.Code);

            return ValidationProblem(ModelState);
        }

        return CreatedAtAction(nameof(GetById), new { id = result.Value!.Id }, result.Value);
    }
}
