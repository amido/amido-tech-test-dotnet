using Xunit;
using System.Net.Http;
using Xunit.Abstractions;
using System.Threading.Tasks;
using System;
using System.Net;
using System.Linq;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;

namespace Amido.TechTest.NetCore.XUnit
{
    public class UsersTestWithHttpClient : IDisposable
    {
        private static HttpClient httpClient;
        private static string baseUrl = "https://amido-tech-test.herokuapp.com/";
        private string resource = "users";
        private string userId = "";

        public UsersTestWithHttpClient() 
        {
            httpClient = new HttpClient()
            {
                BaseAddress = new Uri(baseUrl)
            };
        }

        public void Dispose()
        {
            DisposeAsync();
        }
        

        [Fact]
        public async Task HttpClient_CreateUser()
        {
            string token = await GetBearerToken();
            httpClient.DefaultRequestHeaders.Add("Authorization", $"bearer {token}");

            var joe = new
            {
                name = "Joe",
                password = "MyCurrentPassword"
            };

            var response = await httpClient.PostAsJsonAsync(resource, joe);
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);

            // grab the user id from the Location header
            var location = response.Headers.Location;
            userId = location.Segments.LastOrDefault();

            Assert.NotNull(userId);
        }

        [Fact]
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
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            // check the Joe's password was actually updated!
            var content = await response.Content.ReadAsStringAsync();
            var joeResponse = JsonConvert.DeserializeAnonymousType(content, joe);
            Assert.Equal(joe.password, joeResponse.password);
        }



        private static async Task<string> GetBearerToken()
        {
            // acquire a bearer token from the /token operation  
            var response = await httpClient.GetAsync("token");
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var content = await response.Content.ReadAsStringAsync();
            var token = JObject.Parse(content)["token"]?.ToString();
            Assert.NotNull(token);

            return token;
        }

        public async Task DisposeAsync()
        {
            if (userId != "")
            {
                var response = await httpClient.DeleteAsync($"{resource}/{userId}");
                Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
            }
        }
    }
}
