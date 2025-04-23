/* Rest of To Do:
 *  handle all possible errors so that the application never crashes.
 *  Follow the DRY Principle, and avoid code repetition - maybe can improve somethign in the end?
 *  Your project needs to contain a Read Me file where you'll explain how your app works. Here's a nice example: https://github.com/thags/ConsoleTimeLogger
 *  You can keep all of the code in one single class if you wish. - seperate methods in different files
 *  
 *  Extra challanges:
 *  If you haven't, try using parameterized queries to make your application more secure.
 *  https://reintech.io/blog/mastering-parameterized-queries-ado-net
 *  Let the users create their own habits to track. That will require that you let them choose the unit of measurement of each habit.
 *  Seed Data into the database automatically when the database gets created for the first time, generating a few habits and inserting a 
 hundred records with randomly generated values. This is specially helpful during development so you don't have to reinsert data every 
time you create the database.
 *  Create a report functionality where the users can view specific information (i.e. how many times the user ran in a year? how many kms?) SQL allows you to ask very interesting things from your database.
 *  
 
 */


// installing Spectre Console NuGet Package to easier to create beautiful, cross platform, console applications
using System.Globalization;
using Microsoft.Data.Sqlite;
using Spectre.Console;

string connectionString = @"Data Source = habit-logger.db"; // path to the database file. If it doesn't exist, it will be created in the same directory as the executable

CreateDatabase();

MainMenu();

void MainMenu()
{
    var isMenuRunning = true;

    while (isMenuRunning)
    {
        // Spectre Connsole NuGet allows user to use arrows to select choices.
        // Promt method returns a string which will be the user choice..stored in variable usersChoice

        var usersChoice = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title("What do you want to do?")
                .AddChoices(
                    "Add Record",
                    "Delete Record",
                    "View Records",
                    "Update Record",
                    "Quit"
                )
            );

        switch (usersChoice)
        {
            case "Add Record":
                AddRecord();
                break;
            case "Delete Record":
                DeleteRecord();
                break;
            case "View Records":
                GetRecords();
                break;
            case "Update Record":
                UpdateRecord();
                break;
            case "Quit":
                Console.WriteLine("Goddbye!");
                isMenuRunning = false;
                break;
            default: //since using Spectre consule, don't need necceserily to use default case, since user can't select anything else than the options
                Console.WriteLine("Invalid choice. Please try again.");
                break;
        }
    }
}

void UpdateRecord()
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

void DeleteRecord()
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

void GetRecords()
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

void ViewRecords(List<WalkingRecord> records) // for visualizing the records in a table
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

void AddRecord()
{
    string date = GetDate("\nEnter the date (format - dd-mm-yy) or insert 0 to Go Back to Main Menu:\n");
    int quantity = GetNumber("\nPlease enter number of meters walked (no decimals or negatives allowed) or enter 0 to go back to Main Menu.");

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

int GetNumber(string message)
{
    Console.WriteLine(message);
    string numberInput = Console.ReadLine();

    if (numberInput == "0") MainMenu();
    int output = 0;

    while (!int.TryParse(numberInput, out output) || output < 0)
    {
        Console.WriteLine("\n\nInvalid number. Please try again!\n\n");
        numberInput = Console.ReadLine();
    }
    return output;

}

string GetDate(string message) //will use it display message to user about wanted input
{
    Console.WriteLine(message);
    string dateInput = Console.ReadLine();

    if (dateInput == "0") MainMenu(); //if input 0, return to menu

    // returns input only if parsing successful. if incorrect, stays in the loop
    while (!DateTime.TryParseExact(dateInput, "dd-MM-yy", CultureInfo.InvariantCulture, DateTimeStyles.None, out _))
    {
        Console.WriteLine("\n\nInvalid date. (Format: dd-mm-yy). Please try again!\n\n");
        dateInput = Console.ReadLine();
    }
    return dateInput;
}








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
                    );"; //TEXT (correct SQLite type) instead of TXT (which isn't standard)
            tableCmd.ExecuteNonQuery(); //call the ExecuteNonQuery method when we don't want any data to be returned
        }
    }
}
// Record is a new C# 9 feature. It is a reference type that provides built-in functionality for encapsulating data.
// It is immutable(cannot be changed after it is created) by default and provides value-based equality.
record WalkingRecord(int Id, DateTime Date, int Meters);