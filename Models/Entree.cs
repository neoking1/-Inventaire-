namespace Inventaire.Models
{
    public class Entree
    {
        public int Id { get; set; }
        public DateTime DateEntree { get; set; }
        public int QteEntree { get; set; }
        public int ProduitId { get; set; }
        public int StockId { get; set; }
        public int FournisseurId { get; set; }
        public int OriginId { get; set; }
    }
}