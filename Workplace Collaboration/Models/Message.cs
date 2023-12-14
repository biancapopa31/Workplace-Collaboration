using System.ComponentModel.DataAnnotations;

namespace Workplace_Collaboration.Models
{
    public class Message
    {
        [Key]
        public int Id { get; set; }
        public string UserId { get; set; }
        public string Content { get; set; }
        public int ChannelHasCategoryID { get; set; }
        public int ChannelID { get; set; }
        public int CategoryID { get; set; }


        public DateTime? SentDate { get; set; }

        public virtual ApplicationUser User { get; set; }
        public virtual ChannelHasCategory ChannelHasCategory { get; set; }
    }
}
