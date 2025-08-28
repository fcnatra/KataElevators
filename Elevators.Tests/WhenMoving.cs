namespace Elevators.Tests;

public class WhenMoving
{
    [Fact]
    public void ElevatorDoesNotGoAboveTopFloor()
    {
        // Arrange
        int topFloor = 10;
        var elevator = new Elevator(0, topFloor);

        // Act
        elevator.GoUp(11); // Try to go above top floor

        // Assert
        Assert.Equal(topFloor, elevator.CurrentFloor);
    }

    [Fact]
    public void ElevatorDoesNotGoAboveTopFloor_AfterGoingDownAndUp()
    {
        // Arrange
        int topFloor = 10;
        var elevator = new Elevator(0, topFloor);
        elevator.GoUp(8); // Go up to floor 8
        elevator.GoDown(3); // Go down to floor 5

        // Act
        elevator.GoUp(10); // Try to go above top floor

        // Assert
        Assert.Equal(topFloor, elevator.CurrentFloor);
    }
    [Fact]
    public void ElevatorDoesNotGoBelowLowerFloor_AfterGoingUpAndDown()
    {
        // Arrange
        int lowerFloor = 0;
        var elevator = new Elevator(lowerFloor, 10);
        elevator.GoUp(4);

        // Act
        elevator.GoDown(5);

        // Assert
        Assert.Equal(lowerFloor, elevator.CurrentFloor);
    }
    [Fact]
    public void ElevatorDoesNotGoBelowLowerFloor()
    {
        // Arrange
        int lowerFloor = 0;
        var elevator = new Elevator(lowerFloor, 10);

        // Act
        elevator.GoDown(4);

        // Assert
        Assert.Equal(lowerFloor, elevator.CurrentFloor);
    }

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
