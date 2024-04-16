namespace SharpLox;

internal class RuntimeErrorException(Token token, string message) : Exception(message)
{
	public Token Token { get; } = token;
}
