//using ProTasker.Constants;
//using ProTasker.Data.IRepository;
//using ProTasker.Domain.Extension;
//using ProTasker.Domain.Models;
//using ProTasker.DTOModels.User;
//using ProTasker.Helpers;

//namespace ProTasker.Data.Repository;

//public class UserService : IUserService
//{
//    public void GetAllWorkers() // bu umuman bu yerda turishi mantiqsiz olib tashla
//    {


//        foreach (var lines in workerList.Workers)
//        {
//            var line = lines.Split('\n');
//            foreach (var item in line)
//            {
//                var workerDetails = item.Split(',');
//                Console.WriteLine($"Id: {workerDetails[0]}," +
//                    $" Username: {workerDetails[1]}," +
//                    $" Category: {workerDetails[2]}," +
//                    $" Gender: {workerDetails[3]}," +
//                    $" Rating: {workerDetails[4]}," +
//                    $" Location: {workerDetails[5]}");
//            }
//        }
//    }

//    public string GetAllWorkersByLocation(Location location) //Bu kerakmas bu yerda o'chir
//    {
//        Checker.CheckerMethod(location.ToString());
//        var workersByLocation = new List<string>();
//        foreach (var lines in workerList.Workers)
//        {
//            var line = lines.Split('\n');
//            foreach (var item in line)
//            {
//                var workerDetails = item.Split(',');
//                if (workerDetails[5] == location.ToString())
//                {
//                    workersByLocation.Add(item);
//                }
//            }
//        }
//        return string.Join("\n", workersByLocation);
//    }

//    public void DeleteUser(string Username)  // Id bo'yicha ishlaymiz to'g'irla
//    {
//        Checker.CheckerMethod(Username);

//        foreach (var lines in lists.Users)
//        {
//            lines.Split('\n');
//            if (lines.Contains(Username))
//            {
//                lists.Users.Remove(lines);
//                Console.WriteLine($"User {Username} has been deleted successfully.");
//                return;
//            }
//        }
//    }

//    public string GetUserByUsername(string Username) // Kerakmas bu
//    {
//        Checker.CheckerMethod(Username);
//        foreach (var lines in lists.Users)
//        {
//            var line = lines.Split('\n');
//            foreach (var item in line)
//            {
//                var userDetails = item.Split(',');
//                if (userDetails[1] == Username)
//                {
//                    return item.ToString();
//                }
//            }
//        }
//        return string.Empty;
//    }

//    public void UpdateUser(UserUpdateModel model)
//    {
//        Checker.CheckerMethod(model.FirstName);
//        Checker.CheckerMethod(model.LastName);

//        var text = FileHelper.ReadFromFile(PathHolder.UsersFilePath);

//        var users = text.TextToObjectList<User>();

//        var existUser = users.Find(u => u.PhoneNumber == model.PhoneNumber);
//        if (existUser == null)
//            throw new Exception("This user is not found!");

//        var updateLines = model.UpdateByObj<UserUpdateModel, User>(users, PathHolder.UsersFilePath, existUser.Id);

//        FileHelper.WriteToFile(PathHolder.UsersFilePath, updateLines);
//    }

//    public void ChangeUserPassword(string OldPassword, string NewPassword, string ConfirmPassword)  // buni ham hatolarini to'g'irla 
//    {
//        Checker.CheckerMethod(Username);
//        if (NewPassword != ConfirmPassword)
//        {
//            throw new Exception("New password and confirm password do not match.");
//        }
//        foreach (var lines in lists.Users)
//        {
//            var userDetails = lines.Split(',');
//            if (userDetails[1] == Username && userDetails[3] == OldPassword)
//            {
//                userDetails[3] = NewPassword;
//                Console.WriteLine($"Password for {Username} has been updated successfully.");
//                return;
//            }
//        }
//    }

//}

