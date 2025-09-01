namespace Elevators
{

    public class Elevator : IElevator
    {
        public ElevatorStatus Status { get; private set; } = ElevatorStatus.Stopped;
        public DoorStatus DoorStatus { get; private set; } = DoorStatus.Open;
        public Action<int>? OnFloor { get; set; }
        public Action? OnBeforeMoving { get; set; }
        public Action<int>? OnAfterStop { get; set; }
        public Action? OnDoorsOpened { get; set; }
        public Action? OnDoorsClosed { get; set; }
        public Func<int>? NextStop { get; set; }
        public int TopFloor { get; }
        public int CurrentFloor { get; internal set; }
        public int LowerFloor { get; }

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

        public int TotalFloorsTraveled { get; private set; } = 0;

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

        public void GoToFloor(int destinationFloor)
        {
            if (destinationFloor > TopFloor)
                destinationFloor = TopFloor;

            if (destinationFloor < LowerFloor)
                destinationFloor = LowerFloor;

            System.Threading.Tasks.Task.Run(() =>
            {
                Status = ElevatorStatus.Moving;
                OnBeforeMoving?.Invoke();
                if (destinationFloor > CurrentFloor)
                {
                    GoUp(destinationFloor);
                }
                else if (destinationFloor < CurrentFloor)
                {
                    GoDown(destinationFloor);
                }
                // If destinationFloor == CurrentFloor, do nothing
                Status = ElevatorStatus.Stopped;
                OnAfterStop?.Invoke(CurrentFloor);
            });
        }

        internal void GoDown(int targetFloor)
        {
            int start = CurrentFloor;
            int end = Math.Max(targetFloor, 0);

            Status = ElevatorStatus.Moving;
            for (int floor = start - 1; floor >= end; floor--)
            {
                SetCurrentFloor(floor);
            }
        }

        internal void GoUp(int targetFloor)
        {
            int start = CurrentFloor;
            int end = Math.Min(targetFloor, TopFloor);

            Status = ElevatorStatus.Moving;
            for (int floor = start + 1; floor <= end; floor++)
                SetCurrentFloor(floor);
        }

        private void SetCurrentFloor(int floor)
        {
#if DEBUG
            System.Threading.Thread.Sleep(_secondsPerFloor * 10);
#endif
            CurrentFloor = floor;
            TotalFloorsTraveled++;
            OnFloor?.Invoke(floor);
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
    }
}
