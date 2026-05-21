using Microsoft.AspNetCore.Mvc;

namespace Inventaire.Controllers
{
    public class BaseController : Controller
    {
        protected bool IsLoggedIn() =>
            HttpContext.Session.GetString("UserType") != null;

        protected bool IsAdmin() =>
            HttpContext.Session.GetString("UserType") == "admin";

        protected bool IsManager() =>
            HttpContext.Session.GetString("UserType") is "admin" or "manager";

        protected bool IsEmployee() =>
            HttpContext.Session.GetString("UserType") is "admin" or "manager" or "employee";

        protected bool IsViewer() =>
            HttpContext.Session.GetString("UserType") is "admin" or "manager" or "employee" or "viewer";

        protected IActionResult? CheckAccess(bool allowed)
        {
            if (!IsLoggedIn()) return RedirectToAction("Login", "Auth");
            if (!allowed) return RedirectToAction("Index", "Home");
            return null;
        }
        protected void SetUserViewBag()
        {
            ViewBag.UserType = HttpContext.Session.GetString("UserType");
            ViewBag.UserLogin = HttpContext.Session.GetString("UserLogin");
        }
    }
}