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
        private IDictionary<long, MainSession> sessions;
        private readonly string token;
        private UserUI user;
        private WorkerUI worker;
        private ITelegramBotClient botClient;

        public MainMenu(string token)
        {
            this.token = token;
            botClient = new TelegramBotClient(token);
            sessions = new Dictionary<long, MainSession>();
            user = new UserUI(botClient,sessions);
            worker = new WorkerUI(botClient, sessions);
        }

        public async Task StartAsync()
        {
            using var cts = new CancellationTokenSource();

            botClient.StartReceiving(
                updateHandler: HandleUpdateAsync,
                HandleErrorAsync,
                receiverOptions: new ReceiverOptions
                {
                    AllowedUpdates = new[] { UpdateType.Message, UpdateType.CallbackQuery}
                },
                cancellationToken: cts.Token
            );

            var me = await botClient.GetMe();
            Console.WriteLine($"🤖 Bot @{me.Username} ishga tushdi");

            await Task.Delay(-1);
        }

        private async Task HandleUpdateAsync(ITelegramBotClient bot, Update update, CancellationToken ct)
        {
            long chatId = update.Message?.Chat.Id ?? update.CallbackQuery?.Message.Chat.Id ?? 0;

            if (!sessions.ContainsKey(chatId))
                sessions[chatId] = new MainSession();

            var session = sessions[chatId];

            if (update.Message?.Text == "/start")
            {
                session.CurrentStep = null;

                InlineKeyboardMarkup keyboard = new(new[]
                {
            new[]
            {
                InlineKeyboardButton.WithCallbackData("👤 User", "select_role:user"),
                InlineKeyboardButton.WithCallbackData("👷 Worker", "select_role:worker")
            }
        });

                await botClient.SendMessage(chatId, "Please choose the role!", replyMarkup: keyboard, cancellationToken: ct);
                return;
            }

            if (update.CallbackQuery?.Data?.StartsWith("select_role:") == true)
            {
                string role = update.CallbackQuery.Data.Split(":")[1];
                session.Mode = role;
                session.CurrentStep = "menu";

                await botClient.EditMessageReplyMarkup(
                    chatId: chatId,
                    messageId: update.CallbackQuery.Message.MessageId,
                    replyMarkup: null,
                    cancellationToken: ct
                );

                await botClient.SendMessage(chatId, $"✅ You chose: {role.ToUpper()}", cancellationToken: ct);

                if (role == "user")
                    await user.MainMenu(botClient, update, ct);

                else if (role == "worker")
                    await worker.MainMenu(botClient, update, ct);

                return;
            }

            if (session.Mode == "user")
            {
                await user.MainMenu(botClient, update, ct);
            }
            else if (session.Mode == "worker")
            {
                await worker.MainMenu(botClient, update, ct);
            }
        }


        private Task HandleErrorAsync(ITelegramBotClient bot, Exception exception, CancellationToken ct)
        {
            Console.WriteLine($"❌ Xatolik: {exception.Message}");
            return Task.CompletedTask;
        }
    }
}
