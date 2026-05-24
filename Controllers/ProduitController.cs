using Microsoft.AspNetCore.Mvc;
using Inventaire.Data;

namespace Inventaire.Controllers
{
    public class ProduitController : BaseController
    {
        private readonly DbHelper _db;
        public ProduitController(DbHelper db) { _db = db; }

        public IActionResult Index()
        {
            var check = CheckAccess(IsViewer());
            if (check != null) return check;
            SetUserViewBag();

            var data = _db.ExecuteQuery(@"
                SELECT p.produit_id, p.label, p.categorie_id,
                       c.nom_categorie
                FROM produits p
                LEFT JOIN categories c ON p.categorie_id = c.id
                ORDER BY p.produit_id");

            ViewBag.TotalProduits = data.Rows.Count;
            ViewBag.TotalCategories = _db.ExecuteScalar("SELECT COUNT(*) FROM categories") ?? 0;
            ViewBag.Categories = _db.ExecuteQuery("SELECT id, nom_categorie FROM categories ORDER BY nom_categorie");
            return View(data);
        }

        [HttpPost]
        public IActionResult Create(string label, int categorieId)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(label))
                    return Json(new { success = false, message = "Le label est requis." });

                _db.ExecuteNonQuery(
                    "INSERT INTO produits (label, categorie_id) VALUES (@label, @catId)",
                    new Dictionary<string, object> { { "@label", label.Trim() }, { "@catId", categorieId } });

                return Json(new { success = true, message = "Produit ajouté." });
            }
            catch (Exception ex) { return Json(new { success = false, message = ex.Message }); }
        }

        [HttpPost]
        public IActionResult Update(int id, string label, int categorieId)
        {
            try
            {
                _db.ExecuteNonQuery(
                    "UPDATE produits SET label=@label, categorie_id=@catId WHERE produit_id=@id",
                    new Dictionary<string, object> { { "@label", label.Trim() }, { "@catId", categorieId }, { "@id", id } });

                return Json(new { success = true, message = "Produit mis à jour." });
            }
            catch (Exception ex) { return Json(new { success = false, message = ex.Message }); }
        }

        [HttpPost]
        public IActionResult Delete(int id)
        {
            try
            {
                _db.ExecuteNonQuery("DELETE FROM produits WHERE produit_id=@id",
                    new Dictionary<string, object> { { "@id", id } });
                return Json(new { success = true, message = "Produit supprimé." });
            }
            catch (Exception ex) { return Json(new { success = false, message = ex.Message }); }
        }
    }
}

