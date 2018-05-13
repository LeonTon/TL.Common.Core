using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;

namespace TCFront.FlightCommon.Core.Configuration
{
    /// <summary>
    /// 配置文件帮助类
    /// </summary>
    public class ConfigUtility
    {
        public static void Configurate(string path)
        {
            try
            {
                FileInfo configFile = new FileInfo(path);
                string configString;

                if (!configFile.Exists)
                {
                    throw new Exception("未读取到配置文件");
                }
                using (StreamReader reader = configFile.OpenText())
                {
                    configString = reader.ReadToEnd();
                }
                if (string.IsNullOrWhiteSpace(configString))
                {
                    throw new Exception("配置文件内容为空");
                }
                ReadConfig(configString);
            }
            catch (Exception ex)
            {
                throw new Exception("读取配置文件失败" + ex.Message);
            }
        }

        public static void ReadConfig(string configString)
        {
            XmlDocument xmlDoc = new XmlDocument();
            try
            {
                try
                {
                    xmlDoc.LoadXml(configString);
                }
                catch (Exception ex)
                {
                    throw new Exception("配置文件内容不是正确的XML," + ex.Message);
                }
                XmlNode nodeConfig = xmlDoc.SelectSingleNode("Flight.Front.Interface");
                if ((nodeConfig == null) || string.IsNullOrWhiteSpace(nodeConfig.InnerText))
                {
                    throw new Exception("配置文件内容为空");
                }

                XmlNodeList urlList = nodeConfig.SelectNodes("InterfaceAPI/UrlList/UrlConfig");
                if ((urlList == null) || (urlList.Count == 0))
                {
                    throw new Exception("未配置接口地址");
                }
                foreach (XmlNode urlNode in urlList)
                {
                    XmlNode urlkey = urlNode.SelectSingleNode("UrlKey") ?? urlNode.SelectSingleNode("UrlKey");
                    if ((urlkey == null) || string.IsNullOrWhiteSpace(urlkey.InnerText))
                    {
                        throw new Exception("UrlConfig配置中UrlKey不能为空");
                    }
                    XmlNode val = urlNode.SelectSingleNode("Url");
                    if ((val == null) || string.IsNullOrWhiteSpace(val.InnerText))
                    {
                        throw new Exception("UrlConfig配置中Url不能为空");
                    }
                }
            }
            catch (Exception ex)
            {
                //TODO: 发送邮件
            }
        }

       
    }
}
