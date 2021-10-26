using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;

namespace ClusterStomDB
{
    class Cluster : IComparable<Cluster>
    {
        public double gradient { get; private set; } = 0;
        private SortedDictionary<long,int> ordersTable = new SortedDictionary<long,int>();
        private List<TaskOrder> orders = new List<TaskOrder>();
        private static double r = 0.2;
        public void SetR(double i)
        {
            r = i;
        }
        public SortedDictionary<long, int> GetClusterTable()
        {
            return ordersTable;
        }
        public int OrdersCount()
        {
            return orders.Count;
        }
        public int CompareTo(Cluster p)
        {
            return this.OrdersCount().CompareTo(p.OrdersCount());
        }
        public void Print()
        {
            orders.Sort();

            Console.WriteLine("==================================");
            foreach(TaskOrder o in orders)
            {
                o.Print();
            }
            //Console.ForegroundColor = ConsoleColor.DarkCyan;
            Console.WriteLine("==================================\n");

        }
        public TaskOrder this[int index]
        {
            get
            {
                return orders[index];
            }
        }
        private double RecalculateGradient()
        {
             
            gradient = 0;
            if (orders.Count == 0) return 0;
            foreach (int t in ordersTable.Values)
            {
                double k = 1.0;//временно, потом тут будет коэффициент услуги по докторам при обучении алгоритма
                gradient = gradient + k * t;
            }
            gradient = gradient / (double)ordersTable.Count / (double)ordersTable.Count;
            return gradient / (double)ordersTable.Count / Math.Pow((double)ordersTable.Count,r);
        }
        public void AddOrder(TaskOrder o)
        {
            for (int i = 0; i < o.Len(); i++)
            {
                if (ordersTable.ContainsKey(o[i]))
                {
                    ordersTable[o[i]]++;
                }
                else
                {
                    ordersTable.Add(o[i], 1);
                }
            }
            orders.Add(o);
            gradient = RecalculateGradient();
        }
        public void RemoveOrder(TaskOrder o)
        {
            for (int i = 0; i < o.Len(); i++)
            {
                if (ordersTable[o[i]] == 1)
                {
                    ordersTable.Remove(o[i]);
                }
                else
                {
                    ordersTable[o[i]]--;
                }
            }
            orders.Remove(o);
            gradient = RecalculateGradient(); 
        }
        public double CalculateAddDelta(TaskOrder o)
        {
            Console.WriteLine("OldGradient={0}", gradient);
            this.AddOrder(o);
            double newProfit = this.RecalculateGradient();
            Console.WriteLine("RecGradient={0}", newProfit);
            this.RemoveOrder(o);
            return newProfit - gradient;
        }
        public double CalculateRemoveDelta(TaskOrder o)
        {
            this.RemoveOrder(o);
            double newProfit = this.RecalculateGradient();
            this.AddOrder(o);
            return newProfit - gradient;
        }
    }
}