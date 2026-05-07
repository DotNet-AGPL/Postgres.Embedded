namespace Postgres.Embedded.Exceptions;

public class EmbeddedPostgresException : Exception
{
    public EmbeddedPostgresException(string message) : base(message) { }
    
    public EmbeddedPostgresException(string message, Exception innerException) 
        : base(message, innerException) { }
}