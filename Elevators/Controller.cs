using System.Collections.Generic;

namespace Elevators
{
    public class Controller
    {
        private readonly IElevator _elevator;

        public Controller(IElevator elevator)
        {
            _elevator = elevator;
        }

        private readonly HashSet<int> _pendingUpRequests = new HashSet<int>();

        public void PressUpButton(int floor)
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
