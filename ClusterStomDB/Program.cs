using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Threading.Tasks;


namespace ClusterStomDB
{
    class Program
    {
        static void Main(string[] args)
        {

            MySqlConnection conn = DBUtils.GetDBConnection("stomadb");
            conn.Open();
            try
            {
                string sql = "SELECT distinct ID_Doctor FROM stomadb.case_services WHERE ID_DOCTOR>156 and ID_DOCTOR<158";
                MySqlCommand cmd = new MySqlCommand();
                cmd.Connection = conn;
                cmd.CommandText = sql;
                List<int> ids = new List<int>();
                using (DbDataReader reader = cmd.ExecuteReader())
                {
                    if (reader.HasRows)
                    {
                        while (reader.Read())
                            ids.Add(reader.GetInt32(0));
                    }
                }

                List<Task> taskList = new List<Task>();
                List<Doctor> doctors = new List<Doctor>();
                foreach (int i in ids)
                {
                    Doctor tmp = new Doctor(i, conn);//доктор иницилизировался
                    Task task = Task.Factory.StartNew(() => tmp.MakeTemplates());
                    doctors.Add(tmp);
                    taskList.Add(task);
                }
                Task.WaitAll(taskList.ToArray());
              
                //foreach (KeyValuePair<int, string> entry in price)
                //{
                //    Console.WriteLine(" ID={0} {1}", entry.Key, entry.Value);
                //}
                foreach(Doctor d in doctors)
                {
                    d.PrintDoctorTemp();
                }

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
        

    }
}
