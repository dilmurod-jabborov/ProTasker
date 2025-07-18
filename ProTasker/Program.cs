using ProTasker.Data.Repository;
using ProTasker.Domain.Models;
using ProTasker.Menu;
using ProTasker.Menu.AdminUIFolder;
using ProTasker.Menu.UserUIFolder;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace ProTasker;

public class Program
{
    public static async Task Main(string[] args)
    {
        //AdminUI admin = new AdminUI();

        //await admin.StartAsync();

        UserUI user = new UserUI("8078697381:AAH8WOUIML8fd1AeLHOkgZeuy6uX8Qfp8PU");

        await user.StartAsync();
    }
}