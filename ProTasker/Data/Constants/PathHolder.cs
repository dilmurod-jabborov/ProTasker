using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ProTasker.Domain.Models;

namespace ProTasker.Constants
{
    public static class PathHolder
    {
        private static readonly string parentRoot = Directory.GetParent(Environment.CurrentDirectory).Parent.Parent.FullName;

        public static readonly string UsersFilePath = Path.Combine(parentRoot, "Data", "users.txt");
        public static readonly string WorkersFilePath = Path.Combine(parentRoot, "Data", "workers.txt");
        public static readonly string AdminsFilePath = Path.Combine(parentRoot, "Data", "admins.txt");
    }

}

