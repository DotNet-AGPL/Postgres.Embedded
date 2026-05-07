namespace Postgres.Embedded.Exceptions;

public class DatabaseInitException : EmbeddedPostgresException
{
    public DatabaseInitException(string message) : base(message) { }
    
    public DatabaseInitException(string message, Exception innerException) 
        : base(message, innerException) { }
}