using Ganss.Xss;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using System.Threading.Channels;
using Workplace_Collaboration.Data;
using Workplace_Collaboration.Models;
using Channel = Workplace_Collaboration.Models.Channel;

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

        //Checker method to see whether an user is part of a bigger list of users
        //Useful in determining if user is a moderator of a certain channel
        [NonAction]
        public bool isUserinList(ICollection<ApplicationUser> users, string user_id)
        {
            foreach (ApplicationUser member in users)
                if (member.Id == user_id) return true;
            return false;
        }

        // Method to see if a channel has a category
        // used in New and EditCategory
        [NonAction]
        public bool isCategoryInChannel(int channelId, Category category)
        {
            //Channel with categories
            Channel ch = db.Channels
                            .Include(c => c.ChannelHasCategories)
                                .ThenInclude(chc => chc.Category)
                            .Where(c => c.Id == channelId)
                            .FirstOrDefault();


            foreach (var channelHasCategory in ch.ChannelHasCategories)
            {
                Category cat = channelHasCategory.Category;

                if (cat.Name.ToLower() == category.Name.ToLower())
                {
                    return true;
                }
            }
            return false;
        }

        //Method to see if a category exists in DB
        //if successful returns category.id else -1
        //used in New and EditCategory
        [NonAction]
        public int findCategoryInDB(Category category)
        {
            //All categories in DB
            List<Category> allCategories = db.Categories.ToList();

            foreach (var cat in allCategories)
            {
                if (cat.Name.ToLower() == category.Name.ToLower())
                {
                    return cat.Id;
                }
            }
            return -1;
        }
        [NonAction]
        public IActionResult Index()
        {
            return View();
        }

        [Authorize(Roles = "User,Moderator,Admin")]
        public IActionResult Show(int channelId, int categoryId)
        {
            ChannelHasCategory category = db.ChannelHasCategories
                                          .Include("Channel")
                                          .Include("Channel.Moderators")
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
            if (category.Channel.Moderators.Contains(user) || User.IsInRole("Admin")) ViewBag.isAuthority = true;
            else ViewBag.isAuthority = false;
            ViewBag.currentUser = user;

            return View(category);
        }


        // Action for adding new messages
        [HttpPost]
        [Authorize(Roles = "User,Moderator,Admin")]
        public IActionResult Show([FromForm] Message message)
        {
            var sanitizer = new HtmlSanitizer();
            if (ModelState.IsValid)
            {
                message.SentDate = DateTime.Now;
                message.UserId = _userManager.GetUserId(User);
                
                //message.Content = sanitizer.Sanitize(message.Content); // nu merge pentru video
                db.Messages.Add(message);
                db.SaveChanges();
                return Redirect(Url.Action("Show", "Category", new { channelId = message.ChannelId, categoryId = message.CategoryId }));
            }
            else
            {

                ViewBag.Message = TempData["message"] = "Something went wrong";
                ViewBag.Alert = TempData["messageType"] = "alert-danger";

                return Redirect(Url.Action("Show", "Category", new { channelId = message.ChannelId, categoryId = message.CategoryId }));
            }
        }

        [Authorize(Roles = "User,Moderator,Admin")]
        public IActionResult New(int channelId)
        {
            var cat = new Category();
            Channel ch = db.Channels
                        .Where(c => c.Id == channelId)
                        .First();

            if (TempData.ContainsKey("message"))
            {
                ViewBag.Message = TempData["message"];
                ViewBag.Alert = TempData["messageType"];
            }

            ViewBag.ChannelId = channelId;
            ViewBag.ChannelName = ch.Name;

            return View(cat);
        }
        

        [Authorize(Roles = "User,Moderator,Admin")]
        [HttpPost]
        public IActionResult New(int channelId, Category rqCategory)
        {
            if (ModelState.IsValid)
            {
                
                if (isCategoryInChannel(channelId, rqCategory))
                {
                    TempData["message"] = "This category already exists";
                    TempData["messageType"] = "alert-danger";
                    return Redirect(Url.Action("EditCategoriesFromChannel", "Category", new { channelId = channelId }));

                }
                int rqCategoryIndex = findCategoryInDB(rqCategory);
                if (rqCategoryIndex != -1)
                {
                    ChannelHasCategory cHc = new ChannelHasCategory();
                    cHc.CategoryId = rqCategoryIndex;
                    cHc.ChannelId = channelId;
                    cHc.AddDate = DateTime.Now;
                    db.ChannelHasCategories.Add(cHc);
                    db.SaveChanges();
                }
                else
                {
                    Category newCategory = new Category();
                    newCategory.Name = rqCategory.Name.ToLower();
                    db.Categories.Add(newCategory);
                    db.SaveChanges();

                    ChannelHasCategory cHc = new ChannelHasCategory();
                    cHc.CategoryId = newCategory.Id;
                    cHc.ChannelId = channelId;
                    cHc.AddDate = DateTime.Now;
                    db.ChannelHasCategories.Add(cHc);
                    db.SaveChanges();
                }


                TempData["message"] = "Category added successfully";
                TempData["messageType"] = "alert-success";
                return Redirect(Url.Action("EditCategoriesFromChannel", "Category", new { channelId = channelId }));
            }
            else
            {
                Channel ch = db.Channels
                       .Where(c => c.Id == channelId)
                       .First();
                ViewBag.ChannelId = channelId;
                ViewBag.ChannelName = ch.Name;
                return View(rqCategory);
            }

        }
        [Authorize(Roles = "User,Moderator,Admin")]
        public IActionResult EditCategoriesFromChannel(int channelId)
        {
            ApplicationUser user = db.ApplicationUsers.Where(u => u.Id == _userManager.GetUserId(User))
                                     .First();
            Channel ch = db.Channels.Include("Users")
                                    .Include("Moderators")
                                    .Include("ChannelHasCategories.Category")
                                    .Where(ch => ch.Id == channelId)
                                    .First();
            ViewBag.ChannelName = ch.Name;

            if (TempData.ContainsKey("message"))
            {
                ViewBag.Message = TempData["message"];
                ViewBag.Alert = TempData["messageType"];
            }

            if (ch.Moderators.Contains(user) || User.IsInRole("Admin"))
            {
                return View(ch);
            }
            else
            {
                TempData["message"] = "You do not have the required permissions to make alterations to this channel";
                TempData["messageType"] = "alert-danger";
                return Redirect(Url.Action("Show", "Channel", new { id = channelId }));
            }
        }

        [Authorize(Roles = "User,Moderator,Admin")]
        public IActionResult EditCategory(int channelId, int categoryId)
        {
            ApplicationUser user = db.ApplicationUsers.Where(u => u.Id == _userManager.GetUserId(User))
                                      .First();

            ChannelHasCategory cHc = db.ChannelHasCategories
                                        .Include("Category")
                                        .Include("Channel.Moderators")
                                        .Where(c => c.ChannelId == channelId && c.CategoryId == categoryId)
                                        .First();
            ViewBag.ChannelId = channelId;
            if (TempData.ContainsKey("message"))
            {
                ViewBag.Message = TempData["message"];
                ViewBag.Alert = TempData["messageType"];
            }

            if (cHc.Channel.Moderators.Contains(user) || User.IsInRole("Admin"))
            {
                return View(cHc.Category);
            }
            else
            {
                TempData["message"] = "You do not have the required permissions to make alterations to this channel";
                TempData["messageType"] = "alert-danger";
                return Redirect(Url.Action("Show", "Channel", new { id = channelId }));

            }

        }

        [Authorize(Roles = "User,Moderator,Admin")]
        [HttpPost]
        public IActionResult EditCategory(int channelId, int categoryId, Category rqCategory)
        {
            ApplicationUser user = db.ApplicationUsers.Where(u => u.Id == _userManager.GetUserId(User))
                                      .First();

            ChannelHasCategory cHc = db.ChannelHasCategories
                                        .Include("Category")
                                        .Include("Messages")
                                        .Include("Channel.Moderators")
                                        .Where(c => c.ChannelId == channelId && c.CategoryId == categoryId)
                                        .First();
            ViewBag.ChannelId = channelId;
            if (TempData.ContainsKey("message"))
            {
                ViewBag.Message = TempData["message"];
                ViewBag.Alert = TempData["messageType"];
            }

            if (ModelState.IsValid)
            {
                if (cHc.Channel.Moderators.Contains(user) || User.IsInRole("Admin"))
                {
                    if (isCategoryInChannel(channelId, rqCategory))
                    {
                        TempData["message"] = "This category already exists";
                        TempData["messageType"] = "alert-danger";
                        return Redirect(Url.Action("EditCategory", "Category", new { channelId = channelId, categoryId = categoryId }));

                    }

                    int rqCategoryIndex = findCategoryInDB(rqCategory);
                    if (rqCategoryIndex != -1)
                    {
                        ChannelHasCategory newcHc = new ChannelHasCategory();
                        newcHc.CategoryId = rqCategoryIndex;
                        newcHc.ChannelId = channelId;
                        newcHc.AddDate = DateTime.Now;

                        db.ChannelHasCategories.Add(newcHc);
                        db.SaveChanges();

                        //Move messages from old Category to new one
                        foreach (var msg in cHc.Messages)
                        {
                            msg.ChannelId = newcHc.CategoryId;
                            msg.CategoryId = newcHc.ChannelId;
                            msg.ChannelHasCategoryId = newcHc.Id;
                        }
                        newcHc.Messages = cHc.Messages;
                        
                        db.Remove(cHc);
                        db.SaveChanges();

                    }
                    else
                    {
                        Category newCategory = new Category();
                        newCategory.Name = rqCategory.Name.ToLower();
                        db.Categories.Add(newCategory);
                        db.SaveChanges();

                        ChannelHasCategory newcHc = new ChannelHasCategory();
                        newcHc.CategoryId = newCategory.Id;
                        newcHc.ChannelId = channelId;
                        newcHc.AddDate = DateTime.Now;

                        db.ChannelHasCategories.Add(newcHc);
                        db.SaveChanges();

                        //Move messages from old Category to new one
                        foreach (var msg in cHc.Messages)
                        {
                            msg.ChannelId = newcHc.CategoryId;
                            msg.CategoryId = newcHc.ChannelId;
                            msg.ChannelHasCategoryId = newcHc.Id;
                        }
                        newcHc.Messages = cHc.Messages;

                        db.Remove(cHc);
                        db.SaveChanges();

                    }


                    TempData["message"] = "Category modified successfully";
                    TempData["messageType"] = "alert-success";
                    return Redirect(Url.Action("EditCategoriesFromChannel", "Category", new { channelId = channelId }));


                }
                else
                {
                    TempData["message"] = "You do not have the required permissions to make alterations to this channel";
                    TempData["messageType"] = "alert-danger";
                    return Redirect(Url.Action("Show", "Channel", new { id = channelId }));

                }
            }
            else 
            { 
                return View(cHc.Category); 
            }
            

        }

        [HttpPost]
        [Authorize(Roles = "User,Moderator,Admin")]
        public ActionResult Delete(int channelId, int categoryId)
        {
            Channel ch = db.Channels.Include("Users")
                                         .Include("Moderators")
                                         .Where(ch => ch.Id == channelId)
                                         .First();
            //If user is moderator or admin, then deletion can proceed
            if (isUserinList(ch.Moderators, _userManager.GetUserId(User)) || User.IsInRole("Admin"))
            {
                ChannelHasCategory cHc =db.ChannelHasCategories
                                            .Include("Messages")
                                            .Where(chc => chc.ChannelId == channelId && chc.CategoryId == categoryId)
                                            .First();
                //Delete the messeges from category
                foreach(var msg in cHc.Messages)
                {
                    db.Messages.Remove(msg);
                }

                db.ChannelHasCategories.Remove(cHc);
                db.SaveChanges();
                TempData["message"] = "Category has been deleted";
                TempData["messageType"] = "alert-success";
            }
            //Otherwise turn down deletion request and return to Index page
            else
            {
                TempData["message"] = "You do not have the required permissions to delete this category";
                TempData["messageType"] = "alert-danger";
            }
            return Redirect(Url.Action("EditCategoriesFromChannel", "Category", new { channelId = channelId }));

        }
    }

}
