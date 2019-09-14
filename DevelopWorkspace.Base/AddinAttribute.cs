using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DevelopWorkspace.Base
{
    [AttributeUsage(AttributeTargets.Class)]
    public class AddinMetaAttribute : Attribute
    {
        public string Name { get; set; }
        public string Date { get; set; }
        public string Description { get; set; }
    }

    [AttributeUsage(AttributeTargets.Method)]
    public class MethodMetaAttribute : Attribute
    {
        public string Name { get; set; }
        public string Date { get; set; }
        public string Description { get; set; }
    }
}
