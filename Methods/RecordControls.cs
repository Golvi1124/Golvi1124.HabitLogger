using Microsoft.Data.Sqlite;
using Spectre.Console;
using System.Globalization;
using static Golvi1124.HabitLogger.Methods.Config;
using static Golvi1124.HabitLogger.Methods.Helpers;

namespace Golvi1124.HabitLogger.Methods;

public static class RecordControls
{

    record WalkingRecord(int Id, DateTime Date, int Meters);

    public static void AddRecord()
    {
        string date = GetDate("\nEnter the date (format - dd-mm-yy) or insert 0 to Go Back to Main Menu:\n");
        int quantity = GetNumber("\nPlease enter number of meters walked (no decimals or negatives allowed) or enter 0 to go back to Main Menu.");
        if (date == "0" || quantity == -1) return; // Go back to Main Menu. Special case for 0

        using (SqliteConnection connection = new(connectionString))
        {
            connection.Open();

            using (var command = connection.CreateCommand())
            {
                command.CommandText = $"INSERT INTO walkingHabit (date, meters) VALUES ('{date}', {quantity})";
                command.ExecuteNonQuery();
            }
        }
    }

    public static void GetRecords()
    {
        List<WalkingRecord> records = new(); // Representing rows in db

        // creating an instance of the SQLiteDataReader class.
        using (var connection = new SqliteConnection(connectionString)) // to read the data from the table
        {
            connection.Open();

            using (var command = connection.CreateCommand())
            {
                command.CommandText = "SELECT * FROM walkingHabit"; // select all records from the table

                using (var reader = command.ExecuteReader()) //(SqliteDataReader reader = command.ExecuteReader())
                {
                    /*checking if the reader has rows and if it does we will read the data in a loop. 
                    For each row found in the table, we will add a new WalkingRecord to the list.*/

                    if (reader.HasRows) // check if there are any records in the table
                    {
                        while (reader.Read())
                        {

                            /*The code around the reading operation is wrapped by a try-catch block 
                            to prevent the app from crashing in case the operation against the database goes wrong.*/
                            try
                            {
                                records.Add(new WalkingRecord(
                                    reader.GetInt32(0), // Id
                                    DateTime.ParseExact(reader.GetString(1), "dd-MM-yy", CultureInfo.InvariantCulture), // Date
                                    reader.GetInt32(2) // Meters
                                    )
                                 );
                            }
                            catch (FormatException ex)
                            {
                                Console.WriteLine($"Error reading record: {ex.Message}. Skipping this record.");
                            }
                        }
                    }
                    else
                    {
                        Console.WriteLine("No records found.");
                    }

                }
            }
        }

        ViewRecords(records);
    }

    public static void DeleteRecord()
    {
        GetRecords(); // show the records to the user so they can select which one to delete

        int id = GetNumber("\nPlease enter the ID of the record you want to delete.");

        using (var connection = new SqliteConnection(connectionString))
        {
            connection.Open();
            using (var command = connection.CreateCommand())
            {
                command.CommandText = $"DELETE FROM walkingHabit WHERE Id = {id}"; // delete the record with the given id
                int rowsAffected = command.ExecuteNonQuery();
                if (rowsAffected != 0)
                {
                    Console.WriteLine("Record deleted successfully.");
                }
                else
                {
                    Console.WriteLine("Record not found.");
                }
            }
        }
    }

    public static void UpdateRecord()
    {
        GetRecords();

        int id = GetNumber("\nPlease enter the ID of the record you want to update.");

        bool updateDate = AnsiConsole.Confirm("Do you want to update the date?");
        string date = "";
        if (updateDate)
        {
            date = GetDate("\nEnter the new date (format - dd-mm-yy) or insert 0 to Go Back to Main Menu:\n");
        }

        bool updateQuantity = AnsiConsole.Confirm("Do you want to update the distance?");
        int quantity = 0;
        if (updateQuantity)
        {
            quantity = GetNumber("\nPlease enter the new number of meters walked (no decimals or negatives allowed) or enter 0 to go back to Main Menu.");
        }

        string query; // querry for each combination of updates
        if (updateDate && updateQuantity)
        {
            query = $"UPDATE walkingHabit SET date = '{date}', Meters = {quantity} WHERE Id = {id}";
        }
        else if (updateDate && !updateQuantity)
        {
            query = $"UPDATE walkingHabit SET date = '{date}' WHERE Id = {id}";
        }
        else
        {
            query = $"UPDATE walkingHabit SET Meters = {quantity} WHERE Id = {id}";
        }

        using (var connection = new SqliteConnection(connectionString))
        {
            connection.Open();
            using (var command = connection.CreateCommand())
            {
                command.CommandText = query;
                command.ExecuteNonQuery();
            }
        }
    }

    private static void ViewRecords(List<WalkingRecord> records) // for visualizing the records in a table
    {
        var table = new Table(); // Spectre Console table 
        table.AddColumn("Id"); // table Class from Spectre Console
        table.AddColumn("Date");
        table.AddColumn("Amount");

        foreach (var record in records)
        {
            table.AddRow(record.Id.ToString(), record.Date.ToString("dd-MM-yy"), record.Meters.ToString());
        }

        AnsiConsole.Write(table); // write the table to the console
    }


}
