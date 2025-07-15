using ProTasker.Domain.Models;
using ProTasker.DTOModels.User;

namespace ProTasker.Data.IRepository;

public interface IUserService
{
    void DeleteUser(int id);
    void UpdateUser(UserUpdateModel model);
    void ChangeUserPassword(string phoneNumber, string oldPassword, string newPassword);
    void Register(UserRegisterModel model);
    UserViewModel Login(string phoneNumber, string password);

    UserViewModel GetUser(int id);

    List<User> GetAll();
}
