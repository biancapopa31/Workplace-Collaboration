using System.ComponentModel.DataAnnotations.Schema;

namespace Workplace_Collaboration.Models
{
    public class ChannelHasCategory
    {

        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public int? CategoryId { get; set; }
        public int? ChannelId { get; set; }
        public DateTime? AddDate { get; set; }
        public virtual Category? Category { get; set; }
        public virtual Channel? Channel { get; set; }
        public virtual ICollection<Message>? Messages { get; set; }

    }
}
