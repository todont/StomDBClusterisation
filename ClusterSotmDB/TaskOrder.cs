using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;

namespace ClusterSotmDB
{
    class TaskOrder
    {
        public readonly long id;
        public TaskOrder(long i, long j)
        {
            id = i;
            services = new SortedSet<long> { j };
        }
        private SortedSet<long> services = new SortedSet<long>();
        public void AddServiceById(long i)
        {
            services.Add(i);
        }
        public bool GetServiceById(long i)
        {
            return services.Contains(i);
        }

    }
}