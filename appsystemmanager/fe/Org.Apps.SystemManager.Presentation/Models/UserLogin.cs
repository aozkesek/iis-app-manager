using System.ComponentModel.DataAnnotations;

namespace Org.Apps.SystemManager.Presentation.Models
{
    public class UserLogin
    {
        public string UserName { get; set; }

        public string Password { get; set; }
    }
}