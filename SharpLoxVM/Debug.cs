namespace SharpLoxVM;

internal static class Debug
{
	public static void DisassembleChunk(Chunk chunk, string name)
	{
		Console.WriteLine($"== {name} ==");

		for (int offset = 0; offset < chunk.Count;)
		{
			offset = DisassembleInstruction(chunk, offset);
		}
	}

	public static int DisassembleInstruction(Chunk chunk, int offset)
	{
		Console.Write($"{offset:D4} ");

		if (offset > 0 && chunk.Lines[offset] == chunk.Lines[offset - 1])
		{
			Console.Write("   | ");
		}
		else
		{
			Console.Write($"{chunk.Lines[offset],4} ", chunk.Lines[offset]);
		}

		byte instruction = chunk.Code[offset];
		switch ((OpCode)instruction)
		{
			case OpCode.OP_CONSTANT:
				return ConstantInstruction("OP_CONSTANT", chunk, offset);
			case OpCode.OP_ADD:
				return SimpleInstruction("OP_ADD", offset);
			case OpCode.OP_SUBTRACT:
				return SimpleInstruction("OP_SUBTRACT", offset);
			case OpCode.OP_MULTIPLY:
				return SimpleInstruction("OP_MULTIPLY", offset);
			case OpCode.OP_DIVIDE:
				return SimpleInstruction("OP_DIVIDE", offset);
			case OpCode.OP_NEGATE:
				return SimpleInstruction("OP_NEGATE", offset);
			case OpCode.OP_RETURN:
				return SimpleInstruction("OP_RETURN", offset);
			default:
				Console.WriteLine($"Unknown opcode {instruction}");
				return offset + 1;
		}
	}

	private static int ConstantInstruction(string name, Chunk chunk, int offset) {
		byte constant = chunk.Code[offset + 1];
		Console.Write($"{name,-16} {constant,4} '");
		Console.Write(chunk.Constants.Values[constant].ToString());
		Console.WriteLine("'");
		return offset + 2;
	}

	private static int SimpleInstruction(string name, int offset)
	{
		Console.WriteLine(name);
		return offset + 1;
	}
}
