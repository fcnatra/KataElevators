

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
        }

    public void PressCallDownButton(int floor)
        {
            _pendingDownRequests.Add(floor);
            _elevator.GoToFloor(floor);
            _pendingDownRequests.Remove(floor);
        }

        public bool HasPendingDownRequestForFloor(int floor)
        {
            return _pendingDownRequests.Contains(floor);
        }

    public void PressCallUpButton(int floor)
        {
            _pendingUpRequests.Add(floor);
            _elevator.GoToFloor(floor);
            _pendingUpRequests.RemoveWhere(f => f == floor);
        }

        public bool HasPendingUpRequestForFloor(int floor)
        {
            return _pendingUpRequests.Contains(floor);
        }
    }
}
