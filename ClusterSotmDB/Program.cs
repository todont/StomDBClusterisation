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
            // Получить объект Connection подключенный к DB.
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
                // Закрыть соединение.
                conn.Close();
                // Уничтожить объект, освободить ресурс.
                conn.Dispose();
            }
            Console.Read();
        }
        private static void QueryStomdb(MySqlConnection conn)
        {
            string sql = "Select ID_SERVICE,ID_CASE from case_services ORDER BY ID_SERVICE";

            // Создать объект Command.
            MySqlCommand cmd = new MySqlCommand();

            // Сочетать Command с Connection.
            cmd.Connection = conn;
            cmd.CommandText = sql;
            using (DbDataReader reader = cmd.ExecuteReader())
            {
                if (reader.HasRows)
                {
                    int i = 0;
                    while (reader.Read()&& i<20)
                    {
                        long idService = reader.GetInt64(0);
                        long idCase = reader.GetInt64(1);

                        Console.WriteLine("--------------------");
                        Console.WriteLine("ID_SERVICE=" + idService); 
                        Console.WriteLine("ID_CASE=" + idCase);
                        i++;
                    }
                }
            }
        }
    }
}
