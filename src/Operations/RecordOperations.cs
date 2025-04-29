using Microsoft.Data.Sqlite;
using Spectre.Console;
using Golvi1124.HabitLogger.src.Database;
using Golvi1124.HabitLogger.src.Helpers;
using Golvi1124.HabitLogger.src.Models;


namespace Golvi1124.HabitLogger.src.Operations;
public class RecordOperations
{
    HelperMethods helper = new();
    private readonly string _connectionString;

    public RecordOperations()
    {
        _connectionString = DatabaseConfig.ConnectionString;
    }

    public void ViewRecords(List<RecordWithHabit> records) // for visualizing the records in a table
    {
        var table = new Table(); // Spectre Console table 
        table.AddColumn("Id"); // table Class from Spectre Console
        table.AddColumn("Date");
        table.AddColumn("Amount");
        table.AddColumn("Habit");

        foreach (var record in records)
        {
            table.AddRow(record.Id.ToString(), record.Date.ToString("D"), $"{record.Quantity} {record.MeasurementUnit}", record.HabitName.ToString());
        }

        AnsiConsole.Write(table); // write the table to the console
    }

    public void AddRecord()
    {
        string date = helper.GetDate("\nEnter the date (format - dd-mm-yy) or insert 0 to Go Back to Main Menu:\n");

        helper.GetHabits();
        int habitId = helper.GetNumber("\nPlease enter the ID of the habit you want to add a record for.");

        int quantity = helper.GetNumber("\nPlease enter number of habit's amount (no decimals or negatives allowed) or enter 0 to go back to Main Menu.");

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
        helper.GetRecords();

        int id = helper.GetNumber("\nPlease enter the ID of the record you want to update.");

        bool updateDate = AnsiConsole.Confirm("Do you want to update the date?");
        string date = "";
        if (updateDate)
        {
            date = helper.GetDate("\nEnter the new date (format - dd-mm-yy) or insert 0 to Go Back to Main Menu:\n");
        }

        bool updateQuantity = AnsiConsole.Confirm("Do you want to update the quantity?");
        int quantity = 0;
        if (updateQuantity)
        {
            quantity = helper.GetNumber("\nPlease enter the new quantity (no decimals or negatives allowed) or enter 0 to go back to Main Menu.");
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
        helper.GetRecords(); // show the records to the user so they can select which one to delete

        int id = helper.GetNumber("\nPlease enter the ID of the record you want to delete.");

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
