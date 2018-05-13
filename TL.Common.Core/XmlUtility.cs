using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace TL.Common.Core
{
    /// <summary>
    /// xml工具类
    /// </summary>
    public class XmlUtility
    {
        public static string Serialize<T>(T value)
        {
            return Serialize<T>(value, Encoding.UTF8);
        }

        public static string Serialize<T>(T value, Encoding encoding)
        {
            XmlSerializer ser = new XmlSerializer(value.GetType());
            using (MemoryStream mem = new MemoryStream())
            {
                using (XmlTextWriter writer = new XmlTextWriter(mem, encoding))
                {
                    XmlSerializerNamespaces ns = new XmlSerializerNamespaces();
                    ns.Add("", "");
                    ser.Serialize(writer, value, ns);
                    return encoding.GetString(mem.ToArray()).Trim();
                }
            }
        }

        public static T DeSerializer<T>(string xml)
        {
            var obj = default(T);
            using (var strReader = new StringReader(xml))
            {
                var xmlSerialization = new XmlSerializer(typeof(T));
                obj = (T)xmlSerialization.Deserialize(strReader);
            }
            return obj;
        }

        public static string GetConfigValue(string Target, string XmlPath)
        {
            if (!File.Exists(XmlPath))
                return null;
            XmlDocument xdoc = new XmlDocument();
            try
            {
                xdoc.Load(XmlPath);
                XmlElement root = xdoc.DocumentElement;
                XmlNodeList elemList = root.GetElementsByTagName(Target);
                return elemList[0].InnerText;
            }
            catch
            {
                return null;
            }
        }
    }
}
