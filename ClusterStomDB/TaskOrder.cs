using System;
using System.Collections.Generic;

namespace ClusterStomDB
{
    internal class TaskOrder : IComparable<TaskOrder>, IEquatable<TaskOrder>
    {
        public readonly long id;
        private int type = -1;
        public TaskOrder(long i, string j, int bzp)
        {
            type = bzp;
            id = i;
            services = new List<string> { j };
        }
        public int GetOrderType() { return type; }
        private List<string> services = new List<string>();
        public void RefreshCodes()
        {

        }
        public string this[int index] => services[index];
        public void AddServiceById(string i)
        {
            if (!services.Contains(i)) services.Add(i);
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
        public int Len()
        {
            return services.Count;
        }
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
}