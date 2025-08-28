namespace Elevators.Tests;

public class WhenMoving
{
    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    public void UpElevatorGoesUp(int floors)
    {
        // Arrange
        var elevator = new Elevator();
        elevator.CurrentFloor = 0;

        // Act
        elevator.GoUp(floors);

        // Assert
        Assert.Equal(floors, elevator.CurrentFloor);
    }
}
