using Xunit;
using Elevators;
using FakeItEasy;

namespace Elevators.Tests
{
    public class WhenUsingController
    {
        [Fact]
        public void ControllerProcessesUpRequestAndMovesElevator()
        {
            // Arrange
            var elevator = new Elevator(0, 10);
            var controller = new Controller(elevator);

            // Act
            controller.PressUpButton(3);

            // Assert
            Assert.Equal(3, elevator.CurrentFloor);        
            Assert.False(controller.HasPendingUpRequestForFloor(3));
        }

        [Fact]
        public void UserPressesUpButton_RegistersUpRequest()
        {
            // Arrange
            int destinationFloor = 3;
            var elevator = A.Fake<IElevator>();
            var hasPendingUpRequests = false;
            var controller = new Controller(elevator);

            A.CallTo(() => elevator.GoToFloor(destinationFloor))
                .Invokes(() => hasPendingUpRequests = controller.HasPendingUpRequestForFloor(destinationFloor));

            // Act
            controller.PressUpButton(3);

            // Assert
            Assert.True(hasPendingUpRequests);
        }
    }
}
