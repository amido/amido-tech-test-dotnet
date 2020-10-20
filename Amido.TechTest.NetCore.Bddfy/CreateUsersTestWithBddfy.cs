using TestStack.BDDfy;
using Xunit;

namespace Amido.TechTest.NetCore.Bddfy
{
    [Story(AsA = "As a new user",
       IWant = "To request to create a user",
       SoThat = "So I should get a user id for the new user"
       )]
    public class CreateUsersTestWithBddfy
    {
        private readonly UserSteps userSteps;

        public CreateUsersTestWithBddfy()
        {
            userSteps = new UserSteps();
        }

        [Fact]
        public void Create_a_user()
        {
            this.Given(step => userSteps.GivenANewUser())
                .When(step => userSteps.WhenIRequestToCreateAUserAsync())
                .Then(step => userSteps.IShouldGetAUserIdForTheNewUser())
                .BDDfy();
        }
    }
}
