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
                string sql = "SELECT distinct ID_Doctor FROM stomadb.case_services ";
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
                //разобрать функции на более мелкие функции(clopeMaker
                List<Task> taskList = new List<Task>();
                Dictionary<int, List<TaskOrder>> doctors = new Dictionary<int, List<TaskOrder>>();
                foreach(int i in ids) {
                    if (!InitListByID(doctors, conn, i)) continue;
                    Task task = Task.Factory.StartNew(() => MakeTemplate(doctors[i], i));
                    taskList.Add(task);
                }
                Task.WaitAll(taskList.ToArray());
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
        private static bool InitListByID(Dictionary<int,List<TaskOrder>> doctors,MySqlConnection conn, int id)
        { 
            string sql = "Select ID_SERVICE, ID_CASE,ID_PROFILE from case_services WHERE ID_DOCTOR=" + id.ToString() + " ORDER BY ID_CASE desc limit 5000";
            MySqlCommand cmd = new MySqlCommand();
            cmd.Connection = conn;
            cmd.CommandText = sql;
            long prevCaseId = 0;
            List<TaskOrder> cases = new List<TaskOrder>();
            using (DbDataReader reader = cmd.ExecuteReader())
            {
                if (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        long currCaseId = reader.GetInt64(1);
                        if (currCaseId != prevCaseId || prevCaseId == 0)
                        {
                            TaskOrder t = new TaskOrder(currCaseId, reader.GetInt64(2));
                            cases.Add(t);
                        }
                        else
                        {
                            cases[cases.Count - 1].AddServiceById(reader.GetInt64(2));
                        }
                        prevCaseId = currCaseId;
                    }
                }
            }
            if (cases.Count() > 0)
            {
                doctors.Add(id, cases);
                return true;
            }
            return false;
        }
        private static void MakeTemplate(List<TaskOrder> cases, int id)//рразобраться с мейном и не мейно, что и куда
        {

            List<Cluster> clusters = new List<Cluster>();
            //процесс инициализации
            foreach (TaskOrder t in cases)
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
                    continue;
                }
                if (addProfitECMax-addProfitNC>UtilConst.eps)
                {
                    clusters[max].AddOrder(t);
                }
                else
                {
                    tmp.AddOrder(t);
                    clusters.Add(tmp);
                }
            }
            //итерации, причем создадим отдельно список мувов
            //double p = GlobalProfit(clusters);
           // Console.WriteLine("Global profit before iteration = {0}\n", p);
            for (int i = 0; i < 10; i++)
            {
                if (!MakeIter(clusters)) break;
            }
            //неторого рода оптимизация(мб еще стоит выкинуть шаблоны, где ширина шаблна меньше 2
            clusters.Sort();
            clusters.Reverse();
            int j = 0;
            for (int i = 0; i < clusters.Count(); i++)
            {
                if (clusters[i].Count() == 3)
                {
                    j = i;
                    break;
                }
            }

            clusters.RemoveRange(j, clusters.Count - j);
            Console.WriteLine("DOCTOR_ID={0} Clusters.Count()={1}",id,clusters.Count());
            //PrintResults(clusters);

        }
        private static bool MakeIter(List<Cluster> clusters)
        {
            bool ismoved = false;
            SortedSet<long> moved = new SortedSet<long>();
            for (int i = 0; i < clusters.Count; i++)
            {
                for (int j = 0; j < clusters[i].Count(); j++)
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
                    }
                    if (findMax == true)
                    {
                        clusters[numberMax].AddOrder(tmp);
                       // Console.WriteLine("Moved from {0} to {1} id={2} max profit = {3}", i, numberMax, tmp.id,maxProfit);
                        ismoved = true;
                    }
                    else
                    {
                        clusters[i].AddOrder(tmp);    
                    }
                    moved.Add(tmp.id);
                }
                
            }
            moved.Clear();
            clusters.Sort();
            clusters.Reverse();
            int l = -1;
            for (int i = 0; i < clusters.Count(); i++)
            {
                if (clusters[i].Count() == 0)
                {
                    l = i;
                    break;
                }
            }
            if (l > 0)
            {
                clusters.RemoveRange(l, clusters.Count - l);
            }
            double p = GlobalProfit(clusters);
           // Console.WriteLine("Global profit = {0}\n", p);
            return ismoved;
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
