using Microsoft.Data.Sqlite;

string connectionString = @"Data Source = habit-logger.db";

CreateDatabase();

void CreateDatabase()
{
    using (SqliteConnection connection = new(connectionString)) //changed to C# 9+ shorthand syntax (before:  using (var connection = new SqliteConnection(connectionString)))
    {
        using (SqliteCommand tableCmd = connection.CreateCommand())
        {
            connection.Open(); //now comes after the command is created instead of before
            tableCmd.CommandText = // 'tableCmd' is more descriptive than 'command'. no fifference in functionality
                @"CREATE TABLE IF NOT EXISTS walkingHabit (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    Date TEXT, 
                    Quantity INTEGER
                    )"; //TEXT (correct SQLite type) instead of TXT (which isn't standard)
            tableCmd.ExecuteNonQuery();
        }
    }
}