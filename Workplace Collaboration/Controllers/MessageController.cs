using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Threading.Channels;
using Workplace_Collaboration.Data;
using Workplace_Collaboration.Models;

namespace Workplace_Collaboration.Controllers
{
    public class MessageController : Controller
    {

        private readonly ApplicationDbContext db;

        private readonly UserManager<ApplicationUser> _userManager;

        private readonly RoleManager<IdentityRole> _roleManager;

        public MessageController(
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
        public IActionResult Edit(int channelId, int categoryId, int messageId)
        {
            ChannelHasCategory category = db.ChannelHasCategories
                                          .Include("Channel")
                                          .Include("Channel.Moderators")
                                          .Include("Category")
                                          .Include("Messages")
                                          .Include("Messages.User")
                                          .Where(chc => chc.ChannelId == channelId && chc.CategoryId == categoryId)
                                          .First();
            Message message = db.Messages.Find(messageId);


            ApplicationUser user = db.ApplicationUsers
                                        .Where(u => u.Id == _userManager.GetUserId(User))
                                        .First();
            if(message.User == user)
            {
                if (category.Channel.Moderators.Contains(user) || User.IsInRole("Admin")) ViewBag.isAuthority = true;
                else ViewBag.isAuthority = false;
                ViewBag.currentUser = user;
                ViewBag.messageToEdit = message;

                return View(category);
            }
            else
            {
                TempData["message"] = "You do not have the required permissions to edit this message";
                TempData["messageType"] = "alert-danger";
                return Redirect(Url.Action("Show", "Category", new { channelId = channelId, categoryId = categoryId }));
            }

        }

        [HttpPost]
        [Authorize(Roles = "User,Moderator,Admin")]
        public IActionResult Edit(int messageId, [FromForm] Message rqMessage)
        {
            ApplicationUser user = db.ApplicationUsers
                                        .Where(u => u.Id == _userManager.GetUserId(User))
                                        .First();
            Message message = db.Messages.Find(messageId);

            if (ModelState.IsValid)
            {
                if (message.User == user)
                {
                    message.Content = rqMessage.Content;
                    db.SaveChanges();

                }
                else
                {
                    TempData["message"] = "You do not have the required permissions to edit this message";
                    TempData["messageType"] = "alert-danger";
                }
            }
            else
            {
                TempData["message"] = "Something went wrong";
                TempData["messageType"] = "alert-danger";
            }
            return Redirect(Url.Action("Show", "Category", new { channelId = rqMessage.ChannelId, categoryId = rqMessage.CategoryId }));
        }

        [HttpPost]
        [Authorize(Roles = "User,Moderator,Admin")]
        public ActionResult Delete(int channelId, int categoryId, int messageId)
        {
            Message message = db.Messages.Find(messageId);
            db.Remove(message);
            db.SaveChanges();
            return Redirect(Url.Action("Show", "Category", new { channelId = channelId, categoryId = categoryId }));

        }
    }
}
