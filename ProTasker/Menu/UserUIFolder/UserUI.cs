using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using ProTasker.Data.IRepository;
using ProTasker.Data.Repository;
using ProTasker.Domain.Enum;
using ProTasker.Domain.Models;
using ProTasker.DTOModels.Admin;
using ProTasker.DTOModels.User;
using ProTasker.DTOModels.Worker;
using ProTasker.Menu.AdminUIFolder;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using static System.Collections.Specialized.BitVector32;

namespace ProTasker.Menu.UserUIFolder
{
    public class UserUI
    {
        private IDictionary<long, UserSession> sessions;
        private readonly ITelegramBotClient botClient;
        private readonly IDictionary<long, UserUpdateModel> updatingUsers;
        private readonly IUserService userService;
        private readonly ICategoryService categoryService;
        private UserViewModel userViewModel;
        private readonly IWorkerService workerService;
        public UserUI(string token)
        {
            botClient = new TelegramBotClient(token);
            userService = new UserService();
            categoryService = new CategoryService();
            userViewModel = new UserViewModel();
            updatingUsers = new Dictionary<long, UserUpdateModel>();
            sessions = new Dictionary<long, UserSession>();
            workerService = new WorkerService();
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
            Console.WriteLine($"✅ User bot ishga tushdi: @{me.Username}");

            await Task.Delay(-1);
        }

        private Task HandleErrorAsync(ITelegramBotClient bot, Exception exception, CancellationToken ct)
        {
            Console.WriteLine($"❌ Xatolik: {exception.Message}");
            return Task.CompletedTask;
        }

        private async Task MainMenu(ITelegramBotClient botClient, Update update, CancellationToken ct)
        {
            var chatId = update.Message?.Chat.Id
                         ?? update.CallbackQuery?.Message.Chat.Id
                         ?? 0;

            await CalbackQuerryHelper(chatId, update, ct);

            await MessageHelper(chatId, update, ct);
        }

        private async Task Register(long chatId, string userInput, Update update, UserSession session, CancellationToken ct)
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

                    var model = new UserRegisterModel
                    {
                        FirstName = session.Data["firstname"],
                        LastName = session.Data["lastname"],
                        Age = int.Parse(session.Data["age"]),
                        PhoneNumber = NormalizerPhone(session.Data["phone"]),
                        Password = session.Data["password"],
                        Role = Role.User
                    };

                    try
                    {
                        userService.Register(model);

                        await botClient.SendMessage(chatId, "✅ Registered!", cancellationToken: ct);
                        await botClient.SendMessage(chatId, "⬅️ To return to the main menu, type /start!", cancellationToken: ct);
                    }
                    catch (Exception ex)
                    {
                        await botClient.SendMessage(chatId, $"❌ Error: {ex.Message}   /start", cancellationToken: ct);
                    }
                    sessions.Remove(chatId);
                    break;
            }

            if (update.Message?.Contact is { } contact)
                RegisterPhoneHelper(chatId, contact, ct);
        }

        private async Task Login(long chatId, string userInput, Update update, UserSession session, CancellationToken ct)
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
                    session.Data["phone"] = NormalizerPhone(userInput);
                    session.CurrentStep = "password";
                    await botClient.SendMessage(chatId, "👤 Enter your password...", cancellationToken: ct);
                    break;

                case "password":
                    session.Data["password"] = userInput;

                    try
                    {
                        userViewModel = userService.Login(NormalizerPhone(session.Data["phone"]), session.Data["password"]);

                        session.CurrentStep = "menu";

                        Console.WriteLine($"{userViewModel.FirstName} akkauntiga kirdi");

                        var menu = new ReplyKeyboardMarkup(new[]
                                   {
                                  new[] { new KeyboardButton("📱 My account"),
                                            new KeyboardButton("📂 Search Worker By Categories") },
                                  new[] { new KeyboardButton("📂 Search Worker By Region") },
                                  new[] { new KeyboardButton("🔒 Logout") }
                               })
                        { ResizeKeyboard = true };

                        await botClient.SendMessage(
                            chatId,
                            $"Xush kelibsiz, {userViewModel.FirstName}!\nIltimos kerakli bo‘limni tanlang:",
                            replyMarkup: menu,
                            cancellationToken: ct
                        );
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("❌ Exception: " + ex.ToString());
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
                            await MyAccountMenu(chatId, ct, userViewModel);
                            break;

                        case "📂 Search Worker By Categories":
                            await ShowSearchByCategoryPageAsync(botClient, chatId, 0, session);
                            break;

                        case "📂 Search Worker By Region":
                            await ShowSearchByRegionPageAsync(botClient, chatId, session);
                            break;

                        case "🔒 Logout":
                            await Logout(chatId, ct, userViewModel);
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

        private async Task ShowSearchByRegionPageAsync(ITelegramBotClient botClient, long chatId, UserSession session)
        {
            var buttons = new List<List<InlineKeyboardButton>>();

            foreach (Region region in Enum.GetValues(typeof(Region)))
            {
                string name = GetEnumDescription(region);
  
                buttons.Add(new List<InlineKeyboardButton>
                {
                 InlineKeyboardButton.WithCallbackData(name, $"select_region:{(int)region}")
                });
            }

            buttons.Add(new List<InlineKeyboardButton>
            {
            InlineKeyboardButton.WithCallbackData("❌ Delete", "delete_msg")
            });

            var keyboard = new InlineKeyboardMarkup(buttons);

            await botClient.SendMessage(
                chatId: chatId,
                text: "📍 From which region are you looking for a worker?",
                replyMarkup: keyboard
            );
        }

        public string GetEnumDescription(Enum value)
        {
            var field = value.GetType().GetField(value.ToString());
            var attribute = field?.GetCustomAttribute<DescriptionAttribute>();
            return attribute?.Description ?? value.ToString();
        }

        private async Task ContactHelper(long chatId, Update update, CancellationToken ct)
        {
            if (update.Message?.Contact is { } contact)
            {
                await RegisterPhoneHelper(chatId, contact, ct);

                await LoginPhoneHelper(chatId, contact, ct);
            }
        }

        private async Task RegisterPhoneHelper(long chatId, Contact contact, CancellationToken ct)
        {
            if (sessions.TryGetValue(chatId, out var session) &&
                    session.Mode == "register" &&
                    session.CurrentStep == "phone")
            {
                session.Data["phone"] = NormalizerPhone(contact.PhoneNumber);
                session.CurrentStep = "password";

                await botClient.SendMessage(chatId, "🔐 Enter password...", cancellationToken: ct);
            }
        }

        private async Task LoginPhoneHelper(long chatId, Contact contact, CancellationToken ct)
        {
            if (sessions.TryGetValue(chatId, out var session) &&
                    session.Mode == "login" &&
                    session.CurrentStep == "phone")
            {
                session.Data["phone"] = NormalizerPhone(contact.PhoneNumber);
                session.CurrentStep = "password";

                await botClient.SendMessage(chatId, "🔐 Enter password...", cancellationToken: ct);
            }
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

        private async Task CalbackQuerryHelper(long chatId, Update update, CancellationToken ct)
        {
            var session = sessions.TryGetValue(chatId, out var existing) ? existing : new UserSession();
            sessions[chatId] = session;

            if (update.CallbackQuery?.Data is string data)
            {
                switch (data)
                {
                    case "register":
                        sessions[chatId] = new UserSession
                        {
                            Mode = "register",
                            CurrentStep = "firstname"
                        };
                        await botClient.SendMessage(chatId, "👤 Enter First name...", cancellationToken: ct);
                        return;

                    case "login":
                        sessions[chatId] = new UserSession
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
                        sessions[chatId] = new UserSession
                        {
                            Mode = "update_account",
                            CurrentStep = "new_firstname",
                            Data = new Dictionary<string, string>()
                        };
                        await botClient.SendMessage(chatId, "📝 Enter new first name...", cancellationToken: ct);
                        return;

                    case "change_password":
                        sessions[chatId] = new UserSession
                        {
                            Mode = "change_password",
                            CurrentStep = "old_password",
                            Data = new Dictionary<string, string>()
                        };
                        await botClient.SendMessage(chatId, "📝 Enter old password...", cancellationToken: ct);
                        await botClient.AnswerCallbackQuery(update.CallbackQuery.Id, cancellationToken: ct);
                        return;
                }

                if (data.StartsWith("category_page:"))
                {
                    int page = int.Parse(data.Split(":")[1]);

                    if (session.Data.TryGetValue("LastCategoryMsgId", out var msgIdStr) &&
                        int.TryParse(msgIdStr, out var msgId))
                    {
                        await botClient.DeleteMessage(chatId, msgId);
                    }

                    await ShowSearchByCategoryPageAsync(botClient, chatId, page, session);
                    return;
                }

                if (data == "category_close")
                {
                    if (session.Data.TryGetValue("LastCategoryMsgId", out var msgIdStr) &&
                        int.TryParse(msgIdStr, out var msgId))
                    {
                        await botClient.DeleteMessage(chatId, msgId);
                    }
                    return;
                }

                if (data.StartsWith("select_category:"))
                {
                    int categoryId = int.Parse(data.Split(":")[1]);

                    session.Data["SelectedCategoryId"] = categoryId.ToString();
                    session.Data["WorkerPage"] = "0";

                    var allWorkers = workerService.SearchByCategory(categoryId);

                    if (session.Data.TryGetValue("LastCategoryMsgId", out var lastCatMsgIdStr) &&
                        int.TryParse(lastCatMsgIdStr, out var lastCatMsgId))
                    {
                        await botClient.DeleteMessage(chatId, lastCatMsgId);
                    }

                    await ShowWorkerPageAsync(botClient, chatId, 0, allWorkers, session);
                    return;
                }


                if (data.StartsWith("worker_page:"))
                {
                    int page = int.Parse(data.Split(":")[1]);

                    if (session.Data.TryGetValue("LastWorkerMsgId", out var msgIdStr) &&
                        int.TryParse(msgIdStr, out var msgId))
                    {
                        await botClient.DeleteMessage(chatId, msgId);
                    }

                    var categoryId = int.Parse(session.Data["SelectedCategoryId"]);
                    var workers = workerService.SearchByCategory(categoryId);

                    await ShowWorkerPageAsync(botClient, chatId, page, workers, session);
                    return;
                }

                if (data == "worker_close")
                {
                    if (session.Data.TryGetValue("LastWorkerMsgId", out var msgIdStr) &&
                        int.TryParse(msgIdStr, out var msgId))
                    {
                        await botClient.DeleteMessage(chatId, msgId);
                    }
                    return;
                }

                if (data.StartsWith("select_region:"))
                {
                    var regionId = int.Parse(data.Split(":")[1]);

                    session.Data["SelectedRegionId"] = regionId.ToString();
                    session.Data["WorkerPage"] = "0";
                    
                    var workers = workerService.SearchByRegion(regionId);
                    await ShowWorkerPageAsync(botClient, chatId, 0, workers, session);
                    return;
                }

                if (data == "delete_msg")
                {
                    await botClient.DeleteMessage(chatId, update.CallbackQuery.Message.MessageId);
                    return;
                }
            }
        }

        public TEnum? GetEnumValueFromDescription<TEnum>(string description) where TEnum : struct, Enum
        {
            foreach (var field in typeof(TEnum).GetFields())
            {
                var attr = field.GetCustomAttribute<DescriptionAttribute>();
                if (attr?.Description == description)
                    return (TEnum)field.GetValue(null);

                if (field.Name == description)
                    return (TEnum)field.GetValue(null);
            }

            return null;
        }

        private async Task MessageHelper(long chatId, Update update, CancellationToken ct)
        {
            try
            {
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

                        else if (session.Mode == "login" || session.CurrentStep == "menu")
                            await Login(chatId, userInput, update, session, ct);

                        else if (session.Mode == "update_account")
                            await UpdateAccount(chatId, userInput, update, session, ct);

                        else if (session.Mode == "change_password")
                            await ChangePassword(chatId, userInput, update, session, ct);
                    }
                }

                await ContactHelper(chatId, update, ct);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                await botClient.SendMessage(chatId, $"Error: {ex.Message}", cancellationToken: ct);
            }
        }

        private async Task MyAccountMenu(long chatId, CancellationToken ct, UserViewModel userViewModel)
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
                      $"👤 Ism: {userViewModel.FirstName}\n" +
                      $"👥 Familiya: {userViewModel.LastName}\n" +
                      $"🎂 Yosh: {userViewModel.Age}\n" +
                      $"📞 Tel: {userViewModel.PhoneNumber}",
                replyMarkup: inlineKeyboard,
                cancellationToken: ct
            );
        }

        private async Task UpdateAccount(long chatId, string userInput, Update update, UserSession session, CancellationToken ct)
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

                    var updateModel = new UserUpdateModel
                    {
                        FirstName = session.Data["new_firstname"],
                        LastName = session.Data["new_lastname"],
                        Age = int.Parse(session.Data["new_age"]),
                    };

                    try
                    {
                        userService.UpdateUser(userViewModel.PhoneNumber, updateModel);

                        await botClient.SendMessage(chatId, "✅ Updated!", cancellationToken: ct);

                        session.CurrentStep = "menu";

                        var menu = new ReplyKeyboardMarkup(new[]
                        {
                       new[] { new KeyboardButton("📱 My account"),
                               new KeyboardButton("📂 Search Worker By Categories") },
                       new[] { new KeyboardButton("📂 Search Worker By Region") },
                       new[] { new KeyboardButton("🔒 Logout") }
                    })
                        { ResizeKeyboard = true };

                        await botClient.SendMessage(chatId, "⬅️ You are back to the main panel:", replyMarkup: menu, cancellationToken: ct);
                    }
                    catch (Exception ex)
                    {
                        await botClient.SendMessage(chatId, $"❌ Error: {ex.Message}   /start", cancellationToken: ct);
                    }
                    session.CurrentStep = "menu";
                    break;
            }
        }

        private async Task ChangePassword(long chatId, string userInput, Update update, UserSession session, CancellationToken ct)
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
                        userService.ChangeUserPassword(userViewModel.PhoneNumber, session.Data["old_password"], session.Data["new_password"]);

                        await botClient.SendMessage(chatId, "✅ Password Changed!", cancellationToken: ct);

                        session.CurrentStep = "menu";

                        var menu = new ReplyKeyboardMarkup(new[]
                        {
                       new[] { new KeyboardButton("📱 My account"),
                               new KeyboardButton("📂 Search Worker By Categories") },
                       new[] { new KeyboardButton("📂 Search Worker By Region") },
                       new[] { new KeyboardButton("🔒 Logout") }
                    })
                        { ResizeKeyboard = true };

                        await botClient.SendMessage(chatId, "⬅️ You are back to the main panel:", replyMarkup: menu, cancellationToken: ct);
                    }
                    catch (Exception ex)
                    {
                        await botClient.SendMessage(chatId, $"❌ Error: {ex.Message}  /start", cancellationToken: ct);
                    }
                    session.CurrentStep = "menu";
                    break;
            }
        }

        private async Task ShowSearchByCategoryPageAsync(ITelegramBotClient botClient, long chatId, int page, UserSession session)
        {
            int pageSize = 5;
            var pagedCategories = categoryService.GetAll().Skip(page * pageSize).Take(pageSize).ToList();

            if (pagedCategories.Count == 0)
            {
                await botClient.SendMessage(chatId, "Ushbu sahifada category topilmadi.");
                return;
            }

            var buttons = new List<InlineKeyboardButton[]>();
            foreach (var category in pagedCategories)
            {
                buttons.Add(new[]
                {
            InlineKeyboardButton.WithCallbackData($"📂 {category.Name}", $"select_category:{category.Id}")
                });
            }

            buttons.Add(new[]
            {
            InlineKeyboardButton.WithCallbackData("⏮️", $"category_page:{page - 1}"),
            InlineKeyboardButton.WithCallbackData("⏭️", $"category_page:{page + 1}")
            });

            buttons.Add(new[]
            {
            InlineKeyboardButton.WithCallbackData("❌ Yopish", "category_close")
            });

            var keyboard = new InlineKeyboardMarkup(buttons);

            var sentMsg = await botClient.SendMessage(
                chatId: chatId,
                text: "📁 Kategoriyalar ro'yxati:",
                replyMarkup: keyboard
            );

            session.Data["LastCategoryMsgId"] = sentMsg.MessageId.ToString();
        }

        private async Task ShowWorkerPageAsync(ITelegramBotClient botClient, long chatId, int page, List<WorkerSearchModel> allWorkers, UserSession session)
        {
            int pageSize = 5;
            var pagedWorkers = allWorkers.Skip(page * pageSize).Take(pageSize).ToList();

            if (pagedWorkers.Count == 0)
            {
                await botClient.SendMessage(chatId, "Ushbu sahifada worker topilmadi.");
                return;
            }

            var textBuilder = new StringBuilder();
            for (int i = 0; i < pagedWorkers.Count; i++)
            {
                var w = pagedWorkers[i];
                textBuilder.AppendLine($"👷‍ <b>{w.FirstName} {w.LastName}</b>");
                textBuilder.AppendLine($"📞 Phone: {w.PhoneNumber}");
                textBuilder.AppendLine($" {w.Category}");
                textBuilder.AppendLine($" {w.Bio}\n");
                textBuilder.AppendLine($"---------------------------");
                textBuilder.AppendLine($"---------------------------\n");
            }

            string text = textBuilder.ToString();

            var inlineKeyboard = new InlineKeyboardMarkup(new[]
            {
            new[]
            {
            InlineKeyboardButton.WithCallbackData("⏮️", $"worker_page:{page - 1}"),
            InlineKeyboardButton.WithCallbackData("⏭️", $"worker_page:{page + 1}")
            },
            new[]
            {
            InlineKeyboardButton.WithCallbackData("❌ Close", $"worker_close")
            }
            });

            var sentMsg = await botClient.SendMessage(
                chatId: chatId,
                text: text,
                parseMode: ParseMode.Html,
                replyMarkup: inlineKeyboard);

            session.Data["LastWorkerMsgId"] = sentMsg.MessageId.ToString();
        }

        private async Task Logout(long chatId, CancellationToken ct, UserViewModel UserViewModel)
        {
            sessions.Remove(chatId);
            await botClient.SendMessage(chatId, "🔒 Siz tizimdan chiqdingiz.", cancellationToken: ct);
            await MainMenuButton(chatId, ct);
        }

        private string NormalizerPhone(string phone)
        {
            return phone.Replace("+", "").Trim();
        }
    }
}
