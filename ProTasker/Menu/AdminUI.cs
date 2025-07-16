using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Polling;
using Telegram.Bot.Types.ReplyMarkups;
using ProTasker.Data.IRepository;
using ProTasker.Data.Repository;
using ProTasker.DTOModels.Admin;
using System.Reflection;
using static System.Net.Mime.MediaTypeNames;
using System.Net;
using System.Collections.ObjectModel;

namespace ProTasker.Menu;

public class AdminUI
{
    private readonly ITelegramBotClient botClient;
    private readonly Dictionary<long, string> registrationSteps = new();
    private readonly Dictionary<long, AdminRegisterModel> registeringAdmins = new();
    private readonly Dictionary<long, AdminUpdateModel> updatingAdmins = new();
    private readonly IAdminService adminService;
    private readonly ICategoryService categoryService;
    public AdminUI()
    {
        botClient = new TelegramBotClient("7806562984:AAH1bYTCWl3a3WMj9Wbru71CkevZJ8KyVuk");
        adminService = new AdminService();
        categoryService = new CategoryService();
    }

    public async Task StartAsync()
    {
        using var cts = new CancellationTokenSource();

        botClient.StartReceiving(
            updateHandler: MainMenu,
            HandleErrorAsync,
            receiverOptions: new ReceiverOptions
            {
                AllowedUpdates = new[] { UpdateType.CallbackQuery, UpdateType.Message }
            },
            cancellationToken: cts.Token);

        var me = await botClient.GetMe();
        Console.WriteLine($"✅ Admin bot ishga tushdi: @{me.Username}");

        await Task.Delay(-1);
    }

    private async Task MainMenu(ITelegramBotClient botClient, Update update, CancellationToken ct)
    {
        RegisterMain(botClient, update, ct);

        if (update.Message is { Text: var userInput } message)
        {
            if(userInput == "0")
            {
                await MainMenuButton(update.Message.Chat.Id, ct);
                return;
            }
        }

    }

    private Task HandleErrorAsync(ITelegramBotClient bot, Exception exception, CancellationToken ct)
    {
        Console.WriteLine($"❌ Xatolik: {exception.Message}");
        return Task.CompletedTask;
    }

    private async Task MainMenuButton(long chatId, CancellationToken ct)
    {
        var menu = new InlineKeyboardMarkup(new[]
        {
        new[]
        {
            InlineKeyboardButton.WithCallbackData("🟢 Register", "register"),
            InlineKeyboardButton.WithCallbackData("🔵 Login", "login")
        }
    });

        await botClient.SendMessage(
            chatId: chatId,
            text: "Assalomu alaykum\nIltimos, tanlang:",
            replyMarkup: menu,
            cancellationToken: ct
        );
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
                registeringAdmins[chatId].FirstName = userInput;
                registrationSteps[chatId] = "lastname";

                await botClient.SendMessage(chatId, "👤 Enter last name...", cancellationToken: ct);
            }
            else if (step == "lastname")
            {
                registeringAdmins[chatId].LastName = userInput;
                registrationSteps[chatId] = "age";

                await botClient.SendMessage(chatId, "👤 Enter your age...", cancellationToken: ct);
            }
            else if (step == "age")
            {
                registeringAdmins[chatId].Age = int.Parse(userInput);
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
            }
            else if (step == "password")
            {
                var model = registeringAdmins[chatId];
                model.Password = userInput;
                model.Role = Domain.Enum.Role.Admin;

                try
                {
                    adminService.Register(model);

                    await botClient.SendMessage(chatId, "✅ Registered!", cancellationToken: ct);
                    await botClient.SendMessage(chatId, "Click 0 to return to the menu!", cancellationToken: ct);
                }
                catch (Exception ex)
                {
                    await botClient.SendMessage(chatId, $"❌ Error: {ex.Message}", cancellationToken: ct);
                }

                registrationSteps.Remove(chatId);
                registeringAdmins.Remove(chatId);
            }
        }

        if (update.Message?.Contact is { } contact)
        {
            var chatId = update.Message.Chat.Id;

            await PhoneButton(chatId, contact, ct);
            return;
        }
    }

    private async Task RegisterMain(ITelegramBotClient botClient, Update update, CancellationToken ct)
    {
        try
        {
            if (update.CallbackQuery is not null)
            {
                string data = update.CallbackQuery.Data;
                var chatId = update.CallbackQuery.Message.Chat.Id;

                if (data == "register")
                {
                    registrationSteps[chatId] = "firstname";

                    registeringAdmins[chatId] = new AdminRegisterModel();

                    await botClient.SendMessage(
                        chatId: chatId,
                        text: "👤 Enter First name...",
                        cancellationToken: ct
                    );
                }
                else if (data == "login")
                {
                    await botClient.SendMessage(
                        chatId: update.CallbackQuery.Message.Chat.Id,
                        text: "Enter Phone number...",
                        cancellationToken: ct
                    );
                }
                return;
            }

            if (update.Type == UpdateType.Message &&
                (update.Message!.Text is not null ||
                update.Message.Contact is not null))
            {
                await Register(update, ct);
                return;
            }

            MainMenuButton(update.Message.Chat.Id, ct);


        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }
    }
    private async Task PhoneButton(long chatId, Contact contact, CancellationToken ct)
    {
        if (registrationSteps.TryGetValue(chatId, out var step) && step == "phone")
        {
            var model = registeringAdmins[chatId];
            model.PhoneNumber = contact.PhoneNumber;

            registrationSteps[chatId] = "password";

            await botClient.SendMessage(
                chatId: chatId,
                text: "🔐 Enter password...:",
                cancellationToken: ct
            );
        }
    }
    private async Task LoginMain(ITelegramBotClient botClient, Update update, CancellationToken ct)
    {
        try
        {
            if (update.CallbackQuery is not null)
            {
                string data = update.CallbackQuery.Data;
                var chatId = update.CallbackQuery.Message.Chat.Id;

                if (data == "register")
                {
                    registrationSteps[chatId] = "firstname";

                    registeringAdmins[chatId] = new AdminRegisterModel();

                    await botClient.SendMessage(
                        chatId: chatId,
                        text: "👤 Enter First name...",
                        cancellationToken: ct
                    );
                }
                else if (data == "login")
                {
                    await botClient.SendMessage(
                        chatId: update.CallbackQuery.Message.Chat.Id,
                        text: "Enter Phone number...",
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
                    registeringAdmins[chatId].FirstName = userInput;
                    registrationSteps[chatId] = "lastname";

                    await botClient.SendMessage(chatId, "👤 Enter last name...", cancellationToken: ct);
                }
                else if (step == "lastname")
                {
                    registeringAdmins[chatId].LastName = userInput;
                    registrationSteps[chatId] = "age";

                    await botClient.SendMessage(chatId, "👤 Enter your age...", cancellationToken: ct);
                }
                else if (step == "age")
                {
                    registeringAdmins[chatId].Age = int.Parse(userInput);
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
                }
                else if (step == "password")
                {
                    var model = registeringAdmins[chatId];
                    model.Password = userInput;
                    model.Role = Domain.Enum.Role.Admin;

                    try
                    {
                        adminService.Register(model);

                        await botClient.SendMessage(chatId, "✅ Registered!", cancellationToken: ct);
                    }
                    catch (Exception ex)
                    {
                        await botClient.SendMessage(chatId, $"❌ Error: {ex.Message}", cancellationToken: ct);
                    }

                    registrationSteps.Remove(chatId);
                    registeringAdmins.Remove(chatId);
                }
            }

            MainMenuButton(update.Message.Chat.Id, ct);


        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }


    }
}