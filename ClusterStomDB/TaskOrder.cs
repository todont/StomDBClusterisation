using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;

namespace ClusterStomDB
{
    class TaskOrder : IComparable<TaskOrder>, IEquatable<TaskOrder>
    {
        public readonly long id;
        private int type = -1;
        public TaskOrder(long i, long j, int bzp)
        {
            this.type = bzp;
            id = i;
            services = new List<long> { j };
        }
        public int GetOrderType() { return type; }
        private List<long> services = new List<long>();
        public long this[int index]
        {
            get
            {
                return services[index];
            }
        }
        public void AddServiceById(long i)
        {   
            if(!services.Contains(i))
            services.Add(i);
        }
        public int CompareTo(TaskOrder p)
        {
            return this.id.CompareTo(p.id);
        }
        public bool Equals(TaskOrder other)
        {
            if (other == null)
                return false;

            if (this.id == other.id)
                return true;
            else
                return false;
        }
        public void SortServicesById()
        {
            services.Sort();
        }
        public int Len()
        {
            return services.Count;
        }
        public void Print()
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.Write("{");
            for(int i = 0; i<services.Count-1;i++)
            {   
                Console.Write("{0}, ",services[i]);
            }
            Console.Write("{0}", services[services.Count - 1]);
            Console.Write("} ");
            Console.ResetColor();
            Console.WriteLine("id = {0}", id);
        }
    }
}