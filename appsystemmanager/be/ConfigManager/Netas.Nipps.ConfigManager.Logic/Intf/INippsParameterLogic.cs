using Netas.Nipps.ConfigManager.Data.Model;
using System.Collections.Generic;

namespace Netas.Nipps.ConfigManager.Logic.Intf
{
    public interface INippsParameterLogic
    {
        void AddParameter(string categoryName, string parameterName, string parameterValue);

        void UpdateParameter(string categoryName, string parameterName, string parameterValue);

        void RemoveParameter(string categoryName, string parameterName);

        string GetParameter(string categoryName, string parameterName);

        List<NippsParameter> ListParameter(int pageNo, int pageSize);

        List<NippsParameter> ListParameterByCategory(string categoryName);
    }
}