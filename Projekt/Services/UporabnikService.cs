using System.Security.Cryptography;
using Projekt.Models;

namespace Projekt.Services
{
    // #33 Prijava: registracija in prijava uporabnikov (geslo je shranjeno kot PBKDF2 zgoščena vrednost).
    public class UporabnikService
    {
        private const int SaltSize = 16;
        private const int HashSize = 32;
        private const int Iteracije = 100_000;

        private readonly List<Uporabnik> _uporabniki = new();
        private int _naslednjiId = 1;

        public Uporabnik Registracija(string email, string geslo)
        {
            if (string.IsNullOrWhiteSpace(email) || !email.Contains('@'))
            {
                throw new ArgumentException("Vnesi veljaven e-naslov.", nameof(email));
            }
            if (string.IsNullOrEmpty(geslo) || geslo.Length < 6)
            {
                throw new ArgumentException("Geslo mora imeti vsaj 6 znakov.", nameof(geslo));
            }

            string normaliziran = Normaliziraj(email);
            if (_uporabniki.Any(u => u.Email == normaliziran))
            {
                throw new InvalidOperationException("Uporabnik s tem e-naslovom že obstaja.");
            }

            var uporabnik = new Uporabnik
            {
                Id = _naslednjiId++,
                Email = normaliziran,
                GesloHash = ZgostiGeslo(geslo)
            };
            _uporabniki.Add(uporabnik);
            return uporabnik;
        }

        // Vrne uporabnika, če sta e-naslov in geslo pravilna, sicer null.
        public Uporabnik? Prijava(string email, string geslo)
        {
            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrEmpty(geslo))
            {
                return null;
            }

            string normaliziran = Normaliziraj(email);
            var uporabnik = _uporabniki.FirstOrDefault(u => u.Email == normaliziran);
            if (uporabnik == null)
            {
                return null;
            }

            return PreveriGeslo(geslo, uporabnik.GesloHash) ? uporabnik : null;
        }

        private static string Normaliziraj(string email) => email.Trim().ToLowerInvariant();

        private static string ZgostiGeslo(string geslo)
        {
            byte[] salt = RandomNumberGenerator.GetBytes(SaltSize);
            byte[] hash = Rfc2898DeriveBytes.Pbkdf2(geslo, salt, Iteracije, HashAlgorithmName.SHA256, HashSize);
            return Convert.ToBase64String(salt) + ":" + Convert.ToBase64String(hash);
        }

        private static bool PreveriGeslo(string geslo, string shranjeno)
        {
            string[] deli = shranjeno.Split(':');
            byte[] salt = Convert.FromBase64String(deli[0]);
            byte[] pricakovan = Convert.FromBase64String(deli[1]);
            byte[] dejanski = Rfc2898DeriveBytes.Pbkdf2(geslo, salt, Iteracije, HashAlgorithmName.SHA256, HashSize);
            return CryptographicOperations.FixedTimeEquals(pricakovan, dejanski);
        }
    }
}
