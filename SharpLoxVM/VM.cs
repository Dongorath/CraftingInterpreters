using System.Runtime.CompilerServices;

namespace SharpLoxVM;
internal static class VM
{
	private const int STACK_MAX = 256;

	private static Chunk? _chunk;
	private static int _ip;
	private static readonly Value[] _stack = new Value[STACK_MAX];
	private static int _stackTop = 0;

	public static void Init()
	{
		ResetStack();
	}

	private static void ResetStack()
	{
		_stackTop = 0;
	}

	private static void Push(Value value)
	{
		_stack[_stackTop++] = value;
	}

	private static Value Pop()
	{
		return _stack[--_stackTop];
	}

	public static void Free()
	{
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static byte ReadByte()
	{
		return _chunk!.Code[_ip++];
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static Value ReadConstant()
	{
		return _chunk!.Constants.Values[ReadByte()];
	}

	private static void BinaryOpDouble(OpCode op)
	{
		Value b = Pop();
		Value a = Pop();
		double newVal = op switch
		{
			OpCode.OP_ADD => a.Val + b.Val,
			OpCode.OP_SUBTRACT => a.Val - b.Val,
			OpCode.OP_MULTIPLY => a.Val * b.Val,
			OpCode.OP_DIVIDE => a.Val / b.Val,
			_ => throw new NotSupportedException()
		};
		Push(new Value() { Val = newVal });
	}

	private static InterpretResult Run()
	{
		while (true)
		{
#if DEBUG_TRACE_EXECUTION
			Console.Write("          ");
			for (int i = 0; i < _stackTop; i++)
			{
				Console.Write($"[ {_stack[i]} ]");
			}
			Console.WriteLine();
			Debug.DisassembleInstruction(_chunk!, _ip);
#endif
			OpCode instruction = (OpCode)ReadByte();
			switch (instruction)
			{
				case OpCode.OP_CONSTANT:
					Value constant = ReadConstant();
					Push(constant);
					break;
				case OpCode.OP_ADD:
				case OpCode.OP_SUBTRACT:
				case OpCode.OP_MULTIPLY:
				case OpCode.OP_DIVIDE:
					BinaryOpDouble(instruction);
					break;
				case OpCode.OP_NEGATE:
					Value toNegate = Pop();
					Push(new Value() { Val = -toNegate.Val });
					break;
				case OpCode.OP_RETURN:
					Console.WriteLine(Pop());
					return InterpretResult.INTERPRET_OK;
			}
		}
	}

	public static InterpretResult Interpret(Chunk chunk)
	{
		_chunk = chunk;
		_ip = 0;
		return Run();
	}
}
