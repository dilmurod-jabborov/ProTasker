using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ProTasker.Data.IRepository;
using ProTasker.Data.Repository;
using ProTasker.Domain.Enum;
using ProTasker.Domain.Models;
using ProTasker.DTOModels.Admin;
using ProTasker.DTOModels.User;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace ProTasker.Menu
{
    internal class UserUI
    {
        WorkerService workerService = new WorkerService();
        private readonly ITelegramBotClient botClient;

        private readonly Dictionary<long, string> registrationSteps = new();
        private readonly Dictionary<long, string> updatingSteps = new();
        private readonly Dictionary<long, string> loginSteps = new();
        private readonly Dictionary<long, UserRegisterModel> registeringUsers = new();
        private readonly Dictionary<long, UserUpdateModel> updatingUsers = new();
        private readonly Dictionary<long, UserLoginModel> loginUsers = new();
        private readonly Dictionary<long, string> userSessions = new();
        private readonly IUserService userService;
        public bool IsLoggedIn(long chatId) => userSessions.ContainsKey(chatId);

        public UserUI()
        {
            botClient = new TelegramBotClient("8078697381:AAH8WOUIML8fd1AeLHOkgZeuy6uX8Qfp8PU");
            userService = new UserService();
        }
        public async Task StartAsync()
        {
            using var cts = new CancellationTokenSource();
            var cancellationToken = cts.Token;

            botClient.StartReceiving(
                HandleUpdateAsync,
                HandleErrorAsync,
                new ReceiverOptions
                {
                    AllowedUpdates = Array.Empty<UpdateType>() // Receive all update types  
                },
                cancellationToken
            );
            var me = await botClient.GetMe();

            if (me == null || string.IsNullOrEmpty(me.Username))
            {
                Console.WriteLine("❌ Bot username is not set. Please check your bot token.");
                return;
            }
            Console.WriteLine($"Bot started: {me.FirstName} (@{me.Username})");
            Console.WriteLine("Bot is running... Press Ctrl+C to exit.");

            await Task.Delay(-1);
        }
        private async Task HandleErrorAsync(ITelegramBotClient client, Exception exception, CancellationToken token)
        {
            if (exception is ApiRequestException apiRequestException)
            {
                Console.WriteLine($"Telegram API Error: {apiRequestException.ErrorCode} - {apiRequestException.Message}");
            }
            else
            {
                Console.WriteLine($"General Error: {exception.Message}");
            }
            await Task.CompletedTask;
        }
        private async Task HandleUpdateAsync(ITelegramBotClient client, Update update, CancellationToken token)
        {
            Console.WriteLine("Received update: " + update.Type);
            try
            {
                if (update.Type == UpdateType.Message && update.Message?.Text == "/start")
                {
                    await MainMenuButton(update.Message.Chat.Id, token);
                    return;
                }

                if (update.Type == UpdateType.Message)
                {
                    await MainMenu(client, update, token);
                }
                else if (update.Type == UpdateType.CallbackQuery)
                {
                    await MainMenu(client, update, token);
                }
                if(IsLoggedIn(update.Message.Chat.Id))
                {
                    if (update.CallbackQuery is not null)
                    {
                        switch (update.Message.Text)
                        {
                            case "🔍 Find Worker":
                                var findWorkersMenu = new InlineKeyboardMarkup(new[]
                                {
                                    new[] { InlineKeyboardButton.WithCallbackData("By Location", "ByLocation") },
                                    new[] { InlineKeyboardButton.WithCallbackData("By Category", "ByCategory") },
                                    new[] { InlineKeyboardButton.WithCallbackData("<= Back", "<=Back") }
                                });
                                await client.SendMessage(
                                    chatId: update.Message.Chat.Id,
                                    text: "Please choose an option to find workers:",
                                    replyMarkup: findWorkersMenu,
                                    cancellationToken: token
                                );
                                break;
                            case "⚙️Settings":
                                await Settings(client, update, token);
                                break;
                            case "<= Back":
                                await MainMenuButton(update.Message.Chat.Id, token);
                                break;
                        }
                    }
                }
                if (IsLoggedIn(update.Message.Chat.Id) && update.Type == UpdateType.CallbackQuery)
                {
                    switch (update.CallbackQuery.Data)
                    {
                        case "Settings":
                            await Settings(client, update, token);
                            break;
                        case "ChangePassword":
                            await ChangePassword(client, update, token);
                            break;
                        case "DeleteAccount":
                            await DeleteUser(client, update, token);
                            break;
                        case "Change User Info":
                            await UpdateAccount(client, update, token);
                            break;
                        default:
                            Console.WriteLine($"Unknown callback data: {update.CallbackQuery.Data}");
                            break;
                    }
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error handling update: {ex.Message}");
            }
        }
        private async Task MainMenu(ITelegramBotClient client, Update update, CancellationToken ct)
        {
            if (update.Type != UpdateType.Message && update.Type != UpdateType.CallbackQuery)
            {
                return; 
            }
            await RegisterMain(botClient, update, ct);

            if (update.Message is { Text: var userInput } message)
            {
                if (userInput == "0")
                {
                    await MainMenuButton(update.Message.Chat.Id, ct);
                    return;
                }
            }
        }
        private async Task MainMenuButton(long id, CancellationToken ct)
        {
            var menu = new InlineKeyboardMarkup(new[]
            {
                   new[]
                   {
                       InlineKeyboardButton.WithCallbackData("Register", "Register"),
                       InlineKeyboardButton.WithCallbackData("Login", "Login"),
                   },
                   new[]
                   {
                       InlineKeyboardButton.WithCallbackData("0", "0")
                   }
               });
            try
            {
                await botClient.SendMessage(
                    chatId: id,
                    text: "Welcome to ProTasker! Please choose an option:",
                    replyMarkup: menu,
                    cancellationToken: ct
                );
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error sending main menu: {ex.Message}");
            }
        }
        private async Task RegisterMain(ITelegramBotClient botClient, Update update, CancellationToken ct)
        {
            try
            {
                if (update.CallbackQuery is not null)
                {
                    string data = update.CallbackQuery.Data;
                    long chatId = update.Message?.Chat.Id ?? update.CallbackQuery?.Message?.Chat.Id ?? 0;
                    if (data == "Register")
                    {
                        registrationSteps[chatId] = "firstname";
                        registeringUsers[chatId] = new UserRegisterModel();
                        await botClient.SendMessage(
                            chatId: chatId,
                            text: "👤 Enter First name...",
                            cancellationToken: ct
                        );
                    }
                    else if (data == "update")
                    {
                        updatingSteps[chatId] = "firstname";
                        updatingUsers[chatId] = new UserUpdateModel();
                        await botClient.SendMessage(
                            chatId: chatId,
                            text: "👤 Enter First name...",
                            cancellationToken: ct
                        );
                    }
                    else if (data == "delete")
                    {
                        await DeleteUser(botClient, update, ct);
                    }
                    else if (data == "Change_password")
                    {
                        await ChangePassword(botClient, update, ct);
                    }
                    else if (data == "Login")
                    {
                        loginSteps[chatId] = "login";
                        loginUsers[chatId] = new UserLoginModel();

                        await botClient.SendMessage(
                            chatId: chatId,
                            text: "📞 Enter your phone number:",
                            cancellationToken: ct
                        );
                        return;
                    }
                    else if (data == "0")
                    {
                        await MainMenuButton(chatId, ct);
                    }
                }
                if (update.Type == UpdateType.Message &&
                    update.Message?.Text is not null ||
                    update.Message.Contact is not null)
                {
                    long chatId = update.Message.Chat.Id;
                    if(loginSteps.ContainsKey(chatId))
                    {
                        await Login(update, ct);
                        return;
                    }
                    await Register(update, ct);
                    return;
                }
                await MainMenuButton(update.Message.Chat.Id, ct);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in RegisterMain: {ex.Message}");
            }
        }
        private async Task UpdateAccount(ITelegramBotClient botClient, Update update, CancellationToken ct)
        {
            try
            {
                if (update.CallbackQuery is not null)
                {
                    string data = update.CallbackQuery.Data;
                    long chatId = update.Message?.Chat.Id ?? update.CallbackQuery?.Message?.Chat.Id ?? 0;
                    if (data == "Update")
                    {
                        updatingSteps[chatId] = "firstname";
                        updatingUsers[chatId] = new UserUpdateModel();
                        await botClient.SendMessage(
                            chatId: chatId,
                            text: "👤 Enter First name...",
                            cancellationToken: ct
                        );
                    }
                    return;
                }

                if (update.Message is { Text: var userInput } message)
                {
                    var chatId = message.Chat.Id;

                    if (userInput == "/start")
                    {
                        await MainMenuButton(update.Message.Chat.Id, ct);
                        return;
                    }

                    if (!registrationSteps.TryGetValue(chatId, out var step))
                        return;

                    if (step == "firstname")
                    {
                        updatingUsers[chatId].FirstName = userInput;
                        updatingSteps[chatId] = "lastname";

                        await botClient.SendMessage(chatId, "👤 Enter last name...", cancellationToken: ct);
                    }
                    else if (step == "lastname")
                    {
                        updatingUsers[chatId].LastName = userInput;
                        updatingSteps[chatId] = "age";

                        await botClient.SendMessage(chatId, "👤 Enter your age...", cancellationToken: ct);
                    }
                    else if (step == "age")
                    {
                        updatingUsers[chatId].Age = int.Parse(userInput);
                    }
                    else
                    {
                        await botClient.SendMessage(chatId, "❌ Invalid step. Please try again.", cancellationToken: ct);
                        return;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
        private async Task DeleteUser(ITelegramBotClient botClient, Update update, CancellationToken ct)
        {
            string data = update.CallbackQuery.Data;
            long chatId = update.Message?.Chat.Id ?? update.CallbackQuery?.Message?.Chat.Id ?? 0;
            if (data == "Delete")
            {
                try
                {
                    userService.DeleteUser((int)chatId);

                    await botClient.SendMessage(
                        chatId: chatId,
                        text: "✅ User deleted successfully.",
                        cancellationToken: ct
                    );
                    registrationSteps.Remove(chatId);
                    registeringUsers.Remove(chatId);
                }
                catch (Exception ex)
                {
                    await botClient.SendMessage(
                        chatId: chatId,
                        text: $"❌ Error deleting user: {ex.Message}",
                        cancellationToken: ct
                    );
                }
            }
        }
        private async Task PhoneButton(long chatId, Contact contact, CancellationToken ct)
        {
            if (registrationSteps.TryGetValue(chatId, out var step) && step == "phone")
            {
                var model = registeringUsers[chatId];

                if (model == null)
                {
                    model = new UserRegisterModel();
                    registeringUsers[chatId] = model;
                }

                model.PhoneNumber = contact.PhoneNumber;

                registrationSteps[chatId] = "password";

                await botClient.SendMessage(
                    chatId: chatId,
                    text: "🔐 Enter password...:",
                    cancellationToken: ct
                );
            }
        }
        private async Task Register(Update update, CancellationToken ct)
        {

            if (update.Message is { Text: var userInput } message)
            {
                var chatId = message.Chat.Id;

                if (userInput == "/start")
                {
                    await MainMenuButton(update.Message.Chat.Id, ct);
                    return;
                }

                if (!registrationSteps.TryGetValue(chatId, out var step))
                    return;

                if (step == "firstname")
                {
                    registeringUsers[chatId].FirstName = userInput;
                    registrationSteps[chatId] = "lastname";

                    await botClient.SendMessage(chatId, "👤 Enter last name...", cancellationToken: ct);
                    return;
                }
                else if (step == "lastname")
                {
                    registeringUsers[chatId].LastName = userInput;
                    registrationSteps[chatId] = "age";

                    await botClient.SendMessage(chatId, "👤 Enter your age...", cancellationToken: ct);
                    return;
                }
                else if (step == "age")
                {
                    registeringUsers[chatId].Age = int.Parse(userInput);
                    registrationSteps[chatId] = "phone";

                    var contactKeyboard =
                        new ReplyKeyboardMarkup(new[]
                        {
                                  KeyboardButton.WithRequestContact("📱 Send your phone number")
                        })
                        {
                            ResizeKeyboard = true,
                            OneTimeKeyboard = true
                        };

                    await botClient.SendMessage(chatId, "📞 Click button to send phone number:", replyMarkup: contactKeyboard, cancellationToken: ct);
                    return;
                }
                else if (step == "password")
                {
                    var model = registeringUsers[chatId];
                    model.Password = userInput;
                    model.Role = Domain.Enum.Role.User;

                    try
                    {
                        userService.Register(model);

                        await botClient.SendMessage(chatId, "✅ Registered!", cancellationToken: ct);
                        await botClient.SendMessage(
                            chatId: chatId,
                            text: "Welcome to ProTasker! You can now use the bot.",
                            cancellationToken: ct
                        );

                    }
                    catch (Exception ex)
                    {
                        await botClient.SendMessage(chatId, $"❌ Error: {ex.Message}", cancellationToken: ct);
                    }

                    registrationSteps.Remove(chatId);
                    registeringUsers.Remove(chatId);
                }
            }
            if (update.Message?.Contact is { } contact)
            {
                await PhoneButton(update.Message.Chat.Id, contact, ct);
                return;
            }
        }
        private async Task ChangePassword(ITelegramBotClient botClient, Update update, CancellationToken ct)
        {
            if (update.CallbackQuery is not null)
            {
                string data = update.CallbackQuery.Data;
                long chatId = update.Message?.Chat.Id ?? update.CallbackQuery?.Message?.Chat.Id ?? 0;
                if (data == "Change_password")
                {
                    registrationSteps[chatId] = "old_password";
                    await botClient.SendMessage(
                        chatId: chatId,
                        text: "🔐 Enter your old password:",
                        cancellationToken: ct
                    );
                }
                return;
            }
            if (update.Message is { Text: var userInput })
            {
                var chatId = update.Message.Chat.Id;
                if (userInput == "/start")
                {
                    await MainMenuButton(update.Message.Chat.Id, ct);
                    return;
                }
                if (registrationSteps.TryGetValue(chatId, out var step) && step == "old_password")
                {
                    registrationSteps[chatId] = "new_password";
                    await botClient.SendMessage(chatId, "🔐 Enter your new password:", cancellationToken: ct);
                    return;
                }
            }
            if (update.Message is { Text: var newPasswordInput })
            {
                var chatId = update.Message.Chat.Id;
                if (newPasswordInput == "/start")
                {
                    await MainMenuButton(update.Message.Chat.Id, ct);
                    return;
                }
                if (registrationSteps.TryGetValue(chatId, out var step) && step == "new_password")
                {
                    try
                    {
                        userService.ChangeUserPassword(update.Message.Text, newPasswordInput, update.Message.Text);
                        await botClient.SendMessage(chatId, "✅ Password changed successfully!", cancellationToken: ct);
                    }
                    catch (Exception ex)
                    {
                        await botClient.SendMessage(chatId, $"❌ Error changing password: {ex.Message}", cancellationToken: ct);
                    }
                    registrationSteps.Remove(chatId);
                    registeringUsers.Remove(chatId);
                }
            }
        }
        private async Task Login(Update update, CancellationToken ct)
        {
            if (update.Message is { Text: var userInput } message)
            {
                var chatId = message.Chat.Id;
                if (userInput == "/start")
                {
                    await MainMenuButton(update.Message.Chat.Id, ct);
                    return;
                }
                if (!loginSteps.TryGetValue(chatId, out var step))
                    return;
                if (step == "login")
                {
                    loginUsers[chatId].PhoneNumber = userInput;
                    loginSteps[chatId] = "password";
                    await botClient.SendMessage(chatId, "🔐 Enter your password:", cancellationToken: ct);
                    return;
                }
                else if (step == "password")
                {
                    loginUsers[chatId].Password = userInput;
                    try
                    {
                        var user = userService.Login(loginUsers[chatId].PhoneNumber, loginUsers[chatId].Password);

                        userSessions.Add(chatId, user.PhoneNumber);

                        var userMenu = new InlineKeyboardMarkup(new[]
                        {
                            new[]
                            {
                                InlineKeyboardButton.WithCallbackData("🔍 Find Worker", "🔍 Find Worker"),
                                InlineKeyboardButton.WithCallbackData("⚙️Settings", "⚙️Settings")
                            },
                            new[]
                            {
                                InlineKeyboardButton.WithCallbackData("0", "0")

                            }
                        });
                        await botClient.SendMessage(
                            chatId: chatId,
                            text: $"Welcome, {user.FirstName} {user.LastName}! You are logged in!.",
                            replyMarkup: userMenu,
                            cancellationToken: ct
                        );

                    }
                    catch (Exception ex)
                    {
                        await botClient.SendMessage(chatId, $"❌ Error: {ex.Message}", cancellationToken: ct);
                    }
                }
            }
        }
        private async Task Settings(ITelegramBotClient botClient, Update update, CancellationToken ct)
        {
            if (update.CallbackQuery is not null)
            {
                string data = update.CallbackQuery.Data;
                long chatId = update.Message?.Chat.Id ?? update.CallbackQuery?.Message?.Chat.Id ?? 0;
                if (data == "Settings")
                {
                    var menu = new InlineKeyboardMarkup(new[]
                    {
                        new[]
                        {
                            InlineKeyboardButton.WithCallbackData("Change Password", "ChangePassword"),
                            InlineKeyboardButton.WithCallbackData("Delete Account", "DeleteAccount")
                        },
                        new[]
                        {
                            InlineKeyboardButton.WithCallbackData("Change User Info","Change User Info"),
                            InlineKeyboardButton.WithCallbackData("0", "0")
                        }
                    });
                    await botClient.SendMessage(
                        chatId: chatId,
                        text: "Please choose an option:",
                        replyMarkup: menu,
                        cancellationToken: ct
                    );
                }
            }
        }
    }
}
