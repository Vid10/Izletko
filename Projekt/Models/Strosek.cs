namespace Projekt.Models
{
    public class Strosek
    {
        public int Id { get; set; }
        public string Opis { get; set; } = string.Empty;
        public decimal Znesek { get; set; }
        public DateTime Datum { get; set; }
    }
}
