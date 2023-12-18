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


        // Action for adding new messages NOT WORKING don't know why!?!?!?!??!?!
        [HttpPost]
        [Authorize(Roles = "User,Editor,Admin")]
        public IActionResult Show([FromForm] Message message)
        {
            message.SentDate = DateTime.Now;
            message.UserId = _userManager.GetUserId(User);

            //debug
            var existingChannelHasCategory = db.ChannelHasCategories
                .FirstOrDefault(chc => chc.ChannelId == message.ChannelId && chc.CategoryId == message.CategoryId && chc.Id == message.ChannelHasCategoryId);

            if (existingChannelHasCategory == null)
            {
                // Handle the case where the provided ChannelHasCategoryId does not exist
                ViewBag.Alert = TempData["messageType"] = "alert-danger";
                ViewBag.Message = TempData["message"] = "Nu exisata FK";


                return Redirect(Url.Action("Show", "Category", new { channelId = message.ChannelId, categoryId = message.CategoryId }));

            }
            //debug


            if (ModelState.IsValid)
            {
                System.Diagnostics.Debug.WriteLine($"Message.ChannelId: {message.ChannelId}");
                System.Diagnostics.Debug.WriteLine($"Message.CategoryId: {message.CategoryId}");
                System.Diagnostics.Debug.WriteLine($"Message.ChannelHasCatId: {message.ChannelHasCategoryId}");
                db.Messages.Add(message);
                db.SaveChanges();
                return Redirect(Url.Action("Show", "Category", new { channelId = message.ChannelId, categoryId = message.CategoryId }));
            }
            else
            {
                
               
                ViewBag.Message = TempData["message"] = message.UserId;
                ViewBag.Alert = TempData["messageType"] = "alert-danger";

                ChannelHasCategory category = db.ChannelHasCategories
                                          .Include("Channel")
                                          .Include("Category")
                                          .Include("Messages")
                                          .Include("Messages.User")
                                          .Where(chc => chc.ChannelId == message.ChannelId && chc.CategoryId == message.CategoryId)
                                          .First();
                return View(category);

            }
        }

        [Authorize(Roles = "User,Moderator,Admin")]
        public IActionResult New(int channelId)
        {
            var cat = new Category();
            Channel ch = db.Channels
                        .Where(c => c.Id == channelId)
                        .First();
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
                bool channelHasIt = false;
                bool catExInTabel = false;
                //Channel with categories
                Channel ch = db.Channels
                                .Include(c => c.ChannelHasCategories)
                                    .ThenInclude(chc => chc.Category)
                                .Where(c => c.Id == channelId)
                                .FirstOrDefault();
                //All categories in DB
                List<Category> allCategories = db.Categories.ToList();

                if (ch != null)
                {
                    foreach (var channelHasCategory in ch.ChannelHasCategories)
                    {
                        Category cat = channelHasCategory.Category;

                        if(cat.Name.ToLower() == rqCategory.Name.ToLower())
                        {
                            channelHasIt = true;
                            break;
                        }
                    }
                    if (channelHasIt)
                    {
                        TempData["message"] = "This category already exists";
                        TempData["messageType"] = "alert-danger";
                        return Redirect(Url.Action("Show", "Channel", new { id = channelId }));
                    }
                    
                    foreach(var cat in allCategories)
                    {
                        if (cat.Name.ToLower() == rqCategory.Name.ToLower())
                        {
                            ChannelHasCategory cHc = new ChannelHasCategory();
                            cHc.CategoryId = cat.Id;
                            cHc.ChannelId = channelId;
                            cHc.AddDate = DateTime.Now;
                            db.ChannelHasCategories.Add(cHc);
                            db.SaveChanges();
                            catExInTabel = true;
                            break;
                        }
                    }
                    if (!catExInTabel)
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
                }
                else
                {
                    TempData["message"] = "There was an error";
                    TempData["messageType"] = "alert-danger";
                    return Redirect(Url.Action("Index", "Channel"));
                }


                TempData["message"] = "Category added successfully";
                TempData["messageType"] = "alert-success";
                return Redirect(Url.Action("Show", "Channel", new { id = channelId }));
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
    }
}
