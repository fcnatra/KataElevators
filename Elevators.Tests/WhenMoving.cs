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
        elevator.GoToFloor(11); // Try to go above top floor

        // Assert
        Assert.Equal(topFloor, elevator.CurrentFloor);
    }

    [Fact]
    public void ElevatorDoesNotGoAboveTopFloor_AfterGoingDownAndUp()
    {
        // Arrange
        int topFloor = 10;
        var elevator = new Elevator(0, topFloor);
        elevator.GoToFloor(8); // Go up to floor 8
        elevator.GoToFloor(5); // Go down to floor 5

        // Act
        elevator.GoToFloor(15); // Try to go above top floor

        // Assert
        Assert.Equal(topFloor, elevator.CurrentFloor);
    }
    [Fact]
    public void ElevatorDoesNotGoBelowLowerFloor_AfterGoingUpAndDown()
    {
        // Arrange
        int lowerFloor = 0;
        var elevator = new Elevator(lowerFloor, 10);
        elevator.GoToFloor(4);

        // Act
        elevator.GoToFloor(-1);

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
        elevator.GoToFloor(-4);

        // Assert
        Assert.Equal(lowerFloor, elevator.CurrentFloor);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    public void UpElevatorGoesUp(int floor)
    {
        // Arrange
        var elevator = new Elevator(0, 10);

        // Act
        elevator.GoToFloor(floor);

        // Assert
        Assert.Equal(floor, elevator.CurrentFloor);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    public void DownElevatorGoesDown(int floor)
    {
        // Arrange
        var elevator = new Elevator(0, 10);
        // Move elevator up first
        elevator.GoToFloor(floor);

        // Act
        elevator.GoToFloor(0);

        // Assert
        Assert.Equal(0, elevator.CurrentFloor);
    }
}
