namespace SharpLoxVM;

internal class ValueArray : GenericArray<Value>
{
	public Value[] Values => Array;
}
