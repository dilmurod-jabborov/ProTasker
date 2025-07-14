using ProTasker.Domain.Models;
using ProTasker.DTOModels.User;

namespace ProTasker.Data.IRepository;

public interface IUserService
{
    void DeleteUser(User Id);
    void UpdateUser(UserUpdateModel model);
    void ChangeUserPassword(UserPasswordUpdate passwordUpdate);
    void Register(UserRegissterModel model);
    void Login(string phoneNumber, string password);
}
