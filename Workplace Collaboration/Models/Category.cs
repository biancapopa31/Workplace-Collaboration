using System.ComponentModel.DataAnnotations;

namespace Workplace_Collaboration.Models
{
    public class Category
    {
        [Key]
        public int Id { get; set; }
        [Required(ErrorMessage = "Name is required")]
        public string Name { get; set; }

        //Relatia many-to-many
        public virtual ICollection<ChannelHasCategory>? ChannelHasCategories { get; set; }


    }
}
