using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Simplifier.SQLite
{
    public class DBColumn
    {
        public enum BasicTypes {
            INTEGER = 0,
            TEXT = 1,
            BLOB = 2,
            REAL = 3,
            NUMERIC = 4 };

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
}
