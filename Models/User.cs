using Microsoft.AspNetCore.Identity;
using NET_TASK.Models;

namespace NET_TASK.Models
{
    public class User : IdentityUser
    {
        public ICollection<Catalog>? Catalogs { get; set; }
    }
}