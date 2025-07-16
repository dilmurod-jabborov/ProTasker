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
    private Dictionary<long, AdminSession> sessions = new();
    private readonly ITelegramBotClient botClient;
    private readonly Dictionary<long, AdminUpdateModel> updatingAdmins = new();
    private readonly IAdminService adminService;
    private readonly ICategoryService categoryService;
    private AdminViewModel adminViewModel;
    public AdminUI()
    {
        botClient = new TelegramBotClient("7806562984:AAH1bYTCWl3a3WMj9Wbru71CkevZJ8KyVuk");
        adminService = new AdminService();
        categoryService = new CategoryService();
        adminViewModel = new AdminViewModel();
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
        try
        {
            var chatId = update.Message?.Chat.Id
                         ?? update.CallbackQuery?.Message.Chat.Id
                         ?? 0;

            if (update.CallbackQuery?.Data is string data)
            {
                switch (data)
                {
                    case "register":
                        sessions[chatId] = new AdminSession
                        {
                            Mode = "register",
                            CurrentStep = "firstname"
                        };

                        await botClient.SendMessage(chatId, "👤 Enter First name...", cancellationToken: ct);
                        return;

                    case "login":
                        sessions[chatId] = new AdminSession
                        {
                            Mode = "login",
                            CurrentStep = "phone"
                        };

                        var contactKeyboard = new ReplyKeyboardMarkup(new[]
                        {
                    KeyboardButton.WithRequestContact("📱 Send your phone number")
                })
                        {
                            ResizeKeyboard = true,
                            OneTimeKeyboard = true
                        };

                        await botClient.SendMessage(chatId, "📞 Click button to send phone number:", replyMarkup: contactKeyboard, cancellationToken: ct);
                        return;

                    case "update_account":
                        sessions[chatId] = new AdminSession
                        {
                            Mode = "update_account",
                            CurrentStep = "new_firstname",
                            Data = new Dictionary<string, string>()
                        };

                        await botClient.SendMessage(chatId, "📝 Enter new first name...", cancellationToken: ct);
                        return;

                    case "change_password":
                        sessions[chatId] = new AdminSession
                        {
                            Mode = "change_password",
                            CurrentStep = "old_password",
                            Data = new Dictionary<string, string>()
                        };

                        await botClient.SendMessage(chatId, "📝 Enter old password...", cancellationToken: ct);
                        await botClient.AnswerCallbackQuery(update.CallbackQuery.Id, cancellationToken: ct);
                        return;
                }
            }

            if (update.Message?.Text is string userInput)
            {
                if (userInput == "/start")
                {
                    await MainMenuButton(chatId, ct);
                    sessions.Remove(chatId);
                    return;
                }

                if (sessions.TryGetValue(chatId, out var session))
                {
                    if (session.Mode == "register")
                        await Register(chatId, userInput, update, session, ct);

                    else if (session.Mode == "login")
                        await Login(chatId, userInput, update, session, ct);

                    else if (session.Mode == "update_account")
                        await UpdateAccount(chatId, userInput, update, session, ct);

                    else if (session.Mode == "change_password")
                        await ChangePassword(chatId, userInput, update, session, ct);
                }
            }

            if (update.Message?.Contact is { } contact)
            {
                if (sessions.TryGetValue(chatId, out var session) &&
                    session.Mode == "register" &&
                    session.CurrentStep == "phone")
                {
                    session.Data["phone"] = contact.PhoneNumber;
                    session.CurrentStep = "password";

                    await botClient.SendMessage(chatId, "🔐 Enter password...", cancellationToken: ct);
                }

                if (sessions.TryGetValue(chatId, out var session1) &&
                    session.Mode == "login" &&
                    session.CurrentStep == "phone")
                {
                    session.Data["phone"] = contact.PhoneNumber;
                    session.CurrentStep = "password";

                    await botClient.SendMessage(chatId, "🔐 Enter password...", cancellationToken: ct);
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
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

    private async Task Register(long chatId, string userInput, Update update, AdminSession session, CancellationToken ct)
    {
        if (userInput == "/start")
        {
            await MainMenuButton(chatId, ct);
            sessions.Remove(chatId);
            return;
        }

        switch (session.CurrentStep)
        {
            case "firstname":
                session.Data["firstname"] = userInput;
                session.CurrentStep = "lastname";
                await botClient.SendMessage(chatId, "👤 Enter last name...", cancellationToken: ct);
                break;

            case "lastname":
                session.Data["lastname"] = userInput;
                session.CurrentStep = "age";
                await botClient.SendMessage(chatId, "👤 Enter your age...", cancellationToken: ct);
                break;

            case "age":
                if (!int.TryParse(userInput, out var age))
                {
                    await botClient.SendMessage(chatId, "❌ Please enter a number...", cancellationToken: ct);
                    return;
                }

                session.Data["age"] = userInput;
                session.CurrentStep = "phone";

                var contactKeyboard =
                new ReplyKeyboardMarkup(new[]
                {
                        KeyboardButton.WithRequestContact("📱 Send your phone number")
                })
                {
                    ResizeKeyboard = true,
                    OneTimeKeyboard = true
                };

                await botClient.SendMessage(chatId, "📞 Click button to send phone number..", replyMarkup: contactKeyboard, cancellationToken: ct);
                break;

            case "password":
                session.Data["password"] = userInput;

                var model = new AdminRegisterModel
                {
                    FirstName = session.Data["firstname"],
                    LastName = session.Data["lastname"],
                    Age = int.Parse(session.Data["age"]),
                    PhoneNumber = session.Data["phone"],
                    Password = session.Data["password"],
                    Role = Domain.Enum.Role.Admin
                };

                try
                {
                    adminService.Register(model);

                    await botClient.SendMessage(chatId, "✅ Registered!", cancellationToken: ct);
                    await botClient.SendMessage(chatId, "⬅️ To return to the main menu, type /start!", cancellationToken: ct);
                }
                catch (Exception ex)
                {
                    await botClient.SendMessage(chatId, $"❌ Error: {ex.Message}", cancellationToken: ct);
                }
                sessions.Remove(chatId);
                break;
        }

        if (update.Message?.Contact is { } contact)
        {
            if (session.Mode == "register" && session.CurrentStep == "phone")
            {
                session.Data["phone"] = contact.PhoneNumber;
                session.CurrentStep = "password";

                await botClient.SendMessage(chatId, "🔐 Enter password...", cancellationToken: ct);
            }
        }
    }

    private async Task Login(long chatId, string userInput, Update update, AdminSession session, CancellationToken ct)
    {
        if (userInput == "/start")
        {
            await MainMenuButton(chatId, ct);
            sessions.Remove(chatId);
            return;
        }

        switch (session.CurrentStep)
        {
            case "phone":
                session.Data["phone"] = userInput;
                session.CurrentStep = "password";
                await botClient.SendMessage(chatId, "👤 Enter your password...", cancellationToken: ct);
                break;

            case "password":
                session.Data["password"] = userInput;

                try
                {
                    adminViewModel = adminService.Login(session.Data["phone"], session.Data["password"]);
                    session.CurrentStep = "menu";

                    Console.WriteLine($"{adminViewModel.FirstName} akkauntiga kirdi");

                    var menu = new ReplyKeyboardMarkup(new[]
                               {
                                  new[] { new KeyboardButton("📱 My account"),
                                            new KeyboardButton("📂 Work Categories") },
                                  new[] { new KeyboardButton("➕ Add work category"),
                                            new KeyboardButton("❌ Delete category") },
                                  new[] { new KeyboardButton("🔒 Logout") }
                               })
                    { ResizeKeyboard = true };

                    await botClient.SendMessage(
                        chatId,
                        $"Xush kelibsiz, {adminViewModel.FirstName}!\nIltimos kerakli bo‘limni tanlang:",
                        replyMarkup: menu,
                        cancellationToken: ct
                    );
                }
                catch (Exception ex)
                {
                    Console.WriteLine("❌ Exception: " + ex.ToString()); // To‘liq exception chiqadi
                    await botClient.SendMessage(chatId, $"❌ Error: {ex.Message}", cancellationToken: ct);
                }
                break;
        }

        if (session.CurrentStep == "menu" && update.Message?.Text is string commandText)
        {
            try
            {
                switch (commandText)
                {
                    case "📱 My account":
                        await MyAccountMenu(chatId, ct, adminViewModel);
                        break;

                    case "📂 Work Categories":
                        await WorkCategories(chatId, ct);
                        break;

                    case "➕ Add work category":
                        await AddCategory(chatId, ct);
                        break;

                    case "❌ Delete category":
                        await DeleteCategory(chatId, ct);
                        break;

                    case "🔒 Logout":
                        await Logout(chatId, ct, adminViewModel);
                        break;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("❌ Exception: " + ex.ToString()); // To‘liq exception chiqadi
                await botClient.SendMessage(chatId, $"❌ Error: {ex.Message}", cancellationToken: ct);
            }     
        }
    }

    private async Task MyAccountMenu(long chatId, CancellationToken ct, AdminViewModel adminViewModel)
    {
        var inlineKeyboard = new InlineKeyboardMarkup(new[]
        {
        new[]
        {
            InlineKeyboardButton.WithCallbackData("📝 Update Account", "update_account"),
            InlineKeyboardButton.WithCallbackData("🔑 Change Password", "change_password")
        }
        });

        await botClient.SendMessage(
            chatId: chatId,
            text: $"👤 Sizning profilingiz:\n\n" +
                  $"👤 Ism: {adminViewModel.FirstName}\n" +
                  $"👥 Familiya: {adminViewModel.LastName}\n" +
                  $"🎂 Yosh: {adminViewModel.Age}\n" +
                  $"📞 Tel: {adminViewModel.PhoneNumber}",
            replyMarkup: inlineKeyboard,
            cancellationToken: ct
        );
    }
    
    private async Task UpdateAccount(long chatId, string userInput, Update update, AdminSession session, CancellationToken ct)
    {
        if (userInput == "/start")
        {
            await MainMenuButton(chatId, ct);
            sessions.Remove(chatId);
            return;
        }

        switch (session.CurrentStep)
        {
            case "new_firstname":
                session.Data["new_firstname"] = userInput;
                session.CurrentStep = "new_lastname";
                await botClient.SendMessage(chatId, "👤 Enter new last name...", cancellationToken: ct);
                break;

            case "new_lastname":
                session.Data["new_lastname"] = userInput;
                session.CurrentStep = "new_age";
                await botClient.SendMessage(chatId, "👤 Enter new your age...", cancellationToken: ct);
                break;

            case "new_age":
                if (!int.TryParse(userInput, out var age))
                {
                    await botClient.SendMessage(chatId, "❌ Please enter a number...", cancellationToken: ct);
                    return;
                }

                session.Data["new_age"] = userInput;

                var updateModel = new AdminUpdateModel
                {
                    FirstName = session.Data["new_firstname"],
                    LastName = session.Data["new_lastname"],
                    Age = int.Parse(session.Data["new_age"]),
                };

                try
                {
                    adminService.Update(adminViewModel.PhoneNumber, updateModel);

                    await botClient.SendMessage(chatId, "✅ Updated!", cancellationToken: ct);

                    session.CurrentStep = "menu";

                    var menu = new ReplyKeyboardMarkup(new[]
                    {
                       new[] { new KeyboardButton("📱 My account"),
                               new KeyboardButton("📂 Work Categories") },
                       new[] { new KeyboardButton("➕ Add work category"), 
                               new KeyboardButton("❌ Delete category") },
                       new[] { new KeyboardButton("🔒 Logout") }
                    })
                    { ResizeKeyboard = true };

                    await botClient.SendMessage(chatId, "⬅️ You are back to the main panel:", replyMarkup: menu, cancellationToken: ct);
                }
                catch (Exception ex)
                {
                    await botClient.SendMessage(chatId, $"❌ Error: {ex.Message}", cancellationToken: ct);
                }
                sessions.Remove(chatId);
                break;
        }
    }

    private async Task ChangePassword(long chatId, string userInput, Update update, AdminSession session, CancellationToken ct)
    {
        if (userInput == "/start")
        {
            await MainMenuButton(chatId, ct);
            sessions.Remove(chatId);
            return;
        }

        switch (session.CurrentStep)
        {
            case "old_password":
                session.Data["old_password"] = userInput;
                session.CurrentStep = "new_password";
                await botClient.SendMessage(chatId, "👤 Enter new password...", cancellationToken: ct);
                break;

            case "new_password":

                session.Data["new_password"] = userInput;

                try
                {
                    adminService.ChangePassword(adminViewModel.PhoneNumber, session.Data["old_password"], session.Data["new_password"]);

                    await botClient.SendMessage(chatId, "✅ Password Changed!", cancellationToken: ct);

                    session.CurrentStep = "menu";

                    var menu = new ReplyKeyboardMarkup(new[]
                    {
                       new[] { new KeyboardButton("📱 My account"),
                               new KeyboardButton("📂 Work Categories") },
                       new[] { new KeyboardButton("➕ Add work category"),
                               new KeyboardButton("❌ Delete category") },
                       new[] { new KeyboardButton("🔒 Logout") }
                    })
                    { ResizeKeyboard = true };

                    await botClient.SendMessage(chatId, "⬅️ You are back to the main panel:", replyMarkup: menu, cancellationToken: ct);
                }
                catch (Exception ex)
                {
                    await botClient.SendMessage(chatId, $"❌ Error: {ex.Message}", cancellationToken: ct);
                }
                sessions.Remove(chatId);
                break;
        }
    }
    private async Task WorkCategories(long chatId, CancellationToken ct)
    {
        List<Domain.Models.Category> categories = categoryService.GetAll();
        
        foreach(var category in categories)
        {
            await botClient.SendMessage(
            chatId: chatId,
            text: $" Category lists :\n\n" +
                  $"👥 {category.Id}: {category.Name}\n",
            cancellationToken: ct
        );
        }
    }
    
    private async Task AddCategory(long chatId, CancellationToken ct)
    {

    }
    
    private async Task DeleteCategory(long chatId, CancellationToken ct)
    {

    }
    
    private async Task Logout(long chatId, CancellationToken ct, AdminViewModel adminViewModel)
    {
        sessions.Remove(chatId); 
        await botClient.SendMessage(chatId, "🔒 Siz tizimdan chiqdingiz.", cancellationToken: ct);
        await MainMenuButton(chatId, ct);
    }
}