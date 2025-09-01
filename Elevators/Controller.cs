
using System.Diagnostics;
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
            CaptureElevatorEvents();
            Task.Run(async () => await StartRequestProcessingLoop());
        }

        private void CaptureElevatorEvents()
        {
            _elevator.OnAfterStop += RunStopActions;
            _elevator.OnFloor += (floor) => Debug.WriteLine($"Elevator at floor {floor}");
            _elevator.OnDoorsOpened += () => Debug.WriteLine($"Doors opened");
            _elevator.OnDoorsClosed += () => Debug.WriteLine($"Doors closed");
        }

        private async Task StartRequestProcessingLoop()
        {
            while (true)
            {
                await Task.Delay(100);

                if (_elevator.Status != ElevatorStatus.Stopped || _pendingRequests.Count == 0)
                    continue;
                
                int nextFloor = _pendingRequests.Min;

                _elevator.CloseDoors();
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

        private void RunStopActions(int floor)
        {
            Debug.WriteLine($"Elevator stopped at floor {floor}");

            if (_pendingRequests.Contains(floor))
                _pendingRequests.Remove(floor);

            _elevator.OpenDoors();
        }
    }
}
