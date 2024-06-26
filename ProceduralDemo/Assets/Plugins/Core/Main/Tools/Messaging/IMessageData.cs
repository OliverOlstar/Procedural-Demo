public interface IMessageData
{
}

public class NullMessageData : IMessageData
{
	public static NullMessageData Empty = new();

	// Constructor made private so that outside of this class only NullMessageData.Empty can be used
	private NullMessageData() { }
}