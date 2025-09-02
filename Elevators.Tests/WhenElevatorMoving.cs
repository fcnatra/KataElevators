using System.Diagnostics;

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

        _elevator.GoToFloor(8);
        _elevator.GoToFloor(5);

        // Act
        _elevator.GoToFloor(15); // Try to go above top floor

        // Assert
        await tcs.Task;
        Assert.Equal(_topFloor, _elevator.CurrentFloor);
    }

    [Fact]
    public async Task ElevatorDoesNotGoBelowLowerFloor()
    {
    // Arrange
    bool beforeMovingCalled = false;
    _elevator.OnBeforeMoving += () => beforeMovingCalled = true;

    // Act
    _elevator.GoToFloor(-4);

    // Wait at least the time it would take to move one floor
    await Task.Delay(_elevator.SecondsPerFloor * 1000);

    // Assert
    Assert.False(beforeMovingCalled); // Should not have moved
    Assert.Equal(_lowerFloor, _elevator.CurrentFloor);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    public async Task UpElevatorGoesUp(int floor)
    {
        // Arrange
        var tcs = new TaskCompletionSource();

        _elevator.OnAfterStop += _ => { Debug.WriteLine($"Elevator stopped at floor {_elevator.CurrentFloor}"); tcs.SetResult(); };

        // Act
        _elevator.GoToFloor(floor);

        // Assert
        await tcs.Task;
        Assert.Equal(floor, _elevator.CurrentFloor);
    }

    [Theory]
    [InlineData(1, 0, 0)]
    [InlineData(2, 0, 0)]
    [InlineData(2, -2, 0)]
    public async Task DownElevatorGoesDown(int floorUp, int floorDown, int finalFloor)
    {
        // Arrange
        var tcs = new TaskCompletionSource();
        _elevator.OnAfterStop += _ => { Debug.WriteLine($"Elevator stopped at floor {_elevator.CurrentFloor}"); tcs.SetResult(); };

        // Move elevator up first
        _elevator.GoToFloor(floorUp);
        await tcs.Task;
        tcs = new TaskCompletionSource();

        // Act
        _elevator.GoToFloor(floorDown);

        // Assert
        await tcs.Task;
        Assert.Equal(finalFloor, _elevator.CurrentFloor);
    }
}
