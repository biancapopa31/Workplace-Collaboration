using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.Threading.Channels;
using Workplace_Collaboration.Models;

namespace Workplace_Collaboration.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }
        public DbSet<ApplicationUser> ApplicationUsers { get; set; }
        public DbSet<Models.Channel> Channels { get; set; }
        public DbSet<Category> Categories { get; set; } 
        public DbSet<Message> Messages { get; set; }
        public DbSet<ChannelHasCategory> ChannelHasCategories { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // M-to-M Channel and Users
            modelBuilder.Entity<ApplicationUser>()
                .HasMany(e => e.Channels)
                .WithMany(e => e.Users)
                .UsingEntity(e => e.ToTable("ChannelHasUsers"));


            modelBuilder.Entity<ApplicationUser>()
                .HasMany(e => e.IsModerator)
                .WithMany(e => e.Moderators)
                .UsingEntity(e => e.ToTable("UserIsModInChannel"));
        
            //M-to-M Channel and Category
            modelBuilder.Entity<ChannelHasCategory>()
                .HasKey(e => new { e.Id, e.CategoryId, e.ChannelId });

            modelBuilder.Entity<ChannelHasCategory>()
                .HasOne(e => e.Channel)
                .WithMany(e => e.ChannelHasCategories)
                .HasForeignKey(e => e.ChannelId);

            modelBuilder.Entity<ChannelHasCategory>()
                .HasOne(e => e.Category)
                .WithMany(e => e.ChannelHasCategories)
                .HasForeignKey(e => e.CategoryId);

            modelBuilder.Entity<ChannelHasCategory>()
                .HasMany(e => e.Messages)
                .WithOne(e => e.ChannelHasCategory)
                .HasPrincipalKey(e => new { e.Id, e.CategoryId, e.ChannelId })
                .HasForeignKey(e => new {e.ChannelHasCategoryId, e.CategoryId, e.ChannelId })
                .IsRequired();
            

            //modelBuilder.Entity<Message>()
            //.HasOne(e => e.ChannelHasCategory)
            //.WithMany(e => e.Messages)
            //.HasForeignKey(e => new { e.ChannelHasCategoryId, e.ChannelId, e.CategoryId });
        }


    }
}