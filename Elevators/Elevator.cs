

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

    public void GoDown(int floors)
    {
        int start = CurrentFloor;
        int end = CurrentFloor - floors;
        for (int floor = start - 1; floor >= end; floor--)
        {
            CurrentFloor = floor;
            FloorReached?.Invoke(floor);
        }
    }

    public void GoUp(int floors)
    {
        int start = CurrentFloor;
        int end = CurrentFloor + floors;
        for (int floor = start + 1; floor <= end; floor++)
        {
            CurrentFloor = floor;
            FloorReached?.Invoke(floor);
        }
    }
}
