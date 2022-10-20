using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.Xml.XPath;

namespace AISIN_WFA.Util
{
    public class UseXMLConfig
    {
        private void InitXML(string fileName)
        {
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.Load(fileName);

            XPathDocument xPathDoc = new XPathDocument(fileName);
            XPathNavigator xPN = xPathDoc.CreateNavigator();

            XPathExpression xPE = xPN.Compile("xpath"); //opational

            XPathNodeIterator xNodeIter = xPN.Select("xpath");
            while (xNodeIter.MoveNext())
            {
                XPathNavigator tmp = xNodeIter.Current.Clone();
                //if( tmp.Value )
            }
        }

        public static string GetValueXPath(string xPath, string fileName = "Config.xml")
        {// "//title[@lang='en']"
            string rtn = string.Empty;

            XPathDocument xPathDoc = new XPathDocument(fileName);
            XPathNavigator xPN = xPathDoc.CreateNavigator();

            foreach (XPathNavigator tmp in xPN.Select(xPath))
            {
                rtn = tmp.Value;
            }

            return rtn;
        }

        public static List<string> GetValueXPathList(string xPath, string fileName)
        {
            List<string> list = new List<string>();

            XPathDocument xPathDoc = new XPathDocument(fileName);
            XPathNavigator xPN = xPathDoc.CreateNavigator();

            foreach (XPathNavigator tmp in xPN.Select(xPath))
            {
                list.Add(tmp.Value);
            }

            return list;
        }

        public static Hashtable GetValueXPathTable(string xPath, string fileName)
        {
            Hashtable hash = new Hashtable();

            XPathDocument xPathDoc = new XPathDocument(fileName);
            XPathNavigator xPN = xPathDoc.CreateNavigator();

            foreach (XPathNavigator tmp in xPN.Select(xPath))
            {
                hash.Add(tmp.Name, tmp.Value);
            }

            return hash;
        }

        public static string GetAttrXPath(string xPath, string attrName, string fileName = "Config.xml")
        {
            string rtn = string.Empty;

            XPathDocument xPathDoc = new XPathDocument(fileName);
            XPathNavigator xPN = xPathDoc.CreateNavigator();

            foreach (XPathNavigator tmp in xPN.Select(xPath))
            {
                if (tmp.HasAttributes)
                    rtn = tmp.GetAttribute(attrName, "");
            }

            return rtn;
        }

        public static List<string> GetAttrXPathList(string xPath, string attrName, string fileName = "Config.xml")
        {
            List<string> list = new List<string>();

            XPathDocument xPathDoc = new XPathDocument(fileName);
            XPathNavigator xPN = xPathDoc.CreateNavigator();

            foreach (XPathNavigator tmp in xPN.Select(xPath))
            {
                if (tmp.HasAttributes)
                    list.Add(tmp.GetAttribute(attrName, ""));
                else
                    list.Add("");
            }

            return list;
        }

        public delegate void AttributeParser(Dictionary<string, string> otherAttrs);
        public static List<Dictionary<string, string>> GetAttrsXPathTable(string xPath, AttributeParser parser, string fileName)
        {
            List<Dictionary<string, string>> retList = null;

            if (parser == null) retList = new List<Dictionary<string, string>>();

            XPathDocument xPathDoc = new XPathDocument(fileName);
            XPathNavigator xPN = xPathDoc.CreateNavigator();
            XPathNodeIterator nodes = xPN.Select(xPath);

            while (nodes.MoveNext())
            {
                XPathNavigator attrNavigator = nodes.Current.Clone();
                attrNavigator.MoveToFirstAttribute();
                Dictionary<string, string> innerTable = new Dictionary<string, string>();
                innerTable.Add(attrNavigator.Name, attrNavigator.Value);

                while (attrNavigator.MoveToNextAttribute())
                {
                    innerTable.Add(attrNavigator.Name, attrNavigator.Value);
                }

                if (parser == null)
                    retList.Add(innerTable);
                else
                    parser.Invoke(innerTable);
            }

            return retList;
        }

        public static string GetDBConnectionString(bool isDebugConnection, string fileName = "Config.xml")
        {
            string rtn = string.Empty;
            if (isDebugConnection)
            {
                rtn = GetValueXPath("//ConnectionString[@local='true']", fileName);
            }
            else
            {
                rtn = GetValueXPath("//ConnectionString[@local='false']", fileName);
            }

            return rtn;
        }

        public static void SetAttrXPath(string xPath, string attrName, string attrValue, string fileName = "Config.xml")
        {
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.Load(fileName);

            XmlElement node = (XmlElement)xmlDoc.SelectSingleNode(xPath);
            node.SetAttribute(attrName, attrValue);

            xmlDoc.Save(fileName);
        }

        public static void SetAttrToAllXPath(string xPath, string attrName, string attrValue, string fileName = "Config.xml")
        {
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.Load(fileName);

            XmlNodeList list = xmlDoc.SelectNodes(xPath);
            foreach (XmlElement node in list)
            {
                node.SetAttribute(attrName, attrValue);
            }

            xmlDoc.Save(fileName);
        }

        public static void SetAttrXPathList(string xPath, string attrName, string[] attrValue, string fileName = "Config.xml")
        {
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.Load(fileName);

            XmlNodeList list = xmlDoc.SelectNodes(xPath);
            for (int i = 0; i < attrValue.Length; i++)
            {
                XmlElement node = (XmlElement)list[i];
                node.SetAttribute(attrName, attrValue[i]);
            }

            xmlDoc.Save(fileName);
        }

        public static void SetValueXPath(string xPath, object[] value, string fileName = "Config.xml")
        {
            // xpath = //AppConfig/Process[@name='BOT']/Selected

            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.Load(fileName);

            XPathNavigator xPN = xmlDoc.CreateNavigator();

            try
            {
                bool isChanged = false;
                int idx = 0;
                foreach (XPathNavigator node in xPN.Select(xPath))
                {
                    if (!node.Value.ToUpper().Equals(value[idx].ToString().ToUpper()))
                    {
                        node.SetValue(value[idx].ToString());
                        isChanged = true;
                    }

                    idx++;
                }

                if (isChanged) xmlDoc.Save(fileName);
            }
            catch
            {
                HLog.log(HLog.eLog.EXCEPTION, $"XML Writing Failed. path={xPath}, value={value}");
            }
        }

        public static void AddElementXPath(string xPath, Dictionary<string, string> attrs, KeyValuePair<string, string> value, string fileName = "Config.xml")
        {
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.Load(fileName);

            try
            {
                XmlElement selectedNode = (XmlElement)xmlDoc.SelectSingleNode(xPath);
                if (selectedNode == null)
                {
                    HLog.log(HLog.eLog.ERROR, $"Adding - Cannot find node in xml. {xPath}");
                    return;
                }
                XmlNode newNode = xmlDoc.CreateNode(XmlNodeType.Element, value.Key, "");
                if (!string.IsNullOrWhiteSpace(value.Value)) newNode.InnerText = value.Value;

                if (attrs != null)
                {
                    foreach (KeyValuePair<string, string> attr in attrs)
                    {
                        XmlAttribute att = xmlDoc.CreateAttribute(attr.Key);
                        att.Value = attr.Value;
                        newNode.Attributes.Append(att);
                    }
                }

                selectedNode.AppendChild(newNode);
                xmlDoc.Save(fileName);
            }
            catch
            {
                HLog.log(HLog.eLog.EXCEPTION, $"XML Writing Failed. path={xPath}, value={value}");
            }
        }

        public static void RemoveElementXPath(string xPath, string fileName = "Config.xml", bool logForNotFound = true)
        {
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.Load(fileName);

            try
            {
                bool bSave = false;
                XmlNodeList xmlNodes = xmlDoc.SelectNodes(xPath);
                if (xmlNodes.Count < 1)
                {
                    if (logForNotFound)
                        HLog.log(HLog.eLog.ERROR, $"Removing - Cannot find node in xml. {xPath}");
                    return;
                }

                foreach (XmlNode node in xmlNodes)
                {
                    XmlNode parent = node.ParentNode;
                    parent.RemoveChild(node);

                    bSave = true;
                }

                if (bSave) xmlDoc.Save(fileName);
            }
            catch
            {
                HLog.log(HLog.eLog.EXCEPTION, $"XML Writing Failed. path={xPath}");
            }

        }

    }
}
