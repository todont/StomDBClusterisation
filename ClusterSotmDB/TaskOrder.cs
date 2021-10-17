using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;

namespace ClusterSotmDB
{
    class TaskOrder : IComparable<TaskOrder>
    {
        public readonly long id;
        public TaskOrder(long i, long j)
        {
            id = i;
            services = new List<long> { j };
        }
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
            services.Add(i);
        }
        public int CompareTo(TaskOrder p)
        {
            return this.id.CompareTo(p.id);
        }
        public void SortServicesById()
        {
            services.Sort();
        }
        public int Len()
        {
            return services.Count;
        }
    }
}