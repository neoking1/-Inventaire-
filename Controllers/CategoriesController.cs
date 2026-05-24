using Microsoft.AspNetCore.Mvc;
using Inventaire.Data;
using System.Data;

namespace Inventaire.Controllers
{
    public class CategoriesController : BaseController
    {
        private readonly DbHelper _db;
        public CategoriesController(DbHelper db) { _db = db; }

        public IActionResult Index()
        {
            var check = CheckAccess(IsViewer());
            if (check != null) return check;
            SetUserViewBag();

            var data = _db.ExecuteQuery(@"
                SELECT c.id, c.nom_categorie,
                       COUNT(p.produit_id) AS nb_produits
                FROM categories c
                LEFT JOIN produits p ON p.categorie_id = c.id
                GROUP BY c.id, c.nom_categorie
                ORDER BY c.id");

            ViewBag.TotalCategories = data.Rows.Count;
            ViewBag.TotalProduits = _db.ExecuteScalar("SELECT COUNT(*) FROM produits") ?? 0;

            return View(data);
        }

        [HttpPost]
        public IActionResult Create(string nomCategorie)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(nomCategorie))
                    return Json(new { success = false, message = "Le nom est requis." });

                _db.ExecuteNonQuery(
                    "INSERT INTO categories (nom_categorie) VALUES (@nom)",
                    new Dictionary<string, object> { { "@nom", nomCategorie.Trim() } });

                return Json(new { success = true, message = "Catégorie ajoutée." });
            }
            catch (Exception ex) { return Json(new { success = false, message = ex.Message }); }
        }

        [HttpPost]
        public IActionResult Update(int id, string nomCategorie)
        {
            try
            {
                _db.ExecuteNonQuery(
                    "UPDATE categories SET nom_categorie = @nom WHERE id = @id",
                    new Dictionary<string, object> { { "@nom", nomCategorie.Trim() }, { "@id", id } });

                return Json(new { success = true, message = "Catégorie mise à jour." });
            }
            catch (Exception ex) { return Json(new { success = false, message = ex.Message }); }
        }

        [HttpPost]
        public IActionResult Delete(int id)
        {
            try
            {
                _db.ExecuteNonQuery(
                    "DELETE FROM categories WHERE id = @id",
                    new Dictionary<string, object> { { "@id", id } });

                return Json(new { success = true, message = "Catégorie supprimée." });
            }
            catch (Exception ex) { return Json(new { success = false, message = ex.Message }); }
        }
    }
}

