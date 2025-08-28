using System.Collections.Generic;

namespace Elevators
{
    public class Controller
    {
        private readonly HashSet<int> _pendingUpRequests = new HashSet<int>();

        public void PressUpButton(int usersFloor)
        {
            _pendingUpRequests.Add(usersFloor);
        }

        public bool HasPendingUpRequestForFloor(int floor)
        {
            return _pendingUpRequests.Contains(floor);
        }
    }
}
