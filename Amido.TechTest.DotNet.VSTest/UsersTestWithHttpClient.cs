using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace Amido.TechTest.DotNet
{
    /// <summary>
    /// Tests the /users path using HttpClient extended with MS ASP.Net Web API Client and VSTest
    /// </summary>
    [TestClass]
    public class UsersTestWithHttpClient
    {
        private static HttpClient httpClient;
        private static string baseUrl = "https://amido-tech-test.herokuapp.com/";
        private string resource = "users";
        private string userId = "";

        [ClassInitialize]
        public static async Task Setup(TestContext testContext)
        {
            httpClient = new HttpClient()
            {
                BaseAddress = new Uri(baseUrl)
            };

            string token = await GetBearerToken();
            httpClient.DefaultRequestHeaders.Add("Authorization", $"bearer {token}");
        }

        [TestCleanup]
        public async Task DeleteUser()
        {
            // clean up any user created during tests
            if (userId != "")
            {
                var response = await httpClient.DeleteAsync($"{resource}/{userId}");
                Assert.AreEqual(HttpStatusCode.NoContent, response.StatusCode);
            }
        }

        [TestMethod]
        public async Task HttpClient_CreateUser()
        {
            var joe = new
            {
                name = "Joe",
                password = "MyCurrentPassword"
            };

            var response = await httpClient.PostAsJsonAsync(resource, joe);
            Assert.AreEqual(HttpStatusCode.Created, response.StatusCode);

            // grab the user id from the Location header
            var location = response.Headers.Location;
            userId = location.Segments.LastOrDefault();

            Assert.IsNotNull(userId, "Could not get user Id form the response.");
        }

        [TestMethod]
        public async Task HttpClient_UpdatePassword()
        {
            // ideally you would have known user to update as this creates a dependancy on user creation
            if (userId == "")
                await HttpClient_CreateUser();

            var joe = new
            {
                name = "Joe",
                password = "MyNewPassword"
            };

            var response = await httpClient.PutAsJsonAsync($"{resource}/{userId}", joe);
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);

            // check the Joe's password was actually updated!
            var content = await response.Content.ReadAsStringAsync();
            var joeResponse = JsonConvert.DeserializeAnonymousType(content, joe);
            Assert.AreEqual(joe.password, joeResponse.password);
        }

        private static async Task<string> GetBearerToken()
        {
            // acquire a bearer token from the /token operation  
            var response = await httpClient.GetAsync("token");
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);

            var content = await response.Content.ReadAsStringAsync();
            var token = JObject.Parse(content)["token"]?.ToString();
            Assert.IsNotNull(token, "Could not get token form the response.");

            return token;
        }
    }
}
