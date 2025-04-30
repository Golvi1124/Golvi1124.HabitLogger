using Microsoft.Data.Sqlite;
using Spectre.Console;
using Golvi1124.HabitLogger.src.Database;
using Golvi1124.HabitLogger.src.Helpers;
using Golvi1124.HabitLogger.src.Models;


namespace Golvi1124.HabitLogger.src.Operations;
public class RecordOperations
{
    private readonly HelperMethods? _helper; // Changed to readonly and renamed for clarity
    private readonly string? _connectionString;

      public RecordOperations()
       {
           _helper = new HelperMethods(); // Ensure helper is instantiated
       }
   

    public List<RecordWithHabit> GetRecords()
    {
        return _helper.GetRecords(); // Expose a public method to access records
    }



    public void ViewRecords(List<RecordWithHabit> records)
    {
        var table = new Table();

        // Add columns to the table
        table.AddColumn("ID");
        table.AddColumn("Date");
        table.AddColumn("Habit Name");
        table.AddColumn("Quantity");
        table.AddColumn("Measurement Unit");

        // Add rows to the table
        foreach (var record in records)
        {
            table.AddRow(
                record.Id.ToString(),
                record.Date.ToString("yyyy-MM-dd"),
                record.HabitName,
                record.Quantity.ToString(),
                record.MeasurementUnit
            );
        }

        // Render the table
        AnsiConsole.Write(table);
    }



    public void AddRecord()
    {
        string date = _helper.GetDate("\nEnter the date (format - dd-mm-yy) or insert 0 to Go Back to Main Menu:\n");

        _helper.GetHabits();
        int habitId = _helper.GetNumber("\nPlease enter the ID of the habit you want to add a record for.");

        int quantity = _helper.GetNumber("\nPlease enter number of habit's amount (no decimals or negatives allowed) or enter 0 to go back to Main Menu.");

        Console.Clear();
        using (SqliteConnection connection = new(_connectionString))
        using (SqliteCommand insertCmd = connection.CreateCommand())
        {
            connection.Open();
            insertCmd.CommandText = $"INSERT INTO records(date, quantity, habitId) VALUES('{date}', {quantity}, {habitId})";
            insertCmd.ExecuteNonQuery();
        }
    }

    public void UpdateRecord()
    {
        _helper.GetRecords();

        int id = _helper.GetNumber("\nPlease enter the ID of the record you want to update.");

        bool updateDate = AnsiConsole.Confirm("Do you want to update the date?");
        string date = "";
        if (updateDate)
        {
            date = _helper.GetDate("\nEnter the new date (format - dd-mm-yy) or insert 0 to Go Back to Main Menu:\n");
        }

        bool updateQuantity = AnsiConsole.Confirm("Do you want to update the quantity?");
        int quantity = 0;
        if (updateQuantity)
        {
            quantity = _helper.GetNumber("\nPlease enter the new quantity (no decimals or negatives allowed) or enter 0 to go back to Main Menu.");
        }

        string query; // querry for each combination of updates
        if (updateDate && updateQuantity)
        {
            query = $"UPDATE records SET date = '{date}', Quantity = {quantity} WHERE Id = {id}";
        }
        else if (updateDate && !updateQuantity)
        {
            query = $"UPDATE records SET date = '{date}' WHERE Id = {id}";
        }
        else
        {
            query = $"UPDATE records SET Quantity = {quantity} WHERE Id = {id}";
        }

        using (SqliteConnection connection = new(_connectionString))
        using (SqliteCommand updateCmd = connection.CreateCommand())
        {
            connection.Open();
            updateCmd.CommandText = query; // set the command text to the query
            updateCmd.ExecuteNonQuery();
        }
    }

    public void DeleteRecord()
    {
        _helper.GetRecords(); // show the records to the user so they can select which one to delete

        int id = _helper.GetNumber("\nPlease enter the ID of the record you want to delete.");

        using (SqliteConnection connection = new(_connectionString))
        using (SqliteCommand deleteCmd = connection.CreateCommand())
        {
            connection.Open();
            deleteCmd.CommandText = $"DELETE FROM records WHERE Id = {id}"; // delete the record with the given id
            int rowsAffected = deleteCmd.ExecuteNonQuery();
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
