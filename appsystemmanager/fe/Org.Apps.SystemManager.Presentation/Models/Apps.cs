using Microsoft.Web.Administration;

namespace Org.Apps.SystemManager.Presentation.Models
{
    public class Apps
    {
        public string HostName { get; set; }

        public string SiteName { get; set; }

        public string ApplicationPoolName { get; set; }

        public string ApplicationName { get; set; }

        public string Version { get; set; }

        public ObjectState State { get; set; }

        public string PhysicalPath { get; set; }

        public int ModuleId { get; set; }

        public override string ToString()
        {
            return string.Format("[ModuleId: {0}, ApplicationName: {1}, Version: {2}, HostName: {3}, SiteName: {4}, ApplicationPoolName: {5}, State: {6}, PhysicalPath: {7}]",
                ModuleId, ApplicationName, Version, HostName, SiteName, ApplicationPoolName, State, PhysicalPath);
        }

    }
}