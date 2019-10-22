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
        byte red = 177;
        byte green = 168;
        byte blue = 246;
        public string Name { get; set; }
        public string Date { get; set; }
        public string Description { get; set; }
        public string LargeIcon { get; set; }
        public byte Red {
            get { return red; }
            set { red = value; } }
        public byte Green
        {
            get { return green; }
            set { green = value; }
        }
        public byte Blue
        {
            get { return blue; }
            set { blue = value; }
        }

    }

    [AttributeUsage(AttributeTargets.Method)]
    public class MethodMetaAttribute : Attribute
    {
        public string Name { get; set; }
        public string Date { get; set; }
        public string Control { get; set; }
        public string Init { get; set; }
        public string Category { get; set; }
        public string Description { get; set; }
        public string LargeIcon { get; set; }
    }
}
