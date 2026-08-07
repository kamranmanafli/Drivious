using Drivious.Constants;
using Drivious.Data;
using Drivious.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Drivious.Services.Implements
{
    public class CurrentUser : ICurrentUser
    {
        private readonly IHttpContextAccessor _accessor;
        private readonly AppDbContext _context;

        private bool _driverIdRead;
        private Guid? _driverId;

        public CurrentUser(IHttpContextAccessor accessor, AppDbContext context)
        {
            _accessor = accessor;
            _context = context;
        }

        private ClaimsPrincipal? Principal => _accessor.HttpContext?.User;

        public string? UserId => Principal?.FindFirstValue(ClaimTypes.NameIdentifier);

        /// <summary>
        /// Read from the account rather than from the access token. An administrator
        /// may link an account to a driver at any moment, but a token already issued
        /// keeps the claim it was born with until it expires. Trusting that claim
        /// left a freshly linked driver looking at an empty screen - the account read
        /// as linked, while every query for their data still matched nothing.
        /// Resolved at most once per request: this service is registered scoped.
        /// </summary>
        public Guid? DriverId
        {
            get
            {
                if (_driverIdRead)
                {
                    return _driverId;
                }

                var userId = UserId;

                _driverId = userId == null
                    ? null
                    : _context.Users
                        .AsNoTracking()
                        .Where(x => x.Id == userId)
                        .Select(x => x.DriverId)
                        .FirstOrDefault();

                _driverIdRead = true;

                return _driverId;
            }
        }

        public bool IsDriverOnly =>
            Principal is not null
            && Principal.IsInRole(AppRoles.Driver)
            && !Principal.IsInRole(AppRoles.Admin)
            && !Principal.IsInRole(AppRoles.Manager);
    }
}
