namespace ProTasker.DTOModels.User;

public class UserPasswordUpdate
{
    public string UserName { get; set; }
    public string OldPassword { get; set; }
    public string NewPassword { get; set; }
    public string ConfirmPassword { get; set; }
}
