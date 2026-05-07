namespace Postgres.Embedded.Exceptions;

public class PortConflictException : EmbeddedPostgresException
{
    public int Port { get; }
    
    public PortConflictException(int port, string message) 
        : base(message)
    {
        Port = port;
    }
    
    public PortConflictException(int port, string message, Exception innerException) 
        : base(message, innerException)
    {
        Port = port;
    }
}