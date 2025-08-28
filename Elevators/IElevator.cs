
namespace Elevators;

public interface IElevator
{
    double EnergyConsumptionKWH { get; set; }
    int SecondsPerFloor { get; set; }
    int TotalFloorsTraveled { get; }
    int TopFloor { get; }
    int CurrentFloor { get; }
    int LowerFloor { get; }

    event Action<int>? FloorReached;

    double GetEnergyConsumption();
    void GoToFloor(int destinationFloor);
}
