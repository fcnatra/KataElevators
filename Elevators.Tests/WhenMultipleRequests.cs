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
    }
}
