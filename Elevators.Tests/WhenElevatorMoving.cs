namespace Elevators.Tests;

public class WhenElevatorMoving
{
    private int _topFloor;
    private int _lowerFloor;
    private Elevator _elevator;

    public WhenElevatorMoving()
    {
        _topFloor = 10;
        _lowerFloor = 0;
        _elevator = new Elevator(_lowerFloor, _topFloor);
        _elevator.SecondsPerFloor = 1;
    }

    [Fact]
    public async Task ElevatorDoesNotGoAboveTopFloor()
    {
        // Arrange
        var tcs = new TaskCompletionSource();

        _elevator.OnAfterStop += _ => tcs.SetResult();

        // Act
        _elevator.GoToFloor(11); // Try to go above top floor

        // Assert
        await tcs.Task;
        Assert.Equal(_topFloor, _elevator.CurrentFloor);
    }

    [Fact]
    public async Task ElevatorDoesNotGoAboveTopFloor_AfterGoingDown()
    {
        // Arrange
        var tcs = new TaskCompletionSource();

        int stopCount = 0;
        _elevator.OnAfterStop += _ => { stopCount++; if (stopCount == 3) tcs.SetResult(); };

        _elevator.GoToFloor(8); // Go up to floor 8
        _elevator.GoToFloor(5); // Go down to floor 5

        // Act
        _elevator.GoToFloor(15); // Try to go above top floor

        // Assert
        await tcs.Task;
        Assert.Equal(_topFloor, _elevator.CurrentFloor);
    }

    [Fact]
    public async Task ElevatorDoesNotGoBelowLowerFloor_AfterGoingUp()
    {
        // Arrange
        var tcs = new TaskCompletionSource();

        _elevator.OnAfterStop += _ => tcs.SetResult();

        _elevator.GoToFloor(4);

        // Act
        _elevator.GoToFloor(-1);

        // Assert
        await tcs.Task;
        Assert.Equal(_lowerFloor, _elevator.CurrentFloor);
    }

    [Fact]
    public async Task ElevatorDoesNotGoBelowLowerFloor()
    {
        // Arrange
        var tcs = new TaskCompletionSource();

        _elevator.OnAfterStop += _ => tcs.SetResult();

        // Act
        _elevator.GoToFloor(-4);

        // Assert
        await tcs.Task;
        Assert.Equal(_lowerFloor, _elevator.CurrentFloor);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    public async Task UpElevatorGoesUp(int floor)
    {
        // Arrange
        var tcs = new TaskCompletionSource();

        _elevator.OnAfterStop += _ => tcs.SetResult();

        // Act
        _elevator.GoToFloor(floor);

        // Assert
        await tcs.Task;
        Assert.Equal(floor, _elevator.CurrentFloor);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    public async Task DownElevatorGoesDown(int floor)
    {
        // Arrange
        var tcs = new System.Threading.Tasks.TaskCompletionSource();
        _elevator.OnAfterStop += _ => tcs.SetResult();

        // Move elevator up first
        _elevator.GoToFloor(floor);

        // Act
        _elevator.GoToFloor(0);

        // Assert
        await tcs.Task;
        Assert.Equal(0, _elevator.CurrentFloor);
    }
}
