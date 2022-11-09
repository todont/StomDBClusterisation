using ClusterSotmDB;
using MySql.Data.MySqlClient;
using System.Diagnostics;
using System;



namespace ClusterStomDB
{
    public static class Program
    {
        public static void Main()
        {
            Stopwatch sw = new Stopwatch();
            MySqlConnection conn = DBUtils.GetDBConnection(UtilConst.DataBaseName);
            sw.Start();
            Clinic stom = Clinic.GetInstance(conn);

            stom.Clusterise();
            sw.Stop();
            Console.WriteLine($"Time:{sw.Elapsed}");
            Console.Read();
        }
    }
}
