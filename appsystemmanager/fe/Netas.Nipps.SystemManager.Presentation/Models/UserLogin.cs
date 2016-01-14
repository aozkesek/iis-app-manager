using System.ComponentModel.DataAnnotations;

namespace Netas.Nipps.SystemManager.Presentation.Models
{
    public class UserLogin
    {
        public string UserName { get; set; }

        public string Password { get; set; }
    }
}