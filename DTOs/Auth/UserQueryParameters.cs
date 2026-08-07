using Drivious.DTOs.Common;

namespace Drivious.DTOs.Auth
{
    public class UserQueryParameters : QueryParameters
    {
        /// <summary>
        /// Matched against the user name and the email address. Inherited from
        /// QueryParameters together with paging and sorting.
        /// </summary>
        public string? Role { get; set; }
    }
}
