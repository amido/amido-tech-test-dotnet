using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RestSharp;
using System;
using System.Linq;
using System.Net;
using Xunit;

namespace Amido.TechTest.NetCore.XUnit
{
    public class UsersTestWithRestSharp : IDisposable
    {
        private RestClient restClient;
        private static string baseUrl = "https://amido-tech-test.herokuapp.com/";
        private string resource = "users";
        private string userId = "";

        public UsersTestWithRestSharp()
        {
            restClient = new RestClient(baseUrl);

            var token = GetBearerToken();
            restClient.AddDefaultHeader("Authorization", $"Bearer {token}");
        }

        public void Dispose()
        {
            if (userId != "")
            {
                var request = new RestRequest($"{resource}/{userId}", Method.DELETE);
                var response = restClient.Execute(request);

                Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
            }
        }

        [Fact]
        public void RestSharp_CreateUser()
        {
            var joe = new
            {
                name = "Joe",
                password = "MyCurrentPassword"
            };

            var request = new RestRequest(resource, Method.POST);
            request.AddJsonBody(joe);
            var response = restClient.Execute(request);

            Assert.Equal(HttpStatusCode.Created, response.StatusCode);

            var location = response.Headers.FirstOrDefault(h => h.Name == "Location").Value.ToString();
            userId = new Uri(location).Segments.LastOrDefault();

            Assert.NotEmpty(userId);
        }

        [Fact]
        public void RestSharp_UpdatePassword()
        {
            if (userId == "")
                RestSharp_CreateUser();

            var joe = new
            {
                name = "Joe",
                password = "MyNewPassword"
            };

            var request = new RestRequest($"{resource}/{userId}", Method.PUT);
            request.AddJsonBody(joe);
            var response = restClient.Execute(request);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var joeResponse = JsonConvert.DeserializeAnonymousType(response.Content, joe);
            Assert.Equal(joe.password, joeResponse.password);
        }

        private string GetBearerToken()
        {
            var request = new RestRequest("token");
            var response = restClient.Execute(request);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var token = JObject.Parse(response.Content)["token"]?.ToString();
            Assert.NotEmpty(token);

            return token;
        }
    }
}
