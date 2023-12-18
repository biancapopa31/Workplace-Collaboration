using System.ComponentModel.DataAnnotations;

namespace Workplace_Collaboration.Models
{
    public class Message
    {
        [Key]
        public int Id { get; set; }
        public string? UserId { get; set; }
        public string Content { get; set; }
        public int? ChannelHasCategoryId { get; set; }
        public int? CategoryId { get; set; }
        public int? ChannelId { get; set; }

        public DateTime? SentDate { get; set; }

        public virtual ApplicationUser? User { get; set; }
        public virtual ChannelHasCategory? ChannelHasCategory { get; set; }
    }
}
