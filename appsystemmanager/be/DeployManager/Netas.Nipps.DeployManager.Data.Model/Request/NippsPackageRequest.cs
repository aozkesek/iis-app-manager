using System;
using System.Text;
using System.Collections.Generic;
using System.Runtime.Serialization;

using Netas.Nipps.BaseDao.Model.Request;

namespace Netas.Nipps.DeployManager.Data.Model.Request
{
    [Serializable]
    [DataContract(Name = "NippsPackageRequest")]
    public class NippsPackageRequest : BaseRequest
    {
        [DataMember(Name = "NippsPackages")]
        public List<NippsPackage> NippsPackages { get; set; }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder("[ NippsPackages: [");

            foreach (NippsPackage np in NippsPackages)
                sb.Append(np.ToString());

            sb.AppendFormat("], Version: {0}, PageNo: {1}, PageSize: {2}]", Version, PageNo, PageSize);

            return sb.ToString();
        }
    }
}
