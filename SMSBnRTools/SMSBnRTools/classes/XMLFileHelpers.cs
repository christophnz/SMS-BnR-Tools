using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Serialization;

namespace SMSBnRTools.classes
{
    public struct XMLReadResult
    {
        public bool success { get; set; }
        public List<contact> contacts { get; set; }
    }

    public static class XMLFileHelpers
    {
        public static XMLReadResult ReadXml(string filename)
        {
            XMLReadResult res = new XMLReadResult();

            XmlSerializer serializer = new XmlSerializer(typeof(smses));
            // A FileStream is needed to read the XML document.
            FileStream fs = new FileStream(filename, FileMode.Open);

            // Declare an object variable of the type to be deserialized.
            smses SMSObj;

            try
            {
                // Deserialize the object.
                SMSObj = (smses)serializer.Deserialize(fs);
                res.success = true;

            }
            catch(Exception)
            {
                res.success = false;
                return res;
            }
            fs.Close();

            if (SMSObj.count > 0)
            {
                res.contacts = SMSObj.Items.OfType<smsesSms>().GroupBy(x => new { x.address, x.contact_name })
                    .Select(y => new contact()
                    {
                        address = y.Key.address,
                        contact_name = y.Key.contact_name,
                        smses = y.ToList()
                    }
                    ).OrderBy(o => o.contact_name).ThenBy(p => p.address).ToList();
                res.contacts = contact.ConsolidateContacts(res.contacts);
            }
            else
            {
                res.contacts = new List<contact>();
            }
            return res;
        }

        public static void SaveXml(string filename, smses s)
        {
            XmlSerializer serializer = new XmlSerializer
                (typeof(smses), "http://www.cpandl.com");

            XmlWriterSettings xmlWriterSettings = new XmlWriterSettings()
            {
                Indent = true,
                IndentChars = "  ",
                NewLineOnAttributes = false
            };

            using (XmlWriter w = XmlWriter.Create(filename, xmlWriterSettings))
            {
                w.WriteProcessingInstruction("xml-stylesheet", "type=\"text/xsl\" href=\"sms.xsl\"");
                serializer.Serialize(w, s);
            }
        }
    }
}
