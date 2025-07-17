using ProTasker.Data.Repository;
using ProTasker.Domain.Models;
using ProTasker.Menu;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace ProTasker;

public class Program
{
    public static void Main(string[] args)
    {
        AdminUI admin = new AdminUI();

        admin.StartAsync();

        Console.ReadKey();

    }
}