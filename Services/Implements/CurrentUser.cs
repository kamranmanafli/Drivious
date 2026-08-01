using Drivious.Constants;
using Drivious.Services.Interfaces;
using System.Security.Claims;

namespace Drivious.Services.Implements
{
    public class CurrentUser : ICurrentUser
    {
        private readonly IHttpContextAccessor _accessor;

        public CurrentUser(IHttpContextAccessor accessor)
        {
            _accessor = accessor;
        }

        private ClaimsPrincipal? Principal => _accessor.HttpContext?.User;

        public string? UserId => Principal?.FindFirstValue(ClaimTypes.NameIdentifier);

        public Guid? DriverId =>
            Guid.TryParse(Principal?.FindFirstValue("driverId"), out var id) ? id : null;

        public bool IsDriverOnly =>
            Principal is not null
            && Principal.IsInRole(AppRoles.Driver)
            && !Principal.IsInRole(AppRoles.Admin)
            && !Principal.IsInRole(AppRoles.Manager);
    }
}
