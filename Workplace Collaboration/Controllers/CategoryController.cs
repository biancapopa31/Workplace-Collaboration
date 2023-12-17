using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Threading.Channels;
using Workplace_Collaboration.Data;
using Workplace_Collaboration.Models;

namespace Workplace_Collaboration.Controllers
{
    public class CategoryController : Controller
    {

        private readonly ApplicationDbContext db;

        private readonly UserManager<ApplicationUser> _userManager;

        private readonly RoleManager<IdentityRole> _roleManager;

        public CategoryController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager)
        {
            db = context;

            _userManager = userManager;

            _roleManager = roleManager;
        }
        public IActionResult Index()
        {
            return View();
        }

        [Authorize(Roles = "User,Moderator,Admin")]
        public IActionResult Show(int channelId, int categoryId)
        {
            ChannelHasCategory category = db.ChannelHasCategories
                                          .Include("Channel")
                                          .Include("Category")
                                          .Include("Messages")
                                          .Include("Messages.User")
                                          .Where(chc => chc.ChannelId == channelId && chc.CategoryId == categoryId)
                                          .First();
                                        
                                        
            ApplicationUser user = db.ApplicationUsers
                                        .Where(u => u.Id == _userManager.GetUserId(User))
                                        .First();
            if (TempData.ContainsKey("message"))
            {
                ViewBag.Message = TempData["message"];
                ViewBag.Alert = TempData["messageType"];
            }
          //  if (channel.Moderators.Contains(user) || User.IsInRole("Admin")) ViewBag.isAuthority = true;
          //  else ViewBag.isAuthority = false;

            return View(category);
        }
    }
}
