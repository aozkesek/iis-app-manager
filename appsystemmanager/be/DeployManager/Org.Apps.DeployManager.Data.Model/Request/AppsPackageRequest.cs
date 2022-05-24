using System;
using System.Text;
using System.Collections.Generic;
using System.Runtime.Serialization;

using Org.Apps.BaseDao.Model.Request;

namespace Org.Apps.DeployManager.Data.Model.Request
{
    [Serializable]
    [DataContract(Name = "AppsPackageRequest")]
    public class AppsPackageRequest : BaseRequest
    {
        [DataMember(Name = "AppsPackages")]
        public List<AppsPackage> AppsPackages { get; set; }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder("[ AppsPackages: [");

            foreach (AppsPackage np in AppsPackages)
                sb.Append(np.ToString());

            sb.AppendFormat("], Version: {0}, PageNo: {1}, PageSize: {2}]", Version, PageNo, PageSize);

            return sb.ToString();
        }
    }
}
