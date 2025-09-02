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

        public void GoToFloor(int targetFloor)
        {
            if (Status == ElevatorStatus.Moving)
                return;

            if (targetFloor > TopFloor)
                targetFloor = TopFloor;

            if (targetFloor < LowerFloor)
                targetFloor = LowerFloor;
                
            if (targetFloor != CurrentFloor)
            {
                System.Threading.Tasks.Task.Run(() => MoveToFloor(targetFloor));
            }
            // If destinationFloor == CurrentFloor, do nothing
        }


        internal void MoveToFloor(int targetFloor)
        {
            int start = CurrentFloor;
            int end = Math.Max(LowerFloor, Math.Min(targetFloor, TopFloor)); // keep within bounds
            int step = end > start ? 1 : -1;

            OnBeforeMoving?.Invoke();
            Status = ElevatorStatus.Moving;

            for (int floor = start; floor != end; floor += step)
            {
                System.Threading.Thread.Sleep(SecondsPerFloor * 1000); // Simulate time taken to move between floors
                SetCurrentFloor(floor + step);
            }

            Status = ElevatorStatus.Stopped;
            OnAfterStop?.Invoke(CurrentFloor);
        }

        private void SetCurrentFloor(int floor)
        {
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
