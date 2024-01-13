using System.ComponentModel.DataAnnotations;

namespace Workplace_Collaboration.Models
{
    public class Channel
    {
        [Key]
        public int Id { get; set; }
        [Required(ErrorMessage = "Name is required")]
        [StringLength(100, ErrorMessage = "Name can't have more than 100 characters")]
        [MinLength(3, ErrorMessage = "Name should have at least 3 characters")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Name is required")]
        public string Description { get; set; }

        public DateTime? CreationDate { get; set; }

        public virtual ICollection<ApplicationUser>? Users { get; set; }
        public virtual ICollection<ApplicationUser>? Moderators { get; set; }
        public virtual ICollection<ApplicationUser>? Requester { get; set; }

        //Realtia many-to-many
        public virtual ICollection<ChannelHasCategory>? ChannelHasCategories { get; set; }

    }
}
