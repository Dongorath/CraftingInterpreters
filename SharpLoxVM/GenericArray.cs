namespace SharpLoxVM;

internal class GenericArray<T>
{
	public int Count { get; private set; } = 0;
	public int Capacity => Array.Length;
	private T[] _array = [];
	public T[] Array => _array;

	public virtual void Init()
	{
		Count = 0;
		_array = [];
	}

	public virtual void Free()
	{
		Init();
	}

	public void Write(T val)
	{
		if (Capacity <= Count)
			System.Array.Resize(ref _array, Grow(Capacity));
		Array[Count] = val;
		Count++;
	}

	private static int Grow(int capacity)
	{
		if (capacity < 8)
			return 8;
		return capacity * 2;
	}
}
