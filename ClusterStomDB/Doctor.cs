using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.Common;
using MySql.Data.MySqlClient;
using System.IO;

namespace ClusterStomDB
{
    class Doctor : IComparable<Doctor>
    {
        private class Template
        {
            private int type = -1;
            public string name { get; private set; }
            public void AssignName()
            {
                if (services.Count() == 0) return;
                long keyservice = services[0];
                if (this.type == 0)
                {
                    foreach (long s in services)
                    {
                        if (price[s].Value >= price[keyservice].Value)
                            keyservice = s;
                    }
                    this.name = price[keyservice].Key;
                }
                if (this.type == 1) 
                { 
                    foreach (long s in services)
                    {
                        if (pricebzp[s].Value >= pricebzp[keyservice].Value)
                            keyservice = s;
                    }
                    this.name = pricebzp[keyservice].Key;
                }
            }
            public Template (int bzp)
            {
                this.type = bzp;
            }
            public int GetTemplateType()
            {
                return this.type;
            }
            public void Add(long i)
            {
                services.Add(i);
            }
            public List<long> services = new List<long>();
            override public string ToString()
            {
                string s = "";
                foreach(long serv in services)
                {
                    s += serv.ToString() + "*";
                }
                return s;
            }
        }
        public readonly int id = 0;
        private string name = "";
        private static Dictionary<long, KeyValuePair<string, double>> price = new Dictionary<long, KeyValuePair<string, double>>();
        private static Dictionary<long, KeyValuePair<string, double>> pricebzp = new Dictionary<long, KeyValuePair<string, double>>();
        private List<Template> templates = new List<Template>();
        private List<Cluster> clusters = new List<Cluster>();
        private List<Cluster> clustersBZP = new List<Cluster>();
        private void InitPrice(MySqlConnection conn)
        {
            if (price.Count() != 0) return;
            string sql = "SELECT ID,NAME,COST FROM stomadb.price";
            MySqlCommand cmd = new MySqlCommand();
            cmd.Connection = conn;
            cmd.CommandText = sql;
            using (DbDataReader reader = cmd.ExecuteReader())
            {
                if (reader.HasRows)
                {
                    while (reader.Read())
                        price.Add(reader.GetInt64(0), new KeyValuePair<string, double>(reader.GetString(1),reader.GetDouble(2)));
                }
            }
        }
        public void PushTempToDatabase(MySqlConnection conn)
        {   
            foreach(Template t in templates)
            {   if (t.services.Count() == 0) continue;
                MySqlCommand cmd = new MySqlCommand();
                cmd.Connection = conn;
                t.AssignName();
                cmd.CommandText = "INSERT INTO `stomadb`.`templates` (`template_type`,`id_doctor`,`template_name`,`template_services`) VALUES ('" + t.GetTemplateType().ToString() + "'," + this.id.ToString() + ",'" +t.name+"','" + t.ToString() + "');";
                cmd.ExecuteNonQuery();
            }
        }
        private void InitPriceBZP(MySqlConnection conn)
        {
            if (pricebzp.Count() != 0) return;
            string sql = "SELECT ID,NAME,COST FROM stomadb.price_orto_soc";
            MySqlCommand cmd = new MySqlCommand();
            cmd.Connection = conn;
            cmd.CommandText = sql;
            using (DbDataReader reader = cmd.ExecuteReader())
            {
                if (reader.HasRows)
                {
                    while (reader.Read())
                        pricebzp.Add(reader.GetInt64(0), new KeyValuePair<string, double>(reader.GetString(1), reader.GetDouble(2)));
                }
            }
        }
        public void PrintDoctorTemp()
        {   if(clusters.Count()==0) return;
            using (FileStream fs = new FileStream("test2.txt", FileMode.Append)) {

                StreamWriter w = new StreamWriter(fs, Encoding.Default);
                Console.WriteLine("===============================================================");
                w.WriteLine("===============================================================");
                Console.WriteLine("\n{2}\nDoctor ID = {0}  Количество шаблонов: {1}\n", id, templates.Count(), this.name);
                w.WriteLine("\n{2}\nDoctor ID = {0}  Количество шаблонов: {1}\n", id, templates.Count(), this.name);
                Console.WriteLine("\n++++++++++++++++++++++++++++++++++++++++ \n Шаблоны беспалтного зубопротезирования:\n++++++++++++++++++++++++++++++++++++++++ ");
                w.WriteLine("\n++++++++++++++++++++++++++++++++++++++++ \n Шаблоны беспалтного зубопротезирования:\n++++++++++++++++++++++++++++++++++++++++ ");
                int j = 0;
                for (int i = 0; i < templates.Count(); i++)
                {  
                    if (templates[i].GetTemplateType()==0)
                    {
                        j++;
                        continue;
                    }
                    Console.WriteLine("\nШаблон {0}: ", i + 1 - j);
                    w.WriteLine("\nШаблон {0}: ", i + 1 - j);
                    int counter = 0;
                    foreach (long s in templates[i].services)
                    {
                        counter++;

                        if (pricebzp.ContainsKey(s))
                        {
                            Console.WriteLine("{1}) {0} id({2})", pricebzp[s].Key, counter, s);
                            w.WriteLine("{1}) {0} id({2})", pricebzp[s].Key, counter, s);
                            continue;
                        }
                        else
                        {
                            Console.WriteLine("Нет услуги с таким ID {0}", s);
                            w.WriteLine("Нет услуги с таким ID {0}", s);
                        }
                    }
                }
                Console.WriteLine("\n++++++++++++++++++++++++++++++++++++++++ \n Шаблоны платного зубопротезирования:\n++++++++++++++++++++++++++++++++++++++++ ");
                w.WriteLine("\n++++++++++++++++++++++++++++++++++++++++ \n Шаблоны платного зубопротезирования:\n++++++++++++++++++++++++++++++++++++++++ ");
                j = 0;
                for (int i = 0; i < templates.Count(); i++)
                {
                    if (templates[i].GetTemplateType()==1) 
                    {
                        j++;
                        continue;
                    }
                    Console.WriteLine("\nШаблон {0}: ", i + 1-j);
                    w.WriteLine("\nШаблон {0}: ", i + 1-j);
                    int counter = 0;
                    foreach (long s in templates[i].services)
                    {
                        counter++;
                        if (price.ContainsKey(s))
                        {
                            Console.WriteLine("{1}) {0} id({2})", price[s].Key, counter, s);
                            w.WriteLine("{1}) {0} id({2})", price[s].Key, counter, s);
                        }
                        else
                        {
                            Console.WriteLine("Нет услуги с таким ID {0}", s);
                            w.WriteLine("Нет услуги с таким ID {0}", s);
                        }
                    }
                }
                Console.WriteLine("\n===============================================================");
            w.WriteLine("\n===============================================================");
            }
        }
        public void MakeTemplates()
        {
            //кластеризация
            this.MakeClusters(1);
            clustersBZP = new List<Cluster>(clusters);
            clusters = new List<Cluster>();

            this.MakeClusters(0);
            foreach(Cluster c in this.clusters)
            {
                templates.Add(MakeTemplateFromCluster(c));
            }
            foreach (Cluster c in this.clustersBZP)
            {
                templates.Add(MakeTemplateFromCluster(c));
            }
            if(orders.Count()>0)
            Console.WriteLine("\nDotor_ID={0} NOC={3} Number of temlates = {1} number of orders = {2}", this.id, templates.Count(),orders.Count(),clusters.Count());
        }
        private List<TaskOrder> orders = new List<TaskOrder>();
        private Template MakeTemplateFromCluster(Cluster c)
        {
            //Анализ кластера
            SortedDictionary<long, int> table = c.GetClusterTable();
            var sortedTable = from entry in table where entry.Value > c.OrdersCount()*80/90 orderby entry.Value  descending select entry ;//доделать
            Template tmp = new Template(c.GetClusterType());
            foreach(KeyValuePair<long, int> l in sortedTable)
            {
                tmp.Add(l.Key);
            }
            return tmp;
        }

        public Doctor(int i,MySqlConnection conn)
        {
            id = i;
            this.InitPrice(conn);
            this.InitPriceBZP(conn);
            this.InitByID(conn, i);
            this.InitNameById(conn, i);
        }

        private void InitNameById(MySqlConnection conn, int i)
        {
            string sql = "SELECT doctor_id,user_id,id,name FROM stomadb.doctor_spec inner join users on user_id=id where doctor_id="+ i.ToString();
            MySqlCommand cmd = new MySqlCommand();
            cmd.Connection = conn;
            cmd.CommandText = sql;
            using (DbDataReader reader = cmd.ExecuteReader())
            {
                if (reader.HasRows)
                {
                    while (reader.Read())
                        this.name = reader.GetString(3);
                }
            }
        }

        private void InitByID(MySqlConnection conn, int id)//инциализация доктора, потом пойдет создание шаблонов для него-же
        {
            string sql = "Select ID_SERVICE, ID_CASE,ID_PROFILE, ID_ORDER,ID, orders.ID_DOCTOR, case_services.ID_DOCTOR, order_type from case_services inner join orders on id_order=ID WHERE ID_ORDER>0 and case_services.ID_DOCTOR=" + id.ToString() + " ORDER BY ID_ORDER ";
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
                        long currCaseId = reader.GetInt64(3);
                        if (currCaseId != prevCaseId || prevCaseId == 0)
                        {
                            TaskOrder t = new TaskOrder(currCaseId, reader.GetInt64(2),reader.GetInt32(7));
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
            this.orders = cases;
        }
        public int Count()
        {
            return templates.Count;
        }
        public int CompareTo(Doctor p)
        {
            return this.Count().CompareTo(p.Count());
        }
        //public Template this[int index]
        //{
        //    get
        //    {
        //        return templates[index];
        //    }
        //}
        private void MakeClusters(int isBZP)//рразобраться с мейном и не мейно, что и куда
        {
            //процесс инициализации
            foreach (TaskOrder t in this.orders)
            {
                if (t.GetOrderType()!=isBZP) continue;
                t.SortServicesById();
                Cluster tmp = new Cluster(t.GetOrderType());
                tmp.AddOrder(t);
                this.clusters.Add(tmp);
                double addProfitNC = GlobalProfit();
                this.clusters.Remove(tmp);
                tmp.RemoveOrder(t);
                double addProfitECMax = 0;
                // Console.WriteLine("Add profit(new cluster) = {0}\n\n", addProfitNC);
                int counter = 0;
                int max = 0;
                foreach (Cluster c in this.clusters)
                {
                    c.AddOrder(t);
                    double addProfitEC = GlobalProfit();
                    //Console.WriteLine("Add profit(existing cluster {0} ) = {1}\n", counter, addProfitEC);
                    if (addProfitEC - addProfitNC > UtilConst.eps && addProfitEC - addProfitECMax > UtilConst.eps)
                    {
                        addProfitECMax = addProfitEC;
                        max = counter;
                    }
                    counter++;
                    c.RemoveOrder(t);
                }
                if (this.clusters.Count == 0)
                {
                    tmp.AddOrder(t);
                    this.clusters.Add(tmp);
                    continue;
                }
                if (addProfitECMax - addProfitNC > UtilConst.eps)
                {
                    this.clusters[max].AddOrder(t);
                }
                else
                {
                    tmp.AddOrder(t);
                    this.clusters.Add(tmp);
                }
            }
            //итерации, причем создадим отдельно список мувов
            //double p = GlobalProfit(clusters);
            // Console.WriteLine("Global profit before iteration = {0}\n", p);
            for (int i = 0; i < 10; i++)
            {
                if (!MakeIter()) break;
            }
            //неторого рода оптимизация(мб еще стоит выкинуть шаблоны, где ширина шаблна меньше 2, нужно попробовать отпиливать, пока не сстанет меньше 25.
            this.clusters.Sort();
            this.clusters.Reverse();
            int j = 0;
            for (int i = 0; i < this.clusters.Count(); i++)
            {
                if (this.clusters[i].OrdersCount() == 2)
                {
                    j = i;
                    break;
                }
            }

            this.clusters.RemoveRange(j, this.clusters.Count - j);
            //Console.WriteLine("DOCTOR_ID={0} Clusters.Count()={1}", id, this.clusters.Count());

            //PrintResults(clusters);

        }
        private bool MakeIter()
        {
            bool ismoved = false;
            SortedSet<long> moved = new SortedSet<long>();
            for (int i = 0; i < this.clusters.Count; i++)
            {
                for (int j = 0; j < this.clusters[i].OrdersCount(); j++)
                {
                    if (moved.Contains(this.clusters[i][j].id))
                    {

                        continue;
                    }

                    double currProfit = GlobalProfit();
                    double newProfit = 0;
                    double maxProfit = 0;
                    int numberMax = -1;
                    bool findMax = false;
                    TaskOrder tmp = this.clusters[i][j];
                    this.clusters[i].RemoveOrder(tmp);
                    int c = this.clusters[i].OrdersCount();
                    for (int k = 0; k < this.clusters.Count; k++) //поиск лучшего кластера
                    {
                        if (k == i)
                        {
                            continue;
                        }
                        this.clusters[k].AddOrder(tmp);
                        newProfit = GlobalProfit();
                        this.clusters[k].RemoveOrder(tmp);
                        if (newProfit - currProfit > UtilConst.eps && newProfit - maxProfit > UtilConst.eps)
                        {
                            maxProfit = newProfit;
                            numberMax = k;
                            findMax = true;
                        }
                    }
                    if (findMax == true)
                    {
                        this.clusters[numberMax].AddOrder(tmp);
                        // Console.WriteLine("Moved from {0} to {1} id={2} max profit = {3}", i, numberMax, tmp.id,maxProfit);
                        ismoved = true;
                    }
                    else
                    {
                        this.clusters[i].AddOrder(tmp);
                    }
                    moved.Add(tmp.id);
                }

            }
            moved.Clear();
            this.clusters.Sort();
            this.clusters.Reverse();
            int l = -1;
            for (int i = 0; i < this.clusters.Count(); i++)
            {
                if (this.clusters[i].OrdersCount() == 0)
                {
                    l = i;
                    break;
                }
            }
            if (l > 0)
            {
                clusters.RemoveRange(l, this.clusters.Count - l);
            }
            double p = GlobalProfit();
            // Console.WriteLine("Global profit = {0}\n", p);
            return ismoved;
        }
        private double GlobalProfit()
        {
            double num = 0;
            double denom = 0;
            if (this.clusters.Count == 0) return 0;
            foreach (Cluster c in this.clusters)
            {
                if (c.OrdersCount() == 0) continue;
                num += c.gradient * (double)c.OrdersCount();
                denom += (double)c.OrdersCount();
            }
            return num / denom;
        }
        //private static void Print()
        //{
        //    for(int i =0;i<this.clusters.Count;i++)
        //    {
        //        Console.WriteLine("Cluster {0}:",i);
        //        this.clusters[i].Print();
        //    }

        //}
    }
}