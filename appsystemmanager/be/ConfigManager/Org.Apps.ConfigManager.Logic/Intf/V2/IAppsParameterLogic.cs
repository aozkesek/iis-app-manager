using Org.Apps.ConfigManager.Data.Model;
using System.Collections.Generic;

namespace Org.Apps.ConfigManager.Logic.Intf.V2
{
    public interface IAppsParameterLogicV2
    {
        void AddParameter(string categoryName, string parameterName, string parameterValue);

        void UpdateParameter(string categoryName, string parameterName, string parameterValue);

        void RemoveParameter(string categoryName, string parameterName);

        AppsParameter GetParameter(string categoryName, string parameterName);

        List<AppsParameter> ListParameter(int pageNo, int pageSize);

        List<AppsParameter> ListParameterByCategory(string categoryName);
    }
}