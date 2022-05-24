using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.Serialization;

namespace Org.Apps.BaseDao
{
    [Serializable]
    [DataContract]
    public abstract class AbstractModel<T>
    {
        [Key]
        [DatabaseGeneratedAttribute(DatabaseGeneratedOption.Identity)]
        [DataMember(Name = "Id")]
        public Int32 Id { get; set; }

        public abstract void UpdateFrom(T t);
    }
}