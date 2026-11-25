using Microsoft.VisualStudio.TestTools.UnitTesting;
using Projekt.Models;
using Projekt.Services;

namespace TestIzletko1
{
    [TestClass]
    public class TestKraj
    {
        private static KrajService NovServis() => new KrajService();

        private static KrajService ServisZPodatki()
        {
            var servis = new KrajService();
            var maribor = servis.DodajKraj("Maribor", "Mesto ob Dravi", 46.55, 15.64);
            servis.DodajKraj("Bled", "Jezero in otok", 46.37, 14.11);
            var ptuj = servis.DodajKraj("Ptuj", "Najstarejše mesto", 46.42, 15.87);
            servis.DodajKraj("Celje", null, 46.23, 15.26);
            servis.OznaciKotObiskan(maribor.Id);
            servis.OznaciKotObiskan(ptuj.Id);
            return servis;
        }

        private static string[] Imena(List<Kraj> kraji) => kraji.Select(k => k.Ime).ToArray();

        // ---------- DodajKraj ----------

        [TestMethod]
        public void DodajKraj_VeljavniPodatki_VrneKrajInGaShrani()
        {
            var servis = NovServis();

            var kraj = servis.DodajKraj("Maribor", "Drava", 46.5547, 15.6459);

            Assert.AreEqual(1, kraj.Id);
            Assert.AreEqual("Maribor", kraj.Ime);
            Assert.AreEqual("Drava", kraj.Opis);
            Assert.AreEqual(46.5547, kraj.Latitude);
            Assert.AreEqual(15.6459, kraj.Longitude);
            Assert.AreEqual(1, servis.VsiKraji.Count);
        }

        [TestMethod]
        public void DodajKraj_ImeSePresledki_Trimira()
        {
            var kraj = NovServis().DodajKraj("  Bled  ", null, 46.37, 14.11);

            Assert.AreEqual("Bled", kraj.Ime);
        }

        [TestMethod]
        public void DodajKraj_IdjiSeInkrementirajo()
        {
            var servis = NovServis();

            var a = servis.DodajKraj("A", null, 0, 0);
            var b = servis.DodajKraj("B", null, 0, 0);

            Assert.AreEqual(1, a.Id);
            Assert.AreEqual(2, b.Id);
        }

        [TestMethod]
        [DataRow(null)]
        [DataRow("")]
        [DataRow("   ")]
        public void DodajKraj_PrazenIme_VrzeArgumentException(string? ime)
        {
            var servis = NovServis();

            Assert.ThrowsExactly<ArgumentException>(() => servis.DodajKraj(ime!, null, 0, 0));
            Assert.AreEqual(0, servis.VsiKraji.Count);
        }

        [TestMethod]
        [DataRow(-90.01)]
        [DataRow(90.01)]
        [DataRow(1000.0)]
        public void DodajKraj_NeveljavnaSirina_VrzeArgumentOutOfRange(double lat)
        {
            Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => NovServis().DodajKraj("X", null, lat, 0));
        }

        [TestMethod]
        [DataRow(-180.01)]
        [DataRow(180.01)]
        [DataRow(1000.0)]
        public void DodajKraj_NeveljavnaDolzina_VrzeArgumentOutOfRange(double lon)
        {
            Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => NovServis().DodajKraj("X", null, 0, lon));
        }

        [TestMethod]
        [DataRow(-90.0, -180.0)]
        [DataRow(90.0, 180.0)]
        [DataRow(0.0, 0.0)]
        public void DodajKraj_MejneVrednostiKoordinat_SoVeljavne(double lat, double lon)
        {
            var kraj = NovServis().DodajKraj("Rob", null, lat, lon);

            Assert.AreEqual(lat, kraj.Latitude);
            Assert.AreEqual(lon, kraj.Longitude);
        }

        // ---------- NajdiKraj ----------

        [TestMethod]
        public void NajdiKraj_Obstaja_VrneKraj()
        {
            var servis = NovServis();
            var dodan = servis.DodajKraj("Piran", null, 45.53, 13.57);

            Assert.AreSame(dodan, servis.NajdiKraj(dodan.Id));
        }

        [TestMethod]
        public void NajdiKraj_NeObstaja_VrneNull()
        {
            Assert.IsNull(NovServis().NajdiKraj(999));
        }

        // ---------- OznaciKotObiskan (#31) ----------

        [TestMethod]
        public void OznaciKotObiskan_BrezDatuma_NastaviDanes()
        {
            var servis = NovServis();
            var kraj = servis.DodajKraj("Ptuj", null, 46.42, 15.87);

            Assert.IsTrue(servis.OznaciKotObiskan(kraj.Id));
            Assert.IsTrue(kraj.Obiskan);
            Assert.AreEqual(DateTime.Today, kraj.DatumObiska);
        }

        [TestMethod]
        public void OznaciKotObiskan_ZDatumom_NastaviPodanDatum()
        {
            var servis = NovServis();
            var kraj = servis.DodajKraj("Ptuj", null, 46.42, 15.87);
            var datum = new DateTime(2024, 5, 17);

            Assert.IsTrue(servis.OznaciKotObiskan(kraj.Id, datum));
            Assert.IsTrue(kraj.Obiskan);
            Assert.AreEqual(datum, kraj.DatumObiska);
        }

        [TestMethod]
        public void OznaciKotObiskan_KrajNeObstaja_VrneFalse()
        {
            Assert.IsFalse(NovServis().OznaciKotObiskan(123));
        }

        [TestMethod]
        public void OznaciKotObiskan_NovKraj_JeNeobiskan()
        {
            var kraj = NovServis().DodajKraj("Celje", null, 46.23, 15.26);

            Assert.IsFalse(kraj.Obiskan);
        }

        // ---------- Isci (#32) ----------

        [TestMethod]
        public void Isci_BrezParametrov_VrneVseUrejenePoImenu()
        {
            var rezultat = ServisZPodatki().Isci(null);

            CollectionAssert.AreEqual(new[] { "Bled", "Celje", "Maribor", "Ptuj" }, Imena(rezultat));
        }

        [TestMethod]
        [DataRow("")]
        [DataRow("   ")]
        public void Isci_PrazenNiz_VrneVse(string iskanje)
        {
            Assert.AreEqual(4, ServisZPodatki().Isci(iskanje).Count);
        }

        [TestMethod]
        public void Isci_PoImenu_BrezRazlikovanjaVelikostiCrk()
        {
            var rezultat = ServisZPodatki().Isci("mARiBOR");

            Assert.AreEqual(1, rezultat.Count);
            Assert.AreEqual("Maribor", rezultat[0].Ime);
        }

        [TestMethod]
        public void Isci_PoOpisu_NajdeKraj()
        {
            var rezultat = ServisZPodatki().Isci("jezero");

            Assert.AreEqual(1, rezultat.Count);
            Assert.AreEqual("Bled", rezultat[0].Ime);
        }

        [TestMethod]
        public void Isci_NizVOpisuDvehKrajev_VrneOba()
        {
            var rezultat = ServisZPodatki().Isci("mesto");

            CollectionAssert.AreEqual(new[] { "Maribor", "Ptuj" }, Imena(rezultat));
        }

        [TestMethod]
        public void Isci_NizSePresledki_Trimira()
        {
            Assert.AreEqual(1, ServisZPodatki().Isci("  bled  ").Count);
        }

        [TestMethod]
        public void Isci_KrajBrezOpisa_NeVrzeIzjeme()
        {
            var rezultat = ServisZPodatki().Isci("celje");

            Assert.AreEqual(1, rezultat.Count);
            Assert.AreEqual("Celje", rezultat[0].Ime);
        }

        [TestMethod]
        public void Isci_NiRezultatov_VrnePraznSeznam()
        {
            Assert.AreEqual(0, ServisZPodatki().Isci("xyz-ne-obstaja").Count);
        }

        [TestMethod]
        public void Isci_FilterObiskani_VrneSamoObiskane()
        {
            var rezultat = ServisZPodatki().Isci(null, StatusFilter.Obiskani);

            CollectionAssert.AreEqual(new[] { "Maribor", "Ptuj" }, Imena(rezultat));
            Assert.IsTrue(rezultat.All(k => k.Obiskan));
        }

        [TestMethod]
        public void Isci_FilterNeobiskani_VrneSamoNeobiskane()
        {
            var rezultat = ServisZPodatki().Isci(null, StatusFilter.Neobiskani);

            CollectionAssert.AreEqual(new[] { "Bled", "Celje" }, Imena(rezultat));
            Assert.IsTrue(rezultat.All(k => !k.Obiskan));
        }

        [TestMethod]
        public void Isci_FilterVsi_VrneVse()
        {
            Assert.AreEqual(4, ServisZPodatki().Isci(null, StatusFilter.Vsi).Count);
        }

        [TestMethod]
        public void Isci_IskanjeInFilterSkupaj_Kombinira()
        {
            var servis = ServisZPodatki();

            Assert.AreEqual(0, servis.Isci("mesto", StatusFilter.Neobiskani).Count);
            Assert.AreEqual(2, servis.Isci("mesto", StatusFilter.Obiskani).Count);
        }

        [TestMethod]
        public void Isci_PraznServis_VrnePraznSeznam()
        {
            Assert.AreEqual(0, NovServis().Isci("karkoli").Count);
        }

        // ---------- RazdaljaMedKraji (#30) ----------

        [TestMethod]
        public void RazdaljaMedKraji_IstiKraj_JeNic()
        {
            var servis = NovServis();
            var a = servis.DodajKraj("A", null, 46.0569, 14.5058);

            Assert.AreEqual(0.0, servis.RazdaljaMedKraji(a.Id, a.Id), 0.000001);
        }

        [TestMethod]
        public void RazdaljaMedKraji_LjubljanaMaribor_OkoliDevetdesetKm()
        {
            var servis = NovServis();
            var lj = servis.DodajKraj("Ljubljana", null, 46.0569, 14.5058);
            var mb = servis.DodajKraj("Maribor", null, 46.5547, 15.6459);

            var km = servis.RazdaljaMedKraji(lj.Id, mb.Id);

            Assert.IsTrue(km >= 100 && km <= 107, $"Razdalja {km} km je izven pričakovanega območja.");
        }

        [TestMethod]
        public void RazdaljaMedKraji_JeSimetricna()
        {
            var servis = NovServis();
            var lj = servis.DodajKraj("Ljubljana", null, 46.0569, 14.5058);
            var mb = servis.DodajKraj("Maribor", null, 46.5547, 15.6459);

            Assert.AreEqual(servis.RazdaljaMedKraji(lj.Id, mb.Id),
                            servis.RazdaljaMedKraji(mb.Id, lj.Id), 0.000001);
        }

        [TestMethod]
        public void RazdaljaMedKraji_PrviKrajNeObstaja_VrzeKeyNotFound()
        {
            var servis = NovServis();
            var b = servis.DodajKraj("B", null, 0, 0);

            Assert.ThrowsExactly<KeyNotFoundException>(() => servis.RazdaljaMedKraji(999, b.Id));
        }

        [TestMethod]
        public void RazdaljaMedKraji_DrugiKrajNeObstaja_VrzeKeyNotFound()
        {
            var servis = NovServis();
            var a = servis.DodajKraj("A", null, 0, 0);

            Assert.ThrowsExactly<KeyNotFoundException>(() => servis.RazdaljaMedKraji(a.Id, 999));
        }

        // ---------- DodajStrosek ----------

        [TestMethod]
        public void DodajStrosek_VeljavniPodatki_DodaStrosekKraju()
        {
            var servis = NovServis();
            var kraj = servis.DodajKraj("Bled", null, 46.37, 14.11);
            var datum = new DateTime(2024, 6, 1);

            var strosek = servis.DodajStrosek(kraj.Id, "Kosilo", 12.50m, datum);

            Assert.AreEqual(1, strosek.Id);
            Assert.AreEqual("Kosilo", strosek.Opis);
            Assert.AreEqual(12.50m, strosek.Znesek);
            Assert.AreEqual(datum, strosek.Datum);
            Assert.AreEqual(1, kraj.Stroski.Count);
        }

        [TestMethod]
        public void DodajStrosek_BrezDatuma_NastaviDanes()
        {
            var servis = NovServis();
            var kraj = servis.DodajKraj("Bled", null, 46.37, 14.11);

            Assert.AreEqual(DateTime.Today, servis.DodajStrosek(kraj.Id, "Vstopnina", 5m).Datum);
        }

        [TestMethod]
        public void DodajStrosek_OpisSePresledki_Trimira()
        {
            var servis = NovServis();
            var kraj = servis.DodajKraj("Bled", null, 46.37, 14.11);

            Assert.AreEqual("Kava", servis.DodajStrosek(kraj.Id, "  Kava  ", 2m).Opis);
        }

        [TestMethod]
        public void DodajStrosek_IdjiSoGlobalniCezVseKraje()
        {
            var servis = NovServis();
            var a = servis.DodajKraj("A", null, 0, 0);
            var b = servis.DodajKraj("B", null, 0, 0);

            var s1 = servis.DodajStrosek(a.Id, "x", 1m);
            var s2 = servis.DodajStrosek(b.Id, "y", 1m);
            var s3 = servis.DodajStrosek(a.Id, "z", 1m);

            CollectionAssert.AreEqual(new[] { 1, 2, 3 }, new[] { s1.Id, s2.Id, s3.Id });
        }

        [TestMethod]
        public void DodajStrosek_KrajNeObstaja_VrzeKeyNotFound()
        {
            Assert.ThrowsExactly<KeyNotFoundException>(() => NovServis().DodajStrosek(999, "x", 1m));
        }

        [TestMethod]
        [DataRow(null)]
        [DataRow("")]
        [DataRow("   ")]
        public void DodajStrosek_PrazenOpis_VrzeArgumentException(string? opis)
        {
            var servis = NovServis();
            var kraj = servis.DodajKraj("A", null, 0, 0);

            Assert.ThrowsExactly<ArgumentException>(() => servis.DodajStrosek(kraj.Id, opis!, 1m));
            Assert.AreEqual(0, kraj.Stroski.Count);
        }

        [TestMethod]
        [DataRow(0.0)]
        [DataRow(-1.0)]
        [DataRow(-0.01)]
        public void DodajStrosek_ZnesekNicAliNegativen_VrzeArgumentOutOfRange(double znesek)
        {
            var servis = NovServis();
            var kraj = servis.DodajKraj("A", null, 0, 0);

            Assert.ThrowsExactly<ArgumentOutOfRangeException>(
                () => servis.DodajStrosek(kraj.Id, "x", (decimal)znesek));
            Assert.AreEqual(0, kraj.Stroski.Count);
        }

        [TestMethod]
        public void DodajStrosek_NeveljavenStrosek_NeSpremeniCounterjaId()
        {
            var servis = NovServis();
            var kraj = servis.DodajKraj("A", null, 0, 0);

            Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => servis.DodajStrosek(kraj.Id, "x", 0m));
            var veljaven = servis.DodajStrosek(kraj.Id, "y", 1m);

            Assert.AreEqual(1, veljaven.Id);
        }

        // ---------- IzbrisiStrosek (#34) ----------

        [TestMethod]
        public void IzbrisiStrosek_Obstaja_IzbriseInVrneTrue()
        {
            var servis = NovServis();
            var kraj = servis.DodajKraj("A", null, 0, 0);
            var s1 = servis.DodajStrosek(kraj.Id, "x", 10m);
            var s2 = servis.DodajStrosek(kraj.Id, "y", 20m);

            Assert.IsTrue(servis.IzbrisiStrosek(kraj.Id, s1.Id));
            Assert.AreEqual(1, kraj.Stroski.Count);
            Assert.AreSame(s2, kraj.Stroski[0]);
        }

        [TestMethod]
        public void IzbrisiStrosek_KrajNeObstaja_VrneFalse()
        {
            Assert.IsFalse(NovServis().IzbrisiStrosek(999, 1));
        }

        [TestMethod]
        public void IzbrisiStrosek_StrosekNeObstaja_VrneFalse()
        {
            var servis = NovServis();
            var kraj = servis.DodajKraj("A", null, 0, 0);
            servis.DodajStrosek(kraj.Id, "x", 10m);

            Assert.IsFalse(servis.IzbrisiStrosek(kraj.Id, 999));
            Assert.AreEqual(1, kraj.Stroski.Count);
        }

        [TestMethod]
        public void IzbrisiStrosek_StrosekDrugegaKraja_NeIzbrise()
        {
            var servis = NovServis();
            var a = servis.DodajKraj("A", null, 0, 0);
            var b = servis.DodajKraj("B", null, 0, 0);
            var strosekB = servis.DodajStrosek(b.Id, "x", 10m);

            Assert.IsFalse(servis.IzbrisiStrosek(a.Id, strosekB.Id));
            Assert.AreEqual(1, b.Stroski.Count);
        }

        [TestMethod]
        public void IzbrisiStrosek_DvakratIstiStrosek_DrugicVrneFalse()
        {
            var servis = NovServis();
            var kraj = servis.DodajKraj("A", null, 0, 0);
            var s = servis.DodajStrosek(kraj.Id, "x", 10m);

            Assert.IsTrue(servis.IzbrisiStrosek(kraj.Id, s.Id));
            Assert.IsFalse(servis.IzbrisiStrosek(kraj.Id, s.Id));
        }

        // ---------- SkupajStroski ----------

        [TestMethod]
        public void SkupajStroski_VecStroskov_VrneVsoto()
        {
            var servis = NovServis();
            var kraj = servis.DodajKraj("A", null, 0, 0);
            servis.DodajStrosek(kraj.Id, "x", 10.25m);
            servis.DodajStrosek(kraj.Id, "y", 4.75m);
            servis.DodajStrosek(kraj.Id, "z", 5m);

            Assert.AreEqual(20m, servis.SkupajStroski(kraj.Id));
        }

        [TestMethod]
        public void SkupajStroski_BrezStroskov_VrneNic()
        {
            var servis = NovServis();
            var kraj = servis.DodajKraj("A", null, 0, 0);

            Assert.AreEqual(0m, servis.SkupajStroski(kraj.Id));
        }

        [TestMethod]
        public void SkupajStroski_KrajNeObstaja_VrzeKeyNotFound()
        {
            Assert.ThrowsExactly<KeyNotFoundException>(() => NovServis().SkupajStroski(999));
        }

        [TestMethod]
        public void SkupajStroski_PoBrisanju_Posodobljeno()
        {
            var servis = NovServis();
            var kraj = servis.DodajKraj("A", null, 0, 0);
            var s1 = servis.DodajStrosek(kraj.Id, "x", 10m);
            servis.DodajStrosek(kraj.Id, "y", 5m);

            servis.IzbrisiStrosek(kraj.Id, s1.Id);

            Assert.AreEqual(5m, servis.SkupajStroski(kraj.Id));
        }

        [TestMethod]
        public void SkupajStroski_LoceniKraji_SeNeMesata()
        {
            var servis = NovServis();
            var a = servis.DodajKraj("A", null, 0, 0);
            var b = servis.DodajKraj("B", null, 0, 0);
            servis.DodajStrosek(a.Id, "x", 10m);
            servis.DodajStrosek(b.Id, "y", 99m);

            Assert.AreEqual(10m, servis.SkupajStroski(a.Id));
            Assert.AreEqual(99m, servis.SkupajStroski(b.Id));
        }
    }
}