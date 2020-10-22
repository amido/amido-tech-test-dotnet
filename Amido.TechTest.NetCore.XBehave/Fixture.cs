using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Amido.TechTest.NetCore.XBehave
{
    public class Fixture : IDisposable
    {
        private static HttpClient httpClient;
        private static string baseUrl = "https://amido-tech-test.herokuapp.com/";
        private string resource = "users";
        private string userId = "";

        public Fixture()
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


        public async Task GivenAUserIsCreated()
        {
            string token = await GetBearerToken();
            httpClient.DefaultRequestHeaders.Add("Authorization", $"bearer {token}");

            var joe = new 
            {
                Name = "Joe",
                Password = "MyCurrentPassword"
            };

            var response = await httpClient.PostAsJsonAsync(resource, joe);
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);

            // grab the user id from the Location header
            var location = response.Headers.Location;
            userId = location.Segments.LastOrDefault();
        }

        public void ThenAUserIdShouldExist()
        {
            Assert.NotNull(userId);
        }

        public async void RequestToUpdateUserIsSuccessful()
        {
            var joe = new
            {
                name = "Joe",
                password = "MyNewPassword"
            };

            var response = await httpClient.PutAsJsonAsync($"{resource}/{userId}", joe);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
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
