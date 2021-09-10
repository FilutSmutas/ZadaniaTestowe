using AventStack.ExtentReports;
using NUnit.Framework;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Firefox;
using OpenQA.Selenium.Safari;
using OpenQA.Selenium.Support.UI;
using System;
using System.Collections.ObjectModel;
using System.Text.RegularExpressions;
using Zadania;

namespace ZadaniaTestowe
{
    [TestFixture(typeof(FirefoxDriver))]
    [TestFixture(typeof(ChromeDriver))]
    public class Zadanie2<TWebDriver> where TWebDriver : IWebDriver, new()
    {
        IWebDriver driver;
        ExtentReports rep = ExtentBuilder.GetExtent();


        [SetUp]
        public void Init()
        {
            driver = new TWebDriver();
            //driver = new ChromeDriver();
            driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(10);
            driver.Url = "https://www.google.pl/maps/";

            //Jeśli nastąpi przekierowanie na stronę zgody na pliki cookies, naciśnij
            //przysick zgody żeby przejść do strony z mapami
            if (driver.Url.Contains("consent"))
                driver.FindElement(By.XPath("//form//button")).Click();

        }

        [Test(Description = "Check distance and time of travel between two places in Google Maps")]
        [TestCase("Chłodna 51 Warszawa", "Plac Defilad 1 Warszawa", "pieszo", 40, 3)]
        [TestCase("Chłodna 51 Warszawa", "Plac Defilad 1 Warszawa", "na rowerze", 15, 3)]
        [TestCase("Plac Defilad 1 Warszawa", "Chłodna 51 Warszawa", "pieszo", 40, 3)]
        [TestCase("Plac Defilad 1 Warszawa", "Chłodna 51 Warszawa", "na rowerze", 15, 3)]
        public void CheckDistanceAndTimeOfTravel(string beginPlace, string destinationPlace,
            string typeOfTransport, int ExpectedMaxTime, int ExpectedMaxDistance)
        {
            
            var test = rep.CreateTest(beginPlace + ">" + destinationPlace + " " + typeOfTransport,
                "Check if route from " + beginPlace + " to " + destinationPlace + " on " + typeOfTransport
                + " is shorter than " + ExpectedMaxDistance + " km and " + ExpectedMaxTime + " min");

            try
            {
                Assert.That(driver.Url, Does.Contain("google.pl/maps"));
                test.Pass("Go to google.pl/maps");

            }
            catch (Exception ex)
            {
                test.Fail("Wrong starting site");
                throw ex;
            }


            //Wpisanie miejsca docelowego i wciśnięcie enter w celu wyszukania go
            try
            {
                var destinationSearchBox = driver.FindElement(By.Id("searchboxinput"));
                destinationSearchBox.Clear();
                destinationSearchBox.SendKeys(destinationPlace);
                destinationSearchBox.SendKeys(Keys.Return);
                test.Pass("Insert destination into searchbox and press Enter");

            }
            catch (Exception ex)
            {
                test.Fail("SearchBox not found");
                test.Info(ex);
                throw ex;
            }

            //Wciśnięcie przycisku Wyznaczania trasy
            var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(15));
            wait.Until(d => d.FindElement(By.XPath("//button[@data-value='Wyznacz trasę']")));

            try
            {
                driver.FindElement(By.XPath("//button[@data-value='Wyznacz trasę']")).Click();
                test.Pass("Press button to designate the route");
            }
            catch (Exception ex)
            {
                test.Fail("Button for designating route not found");
                test.Info(ex);
                throw ex;
            }

            //Wpisanie miejsca startowego i wciśnięcie enter w celu wyszukania trasy
            try
            {
                var startPlaceTextBox = driver.FindElement(By.XPath("//div[@id = 'sb_ifc51']/input"));
                startPlaceTextBox.Clear();
                startPlaceTextBox.SendKeys(beginPlace);
                startPlaceTextBox.SendKeys(Keys.Return);
                test.Pass("Insert starting place and press enter");
            }
            catch (Exception ex)
            {
                test.Fail("Starting place search box not found");
                test.Info(ex);
                throw ex;
            }

            //Wybranie rodzaju transportu: pieszo lub na rowerze
            try
            {
                IWebElement typeOfTransportElement;
                switch (typeOfTransport)
                {
                    case "pieszo":
                        typeOfTransportElement = driver.FindElement(By.XPath("//button//img[@data-tooltip='Pieszo']"));
                        break;
                    case "na rowerze":
                        typeOfTransportElement = driver.FindElement(By.XPath("//button//img[@data-tooltip='Na rowerze']"));
                        break;
                    default:
                        throw new Exception("Undefined type of transport as an argument");
                }
                typeOfTransportElement.Click();
                test.Pass("Choose one of the types of transport");
            }
            catch (Exception ex)
            {
                test.Fail("Button for choosing type of ransport not found");
                test.Info(ex);
                throw ex;
            }

            //Znalezienie wszystkich elementów div, z których każdy
            //zawiera dystans lub czas poszczególnej znalezionej trasy
            ReadOnlyCollection<IWebElement> timesAndDistances;
            try
            {
                timesAndDistances = driver.FindElements(By.XPath("//div[@class='xB1mrd-T3iPGc-trip-n5AaSd']//div"));
                Assert.That(timesAndDistances, Is.Not.Empty);
                test.Pass("Found at least one route");
            }
            catch (Exception ex)
            {
                test.Fail("Zero routes");
                test.Info(ex);
                throw ex;
            }

            //Sprawdzenie jak wiele ze znalezionych tras spełnia założone kryteria, jeśli przynajmniej jedna
            //trasa je spełnia, test jest zaliczony
            try
            {
                int numberOfRoutesThatPassesCriteria = 0;
                for (int i = 0; i < timesAndDistances.Count; i += 2)
                {
                    if (timeStringToAmountOfMinutes(timesAndDistances[i].Text) < ExpectedMaxTime &&
                    distanceStringToAmountOfKilometers(timesAndDistances[i + 1].Text) < ExpectedMaxTime)
                        numberOfRoutesThatPassesCriteria++;
                }
                Assert.That(numberOfRoutesThatPassesCriteria, Is.GreaterThan(0));
                test.Pass("Found at least one route that is shorter than "
                    + ExpectedMaxDistance + " km and " + ExpectedMaxTime + " minutes.");
            }
            catch (Exception ex)
            {
                test.Fail("None of the routes fulfills given criteria");
                test.Info(ex);
                throw ex;
            }

        }

        [TearDown]
        public void CleanUp()
        {
            driver.Close();
            rep.Flush();
        }

        //metoda zamieniająca tekst w formacie: \d* godz. \d* min na liczbę minut
        static private int timeStringToAmountOfMinutes(string time)
        {
            string pattern = @"\d* godz.";
            Regex rgx = new Regex(pattern);
            Match hours = rgx.Match(time);

            pattern = @"\d* min";
            rgx = new Regex(pattern);
            Match minutes = rgx.Match(time);

            return stringToInt(hours.Value) * 60 + stringToInt(minutes.Value);
        }

        //metoda zamieniająca tekst w formacie: \d*\,?\d* min na dystans wyrażony liczbą zmiennoprzecinkową
        static private float distanceStringToAmountOfKilometers(string distance)
        {
            string pattern = @"\d*\,?\d*";
            Regex rgx = new Regex(pattern);
            Match amountOfKilometers = rgx.Match(distance);
            TestContext.Out.WriteLine("!!!" + amountOfKilometers.Value);
            return float.Parse(amountOfKilometers.Value);
        }

        //metoda usuwająca ze stringa znaki niebędące liczbami
        static private int stringToInt(string s)
        {
            string b = string.Empty;
            int val = 0;

            for (int i = 0; i < s.Length; i++)
            {
                if (Char.IsDigit(s[i]))
                    b += s[i];
            }

            if (b.Length > 0)
                val = int.Parse(b);

            return val;
        }
    }
}
