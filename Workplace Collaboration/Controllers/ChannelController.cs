﻿using Humanizer;
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
            var channels = db.Channels;

            ViewBag.Channels = channels;

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
                                         .Where(ch => ch.Id == id)
                                         .First();
            if (TempData.ContainsKey("message"))
            {
                ViewBag.Message = TempData["message"];
                ViewBag.Alert = TempData["messageType"];
            }

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
                //IMPORTANT: Can't add user to Mods and Users, needs fixing
                if (user == null) { System.Diagnostics.Debug.WriteLine("va"); }
                if (ch.Moderators == null) ch.Moderators = new Collection<ApplicationUser>();
                ch.Moderators.Add(user);
                if (ch.Users == null) ch.Users = new Collection<ApplicationUser>();
                ch.Users.Add(user);
                if (user.IsModerator == null) user.IsModerator = new Collection<Channel>();
                user.IsModerator.Add(ch);
                if (user.Channels == null) user.Channels = new Collection<Channel>();
                user.Channels.Add(ch);
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
            //This is commented because the instance at line 147 doesn't work
            //Other two calls worked fine
            //Same error as for the New insert
            //foreach (ApplicationUser member in users)
                //if (member.Id == user_id) return true;
            return false;
        }
        //Method to initiate editing of channel with a check for permissions
        [Authorize(Roles = "Moderator,Admin")]
        public IActionResult Edit(int id)
        {

            Channel ch = db.Channels.Include("Users")
                                          .Include("Moderators")
                                          .Where(ch => ch.Id == id)
                                          .First();
            if (isUserinList(ch.Moderators, _userManager.GetUserId(User)) || User.IsInRole("Admin"))
            //if (User.IsInRole("Admin"))
            {
                return View(ch);
            }
            else
            {
                TempData["message"] = "You do not have the required permissions to make alterations to this channel";
                TempData["messageType"] = "alert-danger";
                return RedirectToAction("Index");
            }

        }
        //Updating the database with data received from edit form
        [HttpPost]
        [Authorize(Roles = "Moderator,Admin")]
        public IActionResult Edit(int id, Channel requestChannel)
        {
            Channel ch = db.Channels.Find(id);
            
            if (ModelState.IsValid)
            {
                if (isUserinList(requestChannel.Moderators, _userManager.GetUserId(User)) || User.IsInRole("Admin"))
                //if (User.IsInRole("Admin"))
                {
                    ch.Moderators = requestChannel.Moderators;
                    ch.Users = requestChannel.Users;
                    ch.Name = requestChannel.Name;
                    ch.Description = requestChannel.Description;
                    TempData["message"] = "Channel modified successfully";
                    TempData["messageType"] = "alert-success";
                    db.SaveChanges();
                    return RedirectToAction("Index");
                }
                else
                {
                    TempData["message"] = "You do not have the required permissions to make alterations to this channel";
                    TempData["messageType"] = "alert-danger";
                    return RedirectToAction("Index");
                }
            }
            else
            {
                return View(requestChannel);
            }
        }
        //Method for the deletion of a channel using its id (used only through the interface)
        [HttpPost]
        [Authorize(Roles = "Moderator,Admin")]
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
            //Otherwise turn down deletion request and return to Index page
            else
            {
                TempData["message"] = "You do not have the required permissions to delete this channel";
                TempData["messageType"] = "alert-danger";
                return RedirectToAction("Index");
            }
        }
    }
}