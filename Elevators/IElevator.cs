
namespace Elevators;

public interface IElevator
{
    double EnergyConsumptionKWH { get; set; }
    int SecondsPerFloor { get; set; }
    int TotalFloorsTraveled { get; }
    int TopFloor { get; }
    int CurrentFloor { get; }
    int LowerFloor { get; }

    Action<int>? OnFloorReached { get; set; }
    public Action? OnDoorsOpened { get; }

    double GetEnergyConsumption();
    void GoToFloor(int destinationFloor);
    void OpenDoors();
}
