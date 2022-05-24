using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Org.Apps.SystemManager.Presentation.Models
{
    public class AppsOpMetricData
    {
        public List<List<object>> Data { get; set; }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            if (Data != null)
            {
                sb.Append("[");
                foreach (var row in Data)
                {
                    if (row != null)
                    {
                        sb.Append("[");
                        foreach (var col in row)
                        {
                            sb.AppendFormat("'{0}',", col);
                        }
                        sb.Append("],");
                    }
                }
                sb.Append("]");
            }
                

            return sb.Replace(",]", "]").ToString();
        }
    }
}