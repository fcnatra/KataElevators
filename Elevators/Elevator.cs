
namespace Elevators;

public class Elevator
{
    public int TopFloor { get; }
    public int LowerFloor { get; }
    public int CurrentFloor { get; private set; }

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
            FloorReached?.Invoke(floor);
        }
    }
}
