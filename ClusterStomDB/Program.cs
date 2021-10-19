using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;
using System.Data.Common;


namespace ClusterStomDB
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
            string sql = "Select ID_SERVICE, ID_CASE,ID_PROFILE from case_services WHERE ID_DOCTOR=207 ORDER BY ID_CASE desc ";
            MySqlCommand cmd = new MySqlCommand();
            cmd.Connection = conn;
            cmd.CommandText = sql;
            long prevCaseId = 0;
            List<Cluster> clusters = new List<Cluster>();
            List<TaskOrder> cases207 = new List<TaskOrder>();
    
            using (DbDataReader reader = cmd.ExecuteReader())
            {
                if (reader.HasRows)
                {
                    int i = 0;
                    while (reader.Read() && i<10000)
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
                        i++;
                    }
                }
            }
            //процесс инициализации
            foreach(TaskOrder t in cases207)
            {
                t.SortServicesById();
                Cluster tmp = new Cluster();
                tmp.AddOrder(t);
                clusters.Add(tmp);
                double addProfitNC = GlobalProfit(clusters);
                clusters.Remove(tmp);
                tmp.RemoveOrder(t);
                double addProfitECMax = 0;
               // Console.WriteLine("Add profit(new cluster) = {0}\n\n", addProfitNC);
                int counter = 0;
                int max = 0;
                foreach(Cluster c in clusters)
                {
                    c.AddOrder(t);
                    double addProfitEC = GlobalProfit(clusters);
                    //Console.WriteLine("Add profit(existing cluster {0} ) = {1}\n", counter, addProfitEC);
                    if (addProfitEC - addProfitNC > UtilConst.eps && addProfitEC - addProfitECMax > UtilConst.eps) 
                    {
                        addProfitECMax = addProfitEC;
                        max = counter;
                    }
                    counter++;
                    c.RemoveOrder(t);
                }
                if (clusters.Count == 0)
                {
                    tmp.AddOrder(t);
                    clusters.Add(tmp);
                    double k = GlobalProfit(clusters);
                   // Console.WriteLine("Global profit = {0}\n================\n", k);
                    continue;
                }
                if (addProfitECMax-addProfitNC>0.000001)
                {
                    clusters[max].AddOrder(t);
                }
                else
                {
                    tmp.AddOrder(t);
                    clusters.Add(tmp);
                }
               // double p = GlobalProfit(clusters);
               // Console.WriteLine("Global profit = {0}\n================\n", p);
            }
            //итерации, причем создадим отдельно список мувов
            double p = GlobalProfit(clusters);
            Console.WriteLine("Global profit before iteration = {0}", p);
            for (int i = 0; i < 10; i++)
            {
                Console.ForegroundColor = ConsoleColor.Blue;
                Console.WriteLine("Iteration {0}:",i);
                Console.ResetColor();
                MakeIter(clusters);
            }
            //неторого рода оптимизация(мб еще стоит выкинуть шаблоны, где длина шаблна меньше 2
            clusters.Sort();
            clusters.Reverse();
            int j = 0;
            for(int i = 0; i < clusters.Count(); i++)
            {
                if (clusters[i].Count() == 2)
                {
                    j = i;
                    break;
                }
            }
            
            clusters.RemoveRange(j, clusters.Count - j);
            PrintResults(clusters);

        }
        private static void MakeIter(List<Cluster> clusters)
        {
            SortedSet<long> moved = new SortedSet<long>();
            for (int i = 0; i < clusters.Count; i++)
            {
                for (int j = 0; j < clusters[i].Count()-1; j++)
                {
                    if (moved.Contains(clusters[i][j].id))
                    {

                        continue;
                    }

                    double currProfit = GlobalProfit(clusters);
                    double newProfit = 0;
                    double maxProfit = 0;
                    int numberMax = -1;
                    bool findMax = false;
                    TaskOrder tmp = clusters[i][j];
                    clusters[i].RemoveOrder(tmp);
                    int c = clusters[i].Count();
                    for (int k = 0; k < clusters.Count; k++) //поиск лучшего кластера
                    {
                        if (k == i)
                        {
                            continue;
                        }
                        clusters[k].AddOrder(tmp);
                        newProfit = GlobalProfit(clusters);
                        clusters[k].RemoveOrder(tmp);
                        if (newProfit - currProfit > UtilConst.eps && newProfit - maxProfit > UtilConst.eps)
                        {
                            maxProfit = newProfit;
                            numberMax = k;
                            findMax = true;
                        }
                        if (k == 7 && i == 3)
                        {
                            Console.WriteLine("NewProfit={0} c", newProfit);
                        }
                    }
                    //Console.WriteLine("MaxProfit={0} c", maxProfit);
                    if (findMax == true)
                    {
                        clusters[numberMax].AddOrder(tmp);
                        Console.WriteLine("aaMoved from {0} to {1} id={2} max profit = {3}", i, numberMax, tmp.id,maxProfit);
                    }
                    else
                    {
                        clusters[i].AddOrder(tmp);
                    }
                    moved.Add(tmp.id);
                }
            }
            moved.Clear();
            double p = GlobalProfit(clusters);
            Console.WriteLine("Global profit = {0}", p);
        }
        private static double GlobalProfit(List<Cluster> clusters)
        {
            double num = 0;
            double denom = 0;
            if (clusters.Count == 0) return 0;
            foreach (Cluster c in clusters) 
            {
                if (c.Count() == 0) continue;
                num += c.gradient*(double)c.Count();
                denom += (double)c.Count();
            }
            return num/denom;
        }
        private static void PrintResults(List<Cluster> clusters)
        {
            for(int i =0;i<clusters.Count;i++)
            {
                Console.WriteLine("Cluster {0}:",i);
                clusters[i].Print();
            }

        }
    }
}
