using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Polling;
using Telegram.Bot.Types.ReplyMarkups;
using ProTasker.Data.IRepository;
using ProTasker.Data.Repository;
using ProTasker.DTOModels.Worker;
using ProTasker.Domain.Enum;
using System.ComponentModel;
using System.Reflection;
using ProTasker.Menu.MAIN;

namespace ProTasker.Menu;

public class WorkerUI
{
    private readonly IDictionary<long, WorkerUpdateModel> updatingWorkers;
    private IDictionary<long, WorkerSession> sessions;
    private readonly IWorkerService workerService;
    private readonly ICategoryService categoryService;
    private WorkerViewModel workerViewModel;
    private WorkerRegisterModel workerRegisterModel;
    private ITelegramBotClient botClient;

    public WorkerUI(string token)
    {
        botClient = new TelegramBotClient(token); 
        workerService = new WorkerService();
        categoryService = new CategoryService();
        workerViewModel = new WorkerViewModel();
        updatingWorkers = new Dictionary<long, WorkerUpdateModel>();
        sessions = new Dictionary<long, WorkerSession>();
        workerRegisterModel = new WorkerRegisterModel();
    }

    public async Task StartAsync()
    {
        using var cts = new CancellationTokenSource();

        botClient.StartReceiving(
            updateHandler: MainMenu,
            HandleErrorAsync,
            receiverOptions: new ReceiverOptions
            {
                AllowedUpdates = new[] { UpdateType.Message, UpdateType.CallbackQuery }
            },
            cancellationToken: cts.Token
        );

        var me = await botClient.GetMe();
        Console.WriteLine($"✅ Worker bot ishga tushdi: @{me.Username}");

        await Task.Delay(-1);
    }

    public async Task MainMenu(ITelegramBotClient botClient, Update update, CancellationToken ct)
    {
        var chatId = update.Message?.Chat.Id
                     ?? update.CallbackQuery?.Message.Chat.Id
                     ?? 0;

        await CalbackQuerryHelper(chatId, update, ct);

        await MessageHelper(chatId, update, ct);
    }

    private async Task CalbackQuerryHelper(long chatId, Update update, CancellationToken ct)
    {
        if (update.CallbackQuery?.Data is string data)
        {
            switch (data)
            {
                case "register":
                    sessions[chatId] = new WorkerSession
                    {
                        Mode = "register",
                        CurrentStep = "firstname"
                    };

                    await botClient.SendMessage(chatId, "👤 Enter First name...", cancellationToken: ct);
                    return;

                case "login":
                    sessions[chatId] = new WorkerSession
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

                    await botClient.SendMessage(chatId, "📞 Click button to send phone number:",
                        replyMarkup: contactKeyboard, cancellationToken: ct);
                    return;

                case "update_account":
                    sessions[chatId] = new WorkerSession
                    {
                        Mode = "update_account",
                        CurrentStep = "new_firstname",
                        Data = new Dictionary<string, string>()
                    };

                    await botClient.SendMessage(chatId, "📝 Enter new first name...",
                        cancellationToken: ct);
                    return;

                case "change_password":
                    sessions[chatId] = new WorkerSession
                    {
                        Mode = "change_password",
                        CurrentStep = "old_password",
                        Data = new Dictionary<string, string>()
                    };

                    await botClient.SendMessage(chatId, "📝 Enter old password...", cancellationToken: ct);

                    await botClient.AnswerCallbackQuery(update.CallbackQuery.Id, cancellationToken: ct);
                    return;
            }

            if (data.StartsWith("toggle_category:"))
            {
                var catId = int.Parse(data.Split(":")[1]);

                var session = sessions[chatId];

                if (!session.Data.ContainsKey("SelectedCategoryIds"))
                    session.Data["SelectedCategoryIds"] = "";

                var ids = session.Data["SelectedCategoryIds"]
                    .Split(',', StringSplitOptions.RemoveEmptyEntries)
                    .Select(int.Parse)
                    .ToList();

                if (ids.Contains(catId))
                    ids.Remove(catId);
                else
                    ids.Add(catId);

                session.Data["SelectedCategoryIds"] = string.Join(",", ids);

                Console.WriteLine("✅ SelectedCategoryIds now: " + session.Data["SelectedCategoryIds"]);

                int currentPage = int.Parse(session.Data["CategoryPage"]);
                await ShowCategoryPageAsync(chatId, currentPage, ct);
                return;
            }

            if (data.StartsWith("worker_page:"))
            {
                var session = sessions[chatId];

                if (!session.Data.ContainsKey("SelectedCategoryIds"))
                    session.Data["SelectedCategoryIds"] = "";

                if (!session.Data.ContainsKey("CategoryPage"))
                    session.Data["CategoryPage"] = "0";

                int page = int.Parse(data.Split(":")[1]);
                sessions[chatId].Data["CategoryPage"] = page.ToString();
                await ShowCategoryPageAsync(chatId, page, ct);
                return;
            }

            if (data == "finish_register_categories")
            {
                if (!sessions[chatId].Data.ContainsKey("SelectedCategoryIds") ||
                    string.IsNullOrWhiteSpace(sessions[chatId].Data["SelectedCategoryIds"]))
                {
                    await botClient.SendMessage(chatId, "⚠️ You must select at least one category!", cancellationToken: ct);
                    return;
                }

                sessions[chatId].CurrentStep = "region";

                var regions = Enum.GetValues(typeof(Region)).Cast<Region>().Select(r =>
                    InlineKeyboardButton.WithCallbackData(GetDescription(r), $"region:{(int)r}"))
                    .Chunk(2)
                    .Select(r => r.ToArray())
                    .ToArray();

                var markup = new InlineKeyboardMarkup(regions);

                await botClient.SendMessage(chatId, "🌍 Choose your region:", replyMarkup: markup, cancellationToken: ct);
                await botClient.EditMessageReplyMarkup(chatId, update.CallbackQuery.Message.MessageId, replyMarkup: null, cancellationToken: ct);

                return;
            }

            if (data.StartsWith("region:") && sessions[chatId].Mode == "register")
            {
                var session = sessions[chatId];

                int regionId = int.Parse(data.Split(":")[1]);
                Region selectedRegion = (Region)regionId;

                session.Data["regionId"] = regionId.ToString();
                session.Data["regionName"] = selectedRegion.ToString();

                session.CurrentStep = "district";

                await botClient.SendMessage(chatId, $"📍 Enter your district...", cancellationToken: ct);
                return;
            }

            if (data.StartsWith("region:") && sessions[chatId].CurrentStep == "change_location")
            {
                int regionId = int.Parse(data.Split(":")[1]);
                var selectedRegion = (Region)regionId;

                sessions[chatId].Data["SelectedRegionId"] = regionId.ToString();
                sessions[chatId].Data["SelectedRegionName"] = GetDescription(selectedRegion);

                sessions[chatId].CurrentStep = "new_district";

                await botClient.SendMessage(
                    chatId,
                    $" Enter your new district... {GetDescription(selectedRegion)}",
                    cancellationToken: ct
                );
                return;
            }

            if (data == "category_next" || data == "category_prev")
            {
                int currentPage = int.Parse(sessions[chatId].Data.GetValueOrDefault("CategoryPage", "0"));

                currentPage += data == "category_next" ? 1 : -1;
                if (currentPage < 0) currentPage = 0;

                sessions[chatId].Data["CategoryPage"] = currentPage.ToString();
                await ShowCategoryPageAsync(chatId, currentPage, ct);
            }
            else if (data == "finish_categories")
            {
                try
                {
                    var selected = sessions[chatId].Data.GetValueOrDefault("SelectedCategoryIds", "");

                    var selectedIds = selected.Split(',').Select(int.Parse).ToList();

                    if (string.IsNullOrEmpty(selected))
                    {
                        await botClient.SendMessage(chatId, "⚠️ You must select at least one category!", cancellationToken: ct);
                        return;
                    }

                    workerService.ChangeCategory(workerViewModel.PhoneNumber, selectedIds);

                    await botClient.SendMessage(chatId, $"✅ Categories updated:\n{selected}", cancellationToken: ct);

                    sessions[chatId].Mode = "menu";
                    sessions[chatId].CurrentStep = "menu";
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    await botClient.SendMessage(chatId, $"Error: {ex.Message}  /start", cancellationToken: ct);
                }
            }

            if (sessions[chatId].CurrentStep == "change_location" && data.StartsWith("region:"))
            {
                var regionId = int.Parse(data.Split(":")[1]);
                sessions[chatId].Data["SelectedRegionId"] = regionId.ToString();
                sessions[chatId].Mode = "new_district";

                await botClient.DeleteMessage(chatId, update.CallbackQuery.Message.MessageId);
                await botClient.SendMessage(chatId, "🏙 Enter your new district...");
                return;
            }

            return;

        }
    }

    public static string GetDescription(Enum value)
    {
        return value
            .GetType()
            .GetField(value.ToString())
            ?.GetCustomAttribute<DescriptionAttribute>()
            ?.Description ?? value.ToString();
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

                    if (session.CurrentStep == "new_district")
                    {
                        session.Data["new_district"] = userInput;
                        session.CurrentStep = "new_street";

                        await botClient.SendMessage(chatId, "🏡 Enter your new street :");
                        return;
                    }

                    if (session.CurrentStep == "new_street")
                    {
                        session.Data["new_street"] = userInput;

                        var location = new Domain.Models.Location()
                        {
                            Region = (Region)int.Parse(session.Data["SelectedRegionId"]),
                            District = session.Data["new_district"],
                            Street = session.Data["new_street"]
                        };

                        try
                        {
                            workerService.ChangeLocation(workerViewModel.PhoneNumber, location);

                            session.CurrentStep = "menu";
                            session.Mode = null;

                            await botClient.SendMessage(chatId, "📍 Location updated successfully!");
                            return;
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(ex.Message);
                            await botClient.SendMessage(chatId, $"Error: {ex.Message}   /start", cancellationToken: ct);
                        }

                    }
                }
            }

            await ContactHelper(chatId, update, ct);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            await botClient.SendMessage(chatId, $"Error: {ex.Message} /start", cancellationToken: ct);
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

    private async Task Register(long chatId, string userInput, Update update, WorkerSession session, CancellationToken ct)
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
                session.CurrentStep = "bio";

                await botClient.SendMessage(chatId, "👤 Enter your biography...", cancellationToken: ct);
                break;

            case "bio":
                session.Data["bio"] = userInput;
                session.CurrentStep = "categoryId";

                await ShowCategoryPageAsync(chatId, 0, ct);
                break;

            case "district":
                session.Data["district"] = userInput;
                session.CurrentStep = "street";

                await botClient.SendMessage(chatId, "👤 Enter your street...", cancellationToken: ct);
                break;

            case "street":
                session.Data["street"] = userInput;

                var model = new WorkerRegisterModel
                {
                    FirstName = session.Data["firstname"],
                    LastName = session.Data["lastname"],
                    PhoneNumber = NormalizerPhone(session.Data["phone"]),
                    Password = session.Data["password"],
                    Age = int.Parse(session.Data["age"]),
                    Bio = session.Data["bio"],
                    Role = Role.Worker,
                    CategoryId = session.Data["SelectedCategoryIds"].Split(',').Select(int.Parse).ToList(),
                    Location = new Domain.Models.Location
                    {
                        Region = (Region)int.Parse(session.Data["regionId"]),
                        District = session.Data["district"],
                        Street = session.Data["street"],
                    }
                };

                try
                {
                    workerService.Register(model);

                    await botClient.SendMessage(chatId, "✅ Registered!", cancellationToken: ct);
                    await botClient.SendMessage(chatId, "⬅️ To return to the main menu, type /start!", cancellationToken: ct);
                }
                catch (Exception ex)
                {
                    await botClient.SendMessage(chatId, $"❌ Error: {ex.Message} /start", cancellationToken: ct);
                }
                sessions.Remove(chatId);
                break;
        }

        if (update.Message?.Contact is { } contact)
            RegisterPhoneHelper(chatId, contact, ct);
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

    private async Task Login(long chatId, string userInput, Update update, WorkerSession session, CancellationToken ct)
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
                    workerViewModel = workerService.Login(NormalizerPhone(session.Data["phone"]), session.Data["password"]);

                    session.CurrentStep = "menu";

                    Console.WriteLine($"{workerViewModel.FirstName} akkauntiga kirdi");

                    var menu = new ReplyKeyboardMarkup(new[]
                               {
                                  new[] { new KeyboardButton("📱 My account"),
                                          new KeyboardButton("🛠 Change work categories") },
                                  new[] { new KeyboardButton("📍 Change Location") },
                                  new[] { new KeyboardButton("🔒 Logout") }
                               })
                    { ResizeKeyboard = true };

                    await botClient.SendMessage(
                        chatId,
                        $"Xush kelibsiz, {workerViewModel.FirstName}!\nIltimos kerakli bo‘limni tanlang:",
                        replyMarkup: menu,
                        cancellationToken: ct
                    );
                }
                catch (Exception ex)
                {
                    Console.WriteLine("❌ Exception: " + ex.ToString());
                    await botClient.SendMessage(chatId, $"❌ Error: {ex.Message} /start", cancellationToken: ct);
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
                        await MyAccountMenu(chatId, ct, workerViewModel);
                        break;

                    case "🛠 Change work categories":

                        if (workerRegisterModel?.CategoryId != null && workerRegisterModel.CategoryId.Any())
                        {
                            session.Data["SelectedCategoryIds"] = string.Join(",", workerRegisterModel.CategoryId);
                        }
                        else
                        {
                            session.Data["SelectedCategoryIds"] = "";
                        }

                        session.CurrentStep = "change_category";

                        session.Data["CategoryPage"] = "0";

                        await ShowCategoryPageAsync(chatId, 0, ct);
                        break;

                    case "📍 Change Location":
                        session.CurrentStep = "change_location";

                        var regions = Enum.GetValues(typeof(Region)).Cast<Region>().Select(r =>
                            InlineKeyboardButton.WithCallbackData($"🌍 {GetDescription(r)}", $"region:{(int)r}")
                        ).Chunk(2).Select(x => x.ToArray()).ToArray();

                        await botClient.SendMessage(chatId, "🌍 Choice your new region...", replyMarkup: new InlineKeyboardMarkup(regions), cancellationToken: ct);
                        break;

                    case "🔒 Logout":
                        await Logout(chatId, ct, workerViewModel);
                        break;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("❌ Exception: " + ex.ToString());
                await botClient.SendMessage(chatId, $"❌ Error: {ex.Message} /start", cancellationToken: ct);
            }
        }
    }

    private async Task ShowCategoryPageAsync(long chatId, int page, CancellationToken ct)
    {
        const int PageSize = 6;
        var session = sessions[chatId];

        if (!session.Data.ContainsKey("SelectedCategoryIds"))
            session.Data["SelectedCategoryIds"] = "";

        var selectedIds = session.Data["SelectedCategoryIds"]
            .Split(',', StringSplitOptions.RemoveEmptyEntries)
            .Select(int.Parse)
            .ToList();

        var allCategories = categoryService.GetAll();
        if (page < 0) page = 0;
        if (page * PageSize >= allCategories.Count) page = 0;

        session.Data["CategoryPage"] = page.ToString();

        var categoriesOnPage = allCategories
            .Skip(page * PageSize)
            .Take(PageSize)
            .ToList();

        var buttons = new List<List<InlineKeyboardButton>>();

        foreach (var category in categoriesOnPage)
        {
            bool isSelected = selectedIds.Contains(category.Id);
            string text = (isSelected ? "✅ " : "") + category.Name;

            buttons.Add(new List<InlineKeyboardButton>
        {
            InlineKeyboardButton.WithCallbackData(text, $"toggle_category:{category.Id}")
        });
        }

        var navButtons = new List<InlineKeyboardButton>();
        if (page > 0)
            navButtons.Add(InlineKeyboardButton.WithCallbackData("⬅️", $"worker_page:{page - 1}"));
        if ((page + 1) * PageSize < allCategories.Count)
            navButtons.Add(InlineKeyboardButton.WithCallbackData("➡️", $"worker_page:{page + 1}"));
        if (navButtons.Any())
            buttons.Add(navButtons);

        string doneCallback = session.Mode == "register" ? "finish_register_categories" : "finish_categories";
        buttons.Add(new List<InlineKeyboardButton>
    {
        InlineKeyboardButton.WithCallbackData("✅ Done", doneCallback)
    });

        var markup = new InlineKeyboardMarkup(buttons);

        if (session.Data.ContainsKey("LastCategoryMsgId"))
        {
            int msgId = int.Parse(session.Data["LastCategoryMsgId"]);

            try
            {
                await botClient.EditMessageText(
                    chatId: chatId,
                    messageId: msgId,
                    text: "📂 Choose your category(s):",
                    replyMarkup: markup,
                    cancellationToken: ct);
            }
            catch
            {
                var sent = await botClient.SendMessage(chatId, "📂 Choose your category(s):", replyMarkup: markup, cancellationToken: ct);
                session.Data["LastCategoryMsgId"] = sent.MessageId.ToString();
            }
        }
        else
        {
            var sent = await botClient.SendMessage(chatId, "📂 Choose your category(s):", replyMarkup: markup, cancellationToken: ct);
            session.Data["LastCategoryMsgId"] = sent.MessageId.ToString();
        }
    }

    private async Task MyAccountMenu(long chatId, CancellationToken ct, WorkerViewModel workerViewModel)
    {
        var inlineKeyboard = new InlineKeyboardMarkup(new[]
        {
        new[]
        {
            InlineKeyboardButton.WithCallbackData("📝 <b>Update Account</b>", "update_account"),
            InlineKeyboardButton.WithCallbackData("🔑 <b>Change Password</b>", "change_password")
        }
        });

        await botClient.SendMessage(
            chatId: chatId,
            text: $"<b> Sizning profilingiz:</b>\n\n" +
                  $"👤 <u>Ism:</u> <b>{workerViewModel.FirstName}</b>\n" +
                  $"👥 <u>Familiya:</u> {workerViewModel.LastName}\n" +
                  $"🎂 <u>Yosh:</u> {workerViewModel.Age}\n" +
                  $"📞 <code>Tel:</code> {workerViewModel.PhoneNumber}\n" +
                  $"📝 <u>Bio:</u> {workerViewModel.Bio}\n" +
                  $"💼 <u>Category:</u> {string.Join(", ", workerViewModel.Category)}\n" +
                  $"🌃 <u>Region:</u> {workerViewModel.Location.Region.ToString()}\n" +
                  $"🏘️ <u>District:</u> {workerViewModel.Location.District}\n" +
                  $"🛣️ <u>Street</u>: {workerViewModel.Location.Street}",
            parseMode: ParseMode.Html,
            replyMarkup: inlineKeyboard,
            cancellationToken: ct
        );
    }

    private async Task UpdateAccount(long chatId, string userInput, Update update, WorkerSession session, CancellationToken ct)
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
                session.CurrentStep = "new_bio";

                break;

            case "new_bio":
                session.Data["new_bio"] = userInput;

                var updateModel = new WorkerUpdateModel
                {
                    FirstName = session.Data["new_firstname"],
                    LastName = session.Data["new_lastname"],
                    Age = int.Parse(session.Data["new_age"])
                };

                try
                {
                    workerService.Update(workerViewModel.PhoneNumber, updateModel);

                    await botClient.SendMessage(chatId, "✅ Updated!", cancellationToken: ct);

                    session.CurrentStep = "menu";

                    var menu = new ReplyKeyboardMarkup(new[]
                    {
                       new[] { new KeyboardButton("📱 My account"),
                               new KeyboardButton("🛠 Change work categories") },
                       new[] { new KeyboardButton("📍 Change Location") },
                       new[] { new KeyboardButton("🔒 Logout") }
                    })
                    { ResizeKeyboard = true };

                    await botClient.SendMessage(chatId, "⬅️ You are back to the main panel:", replyMarkup: menu, cancellationToken: ct);
                }
                catch (Exception ex)
                {
                    await botClient.SendMessage(chatId, $"❌ Error: {ex.Message}", cancellationToken: ct);
                }
                session.CurrentStep = "menu";
                break;
        }
    }

    private async Task ChangePassword(long chatId, string userInput, Update update, WorkerSession session, CancellationToken ct)
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
                    workerService.ChangePassword(workerViewModel.PhoneNumber, session.Data["old_password"], session.Data["new_password"]);

                    await botClient.SendMessage(chatId, "✅ Password Changed!", cancellationToken: ct);

                    session.CurrentStep = "menu";

                    var menu = new ReplyKeyboardMarkup(new[]
                    {
                       new[] { new KeyboardButton("📱 My account"),
                               new KeyboardButton("🛠 Cange work categories") },
                       new[] { new KeyboardButton("📍 Change Location") },
                       new[] { new KeyboardButton("🔒 Logout") }
                    })
                    { ResizeKeyboard = true };

                    await botClient.SendMessage(chatId, "⬅️ You are back to the main panel:", replyMarkup: menu, cancellationToken: ct);
                }
                catch (Exception ex)
                {
                    await botClient.SendMessage(chatId, $"❌ Error: {ex.Message}", cancellationToken: ct);
                }
                session.CurrentStep = "menu";
                break;
        }
    }

    private async Task Logout(long chatId, CancellationToken ct, WorkerViewModel adminViewModel)
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


