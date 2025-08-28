using Xunit;
using Elevators;

namespace Elevators.Tests
{
    public class WhenUsingController
    {
        [Fact]
        public void UserPressesUpButton_RegistersUpRequest()
        {
            // Arrange
            var controller = new Controller();

            // Act
            controller.PressUpButton(3);

            // Assert
            Assert.True(controller.HasPendingUpRequestForFloor(3));
        }
    }
}
