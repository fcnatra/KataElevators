using System.Runtime;
using System.Threading.Tasks;
using FakeItEasy;

namespace Elevators.Tests
{
    public class WhenCallingElevator
    {
        private Elevator _elevator;

        public WhenCallingElevator()
        {
            _elevator = new Elevator(0, 10);
            _elevator.SecondsPerFloor = 1;
        }

        [Fact]
        public async Task Up_GoesToTheCallingFloor()
        {
            // Arrange
            var tcs = new TaskCompletionSource();
            _elevator.OnAfterStop += (floor) => tcs.SetResult();

            var controller = new Controller(_elevator);
            controller.CallElevatorUp(3);

            // Assert
            await tcs.Task;
            Assert.Equal(3, _elevator.CurrentFloor);
        }

        [Fact]
        public async Task Up_HandleUpwardCallsFirst()
        {
            // Arrange
            var attendedFloors = new HashSet<int>();
            var stops = 0;
            var tcs = new TaskCompletionSource();
            _elevator.OnDoorsOpened += () =>
            {
                attendedFloors.Add(_elevator.CurrentFloor);
                stops++;
                if (stops == 1 || stops == 3) tcs.SetResult();
            };

            var controller = new Controller(_elevator);
            controller.CallElevatorUp(3);
            await tcs.Task;
            tcs = new TaskCompletionSource();

            // Act
            controller.SelectDestinationFloor(5);
            controller.SelectDestinationFloor(2);

            // Assert
            await tcs.Task;
            Assert.Equal(2, _elevator.CurrentFloor);
            Assert.Equal(new[] { 3, 5, 2 }, attendedFloors);
        }

        [Fact]
        public async Task WhileRespondingCallsUpTo3rd_ProcessesUpCallsTo4th()
        {
            // Assert
            var stops = 0;
            var attendedFloors = new HashSet<int>();
            var tcs = new TaskCompletionSource();
            _elevator.OnAfterStop += (floor) => { stops++; attendedFloors.Add(floor); if (stops == 3) tcs.SetResult(); };

            var controller = new Controller(_elevator);
            controller.CallElevatorUp(3);
            controller.SelectDestinationFloor(6);

            // Act
            controller.CallElevatorUp(4);

            // Assert
            await tcs.Task;
            Assert.Equal(6, _elevator.CurrentFloor);
            Assert.Equal(new[] { 3, 4, 6 }, attendedFloors);
        }

        [Fact]
        public async Task WhileMovingUpTo7th_CapturesCallFrom4th()
        {
            // Assert
            var attendedFloors = new HashSet<int>();
            var movements = 0;
            var tcs = new TaskCompletionSource();
            _elevator.OnBeforeMoving += () => { movements++; if (movements == 2) tcs.SetResult(); };
            _elevator.OnAfterStop += (floor) => { attendedFloors.Add(floor); if (floor == 7) tcs.SetResult(); };

            var controller = new Controller(_elevator);
            controller.CallElevatorUp(1);
            controller.SelectDestinationFloor(7);
            await tcs.Task;
            tcs = new TaskCompletionSource();
            
            // Act
            controller.CallElevatorUp(4);

            // Assert
            await tcs.Task;
            Assert.Equal(7, _elevator.CurrentFloor);
            Assert.Equal(new[] { 1, 4, 7 }, attendedFloors);
        }
    }
}
