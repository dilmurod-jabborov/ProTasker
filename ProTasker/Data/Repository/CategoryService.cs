using System.Xml.Linq;
using ProTasker.Constants;
using ProTasker.Data.IRepository;
using ProTasker.Domain.Extension;
using ProTasker.Domain.Models;
using ProTasker.Helpers;

namespace ProTasker.Data.Repository;

public class CategoryService : ICategoryService
{
    public void Create(string name)
    {
        var text = FileHelper.ReadFromFile(PathHolder.CategoryFilePath);

        var categories = text.ToCategory();

        var existCategory = categories.Find(x => x.Name == name);
        if (existCategory != null)
            throw new Exception($"this is already exists {name}");

        categories.Add(new Category()
        {
            Name = name
        });

        FileHelper.WriteToFile(PathHolder.CategoryFilePath, categories.ConvertToString<Category>());
    }

    public void Update(int id, string name)
    {
        var text = FileHelper.ReadFromFile(PathHolder.CategoryFilePath);

        var categories = text.ToCategory();

        var existCategory = categories.Find(x => x.Id == id)
            ?? throw new Exception("That kind of category does not exist");

        foreach (var category in categories)
        {
            if (category.Name == name)
            throw new Exception("This is already exists");
        }

        existCategory.Name = name;

        FileHelper.WriteToFile(PathHolder.CategoryFilePath, categories.ConvertToString<Category>());
    }

    public string Get(int id)
    {
        var text = FileHelper.ReadFromFile(PathHolder.CategoryFilePath);

        var categories = text.ToCategory();

        var existCategory = categories.Find(x => x.Id == id)
             ?? throw new Exception($"that kind of category does not exist");

        return existCategory.Name;
    }

    public void Delete(int id)
    {
        var text = FileHelper.ReadFromFile(PathHolder.CategoryFilePath);

        var categories = text.ToCategory();

        var existCategory = categories.Find(x => x.Id == id)
            ?? throw new Exception($"this does not exist yet");

        categories.Remove(existCategory);

        FileHelper.WriteToFile(PathHolder.CategoryFilePath, categories.ConvertToString<Category>());
    }

    public List<Category> GetAll()
    {
        var text = FileHelper.ReadFromFile(PathHolder.CategoryFilePath);

        var categories = text.ToCategory();

        return categories;
    }

}

