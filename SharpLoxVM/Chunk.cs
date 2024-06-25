namespace SharpLoxVM;

internal class Chunk : GenericArray<byte>
{
	public byte[] Code => Array;

	private int[] _lines = [];
	public int[] Lines => _lines;
	public ValueArray Constants { get; } = new ValueArray();

	public override void Init()
	{
		base.Init();
		_lines = [];
		Constants.Init();
	}

	public override void Free()
	{
		Constants.Free();
		base.Free();
	}

	public int AddConstant(Value value)
	{
		Constants.Write(value);
		return Constants.Count - 1;
	}

	public void Write(byte val, int line)
	{
		Write(val);
		if (Capacity != Lines.Length)
			System.Array.Resize(ref _lines, Capacity);
		Lines[Count - 1] = line;
	}
}
