namespace Postgres.Embedded.Exceptions;

public class BinaryNotFoundException : EmbeddedPostgresException
{
    public string ExpectedPath { get; }
    
    public BinaryNotFoundException(string path, string message) 
        : base(message)
    {
        ExpectedPath = path;
    }
    
    public BinaryNotFoundException(string path, string message, Exception innerException) 
        : base(message, innerException)
    {
        ExpectedPath = path;
    }
}