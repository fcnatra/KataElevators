using Xunit;
using Elevators;
using FakeItEasy;

namespace Elevators.Tests
{
    public class WhenUsingController
    {
        [Fact]
        public void DownRequest_RegistersDownRequest()
        {
            // Arrange
            int destinationFloor = 5;
            bool hasPendingDownRequests = false;

            var fakeElevator = A.Fake<IElevator>();
            var controller = new Controller(fakeElevator);

            A.CallTo(() => fakeElevator.GoToFloor(destinationFloor))
                .Invokes(() => hasPendingDownRequests = controller.HasPendingDownRequestForFloor(destinationFloor));

            // Act
            controller.PressCallDownButton(destinationFloor);

            // Assert
            Assert.True(hasPendingDownRequests);
        }
        
        [Fact]
        public void DownRequestMovesElevatorToTheDesiredFloor()
        {
            // Arrange
            var elevator = new Elevator(0, 10);
            var controller = new Controller(elevator);
            elevator.GoToFloor(7);

            // Act
            controller.PressCallDownButton(5);

            // Assert
            Assert.Equal(5, elevator.CurrentFloor);
            Assert.False(controller.HasPendingDownRequestForFloor(5));
        }

        [Fact]
        public void UpRequestMovesElevatorToTheDesiredFloor()
        {
            // Arrange
            var elevator = new Elevator(0, 10);
            var controller = new Controller(elevator);

            // Act
            controller.PressCallUpButton(3);

            // Assert
            Assert.Equal(3, elevator.CurrentFloor);
            Assert.False(controller.HasPendingUpRequestForFloor(3));
        }

        [Fact]
        public void UserPressesUpButton_RegistersUpRequest()
        {
            // Arrange
            int destinationFloor = 3;
            var hasPendingUpRequests = false;

            var fakeElevator = A.Fake<IElevator>();
            var controller = new Controller(fakeElevator);

            A.CallTo(() => fakeElevator.GoToFloor(destinationFloor))
                .Invokes(() => hasPendingUpRequests = controller.HasPendingUpRequestForFloor(destinationFloor));

            // Act
            controller.PressCallUpButton(destinationFloor);

            // Assert
            Assert.True(hasPendingUpRequests);
        }
    }
}
