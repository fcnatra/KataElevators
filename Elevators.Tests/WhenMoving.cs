namespace Elevators.Tests;

public class WhenMoving
{
    [Fact]
    public async Task ElevatorDoesNotGoAboveTopFloor()
    {
        // Arrange
        int topFloor = 10;
        var tcs = new TaskCompletionSource();

        var elevator = new Elevator(0, topFloor);
        elevator.OnStop += _ => tcs.SetResult();

        // Act
        elevator.GoToFloor(11); // Try to go above top floor

        // Assert
        await tcs.Task;
        Assert.Equal(topFloor, elevator.CurrentFloor);
    }

    [Fact]
    public async Task ElevatorDoesNotGoAboveTopFloor_AfterGoingDown()
    {
        // Arrange
        int topFloor = 10;
        var tcs = new TaskCompletionSource();

        var elevator = new Elevator(0, topFloor);
        int stopCount = 0;
        elevator.OnStop += _ => { stopCount++; if (stopCount == 3) tcs.SetResult(); };

        elevator.GoToFloor(8); // Go up to floor 8
        elevator.GoToFloor(5); // Go down to floor 5

        // Act
        elevator.GoToFloor(15); // Try to go above top floor

        // Assert
        await tcs.Task;
        Assert.Equal(topFloor, elevator.CurrentFloor);
    }

    [Fact]
    public async Task ElevatorDoesNotGoBelowLowerFloor_AfterGoingUp()
    {
        // Arrange
        int lowerFloor = 0;
        var tcs = new TaskCompletionSource();

        var elevator = new Elevator(lowerFloor, 10);
        elevator.OnStop += _ => tcs.SetResult();

        elevator.GoToFloor(4);

        // Act
        elevator.GoToFloor(-1);

        // Assert
        await tcs.Task;
        Assert.Equal(lowerFloor, elevator.CurrentFloor);
    }

    [Fact]
    public async Task ElevatorDoesNotGoBelowLowerFloor()
    {
        // Arrange
        int lowerFloor = 0;
        var tcs = new TaskCompletionSource();

        var elevator = new Elevator(lowerFloor, 10);
        elevator.OnStop += _ => tcs.SetResult();

        // Act
        elevator.GoToFloor(-4);

        // Assert
        await tcs.Task;
        Assert.Equal(lowerFloor, elevator.CurrentFloor);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    public async Task UpElevatorGoesUp(int floor)
    {
        // Arrange
        var tcs = new TaskCompletionSource();
        
        var elevator = new Elevator(0, 10);
        elevator.OnStop += _ => tcs.SetResult();

        // Act
        elevator.GoToFloor(floor);

        // Assert
        await tcs.Task;
        Assert.Equal(floor, elevator.CurrentFloor);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    public async Task DownElevatorGoesDown(int floor)
    {
        // Arrange
        var elevator = new Elevator(0, 10);
        var tcs = new System.Threading.Tasks.TaskCompletionSource();
        elevator.OnStop += _ => tcs.SetResult();

        // Move elevator up first
        elevator.GoToFloor(floor);

        // Act
        elevator.GoToFloor(0);

        // Assert
        await tcs.Task;
        Assert.Equal(0, elevator.CurrentFloor);
    }
}
