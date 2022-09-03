using Collection.Data;
using Collection.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Dynamic;
using System.Security.Claims;
using System.Text.Json;
using System.Collections.Generic;
using System.Web;
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using System.Linq;

namespace Collection.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly UsersDbContext _uDb;
        private readonly CollectionsDbContext _colDb;
        private readonly ItemsDbContext _itDb;
        private readonly CommentsDbContext _comDb;
        private readonly LikesDbContext _liDb;
        private readonly TagsDbContext _tagDb;
        public HomeController(ILogger<HomeController> logger, UsersDbContext uDb, CollectionsDbContext colDb,
            ItemsDbContext itDb, CommentsDbContext comDb, LikesDbContext liDb, TagsDbContext tagDb)
        {
            _colDb = colDb;
            _comDb = comDb;
            _uDb = uDb;
            _itDb = itDb;
            _liDb = liDb;
            _tagDb = tagDb;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            if (User.Identity.IsAuthenticated)
            {
                if (_uDb.Users.Find(User.Identity.Name) == null || _uDb.Users.Find(User.Identity.Name).isBanned)
                {
                    return RedirectToAction("Logout");
                }
            }
                
            List<Tag> tags = new List<Tag>();
            List<string> ttags = new List<string>();

            List<MCollection> collections = new List<MCollection>();
            if (_colDb.Collections.Count() > 0)
            {
                collections = _colDb.Collections.OrderByDescending(x => x.Size).ToList();
                if (collections.Count > 4)
                    collections = collections.GetRange(0, 5);
                else
                    collections = collections.GetRange(0, collections.Count);
            }
            
            if (_tagDb.Tags.Count() > 0)
            {
                tags = _tagDb.Tags.ToList();

                foreach (Tag tag in tags)
                    ttags.Add(tag.Name);
                ttags = ttags.Distinct().ToList();
            }
            

            Item item = null;
            if (_itDb.Items.Count() > 0)
                item = _itDb.Items.OrderBy(x => x.Id).Last();

            dynamic model = new ExpandoObject();
            model.Tags = ttags;
            model.Item = item;
            model.Collections = collections;
            return View(model);
        }

        public IActionResult AddCollection(List<string> collection)
        {
            if (_uDb.Users.Find(User.Identity.Name) == null || _uDb.Users.Find(User.Identity.Name).isBanned)
            {
                return RedirectToAction("Logout");
            }
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

        public async Task<IActionResult> Profile(string id)
        {
            
            string user;
            if (id == null)
            {
                if (User.Identity.IsAuthenticated)
                {
                    if (_uDb.Users.Find(User.Identity.Name) == null || _uDb.Users.Find(User.Identity.Name).isBanned)
                    {
                        return RedirectToAction("Logout");
                    }
                    user = User.Identity.Name;
                    ViewData["admin"] = "ignor";
                }
                else
                    return RedirectToAction("Index");
                
            }
            else
            {
                user = id;
                if (CurUser.isAdmin == true)
                    ViewData["admin"] = "yes";
                else
                    ViewData["admin"] = "no";
                ViewData["button"] = "no";
            }

            TempData["whois"] = user;

            List<MCollection> collections = new List<MCollection>();
            foreach (MCollection collection in _colDb.Collections)
                if (collection.Owner == user)
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
                CurUser.isAdmin = user.isAdmin;
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

        [HttpPost]
        public ActionResult UpdateCollections(string user, string name, string theme,
            string describtion, IFormFile file)
        {
            if (_uDb.Users.Find(User.Identity.Name) == null || _uDb.Users.Find(User.Identity.Name).isBanned)
            {
                return RedirectToAction("Logout");
            }
            //var inputs = new Inputs
            //{
            //    Name1 = input1,
            //    Name2 = input2,
            //    Name3 = input3,
            //    Type1 = radio1,
            //    Type2 = radio2,
            //    Type3 = radio3,
            //};

            //string jsonString = JsonSerializer.Serialize(inputs);
            //MCollection tmp = new MCollection();
            //tmp.Name = name;
            //tmp.Theme = theme;
            //tmp.Description = describtion;
            //tmp.Owner = user;
            //tmp.InputFields = jsonString;
            //if (file != null)
            //{

            // Cloudinary part
            Account account = new Account(
            "dcogivo7d",
            "612618111115367",
            "kuLk0Gipv7SjZQniqG3-yhVA_QQ");

            Cloudinary cloudinary = new Cloudinary(account);

            using var stream = file.OpenReadStream();
            // Here filepath or Stream are required
            var uploadParams = new ImageUploadParams()
            {
                File = new FileDescription(file.FileName, stream)
            };
            var uploadResult = new ImageUploadResult();
            uploadResult = cloudinary.Upload(uploadParams);
            var xD = uploadResult.SecureUrl.AbsoluteUri;
            //    tmp.Image = uploadResult.SecureUrl.AbsoluteUri;
            //}
            //else
            //{
            //    tmp.Image = "";
            //};

            //_colDb.Collections.Add(tmp);
            //_colDb.SaveChanges();

            List<MCollection> collections = new List<MCollection>();
            //foreach (MCollection collection in _colDb.Collections)
            //    if (collection.Owner == user)
            //        collections.Add(collection);
            return PartialView("_CollectionsList", collections);
        }

        public PartialViewResult ShowCollections(string user)
        {
            List<MCollection> collections = new List<MCollection>();
            foreach (MCollection collection in _colDb.Collections)
                if (collection.Owner == user)
                    collections.Add(collection);
            return PartialView("_CollectionsList", collections);
        }
        public ActionResult DeleteCollection(string name, string user /*string admin*/)
        {
            if (_uDb.Users.Find(User.Identity.Name) == null || _uDb.Users.Find(User.Identity.Name).isBanned)
            {
                return RedirectToAction("Logout");
            }
            var tmp = _colDb.Collections.Find(name);
            _colDb.Collections.Remove(tmp);
            _colDb.SaveChanges();
            List<MCollection> collections = new List<MCollection>();
            foreach (MCollection collection in _colDb.Collections)
                if (collection.Owner == user)
                    collections.Add(collection);
            //ViewData["admin"] = admin;
            return PartialView("_CollectionsList", collections);
        }

        public PartialViewResult ShowUsers()
        {
            List<User> users = new List<User>();
            foreach (User user in _uDb.Users)
                users.Add(user);
            return PartialView("_Users", users);
        }
        [Authorize]
        public ActionResult DeleteUser(string name)
        {
            if (_uDb.Users.Find(User.Identity.Name) == null || _uDb.Users.Find(User.Identity.Name).isBanned)
            {
                return RedirectToAction("Logout");
            }

            var tmp = _uDb.Users.Find(name);
            _uDb.Users.Remove(tmp);
            _uDb.SaveChanges();

            List<MCollection> colls = _colDb.Collections.ToList();
            foreach (MCollection col in colls)
            {
                if (col.Owner == name)
                {
                    List<Item> items = _itDb.Items.ToList();
                    foreach (Item item in items)
                    {
                        if (item.Collection == col.Name)
                        {
                            _itDb.Items.Remove(item);
                            _itDb.SaveChanges();
                        }
                    }
                    _colDb.Collections.Remove(col);
                    _colDb.SaveChanges();
                }
            }

            if (User.Identity.Name == name)
            {
                return RedirectToAction("Logout");
            }
            return RedirectToAction("Admin");
        }

        [Authorize]
        public IActionResult BanUser(string name)
        {
            if (_uDb.Users.Find(User.Identity.Name) == null || _uDb.Users.Find(User.Identity.Name).isBanned)
            {
                return RedirectToAction("Logout");
            }

            var tmp = _uDb.Users.Find(name);
            tmp.isBanned = true;
            _uDb.Users.Update(tmp);
            _uDb.SaveChanges();

            if (User.Identity.Name == name)
            {
                return RedirectToAction("Logout");
            }
            return RedirectToAction("Admin");
        }
        [Authorize]
        public ActionResult DebanUser(string name)
        {
            if (_uDb.Users.Find(User.Identity.Name) == null || _uDb.Users.Find(User.Identity.Name).isBanned)
            {
                return RedirectToAction("Logout");
            }

            var tmp = _uDb.Users.Find(name);
            tmp.isBanned = false;
            _uDb.Users.Update(tmp);
            _uDb.SaveChanges();

            return RedirectToAction("Admin");
        }
        [Authorize]
        public ActionResult AdminUser(string name)
        {
            if (_uDb.Users.Find(User.Identity.Name) == null || _uDb.Users.Find(User.Identity.Name).isBanned)
            {
                return RedirectToAction("Logout");
            }

            var tmp = _uDb.Users.Find(name);
            tmp.isAdmin = true;
            _uDb.Users.Update(tmp);
            _uDb.SaveChanges();

            return RedirectToAction("Admin");
        }
        [Authorize]
        public ActionResult DeadminUser(string name)
        {
            if (_uDb.Users.Find(User.Identity.Name) == null || _uDb.Users.Find(User.Identity.Name).isBanned)
            {
                return RedirectToAction("Logout");
            }

            var tmp = _uDb.Users.Find(name);
            tmp.isAdmin = false;
            _uDb.Users.Update(tmp);
            _uDb.SaveChanges();
            CurUser.isAdmin = false;

            return RedirectToAction("Admin");
        }

        public async Task<IActionResult> Search(string id)
        {
            if (User.Identity.IsAuthenticated)
                if (_uDb.Users.Find(User.Identity.Name) == null || _uDb.Users.Find(User.Identity.Name).isBanned)
                {
                    return RedirectToAction("Logout");
                }
            List<Item> items = new List<Item>();
            foreach (Tag tag in _tagDb.Tags)
                if (tag.Name == id)
                    items.Add(_itDb.Items.Find(tag.ItemId));
            items = items.Distinct().ToList();
            return View(items);
        }
  
        public async Task<IActionResult> Admin()
        {
            if (User.Identity.IsAuthenticated)
            {
                if (_uDb.Users.Find(User.Identity.Name) == null || _uDb.Users.Find(User.Identity.Name).isBanned)
                {
                    return RedirectToAction("Logout");
                }
                else
                {
                    ViewData["name"] = User.Identity.Name;
                    List<User> users = new List<User>();
                    foreach (User user in _uDb.Users)
                        users.Add(user);
                    if (_uDb.Users.Find(User.Identity.Name).isAdmin == true)
                        return View(users);
                    else
                    {
                        TempData["Error"] = "You are not an admin!";
                        return RedirectToAction("Index");
                    }
                }
            }
            return RedirectToAction("Index");
        }

        public ActionResult Collections(string id)
        {
            
            if (CurUser.isAdmin)
                ViewData["admin"] = "true";
            else
            {
                if (User.Identity.IsAuthenticated)
                {
                    if (_uDb.Users.Find(User.Identity.Name) == null || _uDb.Users.Find(User.Identity.Name).isBanned)
                    {
                        return RedirectToAction("Logout");
                    }
                    MCollection cl = _colDb.Collections.Find(id);
                    if (cl.Owner == User.Identity.Name)
                        ViewData["admin"] = "true";
                    else
                        ViewData["admin"] = "false";

                }     
                else
                    ViewData["admin"] = "false";

            }
                
            
            List<Item> items = new List<Item>();
            foreach(Item item in _itDb.Items)
                if (item.Collection == id)
                    items.Add(item);
            ViewData["Collection"] = id;
            return View(items);
        }

        public ActionResult ShowItems(string name)
        {
            List<Item> items = new List<Item>();
            foreach (Item item in _itDb.Items)
                if (item.Collection == name)
                    items.Add(item);
            return PartialView("_ItemsList", items);
        }

        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync();
            return RedirectToAction("Index");
        }

        [Authorize]
        public ActionResult CreateItem(string id)
        {
            if (_uDb.Users.Find(User.Identity.Name) == null || _uDb.Users.Find(User.Identity.Name).isBanned)
            {
                return RedirectToAction("Logout");
            }
            ViewData["Collection"] = id;
            //MCollection collection = _colDb.Collections.Find(id);
            //string jsonString = collection.InputFields;
            //Inputs inputs = JsonSerializer.Deserialize<Inputs>(jsonString)!;
            //ViewData["type1"] = inputs.Type1;
            //ViewData["type2"] = inputs.Type2;
            //ViewData["type3"] = inputs.Type3;
            //ViewData["name1"] = inputs.Name1;
            //ViewData["name2"] = inputs.Name2;
            //ViewData["name3"] = inputs.Name3;
            return View();
        }
        [HttpPost]
        public ActionResult CreateItem(Item item, string collection, string[] options,
            string name1, string name2, string name3)
        {
            //var itoptions = new InputsItem
            //{
            //    Name1 = name1,
            //    Name2 = name2,
            //    Name3 = name3,
            //    Value1 = options[0],
            //    Value2 = options[1],
            //    Value3 = options[2],
            //};
            //string jsonString = JsonSerializer.Serialize(itoptions);
            //item.Options = jsonString;
            
            item.Collection = collection;
            if (item.Collection != null && item.Tags != null && item.Name != null)
            {
                var col = _colDb.Collections.Find(collection);
                col.Size += 1;
                _colDb.Collections.Update(col);
                _colDb.SaveChanges();
                _itDb.Items.Add(item);
                _itDb.SaveChanges();
                TempData["Success"] = "Item created successfully!";
                string[] tags = item.Tags.Split(", ");
                foreach (string tmp in tags)
                {
                    Tag tag = new Tag();
                    tag.Name = tmp;
                    tag.ItemId = item.Id;
                    _tagDb.Tags.Add(tag);
                    _tagDb.SaveChanges();
                }
                return RedirectToAction("Collections", new { id = collection });
            }
            
            return View();
        }

        public ActionResult EditItem(int id)
        {
            if (_uDb.Users.Find(User.Identity.Name) == null || _uDb.Users.Find(User.Identity.Name).isBanned)
            {
                return RedirectToAction("Logout");
            }
            var item = _itDb.Items.Find(id);
            ViewData["Collection"] = item.Collection;
            return View(item);
        }

        [HttpPost]
        public ActionResult EditItem(Item item, string collection)
        {
            if (item.Collection != null && item.Tags != null && item.Name != null)
            {
                _itDb.Items.Update(item);
                _itDb.SaveChanges();
                TempData["Success"] = "Item edited successfully!";
                List<Tag> temp = _tagDb.Tags.ToList();
                foreach(Tag tag in temp)
                    if (tag.ItemId == item.Id)
                    {
                        _tagDb.Tags.Remove(tag);
                        _tagDb.SaveChanges();
                    }

                string[] tags = item.Tags.Split(", ");
                foreach (string tmp in tags)
                {
                    Tag tag = new Tag();
                    tag.Name = tmp;
                    tag.ItemId = item.Id;
                    _tagDb.Tags.Add(tag);
                    _tagDb.SaveChanges();
                }
                return RedirectToAction("Collections", new { id = collection });
                
            }
            return View(item);
        }

        public ActionResult DeleteItem(int id, string collection)
        {
            if (_uDb.Users.Find(User.Identity.Name) == null || _uDb.Users.Find(User.Identity.Name).isBanned)
            {
                return RedirectToAction("Logout");
            }
            var col = _colDb.Collections.Find(collection);
            col.Size -= 1;
            _colDb.Collections.Update(col);
            _colDb.SaveChanges();

            var item = _itDb.Items.Find(id);
            _itDb.Items.Remove(item);
            _itDb.SaveChanges();
            List<Item> items = new List<Item>();
            foreach(Item tmp in _itDb.Items)
                if (tmp.Collection == item.Collection)
                    items.Add(tmp);
            string[] tags = item.Tags.Split(", ");
            foreach (string tmp in tags)
            {
                var tag = _tagDb.Tags.FirstOrDefault(x =>x.Name == tmp && x.ItemId == item.Id);
                _tagDb.Tags.Remove(tag);
                _tagDb.SaveChanges();
            }
            return PartialView("_ItemsList", items);
        }

        public ActionResult Item(int id)
        {
            if (_uDb.Users.Find(User.Identity.Name) == null || _uDb.Users.Find(User.Identity.Name).isBanned)
            {
                return RedirectToAction("Logout");
            }
            if (CurUser.isAdmin)
                ViewData["admin"] = "true";
            else
                ViewData["admin"] = "false";

            var item = _itDb.Items.Find(id);
            List<Comment> comments = new List<Comment>();
            foreach (Comment comment in _comDb.Comments)
                if (comment.ItemId == id)
                    comments.Add(comment);
            dynamic model = new ExpandoObject();
            model.Comments = comments;
            model.Item = item;
            TempData["ItemId"] = id;
            if (_liDb.Likes.FirstOrDefault(l => l.Owner == User.Identity.Name) != null)
                ViewData["liked"] = "yes";

            int numb = 0;
            foreach (Like tmp in _liDb.Likes)
                if (tmp.ItemId == id)
                    numb++;
            ViewData["likes"] = numb;
            return View(model);
        }

        public ActionResult Ascend(string name, string value)
        {
            if (_uDb.Users.Find(User.Identity.Name) == null || _uDb.Users.Find(User.Identity.Name).isBanned)
            {
                return RedirectToAction("Logout");
            }
            List<Item> items = new List<Item>();
            List<Item> tmp = new List<Item>();
            if (value == "id")
                tmp = _itDb.Items.OrderBy(x => x.Id).ToList();
            else
                tmp = _itDb.Items.OrderBy(x => x.Name).ToList();
            foreach (Item item in tmp)
                if (item.Collection == name)
                    items.Add(item);
            return PartialView("_ItemsList", items);
        }

        public ActionResult AscendCol(string value)
        {
            List<MCollection> collections = new List<MCollection>();
            switch (value)
            {
                case "name":
                    collections = _colDb.Collections.OrderBy(x => x.Name).ToList();
                    break;
                case "owner":
                    collections = _colDb.Collections.OrderBy(x => x.Owner).ToList();
                    break;
                case "theme":
                    collections = _colDb.Collections.OrderBy(x => x.Theme).ToList();
                    break;
            }
            return PartialView("_AllCollectionsList", collections);
        }

        public ActionResult Descend(string name, string value)
        {
            if (_uDb.Users.Find(User.Identity.Name) == null || _uDb.Users.Find(User.Identity.Name).isBanned)
            {
                return RedirectToAction("Logout");
            }
            List<Item> items = new List<Item>();
            List<Item> tmp = new List<Item>();
            if (value == "id")
                tmp = _itDb.Items.OrderByDescending(x => x.Id).ToList();
            else
                tmp = _itDb.Items.OrderByDescending(x => x.Name).ToList();
            foreach (Item item in tmp)
                if (item.Collection == name)
                    items.Add(item);
            return PartialView("_ItemsList", items);
        }
        public ActionResult DescendCol(string value)
        {
            List<MCollection> collections = new List<MCollection>();
            switch (value)
            {
                case "name":
                    collections = _colDb.Collections.OrderByDescending(x => x.Name).ToList();
                    break;
                case "owner":
                    collections = _colDb.Collections.OrderByDescending(x => x.Owner).ToList();
                    break;
                case "theme":
                    collections = _colDb.Collections.OrderByDescending(x => x.Theme).ToList();
                    break;
            }
            return PartialView("_AllCollectionsList", collections);
        }

        public ActionResult UpdateComments(int id)
        {
            if (_uDb.Users.Find(User.Identity.Name) == null || _uDb.Users.Find(User.Identity.Name).isBanned)
            {
                return RedirectToAction("Logout");
            }
            List<Comment> comments = new List<Comment>();
            foreach (Comment comment in _comDb.Comments)
                if (comment.ItemId == id)
                    comments.Add(comment);
            dynamic model = new ExpandoObject();
            model.Comments = comments;
            return PartialView("_Comments", model);
        }

        public ActionResult AddComment(int id, string content)
        {
            if (_uDb.Users.Find(User.Identity.Name) == null || _uDb.Users.Find(User.Identity.Name).isBanned)
            {
                return RedirectToAction("Logout");
            }
            Comment tmp = new Comment();
            tmp.Name = User.Identity.Name;
            tmp.ItemId = id;
            tmp.Content = content;
            _comDb.Comments.Add(tmp);
            _comDb.SaveChanges();
            List<Comment> comments = new List<Comment>();
            foreach (Comment comment in _comDb.Comments)
                if (comment.ItemId == id)
                    comments.Add(comment);
            dynamic model = new ExpandoObject();
            model.Comments = comments;
            return PartialView("_Comments", model);
        }

        public ActionResult AllCollections()
        {
            List<MCollection> collections = new List<MCollection>();
            foreach (MCollection col in _colDb.Collections)
                collections.Add(col);
            return View(collections);
        }
        public int AddLike(int id)
        {
            Like like = new Like();
            like.ItemId = id;
            like.Owner = User.Identity.Name;
            if (_liDb.Likes.FirstOrDefault(l => l.Owner == User.Identity.Name) == null)
            {
                _liDb.Likes.Add(like);
                _liDb.SaveChanges();
            }

            int numb = 0;
            foreach (Like tmp in _liDb.Likes)
                if (tmp.ItemId == id)
                    numb++;
            return numb;
        }

        public int RemoveLike(int id)
        {
            var like = _liDb.Likes.FirstOrDefault(x => x.ItemId == id);
            _liDb.Likes.Remove(like);
            _liDb.SaveChanges();
            int numb = 0;
            foreach (Like tmp in _liDb.Likes)
                if (tmp.ItemId == id)
                    numb++;
            return numb;
        }

        [HttpPost]
        public ActionResult SearchItem(string itname)
        {
            var item = _itDb.Items.FirstOrDefault(acc => acc.Name == itname);
            return RedirectToAction("Item", new { id = item.Id });
        }
        public JsonResult GetSearchValue(string search)
        {
            List<string> allsearch = _itDb.Items.Where(x => x.Name.Contains(search)).Select(x => new string(x.Name))
                .ToList();
            var unique = allsearch.Distinct().ToList();
            return Json(unique);
        }
        public JsonResult GetTagsValue(string search)
        {
            List<string> allsearch = _tagDb.Tags.Where(x => x.Name.Contains(search)).Select(x => new string(x.Name))
                .ToList();
            var unique = allsearch.Distinct().ToList();
            return Json(unique);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}