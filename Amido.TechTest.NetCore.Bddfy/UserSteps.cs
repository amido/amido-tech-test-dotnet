using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;

namespace Amido.TechTest.NetCore.Bddfy
{
    internal class UserSteps : IDisposable
    {
        private static HttpClient httpClient;
        private static string baseUrl = "https://amido-tech-test.herokuapp.com/";
        private string resource = "users";
        private string userId = "";
        private static StepsContext stepsContext;

        public UserSteps()
        {
            stepsContext = new StepsContext();

            httpClient = new HttpClient()
            {
                BaseAddress = new Uri(baseUrl)
            };
        }

        public void Dispose()
        {
            DisposeAsync();
        }

        public void CreateNewUser()
        {
            var joe = new User()
            {
                Name = "Joe",
                Password = "MyCurrentPassword"
            };
            
            stepsContext.Current.Add("User", joe);
        }

        public async Task RequestToCreateAUserAsync()
        {
            string token = await GetBearerToken();
            httpClient.DefaultRequestHeaders.Add("Authorization", $"bearer {token}");


            var joe = stepsContext.Current["User"];
            var response = await httpClient.PostAsJsonAsync(resource, joe);
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);

            var location = response.Headers.Location;
            userId = location.Segments.LastOrDefault();
        }
        public void GetAUserIdForTheNewUser()
        {
            Assert.NotNull(userId);
        }

        public async Task RequestToChangePasswordAsync()
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

            stepsContext.Current.Add("joe", joe);
            stepsContext.Current.Add("joeResponse", joeResponse);
        }

        public void GetAUserPasswordUpdated()
        {
            var joe = stepsContext.Current["joe"];
            var joeResponse = stepsContext.Current["joeResponse"];
            Assert.Equal(joe, joeResponse);
        }

        private static async Task<string> GetBearerToken()
        {
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
                stepsContext.Current.Clear();
            }
        }
    }
}