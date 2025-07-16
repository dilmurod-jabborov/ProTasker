using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ProTasker.Domain.Enum;
using ProTasker.Domain.Models;

namespace ProTasker.Domain.Extension;

public static class TextToListExtension
{
    public static List<Admin> ToAdmins(this string text)
    {
        List<Admin> admins = new List<Admin>();

        string[] lines = text.Split('\n');

        foreach (string line in lines)
        {
            if (string.IsNullOrWhiteSpace(line)) continue;

            string[] parts = line.Split(',');

            admins.Add(new Admin
            {
                Id = int.Parse(parts[0]),
                FirstName = (parts[1]),
                LastName = (parts[2]),
                PhoneNumber = (parts[3]),
                Password = (parts[4]),
                Role = System.Enum.Parse<Role>(parts[5]),
                Age = int.Parse(parts[6])
            });
        }

        return admins;
    }

    public static List<Worker> ToWorkers(this string text)
    {
        List<Worker> workers = new List<Worker>();

        string[] lines = text.Split('\n');

        foreach (string line in lines)
        {
            if (string.IsNullOrWhiteSpace(line)) continue;

            string[] parts = line.Split(',');

            var locParts = parts[10].Split('|', StringSplitOptions.RemoveEmptyEntries);

            var location = new Location()
            {
                Region = System.Enum.Parse<Region>(locParts[0]),
                District = locParts[1],
                Street = locParts[2]
            };

            workers.Add(new Worker
            {
                Id = int.Parse(parts[0]),
                FirstName = parts[1],
                LastName = parts[2],
                PhoneNumber = parts[3],
                Password = parts[4],
                Role = System.Enum.Parse<Role>(parts[5]),
                Age = int.Parse(parts[6]),
                Bio = parts[7],
                CategoryId = parts[8]
                           .Split(';', StringSplitOptions.RemoveEmptyEntries)
                           .Select(int.Parse).ToList(),
                Gender = System.Enum.Parse<Gender>(parts[9]),
                Location = location
            });
        }
        return workers;
    }

    public static List<User> ToUser(this string text)
    {
        List<User> users = new();

        string[] lines = text.Split('\n');

        foreach (string line in lines)
        {
            if (string.IsNullOrWhiteSpace(line)) continue;

            string[] parts = line.Split(',');

            users.Add(new User
            {
                Id = int.Parse(parts[0]),
                FirstName = parts[1],
                LastName = parts[2],
                PhoneNumber = parts[3],
                Password = parts[4],
                Role = System.Enum.Parse<Role>(parts[5]),
                Age = int.Parse(parts[6]),
                Gender = System.Enum.Parse<Gender>(parts [7])
            });
        }
    
        return users;
    }

    public static List<Category> ToCategory(this string text)
    {
        List<Category> categories= new();

        string[] lines = text.Split('\n');

        foreach (string line in lines)
        {
            if (string.IsNullOrWhiteSpace(line)) continue;

            string[] parts = line.Split(',');

            categories.Add(new Category
            {
                Id = int.Parse(parts[0]),
                Name = parts[1],
            });
        }

        return categories;
    }
}


