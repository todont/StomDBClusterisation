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
                string sql = "SELECT distinct ID_Doctor FROM stomadb.case_services WHERE ID_DOCTOR>0 and ID_DOCTOR<500";
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
                sql = "SELECT id,code FROM stomadb.price;";
                cmd = new MySqlCommand();
                Dictionary<int, string> bzp = new Dictionary<int, string>();
                Dictionary<int, string> pzp = new Dictionary<int, string>();
                using (DbDataReader reader = cmd.ExecuteReader())
                {
                    if (reader.HasRows)
                    {
                        while (reader.Read())
                            pzp.Add(reader.GetInt32(0), reader.GetString(1));
                    }
                }
                sql = "SELECT id,code FROM stomadb.price_orto_soc;";
                cmd = new MySqlCommand();
                using (DbDataReader reader = cmd.ExecuteReader())
                {
                    if (reader.HasRows)
                    {
                        while (reader.Read())
                            bzp.Add(reader.GetInt32(0), reader.GetString(1));
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
                cmd.CommandText = "drop table templates";//пренести к доктору
                cmd.ExecuteNonQuery();
                cmd.CommandText = "CREATE TABLE templates ( Id INT AUTO_INCREMENT PRIMARY KEY, template_type INT DEFAULT - 1, id_doctor INT DEFAULT - 1, template_name VARCHAR(1024),template_services VARCHAR(1024) NOT NULL ); ";
                cmd.ExecuteNonQuery();
                foreach (Doctor d in doctors)
                {   
                    d.PushTempToDatabase(conn);                   
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
