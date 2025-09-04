
using System.Diagnostics;
using System.Formats.Asn1;
using System.Globalization;

namespace Elevators
{
    public class Controller
    {
        private readonly IElevator _elevator;
    private readonly HashSet<int> _pendingExternalCalls = new();
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

        public void CallElevatorUp(int floor)
        {
            Debug.WriteLine($"Called UP from floor {floor} with status {{_elevator.Status}}");
            AddExternalCall(floor);
            ForceElevatorToTakeTheCallIfItsInthePath(floor);
        }

        public void CallElevatorDown(int floor)
        {
            Debug.WriteLine($"Called DOWN from floor {floor} with status {{_elevator.Status}}");
            AddExternalCall(floor);
            ForceElevatorToTakeTheCallIfItsInthePath(floor);
        }

        private void ForceElevatorToTakeTheCallIfItsInthePath(int floor)
        {
            if (_elevator.IsMoving)
                _elevator.GoToFloor(floor);
        }

        public void SelectDestinationFloor(int floor)
        {
            Debug.WriteLine($"Selected destination floor: {floor}");
            AddInternalSelection(floor);
        }

        public bool HasPendingRequestForFloor(int floor)
        {
            return _pendingExternalCalls.Contains(floor) || _pendingInternalSelections.Contains(floor);
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

        private List<int> GetInternalAndExternalCalls() => _pendingInternalSelections.Concat(_pendingExternalCalls).ToList();

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

        private void AddExternalCall(int floor)
        {
            if (!_pendingExternalCalls.Contains(floor))
            {
                Debug.WriteLine($"Added external call for floor {floor}");
                _pendingExternalCalls.Add(floor);
            }
        }

        private void AddInternalSelection(int floor)
        {
            if (!_pendingInternalSelections.Contains(floor))
            {
                Debug.WriteLine($"Added internal selection for floor {floor}");
                _pendingInternalSelections.Add(floor);
            }
        }

        private void RemoveFloorFromQueues(int floor)
        {
            if (_pendingExternalCalls.Contains(floor))
                _pendingExternalCalls.Remove(floor);

            if (_pendingInternalSelections.Contains(floor))
                _pendingInternalSelections.Remove(floor);
        }
    }
}
