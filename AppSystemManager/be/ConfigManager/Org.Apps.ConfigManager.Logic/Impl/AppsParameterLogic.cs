using Org.Apps.Aspect;
using Org.Apps.BaseDao;
using Org.Apps.ConfigManager.Data.Model;
using Org.Apps.ConfigManager.Logic.Intf;
using System;
using System.Collections.Generic;

namespace Org.Apps.ConfigManager.Logic.Impl
{
    public class AppsParameterLogic : IAppsParameterLogic
    {
        private IGenericDao<SystemParameter> mSystemParameterDao;
        private IGenericDao<ParameterCategory> mParameterCategoryDao;

        public IGenericDao<ParameterCategory> ParameterCategoryDao { get { return mParameterCategoryDao; } set { mParameterCategoryDao = value; } }

        public IGenericDao<SystemParameter> SystemParameterDao { get { return mSystemParameterDao; } set { mSystemParameterDao = value; } }

        [PerformanceLoggingAdvice]
        public void AddParameter(string categoryName, string parameterName, string parameterValue)
        {
            ParameterCategory parameterCategory;
            //check categoryname is exist, if not add
            try
            {
                parameterCategory = ParameterCategoryDao.GetByName(categoryName);
            }
            catch (NoDataFoundException nfe)
            {
                parameterCategory = new ParameterCategory()
                {
                    CategoryName = categoryName
                };

                ParameterCategoryDao.Add(parameterCategory);
            }
            catch (Exception ex)
            {
                throw ex;
            }

            SystemParameter systemParameter = new SystemParameter()
            {
                CategoryId = parameterCategory.CategoryId,
                ParameterName = parameterName,
                ParameterValue = parameterValue
            };

            SystemParameterDao.Add(systemParameter);
        }

        [PerformanceLoggingAdvice]
        public void UpdateParameter(string categoryName, string parameterName, string value)
        {
            SystemParameter systemParameter = GetSystemParameterByName(categoryName, parameterName);
            systemParameter.ParameterValue = value;
            SystemParameterDao.Update(systemParameter);
        }

        [PerformanceLoggingAdvice]
        public void RemoveParameter(string categoryName, string parameterName)
        {
            SystemParameter systemParameter = GetSystemParameterByName(categoryName, parameterName);
            SystemParameterDao.Remove(systemParameter);
        }

        [PerformanceLoggingAdvice]
        public string GetParameter(string categoryName, string parameterName)
        {
            SystemParameter systemParameter = GetSystemParameterByName(categoryName, parameterName);
            return systemParameter.ParameterValue;
        }

        [PerformanceLoggingAdvice]
        public List<AppsParameter> ListParameter(int pageNo, int pageSize)
        {
            List<AppsParameter> listParameters = new List<AppsParameter>();

            if (pageSize > 0)
                SystemParameterDao.PageSize = pageSize;
            if (pageNo < 1)
                pageNo = 1;

            List<SystemParameter> systemParameters = SystemParameterDao.List(pageNo);
            foreach (SystemParameter systemParameter in systemParameters)
            {
                listParameters.Add(
                    new AppsParameter()
                    {
                        CategoryName = ParameterCategoryDao.Get(systemParameter.CategoryId).CategoryName,
                        ParameterName = systemParameter.ParameterName,
                        ParameterValue = systemParameter.ParameterValue,
                        CreateDate = systemParameter.CreateDate,
                        UpdateDate = systemParameter.UpdateDate
                    });
            }
            return listParameters;
        }

        [PerformanceLoggingAdvice]
        public List<AppsParameter> ListParameterByCategory(string categoryName)
        {
            List<SystemParameter> systemParameters = SystemParameterDao.ListByCategory(categoryName);
            List<AppsParameter> listParameters = new List<AppsParameter>();
            foreach (SystemParameter systemParameter in systemParameters)
            {
                listParameters.Add(
                    new AppsParameter()
                    {
                        CategoryName = categoryName, //no need to call service to get name already have
                        ParameterName = systemParameter.ParameterName,
                        ParameterValue = systemParameter.ParameterValue,
                        CreateDate = systemParameter.CreateDate,
                        UpdateDate = systemParameter.UpdateDate
                    });
            }
            return listParameters;
        }

        private SystemParameter GetSystemParameterByName(string categoryName, string parameterName)
        {
            ParameterCategory parameterCategory = ParameterCategoryDao.GetByName(categoryName);
            SystemParameter systemParameter = new SystemParameter()
            {
                CategoryId = parameterCategory.CategoryId,
                ParameterName = parameterName
            };
            return SystemParameterDao.GetByT(systemParameter);
        }
    }
}