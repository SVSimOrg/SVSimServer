namespace Wizard;

public static class ConvertValue
{
	public static int ToInt(object obj)
	{
		return int.Parse(obj.ToString());
	}
}
