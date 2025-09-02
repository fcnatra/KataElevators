
using System.Diagnostics;
using System.Globalization;

namespace Elevators
{
    public class Controller
    {
        private readonly IElevator _elevator;
        private readonly HashSet<int> _pendingRequests = new();
        private bool NoPendingMovements => _pendingRequests.Count == 0;

        public Controller(IElevator elevator)
        {
            _elevator = elevator;
            CaptureElevatorEvents();
            Task.Run(async () => await StartRequestProcessingLoop());
        }

        public void CallElevatorUp(int floor)
        {
            Debug.WriteLine($"Called from floor {floor}");
            AddFloorToQueue(floor);
            if (_elevator.IsMovingUp)
            {
                _elevator.GoToFloor(floor);
            }
        }

        public void SelectDestinationFloor(int floor)
        {
            Debug.WriteLine($"Selected destination floor: {floor}");
            AddFloorToQueue(floor);
        }

        public bool HasPendingRequestForFloor(int floor)
        {
            return _pendingRequests.Contains(floor);
        }

        private void CaptureElevatorEvents()
        {
            _elevator.OnAfterStop += RunAfterStopActions;
            _elevator.OnFloor += (floor) => Debug.WriteLine($"Elevator at floor {floor}");
            _elevator.OnDoorsOpened += () => Debug.WriteLine($"Doors opened");
            _elevator.OnDoorsClosed += () => Debug.WriteLine($"Doors closed");
            _elevator.OnBeforeMoving += () => Debug.WriteLine($"Elevator is about to move: {_elevator.Status}");
        }

        private async Task StartRequestProcessingLoop()
        {
            while (true)
            {
                await Task.Delay(100);

                if (_elevator.Status != ElevatorStatus.Stopped || NoPendingMovements)
                    continue;

                int nextFloor = GetNextFloor();

                _elevator.CloseDoors();
                _elevator.GoToFloor(nextFloor);
            }
        }

        private int GetNextFloor()
        {
            if (MovingUpAndRequestAbove())
                return _pendingRequests.Where(f => f > _elevator.CurrentFloor).Min();

            if (MovingDownAndRequestBelow() || MovingUpButNoMoreMovementsAbove())
                    return _pendingRequests.Where(f => f < _elevator.CurrentFloor).Max();

            return _pendingRequests.First();
        }

        private bool MovingUpButNoMoreMovementsAbove()
        {
            return _elevator.LastMovementDirection == ElevatorStatus.MovingUp
                            && !_pendingRequests.Any(f => f > _elevator.CurrentFloor);
        }

        private bool MovingDownAndRequestBelow()
        {
            return _elevator.LastMovementDirection == ElevatorStatus.MovingDown
                            && _pendingRequests.Any(f => f < _elevator.CurrentFloor);
        }

        private bool MovingUpAndRequestAbove()
        {
            return _elevator.LastMovementDirection == ElevatorStatus.MovingUp
                            && _pendingRequests.Any(f => f > _elevator.CurrentFloor);
        }

        private void RunAfterStopActions(int floor)
        {
            Debug.WriteLine($"Elevator stopped at floor {floor}");
            RemoveFloorFromQueue(floor);
            _elevator.OpenDoors();
        }

        private void AddFloorToQueue(int floor)
        {
            if (!_pendingRequests.Contains(floor))
                _pendingRequests.Add(floor);
        }

        private void RemoveFloorFromQueue(int floor)
        {
            if (_pendingRequests.Contains(floor))
                _pendingRequests.Remove(floor);
        }
    }
}
