using SharpLoxVM;

Chunk chunk = new Chunk();
chunk.Init();

int constant = chunk.AddConstant(new Value() { Val = 1.2 });
chunk.Write((byte)OpCode.OP_CONSTANT, 123);
chunk.Write((byte)constant, 123);

chunk.Write((byte)OpCode.OP_RETURN, 123);

Debug.DisassembleChunk(chunk, "test chunk");
chunk.Free();