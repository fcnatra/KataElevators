

namespace Elevators;

public class Elevator
{
    public int CurrentFloor { get; set; }

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
