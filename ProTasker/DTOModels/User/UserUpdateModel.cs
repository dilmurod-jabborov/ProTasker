using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ProTasker.Domain.Enum;
using ProTasker.Domain.Models;

namespace ProTasker.DTOModels.User;

public class UserUpdateModel
{
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string PhoneNumber { get; set; }
    public string Bio { get; set; }
    public Location Location { get; set; }
}
