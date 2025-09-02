
using System.Diagnostics;
using System.Net;

namespace Elevators
{

    public class Elevator : IElevator
    {
        public ElevatorStatus Status { get; private set; } = ElevatorStatus.Stopped;
        public DoorStatus DoorStatus { get; private set; } = DoorStatus.Open;
        public bool IsMovingUp => Status == ElevatorStatus.MovingUp;
        public bool IsMovingDown => Status == ElevatorStatus.MovingDown;
        public ElevatorStatus LastMovementDirection { get; private set; } = ElevatorStatus.Stopped;

        public Action<int>? OnFloor { get; set; }
        public Action? OnBeforeMoving { get; set; }
        public Action<int>? OnAfterStop { get; set; }
        public Action? OnDoorsOpened { get; set; }
        public Action? OnDoorsClosed { get; set; }
        public int TopFloor { get; }
        public int CurrentFloor { get; internal set; }
        public int LowerFloor { get; }
        public int TotalFloorsTraveled { get; private set; } = 0;

        private int _currentTargetFloor;

        private double _energyConsumptionKWH = 5.5;
        public double EnergyConsumptionKWH
        {
            get => _energyConsumptionKWH;
            set
            {
                _energyConsumptionKWH = value;
                TotalFloorsTraveled = 0;
            }
        }

        private int _secondsPerFloor = 5;
        public int SecondsPerFloor
        {
            get => _secondsPerFloor;
            set
            {
                _secondsPerFloor = value;
                TotalFloorsTraveled = 0;
            }
        }

        public double GetEnergyConsumption()
        {
            double totalSeconds = TotalFloorsTraveled * SecondsPerFloor;
            double hours = totalSeconds / 3600.0;
            return EnergyConsumptionKWH * hours;
        }

        public Elevator(int lowerFloor, int topFloor)
        {
            LowerFloor = lowerFloor;
            TopFloor = topFloor;
            CurrentFloor = LowerFloor;
        }

        public void GoToFloor(int targetFloor)
        {
            if (Status != ElevatorStatus.Stopped)
            {
                TryToAddANewStopToThisMovement(targetFloor);
                return;
            }
            
            if (targetFloor > TopFloor)
                targetFloor = TopFloor;

            if (targetFloor < LowerFloor)
                targetFloor = LowerFloor;

            _currentTargetFloor = targetFloor;

            if (targetFloor != CurrentFloor)
            {
                System.Threading.Tasks.Task.Run(() => MoveToFloor());
            }
            else OnAfterStop?.Invoke(CurrentFloor);// If destinationFloor == CurrentFloor, do nothing
        }

        public void OpenDoors()
        {
            OnDoorsOpened?.Invoke();
            DoorStatus = DoorStatus.Open;
        }

        public void CloseDoors()
        {
            OnDoorsClosed?.Invoke();
            DoorStatus = DoorStatus.Closed;
        }

        private void TryToAddANewStopToThisMovement(int targetFloor)
        {
            Debug.WriteLine($"Trying to add new stop at floor {targetFloor}");
            if (CanAddStopUpAt(targetFloor))
            {
                _currentTargetFloor = targetFloor;
            }
            else if (CanAddStopDownAt(targetFloor))
            {
                _currentTargetFloor = targetFloor;
            }
        }

        private bool CanAddStopUpAt(int floor)
        {
            if (!IsMovingUp) return false;

            int current = CurrentFloor;
            int target = _currentTargetFloor;
            
            return floor > current && floor <= target;
        }

        private bool CanAddStopDownAt(int floor)
        {
            if (!IsMovingDown) return false;

            int current = CurrentFloor;
            int target = _currentTargetFloor;

            return floor < current && floor >= target;
        }

        private void MoveToFloor()
        {
            int start = CurrentFloor;
            int end = GetCurrentTargetFloorInBounds();
            int step = end > start ? 1 : -1;

            Status = step == 1 ? ElevatorStatus.MovingUp : ElevatorStatus.MovingDown;
            OnBeforeMoving?.Invoke();

            int floor = start;
            while (floor != end)
            {
                System.Threading.Thread.Sleep(SecondsPerFloor * 1000); // Simulate time taken to move between floors
                floor += step;
                SetCurrentFloor(floor);
                end = CheckIfTargetHasChanged(end);
            }
            LastMovementDirection = Status;
            Status = ElevatorStatus.Stopped;
            OnAfterStop?.Invoke(CurrentFloor);
        }

        private int CheckIfTargetHasChanged(int end)
        {
            int newTarget = GetCurrentTargetFloorInBounds();
            if (CanAddStopUpAt(newTarget) || CanAddStopDownAt(newTarget))
            {
                end = newTarget;
            }

            return end;
        }

        private int GetCurrentTargetFloorInBounds() => Math.Max(LowerFloor, Math.Min(_currentTargetFloor, TopFloor));

        private void SetCurrentFloor(int floor)
        {
            CurrentFloor = floor;
            TotalFloorsTraveled++;
            OnFloor?.Invoke(floor);
        }
    }
}
