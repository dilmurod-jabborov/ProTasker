using ProTasker.Data.Repository;
using ProTasker.Domain.Models;
using ProTasker.Menu;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace ProTasker;

public class Program
{
    public static async Task Main(string[] args)
    { 
        AdminUI admin = new AdminUI();

        await admin.StartAsync();

        UserUI user = new UserUI();

        await user.StartAsync();
    }
}