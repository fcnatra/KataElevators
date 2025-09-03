namespace Elevators;

public interface IElevator
{
    ElevatorStatus Status { get; }
    DoorStatus DoorStatus { get; }
    ElevatorStatus LastMovementDirection { get; }
    bool IsMovingUp { get; }
    bool IsMovingDown { get; }
    bool IsMoving { get; }

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

    double GetEnergyConsumption();
    void GoToFloor(int targetFloor);
    void OpenDoors();
    void CloseDoors();
}
