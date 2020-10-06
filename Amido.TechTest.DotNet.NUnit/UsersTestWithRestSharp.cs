using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using RestSharp;
using System;
using System.Linq;
using System.Net;

namespace Amido.TechTest.DotNet
{
    [TestFixture]
    [Description("Tests the /users path using RestSharp and NUnit")]
    public class UsersTestWithRestSharp
    {
        private RestClient restClient;
        private string baseUrl = "https://amido-tech-test.herokuapp.com";
        private string resource = "users";
        private string userId = "";
        
        [OneTimeSetUp]
        public void Setup()
        {
            restClient = new RestClient(baseUrl);

            // add a token and add to the request headers for all future requests
            var token = GetBearerToken();
            restClient.AddDefaultHeader("Authorization", $"Bearer {token}");
        }

        [TearDown]
        public void DeleteUser()
        {
            // clean up any user created during tests
            if (userId != "")
            {
                var request = new RestRequest($"{resource}/{userId}", Method.DELETE);
                var response = restClient.Execute(request);

                Assert.AreEqual(HttpStatusCode.NoContent, response.StatusCode);
            }
        }

        [Test]
        public void RestSharp_CreateUser()
        {
            // use an anonymous type to quickly generate a user object
            // alternatively you could create a separate class object
            var joe = new
            {
                name = "Joe",
                password = "MyCurrentPassword"
            };

            var request = new RestRequest(resource, Method.POST);
            request.AddJsonBody(joe);
            var response = restClient.Execute(request);

            Assert.AreEqual(HttpStatusCode.Created, response.StatusCode);

            // grab the user id from the Location header
            var location = response.Headers.FirstOrDefault(h => h.Name == "Location").Value.ToString();
            userId = new Uri(location).Segments.LastOrDefault();

            Assert.IsNotEmpty(userId, "Could not get user Id form the response.");
        }

        [Test]
        public void RestSharp_UpdatePassword()
        {  
            // ideally you would have known user to update as this creates a dependancy on user creation
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

            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);

            // check the Joe's password was actually updated!
            var joeResponse = JsonConvert.DeserializeAnonymousType(response.Content, joe);
            Assert.AreEqual(joe.password, joeResponse.password);
        }

        private string GetBearerToken()
        {
            // acquire a bearer token from the /token operation  
            var request = new RestRequest("token");
            var response = restClient.Execute(request);

            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);

            var token = JObject.Parse(response.Content)["token"]?.ToString();
            Assert.That(token, Is.Not.Null.Or.Empty, "Could not get token form the response.");

            return token;
        }
    }
}
