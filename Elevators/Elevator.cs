
namespace Elevators;

public class Elevator
{
    // Add properties and methods for Elevator here
    public int CurrentFloor { get; set; }

    public void GoUp(int floors)
    {
        CurrentFloor += floors;
    }
}
