using Microsoft.AspNetCore.Mvc;
using Inventaire.Data;

namespace Inventaire.Controllers
{
    public class LocaliteController : BaseController
    {
        private readonly DbHelper _db;
        public LocaliteController(DbHelper db) { _db = db; }

        public IActionResult Index()
        {
            var check = CheckAccess(CanViewFournisseurs());
            if (check != null) return check;
            SetUserViewBag();
            var data = _db.ExecuteQuery("SELECT id, intitule FROM localite ORDER BY id");
            ViewBag.Total = data.Rows.Count;
            return View(data);
        }

        [HttpPost]
        public IActionResult Create(string intitule)
        {
            try
            {
                _db.ExecuteNonQuery("INSERT INTO localite (intitule) VALUES (@intitule)",
                    new Dictionary<string, object> { { "@intitule", intitule ?? "" } });
                return Json(new { success = true, message = "Localité ajoutée." });
            }
            catch (Exception ex) { return Json(new { success = false, message = ex.Message }); }
        }

        [HttpPost]
        public IActionResult Update(int id, string intitule)
        {
            try
            {
                _db.ExecuteNonQuery("UPDATE localite SET intitule=@intitule WHERE id=@id",
                    new Dictionary<string, object> { { "@intitule", intitule ?? "" }, { "@id", id } });
                return Json(new { success = true, message = "Localité mise à jour." });
            }
            catch (Exception ex) { return Json(new { success = false, message = ex.Message }); }
        }

        [HttpPost]
        public IActionResult Delete(int id)
        {
            try
            {
                _db.ExecuteNonQuery("DELETE FROM localite WHERE id=@id",
                    new Dictionary<string, object> { { "@id", id } });
                return Json(new { success = true, message = "Localité supprimée." });
            }
            catch (Exception ex) { return Json(new { success = false, message = ex.Message }); }
        }
    }
}

