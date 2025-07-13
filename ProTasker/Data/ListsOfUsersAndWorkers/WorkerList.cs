using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ProTasker.Constants;
using ProTasker.Domain.Enum;
using ProTasker.Domain.Models;

namespace ProTasker.Data.ListsOfUsersAndWorkers
{
    internal class WorkerList
    {
        public List<string> Workers = File.ReadAllLines(PathHolder.WorkersFilePath).ToList();
        private int workerId = 1;
        public void AddWorker(string name, string bio, Category category, Gender gender, string Rating, Location location)
        {
            foreach (var Worker in Workers)
            {
                var workerDetails = Worker.Split('\n');
                foreach (var detail in workerDetails)
                {
                    var details = detail.Split(',');
                    if (details[1] == name)
                    {
                        Console.WriteLine($"Worker {name} already exists.");
                        return;
                    }
                    else
                    {
                        Workers.Add($"{workerId},{name},{bio},{category},{gender},{Rating},{location}\n");
                        Console.WriteLine($"Worker {name} has been added successfully.");
                        workerId++;
                        return;
                    }
                }
            }
        }
        public bool IsWorkerExists(string worker)
        {
            return Workers.Contains(worker);
        }
    }
}
