using System.Collections.Generic;
using System.Linq;

namespace SMSBnRTools.classes
{
    public class contact
    {
        public const string UNKNOWN_NAME = "(Unknown)";
        public string address { get; set; }
        public string contact_name { get; set; }
        public List<IMobileMessage> messages { get; set; }

        /// <summary>
        /// merges messages from addresses with and without country code
        /// </summary>
        /// <param name="contax"></param>
        /// <returns></returns>
        public static List<contact> ConsolidateContacts(List<contact> contax)
        {
            // use new list as collections cannot be modified within foreach
            List<contact> toConsolidate = contax.Where(x => x.address.StartsWith("0") && x.address.Length > 1).ToList();
            foreach (var c in toConsolidate)
            {
                if (contax.Any(x => !x.address.StartsWith("0") && x.address.EndsWith(c.address.Substring(1))))
                {
                    contact cMerge = contax.First(x => !x.address.StartsWith("0") && x.address.EndsWith(c.address.Substring(1)));
                    cMerge.messages.AddRange(c.messages);
                    cMerge.messages = cMerge.messages.OrderBy(o => o.date).ToList();
                    contax.Remove(c);
                }
            }
            return contax;
        }
    }
}
