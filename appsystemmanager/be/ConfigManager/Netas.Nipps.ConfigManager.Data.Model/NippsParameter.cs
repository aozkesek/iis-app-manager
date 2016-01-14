using System;
using System.Runtime.Serialization;

namespace Netas.Nipps.ConfigManager.Data.Model
{
    [Serializable]
    [DataContract(Name = "NippsParameter")]
    public class NippsParameter
    {
        [DataMember(Name = "CategoryName")]
        public string CategoryName { get; set; }

        [DataMember(Name = "ParameterName")]
        public string ParameterName { get; set; }

        [DataMember(Name = "ParameterValue")]
        public string ParameterValue { get; set; }

        [DataMember(Name = "CreateDate")]
        public DateTime CreateDate { get; set; }

        [DataMember(Name = "UpdateDate")]
        public DateTime UpdateDate { get; set; }

        public override string ToString()
        {
            return string.Format(
                "[CategoryName:{0}; ParameterName:{1}; ParameterValue:{2}; CreateDate:{3}; UpdateDate:{4}]",
                CategoryName, ParameterName, ParameterValue, CreateDate, UpdateDate
                );
        }
    }
}