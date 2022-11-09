using ClusterStomDB;
using MySql.Data.MySqlClient;

using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static ClusterSotmDB.Clinic;

namespace ClusterSotmDB
{
    internal sealed class Clinic
    {
        private static Clinic instance;
        private readonly static Dictionary<string, KeyValuePair<string, double>> price = new Dictionary<string, KeyValuePair<string, double>>();
        private List<Doctor> doctors = new List<Doctor>();
        private MySqlConnection conn;
        private void InitPrices(MySqlConnection conn)
        {
            string sql = "WITH cte AS (SELECT code,NAME,COST,ROW_NUMBER() OVER (PARTITION BY code ORDER BY id DESC) AS rn FROM price) SELECT * FROM cte WHERE rn = 1";
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
                    {
                        string tmp = reader.GetString(1);
                        if (!char.IsLetter(tmp[1]) && char.IsLetter(tmp[0]))
                        {
                            tmp = tmp.Remove(0, tmp.IndexOf(' ') + 1);
                        }
                        price.Add(reader.GetString(0), new KeyValuePair<string, double>(tmp, reader.GetDouble(2)));
                    }
                }
            }
            sql = "WITH cte AS (SELECT code,NAME,COST,ROW_NUMBER() OVER (PARTITION BY code ORDER BY id DESC) AS rn FROM price_orto_soc) SELECT * FROM cte WHERE rn = 1";
            MySqlCommand mySqlCommand = new MySqlCommand
            {
                Connection = conn,
                CommandText = sql
            };
            cmd = mySqlCommand;
            using (DbDataReader reader = cmd.ExecuteReader())
            {
                if (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        if (!price.ContainsKey(reader.GetString(0)))
                        {
                            price.Add(reader.GetString(0), new KeyValuePair<string, double>(reader.GetString(1), reader.GetDouble(2)));
                        }
                    }
                }
            }
        }
        internal class Doctor : IComparable<Doctor>
        {
            public readonly int id = 0;
            private string Name = "";
            private List<Template> Templates = new List<Template>();
            private List<Cluster> Clusters = new List<Cluster>();
            private List<TaskOrder> Orders = new List<TaskOrder>();
            public Doctor(int i, MySqlConnection conn, Dictionary<string, KeyValuePair<string, double>> price)
            {
                id = i;
                InitServisesByID(conn, i);
                InitNameById(conn, i);
            }
            internal class TaskOrder : IComparable<TaskOrder>, IEquatable<TaskOrder>
            {
                public readonly long id;
                private readonly int type;
                public TaskOrder(long i, string j, int bzp)
                {
                    type = bzp;
                    id = i;
                    services = new List<string> { j };
                }
                public int GetOrderType() { return type; }
                private List<string> services = new List<string>();
                public string this[int index] => services[index];
                public void AddServiceById(string i)
                {
                    if (!services.Contains(i))
                    {
                        services.Add(i);
                    }
                }
                public int CompareTo(TaskOrder p)
                {
                    return id.CompareTo(p.id);
                }
                public bool Equals(TaskOrder other)
                {
                    return id == other.id;
                }
                public void SortServicesById()
                {
                    services.Sort();
                }
                public int Len() => services.Count;
                public void Print()
                {
                    Console.ForegroundColor = ConsoleColor.Cyan;
                    Console.Write("{");
                    for (int i = 0; i < services.Count - 1; i++)
                    {
                        Console.Write("{0}, ", services[i]);
                    }
                    Console.Write("{0}", services[services.Count - 1]);
                    Console.Write("} ");
                    Console.ResetColor();
                    Console.WriteLine("id = {0}", id);
                }
            }
            internal class Template
            {
                public Template(int bzp)
                {
                    type = bzp;
                }
                private readonly int type;
                public List<string> services = new List<string>();
                public string Name { get; private set; }
                public void AssignName()
                {
                    if (services.Count == 0)
                    {
                        return;
                    }
                    string keyservice = services[0];

                    foreach (string s in services)
                    {
                        if (price[s].Value >= price[keyservice].Value)
                        {
                            keyservice = s;
                        }
                    }
                    Name = price[keyservice].Key;
                }
                public int GetTemplateType()
                {
                    return type;
                }
                public void Add(string i)
                {
                    services.Add(i);
                }
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
            internal class Cluster : IComparable<Cluster>
            {
                public double Gradient { get; private set; } = 0;
                private SortedDictionary<string, int> OrdersTable = new SortedDictionary<string, int>();
                private List<TaskOrder> Orders = new List<TaskOrder>();
                private readonly int Type;
                private static double r = 0.1;
                public Cluster(int bzp)
                {
                    Type = bzp;
                }
                public int GetClusterType()
                {
                    return Type;
                }
                public void SetR(double i)
                {
                    r = i;
                }
                public SortedDictionary<string, int> GetClusterTable()
                {
                    return OrdersTable;
                }
                public int OrdersCount()
                {
                    return Orders.Count;
                }
                public int CompareTo(Cluster p)
                {
                    return OrdersCount().CompareTo(p.OrdersCount());
                }
                public void Print()
                {
                    Orders.Sort();
                    Console.WriteLine("==================================");
                    foreach (TaskOrder o in Orders)
                    {
                        o.Print();
                    }
                    Console.WriteLine("==================================\n");
                }
                public TaskOrder this[int index] => Orders[index];
                private double RecalculateGradient()
                {
                    Gradient = 0;
                    if (Orders.Count == 0)
                    {
                        return 0;
                    }
                    foreach (int t in OrdersTable.Values)
                    {
                        double k = 1.0;
                        Gradient += k * t;
                    }
                    Gradient = Gradient / OrdersTable.Count / OrdersTable.Count;
                    return Gradient / OrdersTable.Count / Math.Pow(OrdersTable.Count, r);
                }
                public void AddOrder(TaskOrder o)
                {
                    for (int i = 0; i < o.Len(); i++)
                    {
                        if (OrdersTable.ContainsKey(o[i]))
                        {
                            OrdersTable[o[i]]++;
                        }
                        else
                        {
                            OrdersTable.Add(o[i], 1);
                        }
                    }
                    Orders.Add(o);
                    Gradient = RecalculateGradient();
                }
                public void RemoveOrder(TaskOrder o)
                {
                    for (int i = 0; i < o.Len(); i++)
                    {
                        if (OrdersTable[o[i]] == 1)
                        {
                            OrdersTable.Remove(o[i]);
                        }
                        else
                        {
                            OrdersTable[o[i]]--;
                        }
                    }
                    Orders.Remove(o);
                    Gradient = RecalculateGradient();
                }
                private double CalculateAddDelta(TaskOrder o)
                {
                    Console.WriteLine("OldGradient={0}", Gradient);
                    AddOrder(o);
                    double newProfit = RecalculateGradient();
                    Console.WriteLine("RecGradient={0}", newProfit);
                    RemoveOrder(o);
                    return newProfit - Gradient;
                }
                private double CalculateRemoveDelta(TaskOrder o)
                {
                    RemoveOrder(o);
                    double newProfit = RecalculateGradient();
                    AddOrder(o);
                    return newProfit - Gradient;
                }
            }

            public void PushTempToDatabase(MySqlConnection conn)
            {
                foreach (Template t in Templates)
                {
                    if (!t.services.Any())
                    {
                        continue;
                    }

                    MySqlCommand cmd = new MySqlCommand
                    {
                        Connection = conn
                    };
                    t.AssignName();
                    cmd.CommandText = "INSERT INTO `" + UtilConst.DataBaseName + "`.`templates` (`template_type`,`id_doctor`,`template_name`,`template_services`) VALUES ('" + t.GetTemplateType().ToString() + "'," + id.ToString() + ",'" + t.Name + "','" + t.ToString() + "');";
                    cmd.ExecuteNonQuery();
                }
            }

            public void PrintDoctorTemp()
            {
                if (!Clusters.Any())
                {
                    return;
                }
                using (FileStream fs = new FileStream("test2.txt", FileMode.Append))
                {
                    StreamWriter w = new StreamWriter(fs, Encoding.Default);
                    Console.WriteLine("===============================================================");
                    w.WriteLine("===============================================================");
                    Console.WriteLine("\n{2}\nDoctor ID = {0}  Количество шаблонов: {1}\n", id, Templates.Count, Name);
                    w.WriteLine("\n{2}\nDoctor ID = {0}  Количество шаблонов: {1}\n", id, Templates.Count, Name);
                    Console.WriteLine("\n++++++++++++++++++++++++++++++++++++++++ \n Шаблоны платного зубопротезирования:\n++++++++++++++++++++++++++++++++++++++++ ");
                    w.WriteLine("\n++++++++++++++++++++++++++++++++++++++++ \n Шаблоны платного зубопротезирования:\n++++++++++++++++++++++++++++++++++++++++ ");
                    int j = 0;
                    for (int i = 0; i < Templates.Count; i++)
                    {
                        if (Templates[i].GetTemplateType() == 0)
                        {
                            j++;
                            continue;
                        }
                        Console.WriteLine("\nШаблон {0}: ", i + 1 - j);
                        w.WriteLine("\nШаблон {0}: ", i + 1 - j);
                        int counter = 0;
                        foreach (string s in Templates[i].services)
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
                    Console.WriteLine("\n++++++++++++++++++++++++++++++++++++++++ \n Шаблоны бесплатного зубопротезирования:\n++++++++++++++++++++++++++++++++++++++++ ");
                    w.WriteLine("\n++++++++++++++++++++++++++++++++++++++++ \n Шаблоны бесплатного зубопротезирования:\n++++++++++++++++++++++++++++++++++++++++ ");
                    j = 0;
                    for (int i = 0; i < Templates.Count; i++)
                    {
                        if (Templates[i].GetTemplateType() == 1)
                        {
                            j++;
                            continue;
                        }
                        Console.WriteLine("\nШаблон {0}: ", i + 1 - j);
                        w.WriteLine("\nШаблон {0}: ", i + 1 - j);
                        int counter = 0;
                        foreach (string s in Templates[i].services)
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
                MakeClusters();
                foreach (Cluster c in Clusters)
                {
                    Templates.Add(MakeTemplateFromCluster(c));
                }
            }
            private Template MakeTemplateFromCluster(Cluster c)
            {
                SortedDictionary<string, int> table = c.GetClusterTable();
                IOrderedEnumerable<KeyValuePair<string, int>> sortedTable = from entry in table where entry.Value > c.OrdersCount() * 75 / 100 orderby entry.Value descending select entry;
                Template tmp = new Template(c.GetClusterType());
                foreach (KeyValuePair<string, int> l in sortedTable)
                {
                    tmp.Add(l.Key);
                }
                return tmp;
            }
            private void InitNameById(MySqlConnection conn, int i)
            {
                string sql = "SELECT doctor_id,user_id,id,name FROM " + UtilConst.DataBaseName + ".doctor_spec inner join users on user_id=id where doctor_id=" + i.ToString();
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
                        {
                            Name = reader.GetString(3);
                        }
                    }
                }
            }

            private void InitServisesByID(MySqlConnection conn, int id)
            {
                string sql = @"SELECT * FROM (Select ID_SERVICE,ID, ID_ORDER, tmp.ID_DOCTOR as ID_doc, code, order_type from 
(Select ID_SERVICE, ID_CASE,ID_PROFILE, ID_ORDER, orders.ID_DOCTOR, case_services.ID_DOCTOR as ID_doc, orders.order_type from case_services inner join orders on id_order=ID WHERE ID_ORDER>0 and order_type=1) as tmp 
left join price_orto_soc on tmp.ID_PROFILE=price_orto_soc.ID 
union Select ID_SERVICE,ID, ID_ORDER, tmp.ID_DOCTOR as ID_doc, code, order_type from 
(Select ID_SERVICE, ID_CASE,ID_PROFILE, ID_ORDER, orders.ID_DOCTOR, case_services.ID_DOCTOR as ID_doc, orders.order_type from case_services inner join orders on id_order=ID WHERE ID_ORDER>0 and order_type!=1) as tmp 
inner join price on tmp.ID_PROFILE=price.ID) as u
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
                                TaskOrder t = new TaskOrder(currCaseId, reader.GetString(4), reader.GetInt32(5));
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
                Orders = cases;
            }
            public int Count()
            {
                return Templates.Count;
            }
            public int CompareTo(Doctor p)
            {
                return Count().CompareTo(p.Count());
            }
            private void MakeClusters()
            {
                foreach (TaskOrder t in Orders)
                {
                    t.SortServicesById();
                    Cluster tmp = new Cluster(t.GetOrderType());
                    tmp.AddOrder(t);
                    Clusters.Add(tmp);
                    double addProfitNC = GlobalProfit();
                    Clusters.Remove(tmp);
                    tmp.RemoveOrder(t);
                    double addProfitECMax = 0;
                    int counter = 0;
                    int max = 0;
                    foreach (Cluster c in Clusters)
                    {
                        c.AddOrder(t);
                        double addProfitEC = GlobalProfit();
                        if (addProfitEC - addProfitNC > UtilConst.eps && addProfitEC - addProfitECMax > UtilConst.eps)
                        {
                            addProfitECMax = addProfitEC;
                            max = counter;
                        }
                        counter++;
                        c.RemoveOrder(t);
                    }
                    if (Clusters.Count == 0)
                    {
                        tmp.AddOrder(t);
                        Clusters.Add(tmp);
                        continue;
                    }
                    if (addProfitECMax - addProfitNC > UtilConst.eps)
                    {
                        Clusters[max].AddOrder(t);
                    }
                    else
                    {
                        tmp.AddOrder(t);
                        Clusters.Add(tmp);
                    }
                }
                for (int i = 0; i < 10; i++)
                {
                    if (!MakeIter())
                    {
                        break;
                    }
                }
                Clusters.Sort();
                Clusters.Reverse();
                int j = 0;
                for (int i = 0; i < Clusters.Count; i++)
                {
                    if (Clusters[i].OrdersCount() == 2)
                    {
                        j = i;
                        break;
                    }
                }
                Clusters.RemoveRange(j, Clusters.Count - j);
            }
            private bool MakeIter()
            {
                bool ismoved = false;
                SortedSet<long> moved = new SortedSet<long>();
                for (int i = 0; i < Clusters.Count; i++)
                {
                    for (int j = 0; j < Clusters[i].OrdersCount(); j++)
                    {
                        if (moved.Contains(Clusters[i][j].id))
                        {

                            continue;
                        }
                        double currProfit = GlobalProfit();
                        double newProfit = 0;
                        double maxProfit = 0;
                        int numberMax = -1;
                        bool findMax = false;
                        TaskOrder tmp = Clusters[i][j];
                        Clusters[i].RemoveOrder(tmp);
                        for (int k = 0; k < Clusters.Count; k++) //поиск лучшего кластера
                        {
                            if (k == i)
                            {
                                continue;
                            }

                            Clusters[k].AddOrder(tmp);
                            newProfit = GlobalProfit();
                            Clusters[k].RemoveOrder(tmp);
                            if (newProfit - currProfit > UtilConst.eps && newProfit - maxProfit > UtilConst.eps)
                            {
                                maxProfit = newProfit;
                                numberMax = k;
                                findMax = true;
                            }
                        }
                        if (findMax)
                        {
                            Clusters[numberMax].AddOrder(tmp);
                            ismoved = true;
                        }
                        else
                        {
                            Clusters[i].AddOrder(tmp);
                        }
                        moved.Add(tmp.id);
                    }
                }
                moved.Clear();
                Clusters.Sort();
                Clusters.Reverse();
                int l = -1;
                for (int i = 0; i < Clusters.Count; i++)
                {
                    if (Clusters[i].OrdersCount() == 0)
                    {
                        l = i;
                        break;
                    }
                }
                if (l > 0)
                {
                    Clusters.RemoveRange(l, Clusters.Count - l);
                }
                return ismoved;
            }
            private double GlobalProfit()
            {
                double num = 0;
                double denom = 0;
                if (Clusters.Count == 0)
                {
                    return 0;
                }
                foreach (Cluster c in Clusters)
                {
                    if (c.OrdersCount() == 0)
                    {
                        continue;
                    }
                    num += c.Gradient * c.OrdersCount();
                    denom += c.OrdersCount();
                }
                return num / denom;
            }
        }
        public void Clusterise()
        {
            conn.Open();
            try
            {
                List<Task> taskList = new List<Task>();
                foreach (Doctor i in doctors)
                {
                    Task task = Task.Factory.StartNew(() => i.MakeTemplates());
                    taskList.Add(task);
                }
                Task.WaitAll(taskList.ToArray());
                string sql = "drop table if exists templates";
                MySqlCommand cmd = new MySqlCommand
                {
                    Connection = conn,
                    CommandText = "drop table if exists templates"
                };
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
                Console.Read();
            }
            finally
            {
                conn.Close();
                conn.Dispose();
            }

        }
        private Clinic(MySqlConnection conn)//разобрать этот трешак
        {
            this.conn = conn;
            conn.Open();
            try
            {
                InitPrices(conn);
                string sql = "SELECT distinct ID_Doctor FROM " + UtilConst.DataBaseName + ".case_services WHERE ID_DOCTOR>0";
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
                foreach (int i in idDoctor)
                {
                    Doctor tmp = new Doctor(i, conn, price);
                    doctors.Add(tmp);
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
        }
        public static Clinic GetInstance(MySqlConnection conn)
        {
            if (instance == null)
            {
                instance = new Clinic(conn);
            }
            return instance;
        }
    }
}
