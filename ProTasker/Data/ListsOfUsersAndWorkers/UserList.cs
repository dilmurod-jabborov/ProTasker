using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ProTasker.Constants;
using ProTasker.Domain.Enum;

namespace ProTasker.Data.UserList
{
    internal class UserList
    {

        public List<string> Users = File.ReadAllLines(PathHolder.UsersFilePath).ToList();
        private int userId = 1;
        public void AddUser(string username, string phonenumber, string password, Role role, int age)
        {
            foreach (var user in Users)
            {
                var userDetails = user.Split('\n');
                foreach (var detail in userDetails)
                {
                    var details = detail.Split(',');
                    if (details[1] == username)
                    {
                        Console.WriteLine($"User {username} already exists.");
                        return;
                    }
                    else
                    {
                        Users.Add($"{userId},{username},{phonenumber},{password},{role},{age}");
                        Console.WriteLine($"User {username} has been added successfully.");
                        userId++;
                        return;
                    }
                }
            }
        }
        public bool IsUserExists(string user)
        {
            return Users.Contains(user);
        }
    }
}
