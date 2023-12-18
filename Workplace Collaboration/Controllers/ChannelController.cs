using Humanizer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Workplace_Collaboration.Data;
using Workplace_Collaboration.Models;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Workplace_Collaboration.Controllers
{
    public class ChannelController : Controller
    {
        private readonly ApplicationDbContext db;

        private readonly UserManager<ApplicationUser> _userManager;

        private readonly RoleManager<IdentityRole> _roleManager;

        public ChannelController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager)
        {
            db = context;

            _userManager = userManager;

            _roleManager = roleManager;
        }



        [NonAction]
        ApplicationUser getUser(string id)
        {
            foreach (var user in db.ApplicationUsers)
            {
                if (user.Id == id) return user; 
            }
            return null;
        }



        [Authorize(Roles = "User,Moderator,Admin")]
        public IActionResult Index()
        {
            var channels = db.Channels.Include("Users").Include("Moderators");
            ApplicationUser user = db.ApplicationUsers.Where(u => u.Id == _userManager.GetUserId(User))
                                                      .First();
            ViewBag.Channels = channels;
            ViewBag.User = user;
            if (TempData.ContainsKey("message"))
            {
                ViewBag.Message = TempData["message"];
                ViewBag.Alert = TempData["messageType"];
            }

            return View();
        }



        [Authorize(Roles = "User,Moderator,Admin")]
        public IActionResult Show(int id)
        {
            Channel channel = db.Channels.Include("Users")
                                         .Include("Moderators")
                                         .Include("ChannelHasCategories.Category")
                                         .Where(ch => ch.Id == id)
                                         .First();
            ApplicationUser user = db.ApplicationUsers.Where(u => u.Id == _userManager.GetUserId(User))
              .First();
            if (TempData.ContainsKey("message"))
            {
                ViewBag.Message = TempData["message"];
                ViewBag.Alert = TempData["messageType"];
            }
            if (channel.Moderators.Contains(user) || User.IsInRole("Admin")) ViewBag.isAuthority = true;
            else ViewBag.isAuthority = false;
            return View(channel);
        }



        //Method to access the form destined for creating channels
        [Authorize(Roles = "User,Moderator,Admin")]
        public IActionResult New()
        {
            var ch= new Channel();
            return View(ch);
        }



        //Method for creation of new channels 
        [Authorize(Roles = "User,Moderator,Admin")]
        [HttpPost]
        public IActionResult New(Channel ch) 
        {
            if (ModelState.IsValid)
            {
                //If successful, then add the channel to the database and redirect user to view of created channel
                ApplicationUser user = db.ApplicationUsers.Where(u => u.Id == _userManager.GetUserId(User))
                                                      .First();
                //ApplicationUser user = new ApplicationUser();
                //user = getUser(_userManager.GetUserId(User));
                if (user == null) { System.Diagnostics.Debug.WriteLine("va"); }
                if (ch.Moderators == null) ch.Moderators = new Collection<ApplicationUser>();
                ch.Moderators.Add(user);
                if (ch.Users == null) ch.Users = new Collection<ApplicationUser>();
                ch.Users.Add(user);
                if (user.IsModerator == null) user.IsModerator = new Collection<Channel>();
                //user.IsModerator.Add(ch);
                if (user.Channels == null) user.Channels = new Collection<Channel>();
                //user.Channels.Add(ch);
                db.Channels.Add(ch);
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            else 
            {
                //If unsuccessful, pass error message in tempdata and return to channel index 
                TempData["message"] = "Channel creation was unsuccessful";
                return RedirectToAction("Index"); 
            }
        }


        //Checker method to see whether an user is part of a bigger list of users
        //Useful in determining if user is a moderator of a certain channel
        [NonAction]
        public bool isUserinList(ICollection<ApplicationUser>users, string user_id)
        {
            foreach (ApplicationUser member in users)
                if (member.Id == user_id) return true;
            return false;
        }


        //Method to initiate editing of channel with a check for permissions
        [Authorize(Roles = "User,Moderator,Admin")]
        public IActionResult Edit(int id)
        {
            ApplicationUser user = db.ApplicationUsers.Where(u => u.Id == _userManager.GetUserId(User))
                                      .First();
            Channel ch = db.Channels.Include("Users")
                                    .Include("Moderators")
                                    .Include("ChannelHasCategories.Category")
                                    .Where(ch => ch.Id == id)
                                    .First();

            if (ch.Moderators.Contains(user) || User.IsInRole("Admin"))
            //if (User.IsInRole("Admin"))
            {
                return View(ch);
            }
            else
            {
                TempData["message"] = "You do not have the required permissions to make alterations to this channel";
                TempData["messageType"] = "alert-danger";
                return Redirect("/Channel/Show/" + id);
            }

        }


        //Updating the database with data received from edit form
        [HttpPost]
        [Authorize(Roles = "User,Moderator,Admin")]
        public IActionResult Edit(int id, Channel requestChannel)
        {
            Channel ch = db.Channels.Include("Users")
                                          .Include("Moderators")
                                          .Include("ChannelHasCategories.Category")
                                          .Where(ch => ch.Id == id)
                                          .First();
            ApplicationUser user = db.ApplicationUsers.Where(u => u.Id == _userManager.GetUserId(User))
                          .First();
            if (ModelState.IsValid)
            {
                if (ch.Moderators.Contains(user) || User.IsInRole("Admin"))
                //if (User.IsInRole("Admin"))
                {
                    ch.Name = requestChannel.Name;
                    ch.Description = requestChannel.Description;
                    TempData["message"] = "Channel modified successfully";
                    TempData["messageType"] = "alert-success";
                    db.SaveChanges();
                    return Redirect("/Channel/Show/" + id);
                }
                else
                {
                    TempData["message"] = "You do not have the required permissions to make alterations to this channel";
                    TempData["messageType"] = "alert-danger";
                    return Redirect("/Channel/Show/" + id);
                }
            }
            else
            {
                return View(requestChannel);
            }
        }


        //Method for the deletion of a channel using its id (used only through the interface)
        [HttpPost]
        [Authorize(Roles = "User,Moderator,Admin")]
        public ActionResult Delete(int id)
        {
            Channel ch = db.Channels.Include("Users")
                                         .Include("Moderators")
                                         .Where(ch => ch.Id == id)
                                         .First();
            //If user is moderator or admin, then deletion can proceed
            if (isUserinList(ch.Moderators, _userManager.GetUserId(User)) || User.IsInRole("Admin"))
            //if (User.IsInRole("Admin"))
            {
                db.Channels.Remove(ch);
                db.SaveChanges();
                TempData["message"] = "Channel has been deleted";
                TempData["messageType"] = "alert-success";
                return RedirectToAction("Index");
            }
            //Otherwise turn down deletion request and return to Show page
            else
            {
                TempData["message"] = "You do not have the required permissions to delete this channel";
                TempData["messageType"] = "alert-danger";
                return Redirect("/Channel/Show/" + id);
            }
        }


        //Method to show the members of a channel (pretty rudimentary atm)
        [Authorize(Roles = "User,Moderator,Admin")]
        public ActionResult ShowUsers(int id)
        {
            Channel ch = db.Channels.Include("Users")
                             .Include("Moderators")
                             .Where(ch => ch.Id == id)
                             .First();
            ApplicationUser user = db.ApplicationUsers.Where(u => u.Id == _userManager.GetUserId(User))
                                                        .First();
            ViewBag.ChannelUsers = ch.Users;
            if (ch.Moderators.Contains(user)) ViewBag.isModerator = true;
            else ViewBag.isModerator = false;
            ViewBag.Moderators = ch.Moderators;
            return View(ch); 
        }


        //Method that allows user to join a channel that they have not yet joined
        [Authorize(Roles = "User,Moderator,Admin")]
        [HttpPost]
        public ActionResult Join(int id)
        {
            Channel ch = db.Channels.Include("Users")
                 .Include("Moderators")
                 .Where(ch => ch.Id == id)
                 .First();
            ApplicationUser user = db.ApplicationUsers.Where(u => u.Id == _userManager.GetUserId(User))
              .First();
            //Initialise collection of users if not yet done(shouldn't ever happen but jic) 
            if (ch.Users == null) ch.Users = new Collection<ApplicationUser>();
            if (!ch.Users.Contains(user))
            {
                ch.Users.Add(user);
                db.SaveChanges();
                TempData["message"] = "Channel Joined";
                TempData["messageType"] = "alert-success";
                return Redirect("/Channel/Show/" + id);
            }
            else
            {
                TempData["message"] = "Already part of this Channel";
                TempData["messageType"] = "alert-danger";
                return RedirectToAction("Index");
            }
        }


        //Method Allowing user to leave a channel
        //No View, done through Channel Interface
        [Authorize(Roles = "User,Moderator,Admin")]
        [HttpPost]
        public ActionResult Leave(int id) 
        {
            Channel ch = db.Channels.Include("Users")
                                     .Include("Moderators")
                                     .Where(ch => ch.Id == id)
                                     .First();
            ApplicationUser user = db.ApplicationUsers.Where(u => u.Id == _userManager.GetUserId(User))
                                                     .First();
            if (ch.Users == null) ch.Users = new Collection<ApplicationUser>();
            if (ch.Users.Contains(user))
            {
                ch.Users.Remove(user);
                if (ch.Moderators == null) ch.Moderators = new Collection<ApplicationUser>();
                if (ch.Moderators.Contains(user)) ch.Moderators.Remove(user);
                db.SaveChanges();
                TempData["message"] = "Channel Left";
                TempData["messageType"] = "alert-success";
                return RedirectToAction("Index");
            }
            else
            {
                TempData["message"] = "Can't leave a Channel you are not a part of";
                TempData["messageType"] = "alert-danger";
                return RedirectToAction("Index");
            }
        }



        [Authorize(Roles = "User,Moderator,Admin")]
        [HttpPost]
        public ActionResult Remove(int id, string sndid)
        {
            Channel ch = db.Channels.Include("Users")
                                     .Include("Moderators")
                                     .Where(ch => ch.Id == id)
                                     .First();
            ApplicationUser target = db.ApplicationUsers.Where(u => u.Id == sndid)
                                                     .First();
            ApplicationUser user = db.ApplicationUsers.Where(u => u.Id == _userManager.GetUserId(User))
                                                     .First();
            if(!ch.Moderators.Contains(user) && !User.IsInRole("Admin"))
            {
                TempData["message"] = "You do not have the required permissions to remove this user";
                TempData["messageType"] = "alert-danger";
                return Redirect("/Channel/ShowUsers/" + id);
            }
            if(ch.Moderators.Contains(target) && !User.IsInRole("Admin"))
            {
                TempData["message"] = "You do not have the required permissions to remove this user";
                TempData["messageType"] = "alert-danger";
                return Redirect("/Channel/ShowUsers/" + id);
            }
            if (sndid== _userManager.GetUserId(User))
            {
                TempData["message"] = "Can't Remove Yourself. Try Leaving the Channel Instead";
                TempData["messageType"] = "alert-danger";
                return Redirect("/Channel/ShowUsers/" + id);
            }
            if (ch.Users == null) ch.Users = new Collection<ApplicationUser>();
            if (ch.Users.Contains(target))
            {
                ch.Users.Remove(target);
                if (ch.Moderators == null) ch.Moderators = new Collection<ApplicationUser>();
                if (ch.Moderators.Contains(target)) ch.Moderators.Remove(target);
                db.SaveChanges();
                TempData["message"] = "User Successfully Removed";
                TempData["messageType"] = "alert-success";
                return Redirect("/Channel/ShowUsers/" + id);
            }
            else
            {
                TempData["message"] = "Can't remove this User";
                TempData["messageType"] = "alert-danger";
                return Redirect("/Channel/ShowUsers/" + id);
            }
        }


        [Authorize(Roles = "User,Moderator,Admin")]
        [HttpPost]
        public ActionResult Promote(int id, string sndid) 
        {
            Channel ch = db.Channels.Include("Users")
                         .Include("Moderators")
                         .Where(ch => ch.Id == id)
                         .First();
            ApplicationUser target = db.ApplicationUsers.Where(u => u.Id == sndid)
                                                     .First();
            ApplicationUser user = db.ApplicationUsers.Where(u => u.Id == _userManager.GetUserId(User))
                                                     .First();
            if (!ch.Moderators.Contains(user) && !User.IsInRole("Admin"))
            {
                TempData["message"] = "You do not have the required permissions to promote this user";
                TempData["messageType"] = "alert-danger";
                return Redirect("/Channel/ShowUsers/" + id);
            }
            if (ch.Moderators.Contains(target))
            {
                TempData["message"] = "You cannot promote this user any further";
                TempData["messageType"] = "alert-danger";
                return Redirect("/Channel/ShowUsers/" + id);
            }
            if (sndid == _userManager.GetUserId(User))
            {
                TempData["message"] = "No room to promote yourself any further";
                TempData["messageType"] = "alert-danger";
                return Redirect("/Channel/ShowUsers/" + id);
            }
            if (ch.Users == null) ch.Users = new Collection<ApplicationUser>();
            if (ch.Users.Contains(target))
            {
                if (ch.Moderators == null) ch.Moderators = new Collection<ApplicationUser>();
                if (!ch.Moderators.Contains(target)) ch.Moderators.Add(target);
                db.SaveChanges();
                TempData["message"] = "User Successfully Promoted";
                TempData["messageType"] = "alert-success";
                return Redirect("/Channel/ShowUsers/" + id);
            }
            else
            {
                TempData["message"] = "This User isn't part of this Channel";
                TempData["messageType"] = "alert-danger";
                return Redirect("/Channel/ShowUsers/" + id);
            }

        }


        [Authorize(Roles = "Admin")]
        [HttpPost]
        [HttpPost]
        public ActionResult Demote(int id, string sndid)
        {
            Channel ch = db.Channels.Include("Users")
                         .Include("Moderators")
                         .Where(ch => ch.Id == id)
                         .First();
            ApplicationUser target = db.ApplicationUsers.Where(u => u.Id == sndid)
                                                     .First();
            ApplicationUser user = db.ApplicationUsers.Where(u => u.Id == _userManager.GetUserId(User))
                                                     .First();
            if (!User.IsInRole("Admin"))
            {
                TempData["message"] = "You do not have the required permissions to demote this user";
                TempData["messageType"] = "alert-danger";
                return Redirect("/Channel/ShowUsers/" + id);
            }
            if (!ch.Moderators.Contains(target))
            {
                TempData["message"] = "You cannot demote this user any further";
                TempData["messageType"] = "alert-danger";
                return Redirect("/Channel/ShowUsers/" + id);
            }
            if (sndid == _userManager.GetUserId(User))
            {
                TempData["message"] = "Why Would You Demote Yourself?";
                TempData["messageType"] = "alert-danger";
                return Redirect("/Channel/ShowUsers/" + id);
            }
            ch.Moderators.Remove(target);
            db.SaveChanges();
            TempData["message"] = "User Successfully Demoted";
            TempData["messageType"] = "alert-success";
            return Redirect("/Channel/ShowUsers/" + id);
        }

    }
}
