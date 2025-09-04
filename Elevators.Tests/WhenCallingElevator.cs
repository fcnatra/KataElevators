using System.Diagnostics;

namespace Elevators.Tests
{
    public class WhenCallingElevator
    {
        private Elevator _elevator;

        public WhenCallingElevator()
        {
            _elevator = new Elevator(0, 10);
            _elevator.SecondsPerFloor = 1;
        }

        [Fact]
        public async Task Up_GoesToTheCallingFloor()
        {
            // Arrange
            var tcs = new TaskCompletionSource();
            _elevator.OnAfterStop += (floor) => tcs.SetResult();

            var controller = new Controller(_elevator);

            // Act
            controller.CallElevatorUp(3);
            await tcs.Task;

            // Assert
            Assert.Equal(3, _elevator.CurrentFloor);
        }

        [Theory]
        [InlineData(3, 1, new[] { 5, 2 }, new[] { 3, 5, 2 })]
        [InlineData(3, 0, new[] { 5, 2 }, new[] { 3, 2, 5 })]
        public async Task OnADirection_FirstHandleCallsToThatDirection(int call, int direction, int[] selections, int[] expected)
        {
            // Arrange
            var controller = new Controller(_elevator);

            var attendedFloors = new HashSet<int>();
            var stops = 0;
            var tcs = new TaskCompletionSource();
            _elevator.OnDoorsOpened += () =>
            {
                attendedFloors.Add(_elevator.CurrentFloor);
                stops++;
                if (_elevator.IsStoppedAt(call))
                {
                    controller.SelectDestinationFloor(selections[0]);
                    controller.SelectDestinationFloor(selections[1]);
                }
            };
            controller.OnElevatorIdle += () => tcs.SetResult();

            if (direction == 1) controller.CallElevatorUp(call);
            else controller.CallElevatorDown(call);

            // Act
            await tcs.Task;

            // Assert
            Assert.Equal(selections.Last(), _elevator.CurrentFloor);
            Assert.Equal(expected, attendedFloors);
        }

        [Theory]
        [InlineData(1, 7, 4, new[] { 1, 4, 7 })]
        [InlineData(5, 7, 4, new[] { 4, 5, 7 })]
        [InlineData(3, 6, 4, new[] { 3, 4, 6 })]
        public async Task UpFromDifferentFloors_CapturesAllCalls(int firstCall, int destination, int secondCall, int[] expectedFloors)
        {
            // Arrange
            var attendedFloors = new HashSet<int>();
            var tcs = new TaskCompletionSource();

            var controller = new Controller(_elevator);
            _elevator.OnDoorsOpened += () =>
            {
                if (_elevator.IsStoppedAt(firstCall))
                    controller.SelectDestinationFloor(destination);
            };

            _elevator.OnAfterStop += (floor) =>
            {
                attendedFloors.Add(floor);
                if (floor == 7) tcs.TrySetResult();
            };

            controller.OnElevatorIdle += () => tcs.TrySetResult(); // safe net

            controller.CallElevatorUp(firstCall);

            // Act
            controller.CallElevatorUp(secondCall);
            await tcs.Task;

            // Assert
            Assert.Equal(destination, _elevator.CurrentFloor);
            Assert.Equal(expectedFloors, attendedFloors);
        }

        [Fact]
        public async Task UpFrom4th_WhileMovingUpAbove5th_ItReturnsAfterFinishingUpwardMovement()
        {
            // Arrange
            var controller = new Controller(_elevator);

            HashSet<int> attendedFloors = new HashSet<int>();
            var tcs = new TaskCompletionSource();

            _elevator.OnAfterStop += (floor) =>
            {
                attendedFloors.Add(floor);
                if (floor == 4) tcs.SetResult();
            };
            _elevator.OnDoorsOpened += () =>
            {
                if (_elevator.IsStoppedAt(5))
                {
                    tcs.SetResult();
                    tcs = new TaskCompletionSource();
                    controller.SelectDestinationFloor(8);
                    controller.SelectDestinationFloor(9);
                }
            };
            controller.CallElevatorUp(5);
            await tcs.Task;

            // Act
            controller.CallElevatorUp(4);
            await tcs.Task;

            // Assert
            Assert.Equal(new[] { 5, 8, 9, 4 }, attendedFloors);
        }

        [Fact]
        public async Task UpButSelectingDown_GoesDown()
        {
            // Arrange
            var controller = new Controller(_elevator);
            _elevator.OnDoorsOpened += () =>
            {
                if (_elevator.IsStoppedAt(5))
                    controller.SelectDestinationFloor(2);
            };

            var tcs = new TaskCompletionSource();
            controller.OnElevatorIdle += () => tcs.SetResult();

            // Act
            controller.CallElevatorUp(5);

            // Assert
            await tcs.Task;
            Assert.Equal(2, _elevator.CurrentFloor);
        }

        [Fact]
        public async Task Down_GoesToTheCallingFloor()
        {
            // Arrange
            var controller = new Controller(_elevator);
            var tcs = new TaskCompletionSource();
            _elevator.OnAfterStop += (floor) =>
            {
                if (floor == 3)
                    controller.CallElevatorDown(2); // Act
                else
                    tcs.SetResult();
            };

            controller.CallElevatorUp(3);

            // Act
            await tcs.Task;

            // Assert
            Assert.Equal(2, _elevator.CurrentFloor);
        }

        
    }
}
