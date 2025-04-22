// installing Spectre Console NuGet Package to easier to create beautiful, cross platform, console applications
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
            //    AddRecord();
                break;
            case "Delete Record":
            //    DeleteRecord();
                break;
            case "View Records":
            //    ViewRecords();
                break;
            case "Update Record":
             //   UpdateRecord();
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