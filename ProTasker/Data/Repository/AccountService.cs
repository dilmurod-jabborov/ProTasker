using ProTasker.Domain.Extension;
using ProTasker.Domain.Models;
using ProTasker.Helpers;

namespace ProTasker.Data.Repository;

public class AccountService<T> where T : Account, new() 
{
    private readonly string filePath;

    public AccountService(string filePath)
    {
        this.filePath = filePath;
    }

    public void Register(T account)
    {
        var text = FileHelper.ReadFromFile(filePath);

        var accounts = text.TextToObjectList<T>();

        var existaccount = accounts.Find(a => a.PhoneNumber == account.PhoneNumber);

        if(existaccount!=null) 
            throw new Exception("Bu raqam bilan akkaunt mavjud!");

        var newObj=account.ToNewObjDest<T>(); 
        
        accounts.Add(newObj);

        var newText = accounts.ToTextWriteLines<T>();

        FileHelper.WriteToFile(filePath, newText);
    }

    public T? Login(string phone, string password)
    {
        var text = FileHelper.ReadFromFile(filePath);

        var accounts = text.TextToObjectList<T>();

        var existsAccount = accounts.Find(a => a.PhoneNumber == phone)
            ?? throw new Exception("Login or password is wrong!");

        return existsAccount;
    }

    // Fayldan barcha accountlarni o'qish
    private List<T> LoadAccounts()
    {
        if (!File.Exists(filePath)) return new List<T>();

        string text = File.ReadAllText(filePath);
        return text.ToObjectList<T>(); // Reflection orqali
    }
}
