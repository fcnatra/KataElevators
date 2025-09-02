using FakeItEasy;

namespace Elevators.Tests
{
    public class WhenUsingController
    {
        private Elevator _elevator;

        public WhenUsingController()
        {
            _elevator = new Elevator(0, 10);
            _elevator.SecondsPerFloor = 1;
        }

        [Fact]
        public async Task Controller_AttendsToAllThreeRequests_RegardlessOfOrder()
        {
            // Arrange
            var attendedFloors = new HashSet<int>();
            var tcs = new TaskCompletionSource();
            _elevator.OnAfterStop += (floor) =>
            {
                attendedFloors.Add(floor);
                if (attendedFloors.Count == 3) tcs.SetResult();
            };
            var controller = new Controller(_elevator);

            // Act
            controller.SelectDestinationFloor(6); // up
            controller.SelectDestinationFloor(1); // down (before reaching 6)
            controller.SelectDestinationFloor(3); // up (before reaching 6)

            // Assert
            await tcs.Task;
            Assert.Contains(1, attendedFloors);
            Assert.Contains(3, attendedFloors);
            Assert.Contains(6, attendedFloors);
        }

        [Fact]
        public void SelectingDestinationFloor_AddsToCorrectRequestList()
        {
            // Arrange
            var controller = new Controller(_elevator);

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

            _elevator.OnAfterStop += (floor) => { floorReached = floor; tcs.SetResult(); };
            _elevator.OnDoorsOpened += () => doorsOpened = true;

            var controller = new Controller(_elevator);

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
            _elevator.OnDoorsOpened += () => tcs.SetResult();

            var controller = new Controller(_elevator);

            // first, go up
            controller.SelectDestinationFloor(7);
            await tcs.Task;
            tcs = new TaskCompletionSource();

            // Act
            controller.SelectDestinationFloor(5);

            // Assert
            await tcs.Task;
            Assert.Equal(5, _elevator.CurrentFloor);
            Assert.False(controller.HasPendingRequestForFloor(5));
        }

        [Fact]
        public async Task UpRequestMovesElevatorToTheDesiredFloor()
        {
            // Arrange
            var tcs = new TaskCompletionSource();
            _elevator.OnDoorsOpened += () => tcs.SetResult();

            var controller = new Controller(_elevator);

            // Act
            controller.SelectDestinationFloor(3);

            // Assert
            await tcs.Task;
            Assert.Equal(3, _elevator.CurrentFloor);
            Assert.False(controller.HasPendingRequestForFloor(3));
        }

        [Fact]
        public void UpRequestButton_RegistersUpRequest()
        {
            // Arrange
            int destinationFloor = 3;

            var fakeElevator = A.Fake<IElevator>();
            var controller = new Controller(fakeElevator);

            // Act
            controller.SelectDestinationFloor(destinationFloor);

            // Assert
            Assert.True(controller.HasPendingRequestForFloor(destinationFloor));
        }
    }
}
