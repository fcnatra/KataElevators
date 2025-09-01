using Xunit;
using Elevators;
using FakeItEasy;
using Microsoft.VisualBasic;

namespace Elevators.Tests
{
    public class WhenUsingController
    {
        [Fact]
        public void SelectingDestinationFloor_AddsToCorrectRequestList()
        {
            // Arrange
            var elevator = new Elevator(0, 10);

            var controller = new Controller(elevator);

            controller.SelectDestinationFloor(2);

            // Act
            controller.SelectDestinationFloor(8);

            // Assert
            Assert.True(controller.HasPendingRequestForFloor(8));
        }

        [Fact]
        public async Task WhenElevatorReachesUpRequestedFloor_DoorsAreOpened()
        {
            // Arrange
            int requestedFloor = 4;
            int floorReached = 0;
            var doorsOpened = false;
            var tcs = new TaskCompletionSource();

            var elevator = new Elevator(0, 10);
            elevator.OnAfterStop += (floor) => { floorReached = floor; tcs.SetResult(); };
            elevator.OnDoorsOpened += () => doorsOpened = true;

            var controller = new Controller(elevator);

            // Act
            controller.SelectDestinationFloor(requestedFloor);

            // Assert
            await tcs.Task;
            Assert.Equal(floorReached, requestedFloor);
            Assert.True(doorsOpened);
        }

        [Fact]
        public void DownRequest_RegistersDownRequest()
        {
            // Arrange
            int destinationFloor = 5;

            var fakeElevator = A.Fake<IElevator>();
            var controller = new Controller(fakeElevator);

            // Act
            controller.SelectDestinationFloor(destinationFloor);

            // Assert
            Assert.True(controller.HasPendingRequestForFloor(destinationFloor));
        }
        
        [Fact]
        public async Task DownRequestMovesElevatorToTheDesiredFloor()
        {
            // Arrange
            var tcs = new TaskCompletionSource();

            var elevator = new Elevator(0, 10);
            elevator.OnDoorsOpened += () => tcs.SetResult();

            var controller = new Controller(elevator);

            // first, go up
            controller.SelectDestinationFloor(7);
            await tcs.Task;
            tcs = new TaskCompletionSource();

            // Act
            controller.SelectDestinationFloor(5);

            // Assert
            await tcs.Task;
            Assert.Equal(5, elevator.CurrentFloor);
            Assert.False(controller.HasPendingRequestForFloor(5));
        }

        [Fact]
        public async Task UpRequestMovesElevatorToTheDesiredFloor()
        {
            // Arrange
            var tcs = new TaskCompletionSource();
            var elevator = new Elevator(0, 10);
            elevator.OnDoorsOpened += () => tcs.SetResult();

            var controller = new Controller(elevator);

            // Act
            controller.SelectDestinationFloor(3);

            // Assert
            await tcs.Task;
            Assert.Equal(3, elevator.CurrentFloor);
            Assert.False(controller.HasPendingRequestForFloor(3));
        }

        [Fact]
        public void UpRequestButton_RegistersUpRequest()
        {
            // Arrange
            int destinationFloor = 3;
            var hasPendingUpRequests = false;

            var fakeElevator = A.Fake<IElevator>();
            var controller = new Controller(fakeElevator);

            A.CallTo(() => fakeElevator.GoToFloor(destinationFloor))
                .Invokes(() => hasPendingUpRequests = controller.HasPendingRequestForFloor(destinationFloor));

            // Act
            controller.SelectDestinationFloor(destinationFloor);

            // Assert
            Assert.True(hasPendingUpRequests);
        }
    }
}
