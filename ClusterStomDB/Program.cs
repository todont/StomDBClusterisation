using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Threading.Tasks;


namespace ClusterStomDB
{
    public class Program
    {
        public static void Main()
        {

            MySqlConnection conn = DBUtils.GetDBConnection("stomadb");
            conn.Open();
            try
            {
                string sql = "SELECT distinct ID_Doctor FROM stomadb.case_services WHERE ID_DOCTOR>151 and ID_DOCTOR<153";
                MySqlCommand cmd = new MySqlCommand
                {
                    Connection = conn,
                    CommandText = sql
                };
                List<int> idDoctor = new List<int>();
                using (DbDataReader reader = cmd.ExecuteReader())
                {
                    if (reader.HasRows)
                    {
                        while (reader.Read())
                        {
                            idDoctor.Add(reader.GetInt32(0));
                        }
                    }
                }
                List<Task> taskList = new List<Task>();
                List<Doctor> doctors = new List<Doctor>();
                foreach (int i in idDoctor)
                {
                    Doctor tmp = new Doctor(i, conn);
                    Task task = Task.Factory.StartNew(() => tmp.MakeTemplates());
                    doctors.Add(tmp);
                    taskList.Add(task);
                }
                Task.WaitAll(taskList.ToArray());
                cmd.CommandText = "drop table templates";
                cmd.ExecuteNonQuery();
                cmd.CommandText = "CREATE TABLE templates ( Id INT AUTO_INCREMENT PRIMARY KEY, template_type INT DEFAULT - 1, id_doctor INT DEFAULT - 1, template_name VARCHAR(1024),template_services VARCHAR(1024) NOT NULL ); ";
                cmd.ExecuteNonQuery();
                foreach (Doctor d in doctors)
                {
                    d.PushTempToDatabase(conn);
                }
                foreach (Doctor d in doctors)
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
