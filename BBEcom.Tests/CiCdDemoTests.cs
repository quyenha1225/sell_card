namespace BBEcom.Tests
{
    [TestClass]
    public sealed class CiCdDemoTests
    {
        [TestMethod]
        public void CalculateTotal_TwoCards_ShouldReturnCorrectTotal()
        {
            const decimal pricePerCard = 100000m;
            const int quantity = 2;
            const decimal expectedTotal = 200000m;

            // DEMO ONLY: deliberately add an incorrect 10,000 to the total.
            // To make CI pass again, remove "+ 10000m" or delete this demo file.
            decimal actualTotal = pricePerCard * quantity + 10000m;

            Assert.AreEqual(expectedTotal, actualTotal,
                "CICD DEMO: incorrect order total must stop deployment.");
        }
    }
}
