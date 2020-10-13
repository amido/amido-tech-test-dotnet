using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using RestSharp;
using System;
using System.Linq;
using System.Net;
using TechTalk.SpecFlow;

namespace Amido.TechTest.NetCore.Specflow
{
    [Binding]
    public class UsersTestWithSpecFlowSteps
    {
        private static string baseUrl = "https://amido-tech-test.herokuapp.com";
        private static string resource = "users";
        private readonly ScenarioContext scenarioContext;

        public UsersTestWithSpecFlowSteps(ScenarioContext scenarioContext)
        {
            this.scenarioContext = scenarioContext;
        }


        [BeforeScenario]
        public static void SetupRestClient(ScenarioContext scenarioContext)
        {
            var restClient = new RestClient(baseUrl);
            var token = GetBearerToken(restClient);

            restClient.AddDefaultHeader("Authorization", $"bearer {token}");
            scenarioContext.Add("RestClient", restClient);
        }

        [AfterScenario]
        public static void DeleteUser(ScenarioContext scenarioContext)
        {
            if (scenarioContext.ContainsKey("User"))
            {
                var joe = scenarioContext.Get<User>("User");

                if (joe.Id != null)
                {
                    var request = new RestRequest($"{resource}/{joe.Id}", Method.DELETE);

                    var restClient = scenarioContext.Get<RestClient>("RestClient");
                    var response = restClient.Execute(request);

                    Assert.AreEqual(HttpStatusCode.NoContent, response.StatusCode);
                }
            }
        }
        [Given(@"a new user")]
        public void GivenANewUser()
        {
            var joe = new User()
            {
                Name = "Joe",
                Password = "MyCurrentPassword"
            };
            scenarioContext.Add("User", joe);
        }
        
        [Given(@"an existing user with an updated password")]
        public void GivenAnExistingUserWithAnUpdatedPassword()
        {
            var joe = new User()
            {
                Name = "Joe",
                Password = "MyCurrentPassword"
            };

            var json = JsonConvert.SerializeObject(joe);

            var request = new RestRequest(resource, Method.POST);
            request.AddParameter("application/json", json, ParameterType.RequestBody);

            var restClient = scenarioContext.Get<RestClient>("RestClient");
            var response = restClient.Execute(request);

            var location = response.Headers.FirstOrDefault(h => h.Name == "Location").Value.ToString();

            joe.Id = new Uri(location).Segments.LastOrDefault();
            joe.Password = "MyNewPassword";

            scenarioContext.Add("User", joe);
        }
        
        [When(@"I request to create a user")]
        public void WhenIRequestToCreateAUser()
        {
            var joe = scenarioContext.Get<User>("User");
            var json = JsonConvert.SerializeObject(joe);

            var request = new RestRequest(resource, Method.POST);
            request.AddParameter("application/json", json, ParameterType.RequestBody);

            var restClient = scenarioContext.Get<RestClient>("RestClient");
            var response = restClient.Execute(request);

            scenarioContext.Add("Response", response);
        }
        
        [When(@"I request to update a user")]
        public void WhenIRequestToUpdateAUser()
        {
            var joe = scenarioContext.Get<User>("User");
            var json = JsonConvert.SerializeObject(joe);

            var request = new RestRequest($"{resource}/{joe.Id}", Method.PUT);
            request.AddParameter("application/json", json, ParameterType.RequestBody);

            var restClient = scenarioContext.Get<RestClient>("RestClient");
            var response = restClient.Execute(request);

            scenarioContext.Add("Response", response);
        }
        
        [Then(@"I should get a user id for the new user")]
        public void ThenIShouldGetAUserIdForTheNewUser()
        {
            var response = scenarioContext.Get<IRestResponse>("Response");
            Assert.AreEqual(HttpStatusCode.Created, response.StatusCode);

            var location = response.Headers.FirstOrDefault(h => h.Name == "Location").Value.ToString();
            var userId = new Uri(location).Segments.LastOrDefault();

            Assert.IsNotEmpty(userId, "Could not get user Id form the response.");

            var joe = scenarioContext.Get<User>("User");
            joe.Id = userId;
            scenarioContext["User"] = joe;
        }
        
        [Then(@"the user's password should be updated")]
        public void ThenTheUserSPasswordShouldBeUpdated()
        {
            var response = scenarioContext.Get<IRestResponse>("Response");
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);

            var joe = scenarioContext.Get<User>("User");
            var joeResponse = JsonConvert.DeserializeObject<User>(response.Content);
            Assert.AreEqual(joe.Password, joeResponse.Password);
        }

        private static string GetBearerToken(RestClient restClient)
        {
            var request = new RestRequest("token");
            var response = restClient.Execute(request);

            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);

            var token = JObject.Parse(response.Content)["token"]?.ToString();
            Assert.That(token, Is.Not.Null.Or.Empty, "Could not get token form the response.");

            return token;
        }
    }
}
