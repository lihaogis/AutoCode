using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Core
{
    public class DBFactory
    {
        public static DBModel createDBModel(string dbType,string connectString)
        {
            DBModel dbModel = null;
            switch (dbType)
            {
                case "PostgreSQL":
                    dbModel = new PostgreSQL(connectString);
                    break;
                case "MySQL":
                    dbModel=new MySQL();
                    break;
            }
            return dbModel;
        }
    }
}
