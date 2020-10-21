using Xbehave;

namespace Amido.TechTest.NetCore.XBehave
{
    public class UsersTestWithXBehave
    {
        private readonly Fixture fixture;

        public UsersTestWithXBehave()
        {
            fixture = new Fixture();
        }

        [Scenario]
        public void CreateUser()
        {
            "Given there is a request to create a new user".x(() => fixture.GivenAUserIsCreated());
            "Then there should be a user id for the new user".x(() => fixture.ThenAUserIdShouldExist());
        }

        [Scenario]
        public void UpdateUser()
        {
            "Given there is an existing user".x(() => fixture.GivenAUserIsCreated());
            "Then a request to update that user's pasword should be successful".x(() => fixture.RequestToUpdateUserIsSuccessful());
        }
    }
}
