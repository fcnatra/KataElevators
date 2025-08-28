
namespace Elevators;

public class Elevator
{
    /// <summary>
    /// Energía consumida por hora en kW (kilovatios). Por defecto 5.5 kW.
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

    /// <param name="floors">Cantidad de pisos recorridos</param>
    /// <returns>Energía consumida en kWh</returns>
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
    public int TopFloor { get; }
    public int CurrentFloor { get; internal set; }
    public int LowerFloor { get; }

    public Elevator(int lowerFloor, int topFloor)
    {
        LowerFloor = lowerFloor;
        TopFloor = topFloor;
        CurrentFloor = LowerFloor;
    }


    public event Action<int>? FloorReached;

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
            FloorReached?.Invoke(floor);
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
            FloorReached?.Invoke(floor);
        }
    }
}
