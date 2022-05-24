using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Org.Apps.ConfigManager.Data.Model
{
    [Serializable]
    public class SystemParameter
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ParameterId { get; set; }

        public String ParameterName { get; set; }

        public Int32 CategoryId { get; set; }

        public DateTime CreateDate { get; set; }

        public DateTime UpdateDate { get; set; }

        public String ParameterValue { get; set; }

        public void UpdateFrom(SystemParameter t)
        {
            ParameterName = t.ParameterName;
            CategoryId = t.CategoryId;
            ParameterValue = t.ParameterValue;
            UpdateDate = DateTime.Now;
        }

        public override String ToString()
        {
            return String.Format("[ParameterId:{0}, ParameterName:{1}, CategoryId:{2}, CreateDate:{3}, LastUpdateDate:{4}, ParameterValue:{5}]",
                ParameterId, ParameterName, CategoryId, CreateDate, UpdateDate, ParameterValue);
        }
    }
}