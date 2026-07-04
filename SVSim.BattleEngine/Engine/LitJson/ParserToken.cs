namespace LitJson;

internal enum ParserToken
{
	None = 65536,
	Text,
	Object,
	ObjectPrime,
	Pair,
	PairRest,
	Array,
	ArrayPrime,
	Value,
	ValueRest,
	String}
