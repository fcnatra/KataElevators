
namespace Elevators;

public interface IElevator
{
    ElevatorStatus Status { get; }
    double EnergyConsumptionKWH { get; set; }
    int SecondsPerFloor { get; set; }
    int TotalFloorsTraveled { get; }
    int TopFloor { get; }
    int CurrentFloor { get; }
    int LowerFloor { get; }

    Action<int>? OnFloor { get; set; }
    Action<int>? OnStop { get; set; }
    Action? OnDoorsOpened { get; }
    Func<int>? NextStop { get; set; }

    double GetEnergyConsumption();
    void GoToFloor(int destinationFloor);
    void OpenDoors();
}
