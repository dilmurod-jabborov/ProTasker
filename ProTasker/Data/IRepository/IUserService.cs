using ProTasker.Domain.Models;

namespace ProTasker.Data.IRepository;

public interface IUserService
{
    void DeleteUser(string Username);
    void UpdateUser(string Username, string Phonenumber, int age);
    void UpdateUserPassword(string Username, string OldPassword, string NewPassword, string ConfirmPassword);
    string GetUserByUsername(string Username);
    List<string> GetAllWorkers();
    List<string> GetAllWorkersByLocation(Location location);
}
