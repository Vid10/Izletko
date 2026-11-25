using Microsoft.VisualStudio.TestTools.UnitTesting;
using Projekt.Services;

namespace TestIzletko1
{
	[TestClass]
	public class TestDistanceCalculator
	{
		private const double EarthRadiusKm = 6371.0;
		private const double Delta = 0.001;

		[TestMethod]
		public void Test_HaversineKm_IstaTocka_VrneNic()
		{
			var km = DistanceCalculator.HaversineKm(46.0569, 14.5058, 46.0569, 14.5058);

			Assert.AreEqual(0.0, km, Delta);
		}

		[TestMethod]
		public void Test_HaversineKm_IshodiscePrekrivanje_VrneNic()
		{
			Assert.AreEqual(0.0, DistanceCalculator.HaversineKm(0, 0, 0, 0), Delta);
		}

		[TestMethod]
		public void Test_HaversineKm_StopinjaSirine_Je111Km()
		{
			// 1° po meridianu = R * pi / 180
			var pricakovano = EarthRadiusKm * Math.PI / 180.0;

			var km = DistanceCalculator.HaversineKm(0, 0, 1, 0);

			Assert.AreEqual(pricakovano, km, Delta);
		}

		[TestMethod]
		public void Test_HaversineKm_CetrtinaEkvatorja_JeCetrtinaObsega()
		{
			// 90° po ekvatorju = R * pi / 2
			var pricakovano = EarthRadiusKm * Math.PI / 2.0;

			var km = DistanceCalculator.HaversineKm(0, 0, 0, 90);

			Assert.AreEqual(pricakovano, km, Delta);
		}

		[TestMethod]
		public void Test_HaversineKm_NasprotnaTockaNaEkvatorju_JePolObsega()
		{
			var pricakovano = EarthRadiusKm * Math.PI;

			var km = DistanceCalculator.HaversineKm(0, 0, 0, 180);

			Assert.AreEqual(pricakovano, km, Delta);
		}

		[TestMethod]
		public void Test_HaversineKm_OdPolaDoPola_JePolObsega()
		{
			var pricakovano = EarthRadiusKm * Math.PI;

			var km = DistanceCalculator.HaversineKm(90, 0, -90, 0);

			Assert.AreEqual(pricakovano, km, Delta);
		}

		[TestMethod]
		public void Test_HaversineKm_LjubljanaMaribor_OkoliStoKm()
		{
			var km = DistanceCalculator.HaversineKm(46.0569, 14.5058, 46.5547, 15.6459);

			Assert.AreEqual(103.6, km, 0.5);
		}

		[TestMethod]
		public void Test_HaversineKm_LondonParis_OkoliTristoKm()
		{
			var km = DistanceCalculator.HaversineKm(51.5074, -0.1278, 48.8566, 2.3522);

			Assert.IsTrue(km >= 340 && km <= 347, $"Razdalja {km} km je izven pričakovanega območja.");
		}

		[TestMethod]
		public void Test_HaversineKm_JeSimetricna()
		{
			var ab = DistanceCalculator.HaversineKm(46.0569, 14.5058, 48.2082, 16.3738);
			var ba = DistanceCalculator.HaversineKm(48.2082, 16.3738, 46.0569, 14.5058);

			Assert.AreEqual(ab, ba, 0.000001);
		}

		[TestMethod]
		public void Test_HaversineKm_NegativneKoordinate_DelujejoPravilno()
		{
			// Južna in zahodna polobla: Buenos Aires -> Santiago (približno 1140 km)
			var km = DistanceCalculator.HaversineKm(-34.6037, -58.3816, -33.4489, -70.6693);

			Assert.IsTrue(km >= 1100 && km <= 1200, $"Razdalja {km} km je izven pričakovanega območja.");
		}

		[TestMethod]
		public void Test_HaversineKm_PrehodCezDatumskoMejo_VrneKratkoRazdaljo()
		{
			// 179° in -179° sta narazen le 2° po ekvatorju
			var pricakovano = EarthRadiusKm * 2 * Math.PI / 180.0;

			var km = DistanceCalculator.HaversineKm(0, 179, 0, -179);

			Assert.AreEqual(pricakovano, km, Delta);
		}

		[TestMethod]
		public void Test_HaversineKm_RezultatJeNikoliNegativen()
		{
			var km = DistanceCalculator.HaversineKm(10, 20, -30, -40);

			Assert.IsTrue(km >= 0);
		}
	}
}