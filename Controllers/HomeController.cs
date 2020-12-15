using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using WeddingPlanner.Context;
using WeddingPlanner.Models;

namespace WeddingPlanner.Controllers
{
    public class HomeController : Controller
    {
        private MyContext _context;

        public User LoggedInUser()
        {
            int? LoggedID = HttpContext.Session.GetInt32("LoggedInUser");
            User logged = _context.Users.FirstOrDefault(u => u.UserId == LoggedID);
            return logged;
        }

        public int UserID()
        {
            int UserID = LoggedInUser().UserId;
            return UserID;
        }
        public HomeController(MyContext context)
        {
            _context = context;
        }
        public void DeleteOld(Wedding OldW)
        {

            if (OldW.WeddingDate < DateTime.Now)
            {
                Console.WriteLine($"{OldW.WedderOne}'s Wedding was Deleted");
                _context.Remove(OldW);
                _context.SaveChanges();
            }

        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpPost("register")]
        public IActionResult Register(User reg)
        {
            //These two lines will hash our password for us.
            if (ModelState.IsValid)
            {
                if (_context.Users.Any(u => u.Email == reg.Email))
                {
                    ModelState.AddModelError("Email", "Email already in use!");
                    return View("Index");
                }
                else
                {
                    PasswordHasher<User> Hasher = new PasswordHasher<User>();
                    reg.Password = Hasher.HashPassword(reg, reg.Password);
                    _context.Users.Add(reg);
                    _context.SaveChanges();
                    var userInDb = _context.Users.FirstOrDefault(u => u.Email == reg.Email);
                    HttpContext.Session.SetInt32("LoggedInUser", userInDb.UserId);
                    return RedirectToAction("Dashboard");
                }
            }
            else
            {
                return View("Index");
            }
        }
        [HttpPost("login")]
        public IActionResult Login(LoginUser log)
        {

            if (ModelState.IsValid)
            {
                // If inital ModelState is valid, query for a user with provided email
                var userInDb = _context.Users.FirstOrDefault(u => u.Email == log.LoginEmail);
                // If no user exists with provided email
                if (userInDb == null)
                {
                    // Add an error to ModelState and return to View!
                    ModelState.AddModelError("Email", "Invalid Email/Password");
                    return View("Index");
                }

                // Initialize hasher object
                var hasher = new PasswordHasher<LoginUser>();

                // verify provided password against hash stored in db
                var result = hasher.VerifyHashedPassword(log, userInDb.Password, log.LoginPassword);

                // result can be compared to 0 for failure
                if (result == 0)
                {
                    ModelState.AddModelError("Email/Password", "Invalid Email/Password");
                    return View("Index");
                    // handle failure (this should be similar to how "existing email" is handled)
                }
                else
                {
                    HttpContext.Session.SetInt32("LoggedInUser", userInDb.UserId);
                    return RedirectToAction("Dashboard");
                }
            }
            else
            {
                return View("Index");
            }
        }
        [HttpGet("dashboard")]
        public IActionResult Dashboard()
        {
            if (LoggedInUser() != null)
            {
                ViewBag.LoggedInUser = LoggedInUser();
                List<Wedding> Weddings = _context.Weddings
                                            .Include(r => r.Guests)
                                            .ThenInclude(g => g.User)
                                            .Include(p => p.Planner)
                                            .ToList();
                foreach(Wedding w in Weddings)
                {
                    DeleteOld(w);
                }
                return View(Weddings);
            }
            else
            {
                return RedirectToAction("Index");
            }

        }

        [HttpGet("logout")]

        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Index");
        }
        [HttpGet("new/wedding")]
        public IActionResult NewWedding()
        {
            ViewBag.LoggedInUser = LoggedInUser();
            return View();
        }
        [HttpPost("create/wedding")]
        public IActionResult CreateWedding(Wedding newW)
        {
            if (ModelState.IsValid)
            {
                _context.Add(newW);
                _context.SaveChanges();


                return RedirectToAction("Dashboard");
            }
            else
            {
                ViewBag.LoggedInUser = LoggedInUser();
                return View("NewWedding");
            }
        }
        [HttpGet("rsvp/{UID}/{WID}")]
        public IActionResult RSVP(int UID, int WID)
        {
            RSVP rsvp = new RSVP();
            rsvp.UserId = UID;
            rsvp.WeddingId = WID;
            _context.Add(rsvp);
            _context.SaveChanges();
            return RedirectToAction("Dashboard");
        }
        [HttpGet("drsvp/{UID}/{WID}")]
        public IActionResult DRSVP(int UID, int WID)
        {
            RSVP rsvp = _context.RSVPs.FirstOrDefault(u => u.UserId == UID && u.WeddingId == WID);
            _context.Remove(rsvp);
            _context.SaveChanges();
            return RedirectToAction("Dashboard");
        }
        [HttpGet("delete/wedding/{WID}")]
        public IActionResult Delete(int WID)
        {
            Wedding OneW = _context.Weddings.FirstOrDefault(w => w.WeddingId == WID);
            _context.Remove(OneW);
            _context.SaveChanges();
            return RedirectToAction("Dashboard");

        }




















        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
