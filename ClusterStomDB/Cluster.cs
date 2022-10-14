using System;
using System.Collections.Generic;

namespace ClusterStomDB
{
    internal class Cluster : IComparable<Cluster>
    {
        public double Gradient { get; private set; } = 0;
        private SortedDictionary<string, int> OrdersTable = new SortedDictionary<string, int>();
        private List<TaskOrder> Orders = new List<TaskOrder>();
        private int type = -1;
        private static double r = 0.1;
        public Cluster(int bzp)
        {
            type = bzp;
        }
        public int GetClusterType()
        {
            return type;
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
            if (Orders.Count == 0) return 0;
            foreach (int t in OrdersTable.Values)
            {
                double k = 1.0;//временно, потом тут будет коэффициент услуги по докторам при обучении алгоритма
                Gradient += k * t;
            }
            Gradient = Gradient / (double)OrdersTable.Count / (double)OrdersTable.Count;
            return Gradient / (double)OrdersTable.Count / Math.Pow((double)OrdersTable.Count, r);
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
        public double CalculateAddDelta(TaskOrder o)
        {
            Console.WriteLine("OldGradient={0}", Gradient);
            AddOrder(o);
            double newProfit = RecalculateGradient();
            Console.WriteLine("RecGradient={0}", newProfit);
            RemoveOrder(o);
            return newProfit - Gradient;
        }
        public double CalculateRemoveDelta(TaskOrder o)
        {
            RemoveOrder(o);
            double newProfit = RecalculateGradient();
            AddOrder(o);
            return newProfit - Gradient;
        }
    }
}