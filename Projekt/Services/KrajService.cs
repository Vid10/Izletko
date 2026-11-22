using Projekt.Models;

namespace Projekt.Services
{
    public enum StatusFilter
    {
        Vsi,
        Obiskani,
        Neobiskani
    }

    public class KrajService
    {
        private readonly List<Kraj> _kraji = new();
        private int _naslednjiKrajId = 1;
        private int _naslednjiStrosekId = 1;

        public IReadOnlyList<Kraj> VsiKraji => _kraji;

        public Kraj DodajKraj(string ime, string? opis, double latitude, double longitude)
        {
            if (string.IsNullOrWhiteSpace(ime))
            {
                throw new ArgumentException("Ime je obvezno.", nameof(ime));
            }
            if (latitude < -90 || latitude > 90)
            {
                throw new ArgumentOutOfRangeException(nameof(latitude), "Zemljepisna širina mora biti med -90 in 90.");
            }
            if (longitude < -180 || longitude > 180)
            {
                throw new ArgumentOutOfRangeException(nameof(longitude), "Zemljepisna dolžina mora biti med -180 in 180.");
            }

            var kraj = new Kraj
            {
                Id = _naslednjiKrajId++,
                Ime = ime.Trim(),
                Opis = opis,
                Latitude = latitude,
                Longitude = longitude
            };
            _kraji.Add(kraj);
            return kraj;
        }

        public Kraj? NajdiKraj(int id)
        {
            return _kraji.FirstOrDefault(k => k.Id == id);
        }

        // #31 Označi kraj kot obiskan. Vrne false, če kraj ne obstaja.
        public bool OznaciKotObiskan(int krajId, DateTime? datum = null)
        {
            var kraj = NajdiKraj(krajId);
            if (kraj == null)
            {
                return false;
            }

            kraj.Obiskan = true;
            kraj.DatumObiska = datum ?? DateTime.Today;
            return true;
        }

        // #32 Iskanje (ime ali opis, brez razlikovanja velikosti črk) in filtriranje po statusu obiska.
        public List<Kraj> Isci(string? iskanje, StatusFilter status = StatusFilter.Vsi)
        {
            IEnumerable<Kraj> rezultat = _kraji;

            if (!string.IsNullOrWhiteSpace(iskanje))
            {
                string niz = iskanje.Trim();
                rezultat = rezultat.Where(k =>
                    k.Ime.Contains(niz, StringComparison.OrdinalIgnoreCase) ||
                    (k.Opis != null && k.Opis.Contains(niz, StringComparison.OrdinalIgnoreCase)));
            }

            if (status == StatusFilter.Obiskani)
            {
                rezultat = rezultat.Where(k => k.Obiskan);
            }
            else if (status == StatusFilter.Neobiskani)
            {
                rezultat = rezultat.Where(k => !k.Obiskan);
            }

            return rezultat.OrderBy(k => k.Ime, StringComparer.CurrentCultureIgnoreCase).ToList();
        }

        // #30 Razdalja med dvema krajema v km. Če kateri kraj ne obstaja, vrže KeyNotFoundException.
        public double RazdaljaMedKraji(int krajIdA, int krajIdB)
        {
            var a = NajdiKraj(krajIdA) ?? throw new KeyNotFoundException($"Kraj {krajIdA} ne obstaja.");
            var b = NajdiKraj(krajIdB) ?? throw new KeyNotFoundException($"Kraj {krajIdB} ne obstaja.");

            return DistanceCalculator.HaversineKm(a.Latitude, a.Longitude, b.Latitude, b.Longitude);
        }

        public Strosek DodajStrosek(int krajId, string opis, decimal znesek, DateTime? datum = null)
        {
            var kraj = NajdiKraj(krajId) ?? throw new KeyNotFoundException($"Kraj {krajId} ne obstaja.");

            if (string.IsNullOrWhiteSpace(opis))
            {
                throw new ArgumentException("Opis je obvezen.", nameof(opis));
            }
            if (znesek <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(znesek), "Znesek mora biti večji od 0.");
            }

            var strosek = new Strosek
            {
                Id = _naslednjiStrosekId++,
                Opis = opis.Trim(),
                Znesek = znesek,
                Datum = datum ?? DateTime.Today
            };
            kraj.Stroski.Add(strosek);
            return strosek;
        }

        // #34 Brisanje stroška. Vrne false, če kraj ali strošek ne obstajata.
        public bool IzbrisiStrosek(int krajId, int strosekId)
        {
            var kraj = NajdiKraj(krajId);
            if (kraj == null)
            {
                return false;
            }

            var strosek = kraj.Stroski.FirstOrDefault(s => s.Id == strosekId);
            if (strosek == null)
            {
                return false;
            }

            kraj.Stroski.Remove(strosek);
            return true;
        }

        public decimal SkupajStroski(int krajId)
        {
            var kraj = NajdiKraj(krajId) ?? throw new KeyNotFoundException($"Kraj {krajId} ne obstaja.");
            return kraj.Stroski.Sum(s => s.Znesek);
        }
    }
}
