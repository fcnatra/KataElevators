namespace Elevators.Tests;

public class WhenGoingUp
{
    [Fact]
    public void OneFloorElevatorGoesUp()
    {
        // Arrange
        var elevator = new Elevator();
        elevator.CurrentFloor = 0;

        // Act
        elevator.GoUp(1);

        // Assert
        Assert.Equal(1, elevator.CurrentFloor);
    }

    [Fact]
    public void TwoFloorElevatorGoesUp()
    {
        // Arrange
        var elevator = new Elevator();
        elevator.CurrentFloor = 0;

        // Act
        elevator.GoUp(2);

        // Assert
        Assert.Equal(2, elevator.CurrentFloor);
    }
}
