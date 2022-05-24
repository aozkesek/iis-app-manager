using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Org.Apps.BaseDao.V2;

using Org.Apps.ConfigManager.Data.Model;
using Org.Apps.ConfigManager.Logic.Intf.V2;

namespace Org.Apps.ConfigManager.Logic.Impl.V2
{
    public class AppsParameterLogicV2 : IAppsParameterLogicV2
    {
        private IGenericDaoV2<AppsParameter> mAppsParameterDao;

        public IGenericDaoV2<AppsParameter> AppsParameterDao { get { return mAppsParameterDao; } set { mAppsParameterDao = value; } }
        
        public void AddParameter(string categoryName, string parameterName, string parameterValue)
        {
            AppsParameterDao.Add(new AppsParameter { CategoryName = categoryName, ParameterName = parameterName, ParameterValue = parameterValue });
        }

        public void UpdateParameter(string categoryName, string parameterName, string parameterValue)
        {
            AppsParameterDao.Update(new AppsParameter{ CategoryName = categoryName, ParameterName = parameterName, ParameterValue = parameterValue });
        }

        public void RemoveParameter(string categoryName, string parameterName)
        {
            AppsParameterDao.Remove(new AppsParameter{ CategoryName = categoryName, ParameterName = parameterName });
        }

        public AppsParameter GetParameter(string categoryName, string parameterName)
        {
            return AppsParameterDao.GetByT(new AppsParameter { CategoryName = categoryName, ParameterName = parameterName });
        }

        public List<Data.Model.AppsParameter> ListParameter(int pageNo, int pageSize)
        {
            AppsParameterDao.PageSize = pageSize;
            return AppsParameterDao.List(pageNo);
        }

        public List<Data.Model.AppsParameter> ListParameterByCategory(string categoryName)
        {
            return AppsParameterDao.List(new AppsParameter{ CategoryName = categoryName });
        }
    }
}
