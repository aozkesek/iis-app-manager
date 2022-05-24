using System;
using System.Runtime.Serialization;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

using Org.Apps.BaseDao;

namespace Org.Apps.AuthManager.Data.Model
{
    [Serializable]
    [DataContract(Name = "User")]
    public class User
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [DataMember(Name = "UserId")]
        public int UserId { get; set; }

        [DataMember(Name = "UserName")]
        public String UserName { get; set; }

        [DataMember(Name = "FirstName")]
        public String FirstName { get; set; }

        [DataMember(Name = "LastName")]
        public String LastName { get; set; }

        [DataMember(Name = "Email")]
        public String Email { get; set; }

        [DataMember(Name = "CreateDate")]
        public DateTime CreateDate { get; set; }

        [DataMember(Name = "UpdateDate")]
        public DateTime UpdateDate { get; set; }

        [DataMember(Name = "PasswordHash")]
        public String PasswordHash { get; set; }

        [DataMember(Name = "InvalidAttemptCount")]
        public int InvalidAttemptCount { get; set; }

        [DataMember(Name = "LastInvalidAttempt")]
        public DateTime LastInvalidAttempt { get; set; }

        [DataMember(Name = "LastSuccessAttempt")]
        public DateTime LastSuccessAttempt { get; set; }

        [DataMember(Name = "PasswordUpdateDate")]
        public DateTime PasswordUpdateDate { get; set; }


        public override string ToString()
        {
            return string.Format(
                "[UserId:{0}, UserName:{1}, FirstName:{2}, LastName:{3}, Email:{4}, CreateDate:{5}, UpdateDate:{6}, InvalidAttemptCount:{7}, LastInvalidAttempt:{8}, LastSuccessAttempt:{9}, PasswordUpdateDate:{10}]", 
                UserId, UserName, FirstName, LastName, Email, CreateDate, UpdateDate, InvalidAttemptCount, LastInvalidAttempt, LastSuccessAttempt, PasswordUpdateDate);
        }
    }
}
