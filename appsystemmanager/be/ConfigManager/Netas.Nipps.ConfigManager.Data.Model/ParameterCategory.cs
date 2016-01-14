using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.Serialization;

namespace Netas.Nipps.ConfigManager.Data.Model
{
    [Serializable]
    [DataContract(Name = "ParameterCategory")]
    public class ParameterCategory
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [DataMember(Name = "CategoryId")]
        public int CategoryId { get; set; }

        [DataMember(Name = "CategoryName")]
        public String CategoryName { get; set; }

        [DataMember(Name = "CreateDate")]
        public DateTime CreateDate { get; set; }

        [DataMember(Name = "UpdateDate")]
        public DateTime UpdateDate { get; set; }

        public void UpdateFrom(ParameterCategory t)
        {
            CategoryName = t.CategoryName;
            UpdateDate = DateTime.Now;
        }

        public override String ToString()
        {
            return String.Format("[Id:{0}, Name:{1}, CreateDate:{2}, LastUpdateDate:{3}]",
                CategoryId, CategoryName, CreateDate, UpdateDate);
        }
    }
}