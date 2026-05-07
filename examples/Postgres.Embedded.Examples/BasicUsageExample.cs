using Dapper;
using Npgsql;
using Postgres.Embedded;
using Postgres.Embedded.Models;

namespace Postgres.Embedded.Examples;

public class BasicUsageExample
{
    public static void Main(string[] args)
    {
        Console.WriteLine("=== Postgres.Embedded Basic Usage Example ===");
        
        using var postgres = new EmbeddedPostgresBuilder()
            .WithVersion(PostgresVersion.V16)
            .WithPort(5432)
            .WithDatabase("exampledb")
            .WithStartTimeout(TimeSpan.FromSeconds(30))
            .Build();
        
        Console.WriteLine("Starting PostgreSQL...");
        postgres.Start();
        Console.WriteLine($"PostgreSQL started successfully! PID: {postgres.ProcessId}");
        
        var connectionString = postgres.GetConnectionString();
        Console.WriteLine($"Connection String: {connectionString}");
        
        using var conn = new NpgsqlConnection(connectionString);
        conn.Open();
        
        Console.WriteLine("\nCreating table...");
        conn.Execute("CREATE TABLE users (id SERIAL PRIMARY KEY, name TEXT NOT NULL, email TEXT)");
        
        Console.WriteLine("Inserting data...");
        conn.Execute("INSERT INTO users (name, email) VALUES ('Alice', 'alice@example.com')");
        conn.Execute("INSERT INTO users (name, email) VALUES ('Bob', 'bob@example.com')");
        
        Console.WriteLine("Querying data...");
        var users = conn.Query<User>("SELECT id, name, email FROM users ORDER BY id").ToList();
        
        Console.WriteLine("\nUsers:");
        foreach (var user in users)
        {
            Console.WriteLine($"  ID: {user.Id}, Name: {user.Name}, Email: {user.Email}");
        }
        
        Console.WriteLine("\nStopping PostgreSQL...");
        postgres.Stop();
        Console.WriteLine("PostgreSQL stopped successfully!");
        
        Console.WriteLine("\nExample completed successfully!");
    }
}

public class User
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Email { get; set; }
}