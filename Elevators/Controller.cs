
using System.Dynamic;

namespace Elevators
{
    public enum Direction { None, Up, Down }

    public class Controller
    {
        public Direction LastDirection { get; private set; } = Direction.None;
        private readonly IElevator _elevator;
        private readonly SortedSet<int> _pendingRequests = new SortedSet<int>();

        public Controller(IElevator elevator)
        {
            _elevator = elevator;
            _elevator.OnStop += OpenDoors;
            Task.Run(async () => await StartRequestProcessingLoop());
        }

        private async Task StartRequestProcessingLoop()
        {
            while (true)
            {
                await Task.Delay(100);

                if (_elevator.Status != ElevatorStatus.Stopped || _pendingRequests.Count == 0)
                    continue;
                
                int nextFloor = _pendingRequests.Min;
                _elevator.GoToFloor(nextFloor);
            }
        }

        public void SelectDestinationFloor(int floor)
        {
            if (!_pendingRequests.Contains(floor))
                _pendingRequests.Add(floor);
        }

        public bool HasPendingRequestForFloor(int floor)
        {
            return _pendingRequests.Contains(floor);
        }

        private void OpenDoors(int floor)
        {
            System.Diagnostics.Debug.WriteLine($"Opening doors at floor {floor}");
            if (_pendingRequests.Contains(floor))
                _pendingRequests.Remove(floor);

            _elevator.OpenDoors();
        }
    }
}
