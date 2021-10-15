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
        public readonly uint id;
        public TaskOrder(uint i)
        {
            id = i;
        }
        private SortedSet<uint> services;
        public void AddServiceById(uint i)
        {
            services.Add(i);
        }
        public bool GetServiceById(uint i)
        {
            return services.Contains(i);
        }

    }
}