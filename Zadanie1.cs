using AventStack.ExtentReports;
using NUnit.Framework;
using RestSharp;
using RestSharp.Serialization.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Zadania;

namespace ZadaniaTestowe
{
    [TestFixture]
    public class Zadanie1
    {
        ExtentReports rep = ExtentBuilder.GetExtent();

        [Test(Description="Check if Luke Skylwalker was from Tatooine")]
        public void CheckIfLukeIsFromTatooine()
        { 
            var test = rep.CreateTest("SWTest", "Check if Luke Skylwalker was from Tatooine");
            
            //Wyślij żądanie GET żeby pobrać informacje o Luke'u
            var client = new RestClient("https://swapi.dev/api/");
            var request = new RestRequest("people/1", Method.GET);
            test.Info("Send GET request to get data about Luke");
            var response = client.Execute(request);

            //Deserializacja odpowiedzi
            StarWarsHeroResponse swResponse = new JsonDeserializer().
                Deserialize<StarWarsHeroResponse>(response);

            //Sprawdź, czy udało się odczytać URI pod którym można odczytać informacje o planecie Luke'a
            try
            {
                Assert.That(swResponse.Homeworld, Is.Not.Empty);
                test.Pass("response contains information about Luke's HomeWorld");
            }catch(Exception ex)
            {
                test.Fail(ex);
                throw ex;
            }

            //Wyślij żądanie GET żeby pobrać informacje o planecie Luke'a
            request = new RestRequest(swResponse.Homeworld, Method.GET);
            test.Info("Send GET request to get data about Luke's HomeWorld");
            response = client.Execute(request);

            //Deserializacja odpowiedzi
            StarWarsPlanetResponse swPlanetResponse = new JsonDeserializer().
                Deserialize<StarWarsPlanetResponse>(response);

            //Sprawdź czy nazwa planety to Tatooine
            try
            {
                Assert.That(swPlanetResponse.Name, Is.EqualTo("Tatooine"));
                test.Pass("Luke's Homeworld is Tatooine");
            }
            catch(Exception ex)
            {
                test.Fail(ex);
                throw ex;
            }
  
        }

        [TearDown]
        public void CleanUp()
        {
            rep.Flush();
        }
    }

    public class StarWarsHeroResponse
    {
        public string Name { get; set; }
        public int Height { get; set; }
        public int Mass { get; set; }
        public string HairColor { get; set; }
        public string SkinColor { get; set; }
        public string EyeColor { get; set; }
        public string BirthYear { get; set; }
        public string Gender { get; set; }
        public string Homeworld { get; set; }
        public List<string> Films { get; set; }
        public List<string> Species { get; set; }
        public List<string> Vehicles { get; set; }
        public List<string> Starships { get; set; }
        public DateTime Created { get; set; }
        public DateTime Edited { get; set; }
        public string Url { get; set; }

    }

    public class StarWarsPlanetResponse
    {
        public string Name { get; set; }
        public int RotationPeriod { get; set; }
        public int OrbitalPeriod { get; set; }
        public int Diameter { get; set; }
        public string Climate { get; set; }
        public string Gravity { get; set; }
        public string Terrain { get; set; }
        public int SurfaceWater { get; set; }
        public int Population { get; set; }
        public List<string> Residents { get; set; }
        public List<string> Films { get; set; }
        public DateTime Created { get; set; }
        public DateTime Edited { get; set; }
        public string Url { get; set; }
    }

}
