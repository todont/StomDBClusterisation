using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;
using System.Data.Common;


namespace ClusterSotmDB
{
    class Program
    {
        static void Main(string[] args)
        {
            MySqlConnection conn = DBUtils.GetDBConnection();
            conn.Open();
            try
            {
                QueryStomdb(conn);
            }
            catch (Exception e)
            {
                Console.WriteLine("Error: " + e);
                Console.WriteLine(e.StackTrace);
            }
            finally
            {
                conn.Close();
                conn.Dispose();
            }
            Console.Read();
        }
        private static void QueryStomdb(MySqlConnection conn)
        {
            string sql = "Select ID_SERVICE, ID_CASE,ID_PROFILE from case_services WHERE ID_DOCTOR=207 ORDER BY ID_CASE ";
            MySqlCommand cmd = new MySqlCommand();
            cmd.Connection = conn;
            cmd.CommandText = sql;
            long prevCaseId = 0;
            List<TaskOrder> cases207 = new List<TaskOrder>();
            using (DbDataReader reader = cmd.ExecuteReader())
            {
                if (reader.HasRows)
                {
                    int i = 0;
                    while (reader.Read())
                    {
                        long currCaseId = reader.GetInt64(1);
                        if (currCaseId!=prevCaseId || prevCaseId==0)
                        {
                            TaskOrder t = new TaskOrder (currCaseId, reader.GetInt64(2));
                            cases207.Add(t);
                        }
                        else
                        {
                            cases207[cases207.Count - 1].AddServiceById(reader.GetInt64(2));
                        }
                        prevCaseId = currCaseId;
                    }
                }
            }
        
        }
        private static double GlobalProfit(List<Cluster> clusters)
        {

        }
    }
}
