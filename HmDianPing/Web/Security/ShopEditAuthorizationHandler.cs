using HmDianPing.Web.Models;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace HmDianPing.Web.Security
{
    public class ShopEditAuthorizationHandler : AuthorizationHandler<CanEditShopRequirement, Shop>
    {
        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, CanEditShopRequirement requirement, Shop resource)
        {
            if (context.User.IsInRole(RoleConstants.SuperAdmin) || context.User.IsInRole(RoleConstants.Admin))
            {
                context.Succeed(requirement);
                return Task.CompletedTask;
            }

            if (!context.User.IsInRole(RoleConstants.Merchant))
            {
                return Task.CompletedTask;
            }

            var userId = context.User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (long.TryParse(userId, out var currentUserId) && resource.OwnerUserId == currentUserId)
            {
                context.Succeed(requirement);
            }

            return Task.CompletedTask;
        }
    }
}
