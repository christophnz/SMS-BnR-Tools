namespace SMSBnRTools.classes
{
    public abstract class IMobileMessage
    {
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public abstract string address { get; set; }
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public abstract string contact_name { get; set; }
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public abstract ulong date { get; set; }
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public abstract string readable_date { get; set; }
        public abstract string bodyText { get; }
        public abstract byte sentReceived { get; }
    }
}
