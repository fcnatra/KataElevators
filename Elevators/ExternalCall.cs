namespace Elevators;

public class ExternalCall : IComparable<ExternalCall>
{
	public int Floor { get; }
	public Direction Direction { get; }
	public ExternalCall(int floor, Direction direction)
	{
		Floor = floor;
		Direction = direction;
	}
	public int CompareTo(ExternalCall? other)
	{
		if (other is null) return 1;
		return Floor.CompareTo(other.Floor);
	}
	public override bool Equals(object? obj)
	{
		if (obj is not ExternalCall other) return false;
		return Floor == other.Floor && Direction == other.Direction;
	}
	public override int GetHashCode() => HashCode.Combine(Floor, Direction);
}
