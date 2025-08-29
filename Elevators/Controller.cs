

namespace Elevators
{
    public class Controller
    {
        private readonly IElevator _elevator;

        private readonly HashSet<int> _pendingUpRequests = new HashSet<int>();
        private readonly HashSet<int> _pendingDownRequests = new HashSet<int>();

        public Controller(IElevator elevator)
        {
            _elevator = elevator;
            _elevator.OnFloorReached += OnElevatorFloorReached;
        }

        public void SelectDestinationFloor(int floor)
        {
            if (floor > _elevator.CurrentFloor)
            {
                _pendingUpRequests.Add(floor);
            }
            else if (floor < _elevator.CurrentFloor)
            {
                _pendingDownRequests.Add(floor);
            }
            // If floor == CurrentFloor, do nothing
        }

        public void PressCallDownButton(int floor)
        {
            _pendingDownRequests.Add(floor);
            _elevator.GoToFloor(floor);
        }

        public bool HasPendingDownRequestForFloor(int floor)
        {
            return _pendingDownRequests.Contains(floor);
        }

        public void PressCallUpButton(int floor)
        {
            _pendingUpRequests.Add(floor);
            _elevator.GoToFloor(floor);
        }

        public bool HasPendingUpRequestForFloor(int floor)
        {
            return _pendingUpRequests.Contains(floor);
        }
        private void OnElevatorFloorReached(int floor)
        {
            if (_pendingUpRequests.Contains(floor) || _pendingDownRequests.Contains(floor))
            {
                _pendingUpRequests.RemoveWhere(f => f == floor);
                _pendingDownRequests.RemoveWhere(f => f == floor);
                _elevator.OpenDoors();
            }
        }
    }
}
