using BulkyBook.Data;
using BulkyBook.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using Microsoft.EntityFrameworkCore;

namespace BulkyBook.Controllers
{
    public class HomeController : Controller
    {
        public static int Uid = 0;
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDbContext _db;

        public HomeController(ILogger<HomeController> logger, ApplicationDbContext db)
        {
            _db = db;
            _logger = logger;
        }

        public IActionResult Index()
        {
            IndexPageModel ipm = new IndexPageModel();
            ipm.AllBook = _db.Books.FromSqlRaw("Select * From Books order by noofdownloads desc").ToList();
            string sql = "Select * from carts where UserModel = " + Uid;
            ipm.Cart = _db.Carts.FromSqlRaw(sql).ToList();
            List<int> c = new List<int>
            {
                -1
            };
            ipm.SearchBook = c;
            return View(ipm);
        }

        [HttpPost]
        public IActionResult Index(string q)
        {
            IndexPageModel ipm = new IndexPageModel();
            ipm.Cart = _db.Carts.Where(d => d.User.Id == Uid).ToList();
            string sql = "Select * from books where category like '%" + q + "%' or title like '%" + q + "%' or author like '%" + q + "%' order by noofdownloads desc";
            List<BookModel> b = _db.Books.FromSqlRaw(sql).ToList();
            List<int> c = new List<int>();
            for(int i = 0; i < b.Count; i++)
            {
                c.Add(b[i].Id);
            }
            ipm.SearchBook = c;
            ipm.AllBook = _db.Books.FromSqlRaw("Select * From Books order by noofdownloads desc").ToList();
            return View(ipm);
        }

        public IActionResult AddBook()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Login(string uEmail, string uPassword)
        {
            UserModel u = _db.Users.Where(g => g.Email == uEmail).FirstOrDefault();
            if (u != null)
            {
                if (u.Password == uPassword)
                {
                    Uid = u.Id;
                    return RedirectToAction("Index");
                }
            }
            return View();
        }

        public IActionResult Login()
        {
            return View();
        }


        [HttpPost]
        public IActionResult Register(UserModel u)
        {
            _db.Users.Add(u);
            _db.SaveChanges();
            return RedirectToAction("Login");
        }
        public IActionResult Register()
        {
            return View();
        }


        [HttpPost]
        public IActionResult InsertBook(BookModel b)
        {
            b.NoOfDownloads = 0;
            _db.Books.Add(b);
            _db.SaveChanges();
            return RedirectToAction("Index");
        }

        private List<CartModel> GetCartData()
        {
            string sql1 = "Select * from carts where UserModel = " + Uid;
            List<CartModel> ans = _db.Carts.FromSqlRaw(sql1).ToList();
            return ans;
        }

        public IActionResult Privacy()
        {
            return View();
        }


        public IActionResult Cart()
        {
            return View();
        }
        public IActionResult AddCart(int id)
        {
            if (Uid == 0)
            {
                return RedirectToAction("Login");
            }
            CartModel c = new CartModel();
            c.User = _db.Users.Find(Uid);
            c.Book = _db.Books.Find(id);
            c.Quantity = 1;
            _db.Carts.Add(c);
            _db.SaveChanges(true);
            return RedirectToAction("Index");
        }


        

        public IActionResult Buy(int id)
        {
            BuyModel d = new BuyModel();
            var h = _db.Books.Find(id);
            h.NoOfDownloads++;
            _db.Books.Update(h);
            _db.SaveChanges();
            d.Book = h;
            var g = _db.Books.FromSqlRaw("Select * from books where category={0} and id != {1}", h.Category, id).ToList();
            d.Books = g;
            return View(d);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}