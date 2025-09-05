using System.Diagnostics;

namespace Elevators
{
    public class Controller
    {
        private ExternalCall? _lastExternalCallSent = null;
        private readonly IElevator _elevator;
        private readonly HashSet<ExternalCall> _pendingExternalCalls = new();
        private readonly HashSet<int> _pendingInternalSelections = new();
        private bool NoPendingMovements => _pendingExternalCalls.Count == 0 && _pendingInternalSelections.Count == 0;
        public bool ElevatorIsIdle => _elevator.Status == ElevatorStatus.Stopped && NoPendingMovements;
        public Action? OnElevatorIdle;

        public Controller(IElevator elevator)
        {
            _elevator = elevator;
            CaptureElevatorEvents();
            Task.Run(async () => await StartRequestProcessingLoop());
        }

        public void CallElevator(int floor, Direction direction)
        {
            Debug.WriteLine($"Called {direction} from floor {floor} with status {_elevator.Status}");
            ExternalCall call = AddExternalCall(floor, direction);
            ForceElevatorToTakeTheCallIfItsInthePath(call);
        }

        public void SelectDestinationFloor(int floor)
        {
            Debug.WriteLine($"Selected destination floor: {floor}");
            AddInternalSelection(floor);
        }

        public bool HasPendingRequestForFloor(int floor)
        {
            return _pendingExternalCalls.Any(c => c.Floor == floor) || _pendingInternalSelections.Contains(floor);
        }

        private ExternalCall AddExternalCall(int floor, Direction direction)
        {
            var call = new ExternalCall(floor, direction);
            if (!_pendingExternalCalls.Contains(call))
            {
                Debug.WriteLine($"Added external call for floor {floor} direction {direction}");
                _pendingExternalCalls.Add(call);
            }
            return call;
        }

        private void AddInternalSelection(int floor)
        {
            if (!_pendingInternalSelections.Contains(floor))
            {
                Debug.WriteLine($"Added internal selection for floor {floor}");
                _pendingInternalSelections.Add(floor);
            }
        }

        private List<int> GetInternalAndExternalCalls() => _pendingInternalSelections.Concat(_pendingExternalCalls.Select(c => c.Floor)).ToList();

        private void ForceElevatorToTakeTheCallIfItsInthePath(ExternalCall call)
        {
            if (MovingInSameDirection(call.Direction))
            {
                _lastExternalCallSent = _pendingExternalCalls
                    .First(c => c.Floor == call.Floor && c.Direction == call.Direction);

                _elevator.GoToFloor(call.Floor);
            }
        }

        private bool MovingInSameDirection(Direction direction)
        {
            return _elevator.IsMovingUp && direction == Direction.Up
                || _elevator.IsMovingDown && direction == Direction.Down;
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

                if (_elevator.IsMoving || NoPendingMovements)
                    continue;

                int nextFloor = GetNextFloor();

                if (!_elevator.IsStoppedAt(nextFloor))
                    CloseDoors();

                _lastExternalCallSent = _pendingExternalCalls.FirstOrDefault(x => x.Floor == nextFloor);

                _elevator.GoToFloor(nextFloor);
            }
        }

        private void CloseDoors()
        {
            if (_elevator.DoorStatus == DoorStatus.Open) _elevator.CloseDoors();
        }

        private int GetNextFloor()
        {
            var allRequests = GetInternalAndExternalCalls();
            if (allRequests.Count == 0)
                throw new InvalidOperationException("No pending requests.");

            if (MovingUpAndRequestAbove())
                return allRequests.Where(f => f > _elevator.CurrentFloor).Min();

            if (MovingDownAndRequestBelow() || MovingUpButNoMoreMovementsAbove())
                return allRequests.Where(f => f < _elevator.CurrentFloor).Max();

            return allRequests.OrderBy(f => Math.Abs(f - _elevator.CurrentFloor)).First();
        }

        private bool MovingUpButNoMoreMovementsAbove()
        {
            var allRequests = GetInternalAndExternalCalls();
            return _elevator.LastMovementDirection == ElevatorStatus.MovingUp
                            && !allRequests.Any(f => f > _elevator.CurrentFloor);
        }

        private bool MovingDownAndRequestBelow()
        {
            var allRequests = GetInternalAndExternalCalls();
            return _elevator.LastMovementDirection == ElevatorStatus.MovingDown
                            && allRequests.Any(f => f < _elevator.CurrentFloor);
        }

        private bool MovingUpAndRequestAbove()
        {
            var allRequests = GetInternalAndExternalCalls();
            return _elevator.LastMovementDirection == ElevatorStatus.MovingUp
                            && allRequests.Any(f => f > _elevator.CurrentFloor);
        }

        private void RunAfterStopActions(int floor)
        {
            Debug.WriteLine($"Elevator stopped at floor {floor}");
            RemoveFloorFromQueues(floor);
            _elevator.OpenDoors();

            if (ElevatorIsIdle)
                OnElevatorIdle?.Invoke();
        }

        private void RemoveFloorFromQueues(int floor)
        {
            var toRemove = _pendingExternalCalls.Where(c => c.Floor == floor).ToList();

            foreach (var call in toRemove)
                _pendingExternalCalls.Remove(call);

            if (_pendingInternalSelections.Contains(floor))
                _pendingInternalSelections.Remove(floor);
        }
    }
}
