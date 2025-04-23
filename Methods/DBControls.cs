using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;
using static Golvi1124.HabitLogger.Methods.Config;


namespace Golvi1124.HabitLogger.Methods;

public static class DBControls
{
    public static void CreateDatabase()
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
                    );"; //TEXT (correct SQLite type) instead of TXT (which isn't standard)
                tableCmd.ExecuteNonQuery(); //call the ExecuteNonQuery method when we don't want any data to be returned
            }
        }
    }
}
