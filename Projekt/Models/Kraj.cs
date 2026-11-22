namespace Projekt.Models
{
    public class Kraj
    {
        public int Id { get; set; }
        public string Ime { get; set; } = string.Empty;
        public string? Opis { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public bool Obiskan { get; set; }
        public DateTime? DatumObiska { get; set; }
        public List<Strosek> Stroski { get; set; } = new();
    }
}
