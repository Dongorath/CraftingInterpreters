using System.Globalization;

namespace SharpLoxVM;

internal class Value
{
	public double Val { get; set; }

	public override string ToString()
	{
		return Val.ToString(CultureInfo.InvariantCulture);
	}
}
