using HmDianPing.Web.Data;
using HmDianPing.Web.Dtos;
using HmDianPing.Web.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HmDianPing.Controllers
{
    [ApiController]
    [Route("api/shops")]
    [Authorize]
    public class ShopManagementController : ControllerBase
    {
        private readonly HmDbContext _context;
        private readonly IAuthorizationService _authorizationService;

        public ShopManagementController(HmDbContext context, IAuthorizationService authorizationService)
        {
            _context = context;
            _authorizationService = authorizationService;
        }

        [HttpGet("{id:long}")]
        [Authorize(Roles = RoleConstants.User + "," + RoleConstants.Merchant + "," + RoleConstants.Admin + "," + RoleConstants.SuperAdmin)]
        public async Task<IActionResult> GetById(long id)
        {
            var shop = await _context.Shops.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
            if (shop == null)
            {
                return NotFound();
            }

            return Ok(shop);
        }

        [HttpPut("{id:long}")]
        [Authorize(Policy = PolicyNames.CanManageShops)]
        public async Task<IActionResult> Update(long id, [FromBody] ShopUpdateDto dto)
        {
            var shop = await _context.Shops.FirstOrDefaultAsync(x => x.Id == id);
            if (shop == null)
            {
                return NotFound();
            }

            var authResult = await _authorizationService.AuthorizeAsync(User, shop, PolicyNames.CanEditShopResource);
            if (!authResult.Succeeded)
            {
                return Forbid();
            }

            shop.Name = dto.Name;
            shop.Area = dto.Area;
            shop.Address = dto.Address;
            shop.Score = dto.Score;
            shop.AvgPrice = dto.AvgPrice;
            shop.BusinessHours = dto.BusinessHours;
            shop.Phone = dto.Phone;
            shop.ReviewSummary = dto.ReviewSummary;
            shop.UpdateTime = DateTime.Now;

            await _context.SaveChangesAsync();
            return Ok(shop);
        }
    }
}
