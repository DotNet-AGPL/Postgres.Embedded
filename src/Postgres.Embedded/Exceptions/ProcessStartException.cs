namespace Postgres.Embedded.Exceptions;

public class ProcessStartException : EmbeddedPostgresException
{
    public ProcessStartException(string message) : base(message) { }
    
    public ProcessStartException(string message, Exception innerException) 
        : base(message, innerException) { }
}