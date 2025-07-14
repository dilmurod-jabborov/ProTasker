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
        private static readonly string baseRoot =
            Path.Combine(AppContext.BaseDirectory, "..", "..", "..");

        public static readonly string UsersFilePath =
            Path.Combine(baseRoot, "Data", "Database", "users.txt");

        public static readonly string WorkersFilePath =
            Path.Combine(baseRoot, "Data", "Database", "workers.txt");

        public static readonly string AdminsFilePath =
            Path.Combine(baseRoot, "Data", "Database", "admins.txt");
    }
}

