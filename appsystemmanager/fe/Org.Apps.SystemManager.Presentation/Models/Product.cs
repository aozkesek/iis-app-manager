using System.Collections.Generic;

namespace Org.Apps.SystemManager.Presentation.Models
{
    public class Product
    {
        public Product()
        {
            Components = new List<ProductComponent>();
        }

        public string Name { get; set; }

        public string Version { get; set; }

        public string ApplicationPool { get; set; }

        public IEnumerable<ProductComponent> Components { get; private set; }
    }
}