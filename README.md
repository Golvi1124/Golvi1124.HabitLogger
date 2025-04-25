# Habit Logger (WIP)

## Tasks for the project:  
Using guidelines from The C# Academy course - [Link to page](https://thecsharpacademy.com/project/12/habit-logger)

### Requirements:
- [ ] Create a console application where you’ll log occurrences of a habit.
- [x] This habit can't be tracked by time (ex. hours of sleep), only by quantity (ex. number of water glasses a day)
- [x] Users need to be able to input the date of the occurrence of the habit
- [x] The application should store and retrieve data from a real database
- [x] When the application starts, it should create a sqlite database, if one isn’t present.
- [x] It should also create a table in the database, where the habit will be logged.
- [x] The users should be able to insert, delete, update and view their logged habit.
- [ ] You should handle all possible errors so that the application never crashes.
- [x] You can only interact with the database using ADO.NET. You can’t use mappers such as Entity Framework or Dapper.
- [ ] Follow the DRY Principle, and avoid code repetition.
- [ ] Your project needs to contain a Read Me file where you'll explain how your app works. Here's a nice example: https://github.com/thags/ConsoleTimeLogger

### Challanges:
- [ ] If you haven't, try using parameterized queries to make your application more secure. https://reintech.io/blog/mastering-parameterized-queries-ado-net
- [ ] Let the users create their own habits to track. That will require that you let them choose the unit of measurement of each habit.
- [ ] Seed Data into the database automatically when the database gets created for the first time,   
generating a few habits and inserting a  hundred records with randomly generated values.  
This is specially helpful during development so you don't have to reinsert data every time you create the database.
- [ ] Create a report functionality where the users can view specific information (i.e. how many times the user ran in a year? how many kms?) SQL allows you to ask very interesting things from your database.


  
ADDED   "View Habits", 





## General To Do:
- [ ] Handle all possible errors so that the application never crashes.
- [ ] Follow the DRY Principle, and avoid code repetition - maybe can improve something in the end?
- [ ] You can keep all of the code in one single class if you wish. - seperate methods in different files
- [ ] Use the same structure and/or improve readability.


## Extra challanges:


- [ ] Add a feature that allows the user to set a goal for each habit. For example, if you want to run 5 times a week, you can set a goal of 5. The app will then track how many times you have run and how many times you have reached your goal.

---
### Improvements:
- Installed **Spectre Console NuGet Package** to make it easier to create beautiful, cross platform, console applications.
- Added option to delete all data from the database



### Small extra improvements:
- Using file-scoped namespaces to reduce nesting.
- Use same structure and/or improve readability.   
	- 'using' declaration expression (C# 9+) for cleaner disposal  
	>Before:   `using (var connection = new SqliteConnection(connectionString)))`  
	After:    `using (SqliteConnection connection = new(connectionString))`  
	- Make command more descriptive (changed depending on method)  
	>Before:   `command.ExecuteNonQuery();`  = generic, but can be confusing if you have multiple commands  
	After:    `deleteCmd.ExecuteNonQuery();` = more descriptive (better readability)  
- Changed arrays to lists where I wasn't sure about future sizes + for more flexibility.


