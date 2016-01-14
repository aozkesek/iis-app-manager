using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Netas.Nipps.BaseDao.V2;

using Netas.Nipps.ConfigManager.Data.Model;
using Netas.Nipps.ConfigManager.Logic.Intf.V2;

namespace Netas.Nipps.ConfigManager.Logic.Impl.V2
{
    public class NippsParameterLogicV2 : INippsParameterLogicV2
    {
        private IGenericDaoV2<NippsParameter> mNippsParameterDao;

        public IGenericDaoV2<NippsParameter> NippsParameterDao { get { return mNippsParameterDao; } set { mNippsParameterDao = value; } }
        
        public void AddParameter(string categoryName, string parameterName, string parameterValue)
        {
            NippsParameterDao.Add(new NippsParameter { CategoryName = categoryName, ParameterName = parameterName, ParameterValue = parameterValue });
        }

        public void UpdateParameter(string categoryName, string parameterName, string parameterValue)
        {
            NippsParameterDao.Update(new NippsParameter{ CategoryName = categoryName, ParameterName = parameterName, ParameterValue = parameterValue });
        }

        public void RemoveParameter(string categoryName, string parameterName)
        {
            NippsParameterDao.Remove(new NippsParameter{ CategoryName = categoryName, ParameterName = parameterName });
        }

        public NippsParameter GetParameter(string categoryName, string parameterName)
        {
            return NippsParameterDao.GetByT(new NippsParameter { CategoryName = categoryName, ParameterName = parameterName });
        }

        public List<Data.Model.NippsParameter> ListParameter(int pageNo, int pageSize)
        {
            NippsParameterDao.PageSize = pageSize;
            return NippsParameterDao.List(pageNo);
        }

        public List<Data.Model.NippsParameter> ListParameterByCategory(string categoryName)
        {
            return NippsParameterDao.List(new NippsParameter{ CategoryName = categoryName });
        }
    }
}
