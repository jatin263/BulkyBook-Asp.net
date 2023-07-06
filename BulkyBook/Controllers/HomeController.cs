using BulkyBook.Data;
using BulkyBook.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Web;
using Microsoft.EntityFrameworkCore;
using RestSharp;

namespace BulkyBook.Controllers
{
    public class HomeController : Controller
    {
        private readonly IHttpContextAccessor _contextAccessor;
        public static int Uid = 0;
        public static string Name="";
        private static int BookIdf = 0;
        private readonly ApplicationDbContext _db;

        public HomeController( ApplicationDbContext db, IHttpContextAccessor contextAccessor)
        {
            _db = db;
            _contextAccessor = contextAccessor;
        }

        public IActionResult Index()
        {
            if (TempData.ContainsKey("msg"))
            {
                ViewBag.msg = TempData["msg"].ToString();
            }
            if (Name != "")
            {
                ViewBag.Name = Name;
            }
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
            if (Name != "")
            {
                ViewBag.Name = Name;
            }
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


        public IActionResult RecentOrder()
        {
            if (Name != "")
            {
                ViewBag.Name = Name;
            }
            if (Uid == 0)
            {
                return RedirectToAction("Login");
            }
            RecentOrderPage rop = new RecentOrderPage();
            rop.Books = _db.Books.FromSqlRaw("Select * from Books").ToList();
            rop.Orders = _db.Orders.FromSqlRaw("Select * from Orders where UserModel="+Uid).ToList();
            return View(rop);

        }
        public IActionResult AddBook()
        {
            return View();
        }
        public IActionResult OrderNow(int id)
        {
            if (Uid == 0)
            {
                return RedirectToAction("Login");
            }
            if (Name != "")
            {
                ViewBag.Name = Name;
            }
            BookIdf = id;
            OrderPageModel opm = new OrderPageModel();
            opm.Books=_db.Books.FromSqlRaw("Select * from Books where id="+id).ToList();
            opm.Carts = null;
            return View(opm);
        }

        public IActionResult Order()
        {
            if (Uid == 0)
            {
                return RedirectToAction("Login");
            }
            if (Name != "")
            {
                ViewBag.Name = Name;
            }
            BookIdf = 0;
            OrderPageModel opm = new OrderPageModel();
            opm.Books=_db.Books.FromSqlRaw("Select * from Books").ToList() ;
            opm.Carts=_db.Carts.FromSqlRaw("Select * from Carts where UserModel="+Uid).ToList();
            return View(opm);
        }
        [HttpPost]
        public IActionResult SetOrder(int[] qunatityOfBook,string UserAddress,string UserPostalCode,string UserState,string UserCity)
        {
            if (Uid == 0)
            {
                return RedirectToAction("Login");
            }
            if (Name != "")
            {
                ViewBag.Name = Name;
            }
            if (BookIdf != 0)
            {
                OrderModel om = new OrderModel();
                BookModel bm = _db.Books.Find(BookIdf);
                int priceOfBook = bm.Price;
                om.Price = priceOfBook * qunatityOfBook[0];
                om.Quantity = qunatityOfBook[0];
                om.User = _db.Users.Find(Uid);
                om.Book = bm;
                if (UserAddress != null)
                {
                    om.Address = UserAddress;
                    om.City = UserCity;
                    om.State=UserState;
                    om.PostalCode=UserPostalCode;
                }
                else
                {
                    om.Address = om.User.Address;
                    om.City= om.User.City;
                    om.State = om.User.State;
                    om.PostalCode = om.User.PostalCode;
                }
                om.OrderStatus = 1;
                _db.Orders.Add(om);
                _db.SaveChanges();
            }
            else
            {
                List<BookModel> bmo = _db.Books.ToList();
               List<CartModel> cart = _db.Carts.FromSqlRaw("Select * from Carts where UserModel="+Uid).ToList();
                for (int i = 0; i < qunatityOfBook.Length; i++)
                {
                    OrderModel om = new OrderModel();
                    om.Book = cart[i].Book;
                    om.Quantity= qunatityOfBook[i];
                    om.Price = cart[i].Book.Price;
                    om.User = _db.Users.Find(Uid);
                    if (UserAddress != null)
                    {
                        om.Address = UserAddress;
                        om.Address = UserAddress;
                        om.City = UserCity;
                        om.State = UserState;
                        om.PostalCode = UserPostalCode;
                    }
                    else
                    {
                        om.Address = om.User.Address;
                        om.City = om.User.City;
                        om.State = om.User.State;
                        om.PostalCode = om.User.PostalCode;
                    }
                    om.OrderStatus = 1;
                    _db.Orders.Add(om);
                    _db.Carts.Where(a => a.Id == cart[i].Id).ExecuteDelete();
                    _db.SaveChanges();
                }
                _db.SaveChanges();
            }
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
                    Name= u.Name;
                    TempData["msg"] = "Login Successfull";
                    return RedirectToAction("Index");
                }
            }
            TempData["msg"] = "Wrong Username or Password";
            ViewBag.msg = TempData["msg"].ToString();
            return View();
        }

        public IActionResult Login()
        {
            if (TempData.ContainsKey("msg"))
            {
                ViewBag.msg = TempData["msg"].ToString();
            }
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
            if (Uid == 0)
            {
                return RedirectToAction("Login");
            }
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
            
            if (Uid == 0)
            {
                return RedirectToAction("Login");
            }
            if (Name != "")
            {
                ViewBag.Name = Name;
            }
            BuyModel d = new BuyModel();
            var h = _db.Books.Find(id);
            string category = h.Category;
            category = category.Remove(0,1);
            category=category.Remove(category.Length-1,1);
            List<string> res = category?.Split(',').ToList();
            BookIdf = id;
            h.NoOfDownloads++;
            _db.Books.Update(h);
            _db.SaveChanges();
            d.Book = h;
            List<BookModel> books = new List<BookModel>();
            foreach(string s in res)
            {
                string f=s.Remove(0,2);
                f=f.Remove(f.Length-2,2);
                string sqll = "Select * from books where category like '%" + f + "%'";
                List<BookModel> g = _db.Books.FromSqlRaw(sqll).ToList();
                foreach(BookModel b in g)
                {
                    if(!books.Contains(b)) books.Add(b);
                }
            }
            
            d.Books = books;
            List<CartModel> gg=_db.Carts.FromSqlRaw("Select * from carts where UserModel="+Uid).ToList();
            List<int> ids = new List<int>();
            foreach(var c in gg)
            {
                ids.Add(c.Book.Id);
            }
            d.inCart = ids;
            return View(d);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}