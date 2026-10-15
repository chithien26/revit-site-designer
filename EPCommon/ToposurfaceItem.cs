namespace EPCommon;

public class ToposurfaceItem
{
	public string Name;

	public TopographySurfaceContainer Value;

	public ToposurfaceItem(string name, TopographySurfaceContainer value)
	{
		Name = name;
		Value = value;
	}

	public override string ToString()
	{
		return Name;
	}
}
