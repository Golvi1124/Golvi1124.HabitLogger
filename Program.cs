/*

Seperate methods in different files

 */

using System.Globalization;
using Microsoft.Data.Sqlite;
using Spectre.Console;
using System.Linq;

string connectionString = @"Data Source = habit-logger.db"; // path to the database file. If it doesn't exist, it will be created in the same directory as the executable

CreateDatabase();

MainMenu();

void MainMenu()
{
    var isMenuRunning = true;

    while (isMenuRunning)
    {
        var usersChoice = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title("\nWhat do you want to do?")
                .AddChoices(
                    "Habit Options",
                    "Record Options",
                    "Specific Search",
                    "Wipe All Data",
                    "Add Random Data",
                    "Quit"
                    )
        );

        switch (usersChoice)
        {
            case "Habit Options":
                HabitMenu();
                break;
            case "Record Options":
                RecordMenu();
                break;
            case "Specific Search":
                SearchMenu();
                break;
            case "Wipe All Data":
                if (AnsiConsole.Confirm("Are you sure you want to delete ALL data?"))
                    WipeData();
                break;
            case "Add Random Data":
                AddRandomData();
                break;
            case "Quit":
                Console.WriteLine("Goodbye!");
                isMenuRunning = false;
                break;
        }
    }
}

void SearchMenu()
{
    var isSearchMenuRunning = true;

    while (isSearchMenuRunning)
    {
        var searchChoice = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title("\nChoose a Search operation:")
                .AddChoices(
                    "Chart of All Habits",
                    "Top 3 Habits",
                    "Average Habit Quantity",
                    "See Entries for Specific Habit",
                    "See Entries for Specific Month",
                    "Back")
        );
        switch (searchChoice)
        {
            case "Chart of All Habits":
                ShowChart();
                break;
            case "Top 3 Habits":
                ShowTopHabits();
                break;
            case "Average Habit Quantity":
                ShowAverage();
                break;
            case "See Entries for Specific Habit":
                ShowSpecificHabit();
                break;
            case "See Entries for Specific Month": // menu where start with year
                // ShowSpecificMonth();
                break;
            case "Back":
                isSearchMenuRunning = false;
                break;

        }
    }
}

void ShowSpecificHabit()
{
    var isSpecificHabitMenuRunning = true;

    while (isSpecificHabitMenuRunning)
    {

        // GetHabits() returns void, so we need to modify it to return a list of habits.
        // Assuming GetHabits() is updated to return a List<Habit>, we can use it here.

        List<Habit> habits = GetHabits(); // Fetch the list of habits from the database.

        if (habits.Count == 0)
        {
            Console.WriteLine("No habits found. Returning to the previous menu.");
            isSpecificHabitMenuRunning = false;
            return;
        }

        var habitChoice = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title("Choose a Habit:")
                .AddChoices(habits.Select(h => h.Name).Concat(new[] { "Back" })) // Adds "Back" option
);
        if (habitChoice == "Back")
        {
            isSpecificHabitMenuRunning = false;
            return;
        }

        // Additional logic for handling the selected habit can be added here.
        List<(string Date, int Quantity, string MeasurementUnit)> specificHabitData = new();

        using (SqliteConnection connection = new(connectionString))
        using (SqliteCommand command = connection.CreateCommand())
        {
            connection.Open();
            // Query for specific habit data
            command.CommandText = @"
                SELECT records.Date, records.Quantity, habits.MeasurementUnit
                FROM records
                INNER JOIN habits ON records.HabitId = habits.Id
                WHERE habits.Name = @HabitName ";

            command.Parameters.AddWithValue("@HabitName", habitChoice); // Use the selected habit name
            using (SqliteDataReader reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    specificHabitData.Add((reader.GetString(0), reader.GetInt32(1), reader.GetString(2)));
                }
            }
        }

        Console.Clear();
        if (specificHabitData.Count > 0)
        {
       

            var specificHabitTable = new Table();
            specificHabitTable.AddColumn("Date");
            specificHabitTable.AddColumn("Quantity");
            specificHabitTable.AddColumn("Measurement Unit");

            foreach (var data in specificHabitData)
            {
                specificHabitTable.AddRow(data.Date, data.Quantity.ToString(), data.MeasurementUnit);
            }
            AnsiConsole.Write(new Markup($"[bold yellow]Entries for {habitChoice}[/]\n"));
            AnsiConsole.Write(specificHabitTable);
        }
        else
        {
            Console.WriteLine($"No entries found for the habit: {habitChoice}.");
        }
        // name of list = All you wanna know about {chosen habit}
    }
}



void ShowChart()
{
    List<(string HabitName, int LogCount)> habitChartData = new();

    using (SqliteConnection connection = new(connectionString))
    using (SqliteCommand command = connection.CreateCommand())
    {
        connection.Open();

        // Query for total quantity
        command.CommandText = @"
            SELECT habits.Name, COUNT(records.Id) AS LogCount
            FROM records
            INNER JOIN habits ON records.HabitId = habits.Id
            GROUP BY habits.Id";

        using (SqliteDataReader reader = command.ExecuteReader())
        {
            while (reader.Read())
            {
                habitChartData.Add((reader.GetString(0), reader.GetInt32(1)));
            }
        }
    }

    Console.Clear();

    if (habitChartData.Count > 0)
    {
        AnsiConsole.Write(new BarChart()
            .Width(60)
            .Label("[bold yellow]Habit Chart by times done[/]\n")
            .AddItems(habitChartData, (habit) => new BarChartItem(
                habit.HabitName, habit.LogCount, Color.Green)));

    }
    else
    {
        Console.WriteLine("No data available to display chart.");
    }
}

void ShowAverage()
{
    List<(string HabitName, int MinQuantity, double AverageQuantity, int MaxQuantity, string Measurement)> averageQuantities = new();

    using (SqliteConnection connection = new(connectionString))
    using (SqliteCommand command = connection.CreateCommand())
    {
        connection.Open();

        // Query for average quantity
        command.CommandText = @"
            SELECT habits.Name, AVG(records.Quantity) AS AverageQuantity, Min(records.Quantity) AS MinQuantity, Max(records.Quantity) AS MaxQuantity, habits.MeasurementUnit
            FROM records
            INNER JOIN habits ON records.HabitId = habits.Id
            GROUP BY habits.Id";

        using (SqliteDataReader reader = command.ExecuteReader())
        {
            while (reader.Read())
            {
                // Corrected column order
                averageQuantities.Add((reader.GetString(0), reader.GetInt32(2), reader.GetDouble(1), reader.GetInt32(3), reader.GetString(4)));
            }
        }
    }

    Console.Clear();
    // Display average quantity
    if (averageQuantities.Count > 0)
    {
        var averageQuantityTable = new Table();
        averageQuantityTable.AddColumn("Habit");
        averageQuantityTable.AddColumn("Min Quantity");
        averageQuantityTable.AddColumn("Average Quantity");
        averageQuantityTable.AddColumn("Max Quantity");
        averageQuantityTable.AddColumn("Measurement Unit");

        for (int i = 0; i < averageQuantities.Count; i++)
        {
            averageQuantityTable.AddRow(
                averageQuantities[i].HabitName,
                averageQuantities[i].MinQuantity.ToString(),
                averageQuantities[i].AverageQuantity.ToString("F1"), // Format average to 1 decimal place
                averageQuantities[i].MaxQuantity.ToString(),
                averageQuantities[i].Measurement.ToString() // Measurement unit
            );
        }

        AnsiConsole.Write(new Markup("[bold yellow]Average Habit Quantity[/]\n"));
        AnsiConsole.Write(averageQuantityTable);
    }
    else
    {
        Console.WriteLine("No data available to display average quantities.");
    }
}


void HabitMenu()
    {
        var isHabitMenuRunning = true;

        while (isHabitMenuRunning)
        {
            var habitChoice = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("Choose a Habit operation:")
                    .AddChoices(
                        "View Habits",
                        "Add Habit",
                        "Update Habit",
                        "Delete Habit",
                        "Back")
            );

            switch (habitChoice)
            {
                case "View Habits":
                    GetHabits();
                    break;
                case "Add Habit":
                    AddHabit();
                    break;
                case "Update Habit":
                    UpdateHabit();
                    break;
                case "Delete Habit":
                    DeleteHabit();
                    break;
                case "Back":
                    isHabitMenuRunning = false;
                    break;
            }
        }
    }

    void RecordMenu()
    {
        var isRecordMenuRunning = true;

        while (isRecordMenuRunning)
        {
            var recordChoice = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("Choose a Record operation:")
                    .AddChoices(
                    "View Records",
                    "Add Record",
                    "Update Record",
                    "Delete Record",
                    "Back")
            );

            switch (recordChoice)
            {
                case "View Records":
                    GetRecords();
                    break;
                case "Add Record":
                    AddRecord();
                    break;
                case "Update Record":
                    UpdateRecord();
                    break;
                case "Delete Record":
                    DeleteRecord();
                    break;
                case "Back":
                    isRecordMenuRunning = false;
                    break;
            }
        }
    }


    void CreateDatabase()
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

    void ShowTopHabits()
    {
        List<(string HabitName, int TotalQuantity)> topByQuantity = new();
        List<(string HabitName, int LogCount)> topByCount = new();

        using (SqliteConnection connection = new(connectionString))
        using (SqliteCommand command = connection.CreateCommand())
        {
            connection.Open();

            // Query for top 3 habits by total quantity
            command.CommandText = @"
            SELECT habits.Name, SUM(records.Quantity) AS TotalQuantity
            FROM records
            INNER JOIN habits ON records.HabitId = habits.Id
            GROUP BY habits.Id
            ORDER BY TotalQuantity DESC
            LIMIT 3";

            using (SqliteDataReader reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    topByQuantity.Add((reader.GetString(0), reader.GetInt32(1)));
                }
            }

            // Query for top 3 habits by log count
            command.CommandText = @"
            SELECT habits.Name, COUNT(records.Id) AS LogCount
            FROM records
            INNER JOIN habits ON records.HabitId = habits.Id
            GROUP BY habits.Id
            ORDER BY LogCount DESC
            LIMIT 3";

            using (SqliteDataReader reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    topByCount.Add((reader.GetString(0), reader.GetInt32(1)));
                }
            }
        }
        Console.Clear();
        // Display top 3 by total quantity
        if (topByQuantity.Count > 0)
        {
            var quantityTable = new Table();
            quantityTable.AddColumn("Rank");
            quantityTable.AddColumn("Habit");
            quantityTable.AddColumn("Total Quantity");

            for (int i = 0; i < topByQuantity.Count; i++)
            {
                quantityTable.AddRow((i + 1).ToString(), topByQuantity[i].HabitName, topByQuantity[i].TotalQuantity.ToString());
            }

            AnsiConsole.Write(new Markup("[bold yellow]Top 3 Habits by Total Quantity[/]\n"));
            AnsiConsole.Write(quantityTable);
        }
        else
        {
            Console.WriteLine("No data available to display top habits by quantity.");
        }

        // Display top 3 by log count
        if (topByCount.Count > 0)
        {
            var countTable = new Table();
            countTable.AddColumn("Rank");
            countTable.AddColumn("Habit");
            countTable.AddColumn("Log Count");

            for (int i = 0; i < topByCount.Count; i++)
            {
                countTable.AddRow((i + 1).ToString(), topByCount[i].HabitName, topByCount[i].LogCount.ToString());
            }

            AnsiConsole.Write(new Markup("\n[bold yellow]Top 3 Habits by Log Count[/]\n"));
            AnsiConsole.Write(countTable);
        }
        else
        {
            Console.WriteLine("No data available to display top habits by log count.");
        }
    }





    void AddRandomData()
    {
        if (!IsTableEmpty("habits") || !IsTableEmpty("records")) // check if the tables are empty before seeding data
        {
            var wipeData = AnsiConsole.Confirm("Tables should be emptied first. Do you want to wipe all data?");
            if (wipeData)
                WipeData();
            else
                return;
        }

        // Ensuring that habits are seeded before records
        if (IsTableEmpty("habits")) SeedHabits();

        int numberOfRecords = AnsiConsole.Ask<int>("How many random records do you want to add?");

        List<string> dates = GenerateRandomDates(numberOfRecords);
        List<int> quantities = GenerateRandomQuantities(numberOfRecords, 0, 2000);

        using (SqliteConnection connection = new(connectionString))
        {
            connection.Open();

            for (int i = 0; i < numberOfRecords; i++)
            {
                var insertSql = $"INSERT INTO records (Date, Quantity, HabitId) VALUES ('{dates[i]}', {quantities[i]}, {GetRandomHabitId()});";
                var command = new SqliteCommand(insertSql, connection);
                command.ExecuteNonQuery();
            }
        }
    }

    void WipeData()
    {
        using (SqliteConnection connection = new(connectionString))
        using (SqliteCommand wipeCmd = connection.CreateCommand())
        {
            connection.Open();
            wipeCmd.CommandText = "DELETE FROM records; DELETE FROM habits; DELETE FROM sqlite_sequence WHERE name = 'records' OR name = 'habits';";
            wipeCmd.ExecuteNonQuery();
        }
        Console.WriteLine("All data wiped and IDs reset.");
    }


    // Seed data

    //The purpose of this method is to check if any table is empty, since we only want to seed data if both tables are empty.
    bool IsTableEmpty(string tableName)
    {
        using (SqliteConnection connection = new(connectionString))
        using (SqliteCommand command = connection.CreateCommand())
        {
            connection.Open();
            command.CommandText = $"SELECT COUNT(*) FROM {tableName}";
            long count = (long)command.ExecuteScalar();
            return count == 0; //This means it will be true if no rows are found and false otherwise.
        }
    }

    void SeedHabits()
    {
        string[] habitNames = { "Reading", "Running", "Chocolate", "Drinking Water", "Glasses of Wine" };
        string[] habitUnits = { "Pages", "Meters", "Grams", "Mililiters", "Mililiters" };

        using (SqliteConnection connection = new(connectionString)) //isn't it missing a using statement for command?
        {
            connection.Open();

            for (int i = 0; i < habitNames.Length; i++)
            {
                var insertSql = $"INSERT INTO habits (Name, MeasurementUnit) VALUES ('{habitNames[i]}', '{habitUnits[i]}');";
                var command = new SqliteCommand(insertSql, connection);

                command.ExecuteNonQuery();
            }
        }
    }

    List<int> GenerateRandomQuantities(int count, int min, int max)
    {
        Random random = new();
        List<int> quantities = new();

        for (int i = 0; i < count; i++)
        {
            // max + 1 because the top range is excluded 
            quantities.Add(random.Next(min, max + 1));
        }
        return quantities;
    }

    List<string> GenerateRandomDates(int count)
    {
        DateTime startDate = new DateTime(2023, 1, 1);
        DateTime endDate = DateTime.Now; // current date...check if this works
        TimeSpan timeSpan = endDate - startDate;

        List<string> randomDateStrings = new(count);
        Random random = new();

        for (int i = 0; i < count; i++)
        {
            int daysToAdd = random.Next(0, (int)timeSpan.TotalDays);
            DateTime randomDate = startDate.AddDays(daysToAdd);
            randomDateStrings.Add(randomDate.ToString("dd-MM-yy")); // format the date to dd-mm-yy
        }
        return randomDateStrings;
    }

    int GetRandomHabitId()
    {
        using (SqliteConnection connection = new(connectionString))
        using (SqliteCommand command = connection.CreateCommand())
        {
            connection.Open();
            command.CommandText = "SELECT Id FROM habits";

            var reader = command.ExecuteReader();
            List<int> ids = new();
            while (reader.Read())
            {
                ids.Add(reader.GetInt32(0));
            }

            if (ids.Count == 0)
            {
                throw new Exception("No habits found in the database.");
            }

            Random random = new();
            return ids[random.Next(ids.Count)];
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

// Update the GetHabits method to return a List<Habit> instead of void.
List<Habit> GetHabits()
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

    // Return the list of habits.
    return habits;
}



    // Record handling methods
    void AddRecord()
    {
        string date = GetDate("\nEnter the date (format - dd-mm-yy) or insert 0 to Go Back to Main Menu:\n");

        GetHabits();
        int habitId = GetNumber("\nPlease enter the ID of the habit you want to add a record for.");

        int quantity = GetNumber("\nPlease enter number of habit's amount (no decimals or negatives allowed) or enter 0 to go back to Main Menu.");

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

    void ViewRecords(List<RecordWithHabit> records) // for visualizing the records in a table
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

        bool updateQuantity = AnsiConsole.Confirm("Do you want to update the quantity?");
        int quantity = 0;
        if (updateQuantity)
        {
            quantity = GetNumber("\nPlease enter the new quantity (no decimals or negatives allowed) or enter 0 to go back to Main Menu.");
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

