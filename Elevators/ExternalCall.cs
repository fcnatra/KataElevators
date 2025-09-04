namespace Elevators;

public struct ExternalCall
{
	public int Floor { get; }
	public CallDirection Direction { get; }
	public ExternalCall(int floor, CallDirection direction)
	{
		Floor = floor;
		Direction = direction;
	}
	public override bool Equals(object? obj)
	{
		if (obj is not ExternalCall other) return false;
		return Floor == other.Floor && Direction == other.Direction;
	}
	public override int GetHashCode() => HashCode.Combine(Floor, Direction);
}
