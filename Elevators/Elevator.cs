
namespace Elevators;

public class Elevator : IElevator
{
    public Action<int>? OnFloorReached { get; set; }
    public Action? OnDoorsOpened { get; set; }
    public int TopFloor { get; }
    public int CurrentFloor { get; internal set; }
    public int LowerFloor { get; }

    /// <summary>
    /// Energía consumida por hora en kW (kilovatios). Tiene valor por defecto.
    /// </summary>
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

    /// <summary>
    /// Segundos que tarda en recorrer un piso. Tiene valor por defecto.
    /// </summary>
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

    /// <summary>
    /// Devuelve el consumo total de energía (kWh) considerando todos los viajes realizados.
    /// </summary>
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

        if (destinationFloor > CurrentFloor)
        {
            GoUp(destinationFloor - CurrentFloor);
        }
        else if (destinationFloor < CurrentFloor)
        {
            GoDown(CurrentFloor - destinationFloor);
        }
        // If destinationFloor == CurrentFloor, do nothing
    }

    internal void GoDown(int floors)
    {
        int start = CurrentFloor;
        int end = CurrentFloor - floors;
        if (end < LowerFloor)
        {
            end = LowerFloor;
        }
        for (int floor = start - 1; floor >= end; floor--)
        {
            CurrentFloor = floor;
            TotalFloorsTraveled++;
            OnFloorReached?.Invoke(floor);
        }
    }

    internal void GoUp(int floors)
    {
        int start = CurrentFloor;
        int end = CurrentFloor + floors;
        if (end > TopFloor)
        {
            end = TopFloor;
        }
        for (int floor = start + 1; floor <= end; floor++)
        {
            CurrentFloor = floor;
            TotalFloorsTraveled++;
            OnFloorReached?.Invoke(floor);
        }
    }
    public void OpenDoors()
    {
        // Simulate opening doors (could raise an event or just be a placeholder for now)
        OnDoorsOpened?.Invoke();
    }

}
