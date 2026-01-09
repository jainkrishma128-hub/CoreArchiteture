using Microsoft.Data.SqlClient;

var connectionString = "Server=DESKTOP-10Q9Q13\\SQLEXPRESS;Database=CommonArchitectureDb;Integrated Security=True;MultipleActiveResultSets=true;TrustServerCertificate=True";

var sqlScript = File.ReadAllText("../setup-complete-data.sql");

try
{
    using var connection = new SqlConnection(connectionString);
    connection.Open();
    
    using var command = new SqlCommand(sqlScript, connection);
    command.ExecuteNonQuery();
    
    Console.WriteLine("✅ Database seeded successfully!");
}
catch (Exception ex)
{
    Console.WriteLine($"❌ Error: {ex.Message}");
}
