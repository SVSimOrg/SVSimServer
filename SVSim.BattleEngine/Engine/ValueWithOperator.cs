public class ValueWithOperator
{
	public string Value { get; private set; }

	public string Operator { get; private set; }

	public ValueWithOperator(string value, string op)
	{
		Value = value;
		Operator = op;
	}
}
