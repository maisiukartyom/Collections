using Collection.Data;
using Collection.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Security.Claims;

namespace Collection.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly UsersDbContext _uDb;
        private readonly CollectionsDbContext _colDb;

        public HomeController(ILogger<HomeController> logger, UsersDbContext uDb, CollectionsDbContext colDb)
        {
            _colDb = colDb;
            _uDb = uDb;
            _logger = logger;
        }

        [Authorize]
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult AddCollection(List<string> collection)
        {
            MCollection tmp = new MCollection();
            tmp.Name = collection[0];
            tmp.Theme = collection[1];
            tmp.Description = collection[2];
            tmp.Owner = User.Identity.Name;
            tmp.Image = "";
            _colDb.Collections.Add(tmp);
            _colDb.SaveChanges();
            return View("Profile");
        }
        [Authorize]
        public IActionResult Profile()
        {
            List<MCollection> collections = new List<MCollection>();
            foreach (MCollection collection in _colDb.Collections)
                if (collection.Owner == User.Identity.Name)
                    collections.Add(collection);
                
            return View(collections);
        }

        [HttpGet("login")]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost("login")]
        public async Task<IActionResult> Validate(string username, string password, string returnUrl)
        {

            var user = _uDb.Users.Find(username);


            if (user == null)
            {
                TempData["Error"] = "User not found!";
                return RedirectToAction("Login");
            }

            if (user.isBanned)
                return RedirectToAction("Banned");

            if (user.UserName == username && user.Password == password)
            {
                var claims = new List<Claim>();
                claims.Add(new Claim("username", username));
                claims.Add(new Claim(ClaimTypes.NameIdentifier, username));
                claims.Add(new Claim(ClaimTypes.Name, username));
                var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                var claimsPrinciple = new ClaimsPrincipal(claimsIdentity);
                await HttpContext.SignInAsync(claimsPrinciple);
                _uDb.Users.Update(user);
                _uDb.SaveChanges();
                TempData["Success"] = "Successfully logged in!";

                return RedirectToAction("Index");
            }

            return RedirectToAction("Login");

        }

        [HttpPost]
        public IActionResult Signup(User obj)
        {
            if (ModelState.IsValid)
            {
                var tmp = _uDb.Users.Find(obj.UserName);
                if (tmp == null)
                {
                    _uDb.Users.Add(obj);
                    _uDb.SaveChanges();
                    TempData["Success"] = "Registered successfully";
                }
                else
                    TempData["Error"] = "User already exists!";


            }

            return RedirectToAction("Login");
        }

        public PartialViewResult UpdateCollections(string name, string theme, string describtion)
        {
            MCollection tmp = new MCollection();
            tmp.Name = name;
            tmp.Theme = theme;
            tmp.Description = describtion;
            tmp.Owner = User.Identity.Name;
            tmp.Image = "";
            _colDb.Collections.Add(tmp);
            _colDb.SaveChanges();     
            List<MCollection> collections = new List<MCollection>();
            foreach (MCollection collection in _colDb.Collections)
                if (collection.Owner == User.Identity.Name)
                    collections.Add(collection);
            return PartialView("_CollectionsList", collections);
        }

        public PartialViewResult DeleteCollection(string name)
        {
            var tmp = _colDb.Collections.Find(name);
            _colDb.Collections.Remove(tmp);
            _colDb.SaveChanges();
            List<MCollection> collections = new List<MCollection>();
            foreach (MCollection collection in _colDb.Collections)
                if (collection.Owner == User.Identity.Name)
                    collections.Add(collection);
            return PartialView("_CollectionsList", collections);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}