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

        [Fact]
        public async Task Up_HandleUpwardCallsFirst()
        {
            // Arrange
            var attendedFloors = new HashSet<int>();
            var stops = 0;
            var tcs = new TaskCompletionSource();
            _elevator.OnDoorsOpened += () =>
            {
                attendedFloors.Add(_elevator.CurrentFloor);
                stops++;
                if (stops == 1 || stops == 3) tcs.SetResult();
            };

            var controller = new Controller(_elevator);
            controller.CallElevatorUp(3);
            await tcs.Task;
            tcs = new TaskCompletionSource();

            // Act
            controller.SelectDestinationFloor(5);
            controller.SelectDestinationFloor(2);
            await tcs.Task;

            // Assert
            Assert.Equal(2, _elevator.CurrentFloor);
            Assert.Equal(new[] { 3, 5, 2 }, attendedFloors);
        }

        [Fact]
        public async Task WhileRespondingCallsUpTo3rd_ProcessesUpCallsTo4th()
        {
            // Arrange
            var stops = 0;
            var attendedFloors = new HashSet<int>();
            var tcs = new TaskCompletionSource();
            _elevator.OnAfterStop += (floor) => { stops++; attendedFloors.Add(floor); if (stops == 3) tcs.SetResult(); };

            var controller = new Controller(_elevator);
            controller.CallElevatorUp(3);
            controller.SelectDestinationFloor(6);

            // Act
            controller.CallElevatorUp(4);
            await tcs.Task;

            // Assert
            Assert.Equal(6, _elevator.CurrentFloor);
            Assert.Equal(new[] { 3, 4, 6 }, attendedFloors);
        }

        [Theory]
        [InlineData(1, 7, 4, new[] { 1, 4, 7 })]
        [InlineData(5, 4, 7, new[] { 4, 5, 7 })]
        public async Task FromFloorsInTheSameDirection_CapturesCall(int start, int end, int call, int[] expectedFloors)
        {
            // Arrange
            var attendedFloors = new HashSet<int>();
            var tcs = new TaskCompletionSource();
            _elevator.OnAfterStop += (floor) => { attendedFloors.Add(floor); if (floor == 7) tcs.SetResult(); };

            var controller = new Controller(_elevator);
            controller.OnElevatorIdle += () => tcs.TrySetResult(); // safe net
            controller.CallElevatorUp(start);
            controller.SelectDestinationFloor(end);
            // await tcs.Task;
            // tcs = new TaskCompletionSource();
            
            // Act
            controller.CallElevatorUp(call);
            await tcs.Task;

            // Assert
            Assert.Equal(7, _elevator.CurrentFloor);
            Assert.Equal(expectedFloors, attendedFloors);
        }

        [Fact]
        public async Task UpFrom4th_WhileMovingUpAbove5th_ItReturnsAfterFinishingUpwardMovement()
        {
            // Arrange
            HashSet<int> attendedFloors = new HashSet<int>();
            var tcs = new TaskCompletionSource();
            _elevator.OnAfterStop += (floor) => { attendedFloors.Add(floor); if (floor == 4) tcs.SetResult(); };


            var controller = new Controller(_elevator);

            controller.CallElevatorUp(5);
            controller.SelectDestinationFloor(8);
            controller.SelectDestinationFloor(9);

            // Act
            controller.CallElevatorUp(4);
            await tcs.Task;

            // Assert
            Assert.Equal(new[] { 5, 8, 9, 4 }, attendedFloors);
        }
    }
}
