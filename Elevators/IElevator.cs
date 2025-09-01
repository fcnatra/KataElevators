
namespace Elevators;

public enum ElevatorStatus { Stopped, Moving }
public enum DoorStatus { Open, Closed }

public interface IElevator
{
    ElevatorStatus Status { get; }
    DoorStatus DoorStatus { get; }
    double EnergyConsumptionKWH { get; set; }
    int SecondsPerFloor { get; set; }
    int TotalFloorsTraveled { get; }
    int TopFloor { get; }
    int CurrentFloor { get; }
    int LowerFloor { get; }

    Action<int>? OnFloor { get; set; }
    Action? OnBeforeMoving { get; set; }
    Action<int>? OnAfterStop { get; set; }
    Action? OnDoorsOpened { get; set; }
    Action? OnDoorsClosed { get; set; }
    Func<int>? NextStop { get; set; }

    double GetEnergyConsumption();
    void GoToFloor(int destinationFloor);
    void OpenDoors();
    void CloseDoors();
}
