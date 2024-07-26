using SharpLoxVM;

VM.Init();

Chunk chunk = new Chunk();
chunk.Init();

int constant = chunk.AddConstant(new Value() { Val = 1.2 });
chunk.Write((byte)OpCode.OP_CONSTANT, 123);
chunk.Write((byte)constant, 123);

constant = chunk.AddConstant(new Value() { Val = 3.4 });
chunk.Write((byte)OpCode.OP_CONSTANT, 123);
chunk.Write((byte)constant, 123);

chunk.Write((byte)OpCode.OP_ADD, 123);

constant = chunk.AddConstant(new Value() { Val = 5.6 });
chunk.Write((byte)OpCode.OP_CONSTANT, 123);
chunk.Write((byte)constant, 123);

chunk.Write((byte)OpCode.OP_DIVIDE, 123);
chunk.Write((byte)OpCode.OP_NEGATE, 123);

chunk.Write((byte)OpCode.OP_RETURN, 123);

Debug.DisassembleChunk(chunk, "test chunk");
VM.Interpret(chunk);
VM.Free();
chunk.Free();