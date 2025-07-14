using ProTasker.Domain.Models;

namespace ProTasker.Data.IRepository;

public interface IUserService
{
    void DeleteUser(string Username);
    void UpdateUser(string Username, string Phonenumber, int age);
    void ChangeUserPassword(string Username, string OldPassword, string NewPassword, string ConfirmPassword);
    string GetUserByUsername(string Username);
    void GetAllWorkers();
    string GetAllWorkersByLocation(string location);
}
