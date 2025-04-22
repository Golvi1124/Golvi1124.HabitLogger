using Microsoft.Data.Sqlite;

string connectionString = @"Data Source = habit-logger.db";

CreateDatabase();

void CreateDatabase()
{
    using (var connection = new SqliteConnection(connectionString))
    {
        connection.Open();

        using (var command = connection.CreateCommand())
        {
            command.CommandText = @"
                CREATE TABLE IF NOT EXISTS walkingHabit (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    Date TXT,
                    Meters INTEGER );";
                   

            command.ExecuteNonQuery();
                
        }
    }
}