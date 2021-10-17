using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;

namespace ClusterSotmDB
{
    class Cluster
    {
        public double gradient { get; private set; } = 0;
        private SortedDictionary<long,int> ordersTable = new SortedDictionary<long,int>();
        private List<TaskOrder> orders = new List<TaskOrder>();
        public int Count()
        {
            return orders.Count;
        }
        public double TryAddOrder(TaskOrder o,double deltaRemove)//добавим профиль доктора и умножение на k
        {
            double oldProfit = gradient;
            double newProfit = oldProfit;
            for(int i = 0;i<o.Len();i++)
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
            newProfit = this.RecalculateProfit();
            if (newProfit - oldProfit - deltaRemove > 0.000001)
            {
                //действия с табличкой
                orders.Add(o);
                gradient = newProfit;
                return oldProfit - newProfit;
            }
            else
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
            }
            return 0;
        }
        private double RecalculateProfit()
        {
            gradient = 0;
            foreach (int t in ordersTable.Values)
            {
                double k = 1.0;//временно, потом тут будет коэффициент услуги по докторам при обучении
                gradient = gradient + k * t;
            }
            return gradient/(double)ordersTable.Count;
        }
        public double CalculateRemoveDelta(TaskOrder o)
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
            double newProfit = this.RecalculateProfit();
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
            return gradient - newProfit;
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
            gradient = RecalculateProfit();
            orders.Remove(o);
        }
    }
}