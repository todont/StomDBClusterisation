﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;

namespace ClusterStomDB
{
    class DBUtils
    {
        public static MySqlConnection GetDBConnection(string database)
        {
            string host = "localhost";
            int port = 3306;
            
            string username = "root";
            string password = "1911";


            return DBMySQLUtils.GetDBConnection(host, port, database, username, password);
        }
    }
}