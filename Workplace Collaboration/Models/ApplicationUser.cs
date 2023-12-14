using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations.Schema;

namespace Workplace_Collaboration.Models
{
    public class ApplicationUser : IdentityUser
    {
        public virtual ICollection<Message>? Messages { get; set; }
        
        public virtual ICollection<Channel>? Channels { get; set; }
        public virtual ICollection<Channel>? IsModerator { get; set; }


        public string? FirstName { get; set; }
        public string? LastName { get; set; }

        [NotMapped]
        public IEnumerable<SelectListItem>? AllRoles { get; set; }

    }
}
