using Microsoft.AspNetCore.Mvc;
using Inventaire.Data;
using System.Data;

namespace Inventaire.Controllers
{
    public class SortiesController : BaseController
    {
        private readonly DbHelper _db;
        public SortiesController(DbHelper db) { _db = db; }

        public IActionResult Index()
        {
            var check = CheckAccess(CanViewSorties());
            if (check != null) return check;
            SetUserViewBag();

            var data = _db.ExecuteQuery(@"
                SELECT s.id, s.date_sortie, s.qte_sortie, s.codebarre,
                       s.produit_id, p.label AS produit_label,
                       s.personne_id, per.nom + ' ' + per.prenom AS personne_label,
                       s.stock_id
                FROM sorties s
                LEFT JOIN produits p ON s.produit_id = p.produit_id
                LEFT JOIN personne per ON s.personne_id = per.personne_id
                ORDER BY s.date_sortie DESC, s.id DESC");

            var totalQte = data.Rows.Count > 0
                ? data.AsEnumerable().Sum(r => Convert.ToInt32(r["qte_sortie"] == DBNull.Value ? 0 : r["qte_sortie"]))
                : 0;

            ViewBag.TotalSorties = data.Rows.Count;
            ViewBag.TotalQte = totalQte;
            ViewBag.Produits = _db.ExecuteQuery("SELECT produit_id, label FROM produits ORDER BY label");
            ViewBag.Personnes = _db.ExecuteQuery("SELECT personne_id, nom, prenom FROM personne ORDER BY nom");
            ViewBag.Stocks = _db.ExecuteQuery("SELECT id FROM stock ORDER BY id");

            return View(data);
        }

        [HttpPost]
        public IActionResult Create(DateTime dateSortie, int qteSortie, string codebarre, int produitId, int personneId, int stockId)
        {
            try
            {
                _db.ExecuteNonQuery(
                    "INSERT INTO sorties (date_sortie, qte_sortie, codebarre, produit_id, personne_id, stock_id) VALUES (@date,@qte,@code,@pid,@perid,@sid)",
                    new Dictionary<string, object> {
                        {"@date", dateSortie}, {"@qte", qteSortie}, {"@code", codebarre??""},
                        {"@pid", produitId}, {"@perid", personneId}, {"@sid", stockId}
                    });

                _db.ExecuteNonQuery("UPDATE stock SET qte_stock = qte_stock - @qte WHERE id=@sid",
                    new Dictionary<string, object> { { "@qte", qteSortie }, { "@sid", stockId } });

                return Json(new { success = true, message = "Sortie ajoutée." });
            }
            catch (Exception ex) { return Json(new { success = false, message = ex.Message }); }
        }

        [HttpPost]
        public IActionResult Update(int id, DateTime dateSortie, int qteSortie, string codebarre, int produitId, int personneId, int stockId)
        {
            try
            {
                var old = _db.ExecuteQuery("SELECT qte_sortie, stock_id FROM sorties WHERE id=@id",
                    new Dictionary<string, object> { { "@id", id } });

                _db.ExecuteNonQuery(
                    "UPDATE sorties SET date_sortie=@date, qte_sortie=@qte, codebarre=@code, produit_id=@pid, personne_id=@perid, stock_id=@sid WHERE id=@id",
                    new Dictionary<string, object> {
                        {"@date", dateSortie}, {"@qte", qteSortie}, {"@code", codebarre??""},
                        {"@pid", produitId}, {"@perid", personneId}, {"@sid", stockId}, {"@id", id}
                    });

                if (old.Rows.Count > 0)
                {
                    int oldQte = Convert.ToInt32(old.Rows[0]["qte_sortie"]);
                    int oldSid = Convert.ToInt32(old.Rows[0]["stock_id"]);
                    _db.ExecuteNonQuery("UPDATE stock SET qte_stock = qte_stock + @qte WHERE id=@sid",
                        new Dictionary<string, object> { { "@qte", oldQte }, { "@sid", oldSid } });
                    _db.ExecuteNonQuery("UPDATE stock SET qte_stock = qte_stock - @qte WHERE id=@sid",
                        new Dictionary<string, object> { { "@qte", qteSortie }, { "@sid", stockId } });
                }

                return Json(new { success = true, message = "Sortie mise à jour." });
            }
            catch (Exception ex) { return Json(new { success = false, message = ex.Message }); }
        }

        [HttpPost]
        public IActionResult Delete(int id)
        {
            try
            {
                var old = _db.ExecuteQuery("SELECT qte_sortie, stock_id FROM sorties WHERE id=@id",
                    new Dictionary<string, object> { { "@id", id } });

                _db.ExecuteNonQuery("DELETE FROM sorties WHERE id=@id",
                    new Dictionary<string, object> { { "@id", id } });

                if (old.Rows.Count > 0)
                {
                    int oldQte = Convert.ToInt32(old.Rows[0]["qte_sortie"]);
                    int oldSid = Convert.ToInt32(old.Rows[0]["stock_id"]);
                    _db.ExecuteNonQuery("UPDATE stock SET qte_stock = qte_stock + @qte WHERE id=@sid",
                        new Dictionary<string, object> { { "@qte", oldQte }, { "@sid", oldSid } });
                }

                return Json(new { success = true, message = "Sortie supprimée." });
            }
            catch (Exception ex) { return Json(new { success = false, message = ex.Message }); }
        }
    }
}



