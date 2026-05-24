using Microsoft.AspNetCore.Mvc;
using Inventaire.Data;

namespace Inventaire.Controllers
{
    public class OriginController : BaseController
    {
        private readonly DbHelper _db;
        public OriginController(DbHelper db) { _db = db; }

        public IActionResult Index()
        {
            var check = CheckAccess(CanViewFournisseurs());
            if (check != null) return check;
            SetUserViewBag();
            var data = _db.ExecuteQuery("SELECT id, type FROM origin ORDER BY id");
            ViewBag.Total = data.Rows.Count;
            return View(data);
        }

        [HttpPost]
        public IActionResult Create(string type)
        {
            try
            {
                _db.ExecuteNonQuery("INSERT INTO origin (type) VALUES (@type)",
                    new Dictionary<string, object> { { "@type", type ?? "" } });
                return Json(new { success = true, message = "Origine ajoutée." });
            }
            catch (Exception ex) { return Json(new { success = false, message = ex.Message }); }
        }

        [HttpPost]
        public IActionResult Update(int id, string type)
        {
            try
            {
                _db.ExecuteNonQuery("UPDATE origin SET type=@type WHERE id=@id",
                    new Dictionary<string, object> { { "@type", type ?? "" }, { "@id", id } });
                return Json(new { success = true, message = "Origine mise à jour." });
            }
            catch (Exception ex) { return Json(new { success = false, message = ex.Message }); }
        }

        [HttpPost]
        public IActionResult Delete(int id)
        {
            try
            {
                _db.ExecuteNonQuery("DELETE FROM origin WHERE id=@id",
                    new Dictionary<string, object> { { "@id", id } });
                return Json(new { success = true, message = "Origine supprimée." });
            }
            catch (Exception ex) { return Json(new { success = false, message = ex.Message }); }
        }
    }
}

