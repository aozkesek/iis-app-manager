using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

using Netas.Nipps.BaseDao.Model.Response;

namespace Netas.Nipps.AuthManager.Data.Model.Response
{
    [Serializable]
    [DataContract(Name="UserResponse")]
    public class UserResponse : BaseResponse
    {
        [DataMember(Name = "Users")]
        public List<User> Users { get; set; }
    }
}
