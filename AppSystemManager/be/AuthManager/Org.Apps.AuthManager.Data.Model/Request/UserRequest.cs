using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

using Org.Apps.BaseDao.Model.Request;

namespace Org.Apps.AuthManager.Data.Model.Request
{
    [Serializable]
    [DataContract(Name = "UserRequest")]
    public class UserRequest : BaseRequest
    {
        [DataMember(Name = "NewPasswordHash")]
        public string NewPasswordHash { get; set; }
        
        [DataMember(Name = "Users")]
        public List<User> Users { get; set; }
    }
}
