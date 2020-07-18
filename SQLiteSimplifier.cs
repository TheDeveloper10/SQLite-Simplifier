/*
                 ██████╗ ██████╗ ██╗     ██╗████████╗███████╗
                ██╔════╝██╔═══██╗██║     ██║╚══██╔══╝██╔════╝
                ╚█████╗ ██║██╗██║██║     ██║   ██║   █████╗  
                 ╚═══██╗╚██████╔╝██║     ██║   ██║   ██╔══╝  
                ██████╔╝ ╚═██╔═╝ ███████╗██║   ██║   ███████╗
                ╚═════╝    ╚═╝   ╚══════╝╚═╝   ╚═╝   ╚══════╝

         ██████╗██╗███╗   ███╗██████╗ ██╗     ██╗███████╗██╗███████╗██████╗ 
        ██╔════╝██║████╗ ████║██╔══██╗██║     ██║██╔════╝██║██╔════╝██╔══██╗
        ╚█████╗ ██║██╔████╔██║██████╔╝██║     ██║█████╗  ██║█████╗  ██████╔╝
         ╚═══██╗██║██║╚██╔╝██║██╔═══╝ ██║     ██║██╔══╝  ██║██╔══╝  ██╔══██╗
        ██████╔╝██║██║ ╚═╝ ██║██║     ███████╗██║██║     ██║███████╗██║  ██║
        ╚═════╝ ╚═╝╚═╝     ╚═╝╚═╝     ╚══════╝╚═╝╚═╝     ╚═╝╚══════╝╚═╝  ╚═╝
        ====================================================================
            █▄▄ █▄█ ▀   ▀█▀ █ █ █▀▀   █▀▄ █▀▀ █ █ █▀▀ █   █▀█ █▀█ █▀▀ █▀█
            █▄█  █  ▄    █  █▀█ ██▄   █▄▀ ██▄ ▀▄▀ ██▄ █▄▄ █▄█ █▀▀ ██▄ █▀▄
*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Data.SQLite;
using System.Data;

namespace Simplifier.SQLite
{
    public class DBValue
    {
        public string columnName = "";
        public object value = null;
    }

    public class DBColumn
    {
        public enum BasicTypes
        {
            INTEGER = 0,
            TEXT = 1,
            BLOB = 2,
            REAL = 3,
            NUMERIC = 4
        };

        public DBColumn() { }

        public string name = "";
        public string columnTypeStr = "";
        public BasicTypes columnType = BasicTypes.TEXT;
        public bool notNull = false;
        public bool primaryKey = false;
        public bool autoIncrement = false;
        public bool unique = false;

        /// <summary>
        /// Converts the info for this column to the following string format (example):
        /// Id INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT
        /// </summary>
        public string SQLString()
        {
            StringBuilder strBuilder = new StringBuilder(name);

            strBuilder.Append(" ");
            strBuilder.Append(columnType.ToString());

            if (notNull)
                strBuilder.Append(" NOT NULL");
            if (primaryKey)
                strBuilder.Append(" PRIMARY KEY");
            if (autoIncrement)
                strBuilder.Append(" AUTOINCREMENT");
            if (unique)
                strBuilder.Append(" UNIQUE");

            return strBuilder.ToString();
        }
    }

    /// <summary>
    /// This class simplifies the basic work with SQLite.
    /// If you want to send more specified queries and/or this class doesn't have something you need you can still use ExecuteQuery/ExecuteReader/ExecuteScalar;
    /// </summary>
    public class SQLiteSimplifier
    {
        private SQLiteConnection conn;

        public SQLiteConnection Connection { get { return conn; } }

        public SQLiteSimplifier() { }
        public SQLiteSimplifier(string connectionString) { OpenDatabase(connectionString); }

        /// <summary>
        /// Open a database using a connection string
        /// </summary>
        public void OpenDatabase(string connectionString)
        {
            if (conn != null)
                conn.Close();

            conn = new SQLiteConnection(connectionString);
            conn.Open();
        }

        /// <summary>
        /// Close the opened database
        /// </summary>
        public void CloseDatabase()
        {
            if (conn == null)
                throw new Exception("You cannot close a database that is not opened!");
            conn.Close();
        }
        
        public void Dispose()
        {
            if (conn == null)
                throw new Exception("You cannot dispose a database that is not opened!");
            conn.Dispose();
        }

        /// <summary>
        /// Create a new database
        /// </summary>
        public void CreateDatabase(string fileName)
        {
            SQLiteConnection.CreateFile(fileName);
        }

        /// <summary>
        /// Get the names of all tables
        /// </summary>
        public string[] TableNames()
        {
            if (conn == null)
                throw new Exception("Open a database first!");
            string[] tableNames = null;

            using (DataTable schema = conn.GetSchema("Tables"))
            {
                if (schema != null && schema.Rows.Count > 0)
                {
                    tableNames = new string[schema.Rows.Count];
                    for (int i = 0; i < schema.Rows.Count; ++i)
                        tableNames[i] = schema.Rows[i]["TABLE_NAME"].ToString();
                }
            }

            return tableNames;
        }

        /// <summary>
        /// Get the count of tables
        /// </summary>
        public int TableCount
        {
            get
            {
                if (conn == null)
                    throw new Exception("Open a database first!");

                int count = 0;

                using (DataTable schema = conn.GetSchema("Tables"))
                    count = schema.Rows.Count;

                return count;
            }
        }

        /// <summary>
        /// Get information about columns in a table
        /// </summary>
        public DBColumn[] GetColumns(string tableName)
        {
            if (conn == null)
                throw new Exception("Open a database first!");

            DBColumn[] columns = null;
            int columnsCount = 0;

            using (DataTable schema = conn.GetSchema("COLUMNS"))
            {
                DataRow[] rows = schema.Select("TABLE_NAME = '" + tableName + "'");
                if (rows.Length > 0)
                {
                    columnsCount = rows.Length;
                    columns = new DBColumn[columnsCount];

                    int pkIndx = schema.Columns["PRIMARY_KEY"].Ordinal,
                        aiIndx = schema.Columns["AUTOINCREMENT"].Ordinal,
                        uIndx = schema.Columns["UNIQUE"].Ordinal,
                        inIndx = schema.Columns["IS_NULLABLE"].Ordinal,
                        i = 0;
                    bool autoIncrement = false, unique = false, primaryKey = false, is_nullable = false;

                    object dataType;
                    string dataTypeStr;
                    for (; i < columnsCount; ++i)
                    {
                        columns[i] = new DBColumn()
                        {
                            name = rows[i]["COLUMN_NAME"].ToString()
                        };

                        #region TypeAssigning
                        dataType = rows[i]["DATA_TYPE"];
                        columns[i].columnTypeStr = dataType.ToString();

                        if (dataType == null)
                            columns[i].columnType = DBColumn.BasicTypes.TEXT;
                        else
                        {
                            dataTypeStr = dataType.ToString().ToLower();
                            if (dataTypeStr.Contains("int"))
                                // INTEGER -> sint, integer, tinyint, smallint, mediumint, bigint, unsigned big int, int2, int8, etc.
                                columns[i].columnType = DBColumn.BasicTypes.INTEGER;
                            else if (dataTypeStr.Contains("blob"))
                                // BLOB -> blob
                                columns[i].columnType = DBColumn.BasicTypes.BLOB;
                            else if (dataTypeStr == "real" || dataTypeStr.Contains("double") || dataTypeStr.Contains("float"))
                                // REAL -> real, double, double precision, float
                                columns[i].columnType = DBColumn.BasicTypes.REAL;
                            else if (dataTypeStr == "numeric" || dataTypeStr == "decimal" || dataTypeStr.Contains("bool") || dataTypeStr.Contains("date"))
                                // NUMERIC -> numeric, decimal, boolean, date, datetime
                                columns[i].columnType = DBColumn.BasicTypes.NUMERIC;
                            else
                                // TEXT -> character, varchar, varying character, nchar, native character, nvarchar, text, clob, etc.
                                columns[i].columnType = DBColumn.BasicTypes.TEXT;
                        }
                        #endregion

                        #region PropertiesAssignment
                        if (bool.TryParse(rows[i].ItemArray[aiIndx].ToString(), out autoIncrement))
                            columns[i].autoIncrement = autoIncrement;
                        else
                            columns[i].autoIncrement = false;

                        if (bool.TryParse(rows[i].ItemArray[uIndx].ToString(), out unique))
                            columns[i].unique = unique;
                        else
                            columns[i].unique = false;

                        if (bool.TryParse(rows[i].ItemArray[pkIndx].ToString(), out primaryKey))
                            columns[i].primaryKey = primaryKey;
                        else
                            columns[i].primaryKey = false;

                        if (bool.TryParse(rows[i].ItemArray[inIndx].ToString(), out is_nullable))
                            columns[i].notNull = !is_nullable;
                        else
                            columns[i].notNull = false;
                        #endregion
                    }
                }
            }

            return columns;
        }

        private string GenerateRandomTableName()
        {
            Random r = new Random();
            string name = "";
            string[] tables = TableNames();

            do
            {
                name = tables[r.Next(0, tables.Length - 1)] + (int)(r.NextDouble() * 100000.0f);
            } while (Array.Exists(tables, x => x == name));

            return name;
        }

        private string GenerateFullColumnString(DBColumn[] columns)
        {
            if (columns.Length == 0)
                return "";

            StringBuilder strBuilder = new StringBuilder();

            for (int i = 0; i < columns.Length; ++i)
                strBuilder.Append(columns[i].SQLString() + ", ");
            strBuilder.Remove(strBuilder.Length - 2, 2);

            string str = strBuilder.ToString();
            strBuilder.Clear();
            return str;
        }

        private string GenerateColumnNameString(DBColumn[] columns)
        {
            if (columns.Length == 0)
                return "";

            StringBuilder strBuilder = new StringBuilder();

            for (int i = 0; i < columns.Length; ++i)
                strBuilder.Append(columns[i].name + ", ");
            strBuilder.Remove(strBuilder.Length - 2, 2);

            string str = strBuilder.ToString();
            strBuilder.Clear();
            return str;
        }

        /// <summary>
        /// Add a new column to the table
        /// </summary>
        /// <param name="tableName"> The name of the table </param>
        /// <param name="column"> The information about the column </param>
        public void AddColumn(string tableName, DBColumn column)
        {
            if (conn == null)
                throw new Exception("Open a database first!");

            // Gonna cause problems when a column is not null & doesn't have a default value or is unique
            using (SQLiteCommand cmd = new SQLiteCommand("ALTER TABLE " + tableName + " ADD COLUMN " + column.SQLString(), conn))
                cmd.ExecuteNonQuery();
        }

        /// <summary>
        /// Remove a column from selected table
        /// </summary>
        /// <param name="tableName"> The name of the selected table </param>
        /// <param name="columnName"> The name of the column </param>
        public void RemoveColumn(string tableName, string columnName)
        {
            if (conn == null)
                throw new Exception("Open a database first!");

            DBColumn[] columns = GetColumns(tableName).Where(x => x.name != columnName).ToArray();

            string newTableName = GenerateRandomTableName(), columnNames = GenerateColumnNameString(columns);

            /*
                BEGIN TRANSACTION;
                CREATE TABLE demo_backup(ID, Name);
                INSERT INTO demo_backup SELECT ID, Name FROM demo;
                DROP TABLE demo;
                ALTER TABLE demo_backup RENAME TO demo;
                COMMIT;
            */
            using (SQLiteCommand cmd = new SQLiteCommand(
                "BEGIN TRANSACTION; " +
                "CREATE TABLE " + newTableName + "(" + columnNames + "); " +
                "INSERT INTO " + newTableName + " SELECT " + columnNames + " FROM " + tableName + "; " +
                "DROP TABLE " + tableName + "; " +
                "ALTER TABLE " + newTableName + " RENAME TO " + tableName + "; " +
                "COMMIT; "
                , conn))
                cmd.ExecuteNonQuery();
        }

        /// <summary>
        /// Create a new table
        /// </summary>
        public void CreateTable(string tableName, params DBColumn[] columns)
        {
            if (conn == null)
                throw new Exception("Open a database first!");

            using (SQLiteCommand cmd = new SQLiteCommand("CREATE TABLE IF NOT EXISTS " + tableName + "(" + GenerateFullColumnString(columns) + ")", conn))
                cmd.ExecuteNonQuery();
        }

        /// <summary>
        /// Delete a table
        /// </summary>
        public void DeleteTable(string tableName)
        {
            if (conn == null)
                throw new Exception("Open a database first!");

            using (SQLiteCommand cmd = new SQLiteCommand("DROP TABLE " + tableName))
                cmd.ExecuteNonQuery();
        }

        /// <summary>
        /// Empty a table
        /// </summary>
        public void TruncateTable(string tableName)
        {
            if (conn == null)
                throw new Exception("Open a database first!");

            using (SQLiteCommand cmd = new SQLiteCommand("TRUNCATE TABLE " + tableName))
                cmd.ExecuteNonQuery();
        }

        /// <summary>
        /// Get count of rows in a table
        /// </summary>
        public int GetRowCount(string tableName)
        {
            if (conn == null)
                throw new Exception("Open a database first!");

            int rowCount = 0;
            using (SQLiteCommand cmd = new SQLiteCommand("SELECT COUNT(*) FROM " + tableName, conn))
                rowCount = Convert.ToInt32(cmd.ExecuteScalar());

            return rowCount;
        }

        /// <summary>
        /// Get count of columns in a table
        /// </summary>
        public int GetColumnCount(string tableName)
        {
            if (conn == null)
                throw new Exception("Open a database first!");

            int columnCount = 0;
            using (DataTable schema = conn.GetSchema("COLUMNS"))
                columnCount = schema.Select("TABLE_NAME = '" + tableName + "'").Length;

            return columnCount;
        }

        /// <summary>
        /// Get row from index in a table
        /// </summary>
        public DataRow GetRow(string tableName, int rowIndex)
        {
            if (conn == null)
                throw new Exception("Open a database first!");

            DataRow dr = null;

            using (SQLiteDataAdapter adapter = new SQLiteDataAdapter("SELECT * FROM " + tableName + " LIMIT 1 OFFSET " + rowIndex, conn))
            {
                using (DataTable dt = new DataTable())
                {
                    adapter.Fill(dt);
                    if (dt.Rows.Count > 0)
                        dr = dt.Rows[0];
                }
            }

            return dr;
        }

        /// <summary>
        /// Add row in a table
        /// </summary>
        public void AddRow(string tableName, params DBValue[] values)
        {
            if (conn == null)
                throw new Exception("Open a database first!");

            if (values.Length == 0)
            {
                using (SQLiteCommand cmd = new SQLiteCommand("INSERT INTO " + tableName + " DEFAULT VALUES"))
                    cmd.ExecuteNonQuery();
                return;
            }

            StringBuilder str1 = new StringBuilder(), str2 = new StringBuilder();

            for (int i = 0; i < values.Length; ++i)
            {
                str1.Append(",");
                str1.Append(values[i].columnName);

                str2.Append(",@");
                str2.Append(values[i].columnName);
            }

            str1.Remove(0, 1);
            str2.Remove(0, 1);

            using (SQLiteCommand cmd = new SQLiteCommand("INSERT INTO " + tableName + "(" + str1.ToString() + ") VALUES(" + str2.ToString() + ")", conn))
            {
                str1.Clear();
                str2.Clear();
                for (int i = 0; i < values.Length; ++i)
                    cmd.Parameters.Add(new SQLiteParameter("@" + values[i].columnName, values[i].value));
                cmd.ExecuteNonQuery();
            }
        }

        /// <summary>
        /// Replace all previous values with new ones
        /// </summary>
        public void UpdateValue(string tableName, string columnName, object previousValue, object newValue)
        {
            if (conn == null)
                throw new Exception("Open a database first!");

            using (SQLiteCommand cmd = new SQLiteCommand("UPDATE " + tableName + " SET " + columnName + "=@newValue WHERE " + columnName + "=@prevValue", conn))
            {
                cmd.Parameters.Add(new SQLiteParameter("@prevValue", previousValue));
                cmd.Parameters.Add(new SQLiteParameter("@newValue", newValue));
                cmd.ExecuteNonQuery();
            }
        }

        /// <summary>
        /// Replace all values in a column with a new value
        /// </summary>
        public void UpdateValue(string tableName, string columnName, object newValue)
        {
            if (conn == null)
                throw new Exception("Open a database first!");

            using (SQLiteCommand cmd = new SQLiteCommand("UPDATE " + tableName + " SET " + columnName + "=@newValue", conn))
            {
                cmd.Parameters.Add(new SQLiteParameter("@newValue", newValue));
                cmd.ExecuteNonQuery();
            }
        }

        public enum OrderBy { Ascending, Descending };

        /// <summary>
        /// Get table information and its values
        /// </summary>
        public DataTable GetTable(string tableName, OrderBy orderBy, params string[] columns)
        {
            if (conn == null)
                throw new Exception("Open a database first!");

            DataTable dt = new DataTable();

            StringBuilder columnString = new StringBuilder();

            for (int i = 0; i < columns.Length; ++i)
                columnString.Append("," + columns[i]);
            columnString.Remove(0, 1);

            using (SQLiteDataAdapter adapter = new SQLiteDataAdapter("SELECT * FROM " + tableName + " ORDER BY " + columnString.ToString() + " " + (orderBy == OrderBy.Ascending ? "ASC" : "DESC"), conn))
                adapter.Fill(dt);

            columnString.Clear();

            return dt;
        }

        /// <summary>
        /// Get table information and its values
        /// </summary>
        public DataTable GetTable(string tableName)
        {
            if (conn == null)
                throw new Exception("Open a database first!");

            DataTable dt = new DataTable();

            using (SQLiteDataAdapter adapter = new SQLiteDataAdapter("SELECT * FROM " + tableName, conn))
                adapter.Fill(dt);

            return dt;
        }

        /// <summary>
        /// Does a given table exist?
        /// </summary>
        public bool TableExists(string tableName)
        {
            int c = 0;

            using (SQLiteCommand cmd = new SQLiteCommand("SELECT COUNT(type) FROM sqlite_master WHERE name='" + tableName + "'", conn))
                c = Convert.ToInt32(cmd.ExecuteScalar());

            return c != 0;
        }

        /// <summary>
        /// Get a shema(COLUMNS, TABLES, etc.)
        /// </summary>
        public DataTable GetSchema(string schema)
        {
            if (conn == null)
                throw new Exception("Open a database first!");

            return conn.GetSchema(schema);
        }

        /// <summary>
        /// A method equivalent to connection.ExecuteNonQuery(commandText);
        /// </summary>
        public int ExecuteNonQuery(string commandText)
        {
            if (conn == null)
                throw new Exception("Open a database first!");

            int x;
            using (SQLiteCommand cmd = new SQLiteCommand(commandText, conn))
                x = cmd.ExecuteNonQuery();
            return x;
        }

        /// <summary>
        /// A method equivalent to connection.ExecuteScalar(commandText);
        /// </summary>
        public object ExecuteScalar(string commandText)
        {
            if (conn == null)
                throw new Exception("Open a database first!");

            object obj;
            using (SQLiteCommand cmd = new SQLiteCommand(commandText, conn))
                obj = cmd.ExecuteScalar();
            return obj;
        }

        /// <summary>
        /// A method equivalent to connection.ExecuteReader(commandText);
        /// </summary>
        public SQLiteDataReader ExecuteReader(string commandText)
        {
            if (conn == null)
                throw new Exception("Open a database first!");

            SQLiteDataReader reader;
            using (SQLiteCommand cmd = new SQLiteCommand(commandText, conn))
                reader = cmd.ExecuteReader();
            return reader;
        }
    }
}
