using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProTasker.Helpers
{
    public class FileHelper
    {
        public static string ReadFromFile(string filePath)
        {
            if (!File.Exists(filePath))
            {
                File.Create(filePath).Close(); // File bo'lmasa yangi fayl yaratadi
            }

            return File.ReadAllText(filePath);
        }

        public static void WriteToFile(string filePath, List<string> content)
        {
            File.WriteAllLines(filePath, content);
        }
    }
}
