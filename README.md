


# SQLite-Simplifier
Simplifies the work with SQLite in C#

### REQUIREMENTS
```
System.Data.SQLite <- nuget package
```
### WORKFLOW
There are many more things you can do with this Simplifier than in this example
```csharp
SQLiteSimplifier simplifier = new SQLiteSimplifier();

// Creating a database
simplifier.CreateDatabase("MyDb.db");

// Opening a database
simplifier.OpenDatabase("Data Source=MyDb.db;Version=3;");
            
// Creating a table
simplifier.CreateTable("MyTable",
                new DBColumn() { name = "Id", columnType = DBColumn.BasicTypes.INTEGER, columnTypeStr = "INT", notNull = true, autoIncrement = true, primaryKey = true, unique = true },
                new DBColumn() { name = "Name", columnType = DBColumn.BasicTypes.TEXT, columnTypeStr = "TEXT", notNull = true, unique = true });
            
// Adding a column in a table
// Note: You should not add a Not NULL or Unique column because the 
// default value of a column is a null
simplifier.AddColumn("MyTable", new DBColumn() { name = "Age", columnType = DBColumn.BasicTypes.INTEGER, columnTypeStr = "INT" });
            
// Removing a column from a table
simplifier.RemoveColumn("MyTable", "Name");
            
// Adding new rows to a table
for (int i = 0; i < 100; ++i)
    simplifier.AddRow("MyTable", new DBValue() { columnName = "Age", value = i });
            
// Updating the value 77 with 20 in a table
simplifier.UpdateValue("MyTable", "Age", 77, 20);
            
// Get data in a row
DataRow dr = simplifier.GetRow("MyTable", rowIndex: 0);
            
// Get data from a table
DataTable dt = simplifier.GetTable("MyTable");
            
// Get information about a specific table
int rowCount = simplifier.GetRowCount("MyTable");
int columnCount = simplifier.GetColumnCount("MyTable");
            
// Get names of all tables
string[] tables = simplifier.TableNames();

// An example for a specific query that doesn't exist in the simplifier:
using (SQLiteDataAdapter adapter = new SQLiteDataAdapter("SELECT Id FROM MyTable WHERE Age=22", simplifier.Connection))
    using (DataTable result = new DataTable())
        adapter.Fill(result);
            
// Closing the database
simplifier.CloseDatabase();
```

### LICENSE: Apache License 2.0
#
#
▁▂▅▆▇ 📲 Social Media and Contacts 📲 ▇▆▅▂▁ <br>
➡ WEBSITE - https://thedevelopers.tech <br>
📌YOUTUBE - https://www.youtube.com/channel/UCwO0k5dccZrTW6-GmJsiFrg <br>
📘FACEBOOK - https://www.facebook.com/VicTor-372230180173180 <br>
📒INSTAGRAM - https://www.instagram.com/thedeveloper10/ <br>
💎TWITTER - https://twitter.com/the_developer10 <br>
✶LINKEDIN - https://www.linkedin.com/company/65346254
