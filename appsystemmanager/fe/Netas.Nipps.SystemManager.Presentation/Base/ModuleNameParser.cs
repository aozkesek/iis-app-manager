using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Netas.Nipps.SystemManager.Presentation.Base
{
    public class ModuleNameParser
    {
        public string Host;
        public string Site;
        public string Application;
        public string Service;

        public ModuleNameParser(string source)
        {
            try
            {
                string[] moduleNameParts = source.Split('>');
                string[] serviceNameParts = moduleNameParts[2].Split('.');
                Host = moduleNameParts[0];
                Site = moduleNameParts[1];
                Application = moduleNameParts[2];
                Service = serviceNameParts[serviceNameParts.Length - 1];
            }
            catch (Exception ex) { }
            
        }

        public override string ToString()
        {
            return string.Format("[Host={0}, Site={1}, Application={2}, Service={3}]", Host, Site, Application, Service);
        }
    }
}