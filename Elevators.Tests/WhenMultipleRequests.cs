namespace Elevators.Tests
{
    public class WhenMultipleRequests
    {
        private Elevator _elevator;

        public WhenMultipleRequests()
        {
            _elevator = new Elevator(0, 10);
            _elevator.SecondsPerFloor = 1;
        }

        [Fact]
        public async Task FromGroundFloor_AllAreAttended()
        {
            // Arrange
            var controller = new Controller(_elevator);

            var attendedFloors = new List<int>();
            var floorsToVisit = new[] { 2, 4, 6, 8, 10 };
            var tcs = new TaskCompletionSource();

            _elevator.OnAfterStop += floor =>
            {
                attendedFloors.Add(floor);
                if (_elevator.IsStoppedAt(0))
                {
                    foreach (var f in floorsToVisit)
                        controller.SelectDestinationFloor(f);
                }
                if (attendedFloors.Count == floorsToVisit.Length)
                    tcs.TrySetResult();
            };

            _elevator.GoToFloor(0);

            // Act
            await tcs.Task;

            // Assert
            Assert.All(floorsToVisit, f => attendedFloors.Contains(f));
        }

        [Fact]
        public async Task PathIsTheOptimal()
        {
            // Arrange
            _elevator = new Elevator(0, 12);
            _elevator.SecondsPerFloor = 1;
            var elevatorCalls = new List<(int, Direction, int[])>
            {
                (03, Direction.Up, [7]),
                (10, Direction.Down, [2, 5]),
                (01, Direction.Up, [9]),
                (06, Direction.Down, [0, 8])
            };
            var expectedStops = elevatorCalls.Count + elevatorCalls.Sum(c => c.Item3.Length);
            var controller = new Controller(_elevator);

            var attendedFloors = new List<int>();
            var tcs = new TaskCompletionSource();

            _elevator.OnAfterStop += floor =>
            {
                attendedFloors.Add(floor);
                var call = elevatorCalls.FirstOrDefault(x => x.Item1 == floor);
                if (call != default)
                {
                    var selections = call.Item3;
                    foreach (var destination in selections)
                        controller.SelectDestinationFloor(destination);
                }
                if (attendedFloors.Count == expectedStops)
                    tcs.TrySetResult();
            };
            
            foreach (var call in elevatorCalls)
                controller.CallElevator(call.Item1, call.Item2);

            // Act
            await tcs.Task;

            // Assert
            Assert.Equal([1, 3, 7, 9, 10, 8, 6, 5, 2, 0], attendedFloors);
        }
    }
}
