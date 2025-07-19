using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ProTasker.Domain.Extension;
using ProTasker.Domain.Models;
using ProTasker.Menu.AdminUIFolder;
using ProTasker.Menu.UserUIFolder;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using static System.Collections.Specialized.BitVector32;

namespace ProTasker.Menu.MAIN
{
    public class MainMenu
    {
        private UserUI user;
        private WorkerUI worker;
        private AdminUI admin;

        public MainMenu()
        {
            user = new UserUI("8078697381:AAH8WOUIML8fd1AeLHOkgZeuy6uX8Qfp8PU");
            admin = new AdminUI("7806562984:AAH1bYTCWl3a3WMj9Wbru71CkevZJ8KyVuk");
            worker = new WorkerUI("7906630577:AAF5n3GNVwFkBl6i1jRHEZzDxf_uLHO3xLI");
        }

        public async Task Menu()
        {
            await Task.WhenAll(
                admin.StartAsync(),
                user.StartAsync(),
                worker.StartAsync()
            );
        }
    }
}
