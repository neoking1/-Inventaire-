using Microsoft.AspNetCore.Mvc;
using Inventaire.Data;

namespace Inventaire.Controllers
{
    public class AuthController : Controller
    {
        private readonly DbHelper _db;
        public AuthController(DbHelper db) { _db = db; }

        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Login(string login, string mdp)
        {
            var result = _db.ExecuteQuery(
                "SELECT * FROM users WHERE login = @login AND mdp = @mdp",
                new Dictionary<string, object> { { "@login", login }, { "@mdp", mdp } }
            );

            if (result.Rows.Count > 0)
            {
                HttpContext.Session.SetInt32("UserId", Convert.ToInt32(result.Rows[0]["id"]));
                HttpContext.Session.SetString("UserLogin", result.Rows[0]["login"].ToString()!);
                HttpContext.Session.SetString("UserType", result.Rows[0]["type"].ToString()!);
                return RedirectToAction("Index", "Home");
            }

            ViewBag.Error = "Login ou mot de passe incorrect.";
            return View();
        }

        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }

        public IActionResult Register()
        {
            return View();
        }
    }
}