/*
 Need to adjust things like this (habit is running so not gonna work for this or in a future):

    Please enter the ID of the habit you want to add a record for.
    1
    Please enter number of meters walked (no decimals or negatives allowed) or enter 0 to go back to Main Menu.
--------------------------------------------------------------------------------------------------------
Please enter the ID of the record you want to update.
1
Do you want to update the date? [y/n] (y): n
Do you want to update -> the distance? <-   [y/n] (y): y
Please enter the new number of meters walked (no decimals or negatives allowed) or enter 0 to go back to Main Menu.
45
DOESNT WORK/UPDATE THE ENTRY
--------------------------------------------------------------------------------------------------------
void UpdateHabit()
=> Microsoft.Data.Sqlite.SqliteException: 'SQLite Error 1: 'no such column: Unit'.' 
in     string query;
    if (updateName && updateUnit)
    {
        query = $"UPDATE habits SET Name = '{name}', MeasurementUnit = '{unit}' WHERE Id = {id}";
    }
    else if (updateName && !updateUnit)
    {
        query = $"UPDATE habits SET Name = '{name}' WHERE Id = {id}";           <==
--------------------------------------------------------------------------------------------------------
 */

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
                    "Add Habit",
                    "Delete Habit",
                    "View Habits", 
                    "Update Habit",
                    "Add Record",
                    "Delete Record",
                    "View Records",
                    "Update Record",
                    "Quit"
                )
            );

        switch (usersChoice)
        {
            case "Add Habit":
                AddHabit();
                break;
            case "Delete Habit":
                DeleteHabit();
                break;
            case "View Habits":
                GetHabits();
                break; 
            case "Update Habit":
                UpdateHabit();
                break;
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
                // Because of using Spectre console, don't need to use default case, since user can't select anything else than the options
        }
    }
}

void CreateDatabase()
{
    {
        using (SqliteConnection connection = new(connectionString))
        using (SqliteCommand tableCmd = connection.CreateCommand())
        {
            connection.Open();

            tableCmd.CommandText =
                @"CREATE TABLE IF NOT EXISTS records (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    Date TEXT, 
                    Quantity INTEGER,
                    HabitId INTEGER,
                    FOREIGN KEY (HabitId) REFERENCES habits(Id) ON DELETE CASCADE
                    )";
            /*
                * HabitId property associate all rows with a row in the Habits table. The association happens due to the FOREIGN KEY constraint.
            Script's structure : FOREIGN KEY(<column that will be linked to the external table>) REFERENCES habits(<column we will link to, in the external table>)
                * ON DELETE CASCADE means if we delete a record in the habits table, all records that have it as a foreign key will be deleted in the records table. 
            It's a very important script for data integrity.
             */
            tableCmd.ExecuteNonQuery(); // call the ExecuteNonQuery method when we don't want any data to be returned

            tableCmd.CommandText =
                @"CREATE TABLE IF NOT EXISTS habits (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    Name TEXT, 
                    MeasurementUnit TEXT
                    )";
            tableCmd.ExecuteNonQuery();
        }
    }
}

// Habit handling methods

void AddHabit()
{
    var name = AnsiConsole.Ask<string>("What's the habit's name?");
    while (string.IsNullOrWhiteSpace(name)) //added check for whitespace 
    {
        name = AnsiConsole.Ask<string>("Habit's name can't be empty. Try again:");
    }

    var unit = AnsiConsole.Ask<string>("What's the habit's unit of measurement?");
    while (string.IsNullOrEmpty(unit))
    {
        unit = AnsiConsole.Ask<string>("Unit of measurement can't be empty. Try again:");
    }
    using (SqliteConnection connection = new(connectionString))
    using (SqliteCommand insertCmd = connection.CreateCommand())
    {
        connection.Open();
        insertCmd.CommandText = $"INSERT INTO habits (Name, MeasurementUnit) VALUES ('{name}', '{unit}')";
        insertCmd.ExecuteNonQuery();
    }
}

void DeleteHabit()
{
    GetHabits();

    var id = GetNumber("Please type the id of the habit you want to delete.");
    using (SqliteConnection connection = new(connectionString))
    using (SqliteCommand deleteCmd = connection.CreateCommand())
    {
        connection.Open();
        deleteCmd.CommandText = $"DELETE FROM habits WHERE Id = {id}";
        int rowsAffected = deleteCmd.ExecuteNonQuery();
        if (rowsAffected != 0)
        {
            Console.WriteLine("Habit deleted successfully.");
        }
        else
        {
            Console.WriteLine("Habit not found.");
        }
        deleteCmd.ExecuteNonQuery();
    }
}

void ViewHabits(List<Habit> habits) // for visualizing the habits in a table
{
    var table = new Table(); // Spectre Console table 
    table.AddColumn("Id"); // table Class from Spectre Console
    table.AddColumn("Name");
    table.AddColumn("Unit of Measurement");
    foreach (var habit in habits)
    {
        table.AddRow(habit.Id.ToString(), habit.Name.ToString(), habit.UnitOfMeasurement.ToString());
    }
    AnsiConsole.Write(table); // write the table to the console
}
void UpdateHabit()
{
    GetHabits();
    var id = GetNumber("Please type the id of the habit you want to update.");

    string name = "";
    bool updateName = AnsiConsole.Confirm("Do you want to update the name?");
    if (updateName)
    {
        name = AnsiConsole.Ask<string>("Habit's new name:");
        while (string.IsNullOrWhiteSpace(name))
        {
            name = AnsiConsole.Ask<string>("Habit's name can't be empty. Try again:");
        }
    }

    string unit = "";
    bool updateUnit = AnsiConsole.Confirm("Update Unit of Measurement?");
    if (updateUnit)
    {
        unit = AnsiConsole.Ask<string>("Habit's Unit of Measurement:");
        while (string.IsNullOrEmpty(unit))
        {
            unit = AnsiConsole.Ask<string>("Habit's unit can't be empty. Try again:");
        }
    }

    string query;
    if (updateName && updateUnit)
    {
        query = $"UPDATE habits SET Name = '{name}', MeasurementUnit = '{unit}' WHERE Id = {id}";
    }
    else if (updateName && !updateUnit)
    {
        query = $"UPDATE habits SET Name = '{name}' WHERE Id = {id}";
    }
    else
    {
        query = $"UPDATE habits SET MeasurementUnit = '{unit}' WHERE Id = {id}";
    }

    using (SqliteConnection connection = new(connectionString))
    using (SqliteCommand updateCmd = connection.CreateCommand())
    {
        connection.Open();
        updateCmd.CommandText = query;
        updateCmd.ExecuteNonQuery();
    }
}

void GetHabits()
{
    List<Habit> habits = new();

    using (SqliteConnection connection = new(connectionString))
    using (SqliteCommand getCmd = connection.CreateCommand())
    {
        connection.Open();
        getCmd.CommandText = "SELECT * FROM habits";

        using (SqliteDataReader reader = getCmd.ExecuteReader())
        {
            if (reader.HasRows)
            {
                while (reader.Read())
                {
                    try
                    {
                        habits.Add(
                            new Habit(
                            reader.GetInt32(0), // Id
                            reader.GetString(1), // Name
                            reader.GetString(2) // MeasurementUnit
                            )
                         );
                    }
                    catch (Exception ex)
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
    ViewHabits(habits);
}



    // Record handling methods
    void AddRecord()
{
    string date = GetDate("\nEnter the date (format - dd-mm-yy) or insert 0 to Go Back to Main Menu:\n");

    GetHabits();
    int habitId = GetNumber("\nPlease enter the ID of the habit you want to add a record for.");

    int quantity = GetNumber("\nPlease enter number of meters walked (no decimals or negatives allowed) or enter 0 to go back to Main Menu.");

    Console.Clear();
    using (SqliteConnection connection = new(connectionString))
    using (SqliteCommand insertCmd = connection.CreateCommand())
    {
        connection.Open();
        insertCmd.CommandText = $"INSERT INTO records(date, quantity, habitId) VALUES('{date}', {quantity}, {habitId})";
        insertCmd.ExecuteNonQuery();
    }
}

void DeleteRecord()
{
    GetRecords(); // show the records to the user so they can select which one to delete

    int id = GetNumber("\nPlease enter the ID of the record you want to delete.");

    using (SqliteConnection connection = new(connectionString))
    using (SqliteCommand deleteCmd = connection.CreateCommand())
    {
        connection.Open();
        deleteCmd.CommandText = $"DELETE FROM walkingHabit WHERE Id = {id}"; // delete the record with the given id
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

void ViewRecords(List<RecordWithHabit> records) // for visualizing the records in a table
{
    var table = new Table(); // Spectre Console table 
    table.AddColumn("Id"); // table Class from Spectre Console
    table.AddColumn("Date");
    table.AddColumn("Amount");
    table.AddColumn("Habit");

    foreach (var record in records)
    {
        table.AddRow(record.Id.ToString(), record.Date.ToString(), $"{record.Quantity} {record.MeasurementUnit}", record.HabitName.ToString());
    }

    AnsiConsole.Write(table); // write the table to the console
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

    using (SqliteConnection connection = new(connectionString))
    using (SqliteCommand updateCmd = connection.CreateCommand())
    {
        connection.Open();
        updateCmd.CommandText = query; // set the command text to the query
        updateCmd.ExecuteNonQuery();
    }
}

void GetRecords()
{
    List<RecordWithHabit> records = new(); // Representing rows in db

    using (SqliteConnection connection = new(connectionString))
    using (SqliteCommand getCmd = connection.CreateCommand())
    {
        connection.Open();
        getCmd.CommandText = @"
    SELECT records.Id, records.Date, records.Quantity, records.HabitId, habits.Name AS HabitName, habits.MeasurementUnit
    FROM records
    INNER JOIN habits ON records.HabitId = habits.Id";

        using (SqliteDataReader reader = getCmd.ExecuteReader()) //(SqliteDataReader reader = command.ExecuteReader())
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
                        records.Add(
                        new RecordWithHabit(
                            reader.GetInt32(0), // Id
                            DateTime.ParseExact(reader.GetString(1), "dd-MM-yy", CultureInfo.InvariantCulture),
                            reader.GetInt32(2), // Quantity
                            reader.GetString(4), // HabitName
                            reader.GetString(5) // MeasurementUnit
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
    ViewRecords(records);
}

// Extra methods

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


// Record is a new C# 9 feature. It is a reference type that provides built-in functionality for encapsulating data.
// It is immutable(cannot be changed after it is created) by default and provides value-based equality.
record RecordWithHabit(int Id, DateTime Date, int Quantity, string HabitName, string MeasurementUnit);

record Habit(int Id, string Name, string UnitOfMeasurement);