using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Inventaire.Controllers
{
    public class BaseController : Controller
    {
        protected bool IsLoggedIn() =>
            HttpContext.Session.GetString("UserType") != null;

        protected string? UserType => HttpContext.Session.GetString("UserType");

        protected bool IsAdmin() => UserType == "admin";
        protected bool IsManager() => UserType is "admin" or "manager";
        protected bool IsEmployee() => UserType is "admin" or "manager" or "employee";
        protected bool IsViewer() => UserType is "admin" or "manager" or "employee" or "viewer";

        // Specific Matrix Permissions
        protected bool CanViewEntrees() => UserType is "admin" or "manager" or "employee";
        protected bool CanViewSorties() => UserType is "admin" or "manager" or "employee";
        protected bool CanViewFournisseurs() => UserType is "admin" or "manager";
        protected bool CanViewPersonnes() => UserType is "admin" or "manager";
        protected bool CanViewUsers() => UserType == "admin";
        protected bool CanViewReports() => UserType is "admin" or "manager" or "viewer";
        protected bool CanEdit() => UserType is "admin" or "manager" or "employee";

        protected IActionResult? CheckAccess(bool allowed)
        {
            if (!IsLoggedIn()) return RedirectToAction("Login", "Auth");
            if (!allowed) return RedirectToAction("Index", "Home");
            return null;
        }

        protected void SetUserViewBag()
        {
            ViewBag.UserType = UserType;
            ViewBag.UserLogin = HttpContext.Session.GetString("UserLogin");

            ViewBag.CanViewEntrees = CanViewEntrees();
            ViewBag.CanViewSorties = CanViewSorties();
            ViewBag.CanViewFournisseurs = CanViewFournisseurs();
            ViewBag.CanViewPersonnes = CanViewPersonnes();
            ViewBag.CanViewUsers = CanViewUsers();
            ViewBag.CanViewReports = CanViewReports();
            ViewBag.CanEdit = CanEdit();
        }
    }
}

