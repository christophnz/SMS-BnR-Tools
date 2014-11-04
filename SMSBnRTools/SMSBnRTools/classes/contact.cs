using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SMSBnRTools.classes
{
    class contact
    {
        public const string UNKNOWN_NAME = "(Unknown)";
        public string address { get; set; }
        public string contact_name { get; set; }
        public List<smsesSms> smses { get; set; }
    }
}
