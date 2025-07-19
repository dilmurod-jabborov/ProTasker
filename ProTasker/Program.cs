using ProTasker.Data.Repository;
using ProTasker.Domain.Enum;
using ProTasker.Domain.Models;
using ProTasker.Menu;
using ProTasker.Menu.AdminUIFolder;
using ProTasker.Menu.MAIN;
using ProTasker.Menu.UserUIFolder;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace ProTasker;

public class Program
{
    public static async Task Main(string[] args)
    {
        var admin = new AdminUI("7806562984:AAH1bYTCWl3a3WMj9Wbru71CkevZJ8KyVuk");

        var UserWorker = new MainMenu("8078697381:AAH8WOUIML8fd1AeLHOkgZeuy6uX8Qfp8PU");

        await Task.WhenAll(admin.StartAsync(), UserWorker.StartAsync());
    }
}