using System.Diagnostics;

namespace Elevators
{
    public class Controller
    {
        private ExternalCall? _lastExternalCallAttended = null;
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
            Task.Run(async () =>
            {
                try
                {
                    await ProcessRequestsInLoop();
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.ToString());
                    throw; // Optional: rethrow to propagate
                }
            });
        }

        public void CallElevator(int floor, Direction direction)
        {
            Debug.WriteLine($"{_elevator.Id} called {direction} from floor {floor} with status {_elevator.Status}");
            ExternalCall call = AddExternalCall(floor, direction);
            ForceElevatorToTakeTheCallIfItsInthePath(call);
        }

        public void SelectDestinationFloor(int floor)
        {
            AddInternalSelection(floor);
            Debug.WriteLine($"{_elevator.Id} selected destination floor: {floor}");
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
                _pendingExternalCalls.Add(call);
                Debug.WriteLine($"{_elevator.Id} added external call for floor {floor} direction {direction}");
            }
            return call;
        }

        private void AddInternalSelection(int floor)
        {
            if (!_pendingInternalSelections.Contains(floor))
            {
                _pendingInternalSelections.Add(floor);
                Debug.WriteLine($"{_elevator.Id} added internal selection for floor {floor}");
            }
        }

        private void ForceElevatorToTakeTheCallIfItsInthePath(ExternalCall call)
        {
            if (MovingInSameDirection(call.Direction))
            {
                _lastExternalCallAttended = _pendingExternalCalls
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
            _elevator.OnStatusChanged += () => Debug.WriteLine($"{_elevator.Id} status changed: {_elevator.Status}");
            _elevator.OnAfterStop += RunAfterStopActions;
            _elevator.OnFloor += (floor) => Debug.WriteLine($"{_elevator.Id} at floor {floor}");
            _elevator.OnDoorsOpened += () => Debug.WriteLine($"{_elevator.Id} doors opened");
            _elevator.OnDoorsClosed += () => Debug.WriteLine($"{_elevator.Id} doors closed");
        }

        private async Task ProcessRequestsInLoop()
        {
            while (true)
            {
                await Task.Delay(100); // don't need to process on real time - relax
                if (_elevator.IsMoving || NoPendingMovements)
                    continue;

                int nextFloor = GetNextFloor();

                if (!_elevator.IsStoppedAt(nextFloor))
                    CloseDoors();

                _lastExternalCallAttended = _pendingExternalCalls.FirstOrDefault(x => x.Floor == nextFloor);

                _elevator.GoToFloor(nextFloor);
            }
        }

        private void CloseDoors()
        {
            if (_elevator.DoorStatus == DoorStatus.Open) _elevator.CloseDoors();
        }

        private int GetNextFloor()
        {
            if (_elevator.IsMoving)
                throw new InvalidOperationException($"{_elevator.Id} Can't request for a new floor when is moving");

            if (NoPendingMovements)
                throw new InvalidOperationException($"{_elevator.Id}'t request for a new floor when there are no pending movements.");

            if (_elevator.LastMovementDirection == ElevatorStatus.MovingUp || _elevator.LastMovementDirection == ElevatorStatus.Stopped)
            {
                int? closestInternalRequestUp = GetClosestInternalRequestUp();
                ExternalCall? closestExternalRequestUp = GetClosestExternalRequestUp();

                int? closestFloorUp = GetClosestFloor(closestInternalRequestUp, closestExternalRequestUp, Direction.Up);

                if (closestFloorUp is not null)
                    return closestFloorUp.Value;
            }

            // let's process downward movements
            int? closestInternalRequestDown = GetClosestInternalRequestDown();
            ExternalCall? closestExternalRequestDown = GetClosestExternalRequestDown();

            int? closestFloorDown = GetClosestFloor(closestInternalRequestDown, closestExternalRequestDown, Direction.Down);

            if (closestFloorDown is null)
                throw new InvalidOperationException($"{_elevator.Id} Can't request for a new floor when there are no pending movements.");

            return closestFloorDown.Value;
        }

        private int? GetClosestFloor(int? closestInternalRequestUp, ExternalCall? closestExternalRequestUp, Direction direction)
        {
            int? closestFloorUp = closestInternalRequestUp;

            if (closestExternalRequestUp is not null)
            {
                closestFloorUp ??= closestExternalRequestUp.Floor;

                if (direction is Direction.Up)
                    closestFloorUp = Math.Min(closestFloorUp.Value, closestExternalRequestUp.Floor);
                else
                    closestFloorUp = Math.Max(closestFloorUp.Value, closestExternalRequestUp.Floor);

                if (closestFloorUp == closestExternalRequestUp.Floor)
                    _lastExternalCallAttended = closestExternalRequestUp;
            }

            return closestFloorUp;
        }

        private ExternalCall? GetClosestExternalRequestDown()
        {
            var calls = _pendingExternalCalls
                .Where(c => c.Floor < _elevator.CurrentFloor && c.Direction == Direction.Down);

            return calls.Any() ? calls.Min() : null;
        }

        private int? GetClosestInternalRequestDown()
        {
            var selections = _pendingInternalSelections
                .Where(f => f < _elevator.CurrentFloor);

            return selections.Any() ? selections.Min() : null;
        }

        private ExternalCall? GetClosestExternalRequestUp()
        {
            var calls = _pendingExternalCalls
                .Where(c => c.Floor > _elevator.CurrentFloor && c.Direction == Direction.Up);

            return calls.Min();// calls.Any() ? calls.Min() : null;
        }

        private int? GetClosestInternalRequestUp()
        {
            var selections = _pendingInternalSelections
                .Where(f => f >= _elevator.CurrentFloor);

            return selections.Any() ? selections.Min() : null;
        }

        private void RunAfterStopActions(int floor)
        {
            Console.WriteLine($"{_elevator.Id} stopped at floor {floor}");
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
