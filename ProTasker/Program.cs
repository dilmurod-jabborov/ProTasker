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
        MainMenu mainMenu = new MainMenu();

        await mainMenu.Menu();
    }
}