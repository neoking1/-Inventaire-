namespace Inventaire.Models
{
    public class Sortie
    {
        public int Id { get; set; }
        public DateTime DateSortie { get; set; }
        public int QteSortie { get; set; }
        public string Codebarre { get; set; }
        public int ProduitId { get; set; }
        public int PersonneId { get; set; }
        public int StockId { get; set; }
    }
}