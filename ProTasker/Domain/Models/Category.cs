using ProTasker.Constants;
using ProTasker.Helpers;

namespace ProTasker.Domain.Models;

public class Category
{
    public Category()
    {
        Id = GeneratorHelper.GenerateId(PathHolder.CategoryFilePath);
    }

    public int Id { get; set; }
    public string Name { get; set; } // Elektrchi, Santexnik, Oshxona, Plitkachi

    public override string ToString()
    {
        return $"{Id},{Name}";
    }
}
