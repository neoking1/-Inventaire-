using Microsoft.AspNetCore.Mvc;
using Inventaire.Data;
using System.Data;

namespace Inventaire.Controllers
{
    public class EntreesController : BaseController
    {
        private readonly DbHelper _db;
        public EntreesController(DbHelper db) { _db = db; }

        public IActionResult Index()
        {
            var check = CheckAccess(CanViewEntrees());
            if (check != null) return check;
            SetUserViewBag();

            var data = _db.ExecuteQuery(@"
                SELECT e.id, e.date_entree, e.qte_entree,
                       e.produit_id, p.label AS produit_label,
                       e.stock_id,
                       e.fournisseur_id, f.raison_social AS fournisseur_label,
                       e.origin_id, o.type AS origin_label
                FROM entrees e
                LEFT JOIN produits p ON e.produit_id = p.produit_id
                LEFT JOIN fournisseurs f ON e.fournisseur_id = f.fournisseur_id
                LEFT JOIN origin o ON e.origin_id = o.id
                ORDER BY e.date_entree DESC, e.id DESC");

            var totalQte = data.Rows.Count > 0
                ? data.AsEnumerable().Sum(r => Convert.ToInt32(r["qte_entree"] == DBNull.Value ? 0 : r["qte_entree"]))
                : 0;

            ViewBag.TotalEntrees = data.Rows.Count;
            ViewBag.TotalQte = totalQte;
            ViewBag.Produits = _db.ExecuteQuery("SELECT produit_id, label FROM produits ORDER BY label");
            ViewBag.Fournisseurs = _db.ExecuteQuery("SELECT fournisseur_id, raison_social FROM fournisseurs ORDER BY raison_social");
            ViewBag.Origins = _db.ExecuteQuery("SELECT id, type FROM origin ORDER BY type");
            ViewBag.Stocks = _db.ExecuteQuery("SELECT id FROM stock ORDER BY id");

            return View(data);
        }

        [HttpPost]
        public IActionResult Create(DateTime dateEntree, int qteEntree, int produitId, int stockId, int fournisseurId, int originId)
        {
            try
            {
                _db.ExecuteNonQuery(
                    "INSERT INTO entrees (date_entree, qte_entree, produit_id, stock_id, fournisseur_id, origin_id) VALUES (@date,@qte,@pid,@sid,@fid,@oid)",
                    new Dictionary<string, object> {
                        {"@date", dateEntree}, {"@qte", qteEntree},
                        {"@pid", produitId}, {"@sid", stockId},
                        {"@fid", fournisseurId}, {"@oid", originId}
                    });

                // Update stock quantity
                _db.ExecuteNonQuery(
                    "UPDATE stock SET qte_stock = qte_stock + @qte WHERE id = @sid",
                    new Dictionary<string, object> { { "@qte", qteEntree }, { "@sid", stockId } });

                return Json(new { success = true, message = "Entrée ajoutée." });
            }
            catch (Exception ex) { return Json(new { success = false, message = ex.Message }); }
        }

        [HttpPost]
        public IActionResult Update(int id, DateTime dateEntree, int qteEntree, int produitId, int stockId, int fournisseurId, int originId)
        {
            try
            {
                // Get old quantity to adjust stock
                var old = _db.ExecuteQuery("SELECT qte_entree, stock_id FROM entrees WHERE id=@id",
                    new Dictionary<string, object> { { "@id", id } });

                _db.ExecuteNonQuery(
                    "UPDATE entrees SET date_entree=@date, qte_entree=@qte, produit_id=@pid, stock_id=@sid, fournisseur_id=@fid, origin_id=@oid WHERE id=@id",
                    new Dictionary<string, object> {
                        {"@date", dateEntree}, {"@qte", qteEntree},
                        {"@pid", produitId}, {"@sid", stockId},
                        {"@fid", fournisseurId}, {"@oid", originId}, {"@id", id}
                    });

                if (old.Rows.Count > 0)
                {
                    int oldQte = Convert.ToInt32(old.Rows[0]["qte_entree"]);
                    int oldSid = Convert.ToInt32(old.Rows[0]["stock_id"]);
                    // Reverse old, apply new
                    _db.ExecuteNonQuery("UPDATE stock SET qte_stock = qte_stock - @qte WHERE id=@sid",
                        new Dictionary<string, object> { { "@qte", oldQte }, { "@sid", oldSid } });
                    _db.ExecuteNonQuery("UPDATE stock SET qte_stock = qte_stock + @qte WHERE id=@sid",
                        new Dictionary<string, object> { { "@qte", qteEntree }, { "@sid", stockId } });
                }

                return Json(new { success = true, message = "Entrée mise à jour." });
            }
            catch (Exception ex) { return Json(new { success = false, message = ex.Message }); }
        }

        [HttpPost]
        public IActionResult Delete(int id)
        {
            try
            {
                var old = _db.ExecuteQuery("SELECT qte_entree, stock_id FROM entrees WHERE id=@id",
                    new Dictionary<string, object> { { "@id", id } });

                _db.ExecuteNonQuery("DELETE FROM entrees WHERE id=@id",
                    new Dictionary<string, object> { { "@id", id } });

                if (old.Rows.Count > 0)
                {
                    int oldQte = Convert.ToInt32(old.Rows[0]["qte_entree"]);
                    int oldSid = Convert.ToInt32(old.Rows[0]["stock_id"]);
                    _db.ExecuteNonQuery("UPDATE stock SET qte_stock = qte_stock - @qte WHERE id=@sid",
                        new Dictionary<string, object> { { "@qte", oldQte }, { "@sid", oldSid } });
                }

                return Json(new { success = true, message = "Entrée supprimée." });
            }
            catch (Exception ex) { return Json(new { success = false, message = ex.Message }); }
        }
    }
}



