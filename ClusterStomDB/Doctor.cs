using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.IO;
using System.Linq;
using System.Text;

namespace ClusterStomDB
{
    internal class Doctor : IComparable<Doctor>
    {
        private class Template
        {
            private int type = -1;
            public string Name { get; private set; }
            public void AssignName()
            {
                if (services.Count() == 0) return;
                string keyservice = services[0];
                if (type == 1)
                {
                    foreach (string s in services)
                    {
                        if (price[s].Value >= price[keyservice].Value)
                            keyservice = s;
                    }
                    Name = price[keyservice].Key;
                }
                if (type != 1)
                {
                    foreach (string s in services)
                    {
                        if (pricebzp[s].Value >= pricebzp[keyservice].Value)
                            keyservice = s;
                    }
                    Name = pricebzp[keyservice].Key;
                }
            }
            public Template(int bzp)
            {
                type = bzp;
            }
            public int GetTemplateType()
            {
                return type;
            }
            public void Add(string i)
            {
                services.Add(i);
            }
            public List<string> services = new List<string>();
            public override string ToString()
            {
                string s = "";
                foreach (string serv in services)
                {
                    s += serv.ToString() + "*";
                }
                return s;
            }
        }
        public readonly int id = 0;
        private string name = "";
        private static Dictionary<string, KeyValuePair<string, double>> price = new Dictionary<string, KeyValuePair<string, double>>();
        private static Dictionary<string, KeyValuePair<string, double>> pricebzp = new Dictionary<string, KeyValuePair<string, double>>();
        private List<Template> templates = new List<Template>();
        private List<Cluster> clusters = new List<Cluster>();
        private List<TaskOrder> orders = new List<TaskOrder>();
        private void InitPrice(MySqlConnection conn)//добавлять по уму
        {
            if (price.Count() != 0) return;
            string sql = "WITH cte AS (SELECT code,NAME,COST,ROW_NUMBER() OVER (PARTITION BY code ORDER BY id DESC) AS rn FROM price_orto_soc) SELECT * FROM cte WHERE rn = 1";
            MySqlCommand cmd = new MySqlCommand
            {
                Connection = conn,
                CommandText = sql
            };
            using (DbDataReader reader = cmd.ExecuteReader())
            {
                if (reader.HasRows)
                {
                    while (reader.Read())
                        price.Add(reader.GetString(0), new KeyValuePair<string, double>(reader.GetString(1), reader.GetDouble(2)));
                }
            }
        }
        public void PushTempToDatabase(MySqlConnection conn)
        {
            foreach (Template t in templates)
            {
                if (t.services.Count() == 0) continue;
                MySqlCommand cmd = new MySqlCommand
                {
                    Connection = conn
                };
                t.AssignName();
                cmd.CommandText = "INSERT INTO `stomadb`.`templates` (`template_type`,`id_doctor`,`template_name`,`template_services`) VALUES ('" + t.GetTemplateType().ToString() + "'," + id.ToString() + ",'" + t.Name + "','" + t.ToString() + "');";
                cmd.ExecuteNonQuery();
            }
        }
        private void InitPriceBZP(MySqlConnection conn)
        {
            if (pricebzp.Count() != 0) return;
            string sql = "WITH cte AS (SELECT code,NAME,COST,ROW_NUMBER() OVER (PARTITION BY code ORDER BY id DESC) AS rn FROM stomadb.price) SELECT * FROM cte WHERE rn = 1";
            MySqlCommand mySqlCommand = new MySqlCommand
            {
                Connection = conn,
                CommandText = sql
            };
            MySqlCommand cmd = mySqlCommand;
            using (DbDataReader reader = cmd.ExecuteReader())
            {
                if (reader.HasRows)
                {
                    while (reader.Read())
                        pricebzp.Add(reader.GetString(0), new KeyValuePair<string, double>(reader.GetString(1), reader.GetDouble(2)));
                }
            }
        }
        public void PrintDoctorTemp()
        {
            if (clusters.Count() == 0) return;
            using (FileStream fs = new FileStream("test2.txt", FileMode.Append))
            {

                StreamWriter w = new StreamWriter(fs, Encoding.Default);
                Console.WriteLine("===============================================================");
                w.WriteLine("===============================================================");
                Console.WriteLine("\n{2}\nDoctor ID = {0}  Количество шаблонов: {1}\n", id, templates.Count(), name);
                w.WriteLine("\n{2}\nDoctor ID = {0}  Количество шаблонов: {1}\n", id, templates.Count(), name);
                Console.WriteLine("\n++++++++++++++++++++++++++++++++++++++++ \n Шаблоны беспалтного зубопротезирования:\n++++++++++++++++++++++++++++++++++++++++ ");
                w.WriteLine("\n++++++++++++++++++++++++++++++++++++++++ \n Шаблоны беспалтного зубопротезирования:\n++++++++++++++++++++++++++++++++++++++++ ");
                int j = 0;
                for (int i = 0; i < templates.Count(); i++)
                {
                    if (templates[i].GetTemplateType() == 0)
                    {
                        j++;
                        continue;
                    }
                    Console.WriteLine("\nШаблон {0}: ", i + 1 - j);
                    w.WriteLine("\nШаблон {0}: ", i + 1 - j);
                    int counter = 0;
                    foreach (string s in templates[i].services)
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
                    if (templates[i].GetTemplateType() == 1)
                    {
                        j++;
                        continue;
                    }
                    Console.WriteLine("\nШаблон {0}: ", i + 1 - j);
                    w.WriteLine("\nШаблон {0}: ", i + 1 - j);
                    int counter = 0;
                    foreach (string s in templates[i].services)
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
            MakeClusters();
            foreach (Cluster c in clusters)
            {
                templates.Add(MakeTemplateFromCluster(c));
            }
            if (orders.Count() > 0)
                Console.WriteLine("\nDotor_ID={0} NOC={3} Number of temlates = {1} number of orders = {2}", id, templates.Count(), orders.Count(), clusters.Count());
        }

        private Template MakeTemplateFromCluster(Cluster c)
        {
            //Анализ кластера
            SortedDictionary<string, int> table = c.GetClusterTable();
            IOrderedEnumerable<KeyValuePair<string, int>> sortedTable = from entry in table where entry.Value > c.OrdersCount() * 80 / 90 orderby entry.Value descending select entry;//доделать
            Template tmp = new Template(c.GetClusterType());
            foreach (KeyValuePair<string, int> l in sortedTable)
            {
                tmp.Add(l.Key);
            }
            return tmp;
        }

        public Doctor(int i, MySqlConnection conn)
        {
            id = i;
            InitPrice(conn);
            InitPriceBZP(conn);
            InitByID(conn, i);
            InitNameById(conn, i);
        }

        private void InitNameById(MySqlConnection conn, int i)
        {
            string sql = "SELECT doctor_id,user_id,id,name FROM stomadb.doctor_spec inner join users on user_id=id where doctor_id=" + i.ToString();
            MySqlCommand cmd = new MySqlCommand
            {
                Connection = conn,
                CommandText = sql
            };
            using (DbDataReader reader = cmd.ExecuteReader())
            {
                if (reader.HasRows)
                {
                    while (reader.Read())
                        name = reader.GetString(3);
                }
            }
        }

        private void InitByID(MySqlConnection conn, int id)//инциализация доктора, потом пойдет создание шаблонов для него-же
        {
            string sql = @"SELECT * FROM (Select ID_SERVICE,ID, ID_ORDER, tmp.ID_DOCTOR as ID_doc, code, order_type from 
(Select ID_SERVICE, ID_CASE,ID_PROFILE, ID_ORDER, orders.ID_DOCTOR, case_services.ID_DOCTOR as ID_doc, orders.order_type from case_services inner join orders on id_order=ID WHERE ID_ORDER>0 and order_type=1) as tmp 
left join stomadb.price_orto_soc on tmp.ID_PROFILE=stomadb.price_orto_soc.ID 
union Select ID_SERVICE,ID, ID_ORDER, tmp.ID_DOCTOR as ID_doc, code, order_type from 
(Select ID_SERVICE, ID_CASE,ID_PROFILE, ID_ORDER, orders.ID_DOCTOR, case_services.ID_DOCTOR as ID_doc, orders.order_type from case_services inner join orders on id_order=ID WHERE ID_ORDER>0 and order_type!=1) as tmp 
inner join stomadb.price on tmp.ID_PROFILE=stomadb.price.ID) as u
where u.ID_doc =" + id.ToString() + " ORDER BY ID_ORDER ";
            MySqlCommand cmd = new MySqlCommand
            {
                Connection = conn,
                CommandText = sql
            };
            long prevCaseId = 0;
            List<TaskOrder> cases = new List<TaskOrder>();
            using (DbDataReader reader = cmd.ExecuteReader())
            {
                if (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        long currCaseId = reader.GetInt64(2);
                        if (currCaseId != prevCaseId || prevCaseId == 0)
                        {
                            TaskOrder t = new TaskOrder(currCaseId, reader.GetString(4), reader.GetInt32(5));//отредактировать с учетом типов
                            cases.Add(t);
                        }
                        else
                        {
                            cases[cases.Count - 1].AddServiceById(reader.GetString(4));
                        }
                        prevCaseId = currCaseId;
                    }
                }
            }
            orders = cases;
        }
        public int Count()
        {
            return templates.Count;
        }
        public int CompareTo(Doctor p)
        {
            return Count().CompareTo(p.Count());
        }
        //public Template this[int index]
        //{
        //    get
        //    {
        //        return templates[index];
        //    }
        //}
        private void MakeClusters()//рразобраться с мейном и не мейно, что и куда
        {
            //процесс инициализации
            foreach (TaskOrder t in orders)
            {

                t.SortServicesById();
                Cluster tmp = new Cluster(t.GetOrderType());
                tmp.AddOrder(t);
                clusters.Add(tmp);
                double addProfitNC = GlobalProfit();
                clusters.Remove(tmp);
                tmp.RemoveOrder(t);
                double addProfitECMax = 0;
                // Console.WriteLine("Add profit(new cluster) = {0}\n\n", addProfitNC);
                int counter = 0;
                int max = 0;
                foreach (Cluster c in clusters)
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
                if (clusters.Count == 0)
                {
                    tmp.AddOrder(t);
                    clusters.Add(tmp);
                    continue;
                }
                if (addProfitECMax - addProfitNC > UtilConst.eps)
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
                if (!MakeIter()) break;
            }
            //неторого рода оптимизация(мб еще стоит выкинуть шаблоны, где ширина шаблна меньше 2, нужно попробовать отпиливать, пока не сстанет меньше 25.
            clusters.Sort();
            clusters.Reverse();
            int j = 0;
            for (int i = 0; i < clusters.Count(); i++)
            {
                if (clusters[i].OrdersCount() == 2)
                {
                    j = i;
                    break;
                }
            }

            clusters.RemoveRange(j, clusters.Count - j);
            //Console.WriteLine("DOCTOR_ID={0} Clusters.Count()={1}", id, this.clusters.Count());

            //PrintResults(clusters);

        }
        private bool MakeIter()
        {
            bool ismoved = false;
            SortedSet<long> moved = new SortedSet<long>();
            for (int i = 0; i < clusters.Count; i++)
            {
                for (int j = 0; j < clusters[i].OrdersCount(); j++)
                {
                    if (moved.Contains(clusters[i][j].id))
                    {

                        continue;
                    }

                    double currProfit = GlobalProfit();
                    double newProfit = 0;
                    double maxProfit = 0;
                    int numberMax = -1;
                    bool findMax = false;
                    TaskOrder tmp = clusters[i][j];
                    clusters[i].RemoveOrder(tmp);
                    for (int k = 0; k < clusters.Count; k++) //поиск лучшего кластера
                    {
                        if (k == i) continue;
                        clusters[k].AddOrder(tmp);
                        newProfit = GlobalProfit();
                        clusters[k].RemoveOrder(tmp);
                        if (newProfit - currProfit > UtilConst.eps && newProfit - maxProfit > UtilConst.eps)
                        {
                            maxProfit = newProfit;
                            numberMax = k;
                            findMax = true;
                        }
                    }
                    if (findMax)
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
                if (clusters[i].OrdersCount() == 0)
                {
                    l = i;
                    break;
                }
            }
            if (l > 0)
            {
                clusters.RemoveRange(l, clusters.Count - l);
            }
            return ismoved;
        }
        private double GlobalProfit()
        {
            double num = 0;
            double denom = 0;
            if (clusters.Count == 0) return 0;
            foreach (Cluster c in clusters)
            {
                if (c.OrdersCount() == 0) continue;
                num += c.Gradient * (double)c.OrdersCount();
                denom += (double)c.OrdersCount();
            }
            return num / denom;
        }
    }
}