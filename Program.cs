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
using Golvi1124.HabitLogger.Methods;
using Spectre.Console;

class Program
{
    static void Main()
    {
        DBControls.CreateDatabase();
        MainMenu();
    }

    public static void MainMenu()
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
                    RecordControls.AddRecord();
                    break;
                case "Delete Record":
                    RecordControls.DeleteRecord();
                    break;
                case "View Records":
                    RecordControls.GetRecords();
                    break;
                case "Update Record":
                    RecordControls.UpdateRecord();
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
}

