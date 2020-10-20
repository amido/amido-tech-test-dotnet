using TestStack.BDDfy;
using Xunit;

namespace Amido.TechTest.NetCore.Bddfy
{
    [Story(AsA = "As a existing user",
       IWant = "To update an existing password",
       SoThat = "this user password is updated"
       )]
    public class UpdateUsersTestWithBddfy
    {
        private readonly UserSteps userSteps;

        public UpdateUsersTestWithBddfy()
        {
            userSteps = new UserSteps();
        }

        [Fact]
        public void Update_user_password()
        {
            this.Given(step => userSteps.GivenAUserExists())
                .When(step => userSteps.WhenIRequestToChangePasswordAsync())
                .Then(step => userSteps.IShouldGetAUserPasswordUpdated())
                .BDDfy();
        }
    }
}
