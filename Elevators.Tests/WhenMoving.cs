namespace Elevators.Tests;

public class WhenMoving
{
    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    public void UpElevatorGoesUp(int floors)
    {
        // Arrange
        var elevator = new Elevator(0, 10);

        // Act
        elevator.GoUp(floors);

        // Assert
        Assert.Equal(floors, elevator.CurrentFloor);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    public void DownElevatorGoesDown(int floors)
    {
        // Arrange
        var elevator = new Elevator(0, 10);
        // Move elevator up first
        elevator.GoUp(floors);

        // Act
        elevator.GoDown(floors);

        // Assert
        Assert.Equal(0, elevator.CurrentFloor);
    }
}
