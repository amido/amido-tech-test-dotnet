using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using RestSharp;
using System;
using System.Linq;
using System.Net;
using TechTalk.SpecFlow;

namespace Amido.TechTest.DotNet
{
    [Binding]
    public class UsersTestWithSpecFlowSteps
    {
        private static string baseUrl = "https://amido-tech-test.herokuapp.com";
        private static string resource = "users";

        [BeforeFeature]
        public static void SetupRestClient()
        {
            var restClient = new RestClient(baseUrl);
            var token = GetBearerToken(restClient);

            restClient.AddDefaultHeader("Authorization", $"bearer {token}");
            FeatureContext.Current.Add("RestClient", restClient);
        }

        [AfterScenario]
        public static void DeleteUser()
        {
            if (ScenarioContext.Current.ContainsKey("User"))
            {
                var joe = ScenarioContext.Current.Get<User>("User");

                // clean up any user created during tests
                if (joe.Id != null)
                {
                    var request = new RestRequest($"{resource}/{joe.Id}", Method.DELETE);

                    var restClient = FeatureContext.Current.Get<RestClient>("RestClient");
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

            ScenarioContext.Current.Add("User", joe);
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

            var restClient = FeatureContext.Current.Get<RestClient>("RestClient");
            var response = restClient.Execute(request);

            var location = response.Headers.FirstOrDefault(h => h.Name == "Location").Value.ToString();

            joe.Id = new Uri(location).Segments.LastOrDefault();
            joe.Password = "MyNewPassword";

            ScenarioContext.Current.Add("User", joe);
        }
        
        [When(@"I request to create a user")]
        public void WhenIRequestToCreateAUser()
        {
            var joe = ScenarioContext.Current.Get<User>("User");
            var json = JsonConvert.SerializeObject(joe);

            var request = new RestRequest(resource, Method.POST);
            request.AddParameter("application/json", json, ParameterType.RequestBody);

            var restClient = FeatureContext.Current.Get<RestClient>("RestClient");
            var response = restClient.Execute(request);

            ScenarioContext.Current.Add("Response", response);
        }
        
        [When(@"I request to update a user")]
        public void WhenIRequestToUpdateAUser()
        {
            var joe = ScenarioContext.Current.Get<User>("User");
            var json = JsonConvert.SerializeObject(joe);

            var request = new RestRequest($"{resource}/{joe.Id}", Method.PUT);
            request.AddParameter("application/json", json, ParameterType.RequestBody);

            var restClient = FeatureContext.Current.Get<RestClient>("RestClient");
            var response = restClient.Execute(request);

            ScenarioContext.Current.Add("Response", response);
        }
        
        [Then(@"I should get a user id for the new user")]
        public void ThenIShouldGetAUserIdForTheNewUser()
        {
            var response = ScenarioContext.Current.Get<IRestResponse>("Response");
            Assert.AreEqual(HttpStatusCode.Created, response.StatusCode);

            // grab the user id from the Location header
            var location = response.Headers.FirstOrDefault(h => h.Name == "Location").Value.ToString();
            var userId = new Uri(location).Segments.LastOrDefault();

            Assert.IsNotEmpty(userId, "Could not get user Id form the response.");

            // for cleaning up
            var joe = ScenarioContext.Current.Get<User>("User");
            joe.Id = userId;
            ScenarioContext.Current["User"] = joe;
        }
        
        [Then(@"the user's password should be updated")]
        public void ThenTheUsersPasswordShouldBeUpdated()
        {
            var response = ScenarioContext.Current.Get<IRestResponse>("Response");
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);

            // check the Joe's password was actually updated!
            var joe = ScenarioContext.Current.Get<User>("User");
            var joeResponse = JsonConvert.DeserializeObject<User>(response.Content);
            Assert.AreEqual(joe.Password, joeResponse.Password);
        }

        private static string GetBearerToken(RestClient restClient)
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
