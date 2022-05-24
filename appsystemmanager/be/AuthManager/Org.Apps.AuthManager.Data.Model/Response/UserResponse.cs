using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

using Org.Apps.BaseDao.Model.Response;

namespace Org.Apps.AuthManager.Data.Model.Response
{
    [Serializable]
    [DataContract(Name="UserResponse")]
    public class UserResponse : BaseResponse
    {
        [DataMember(Name = "Users")]
        public List<User> Users { get; set; }
    }
}
