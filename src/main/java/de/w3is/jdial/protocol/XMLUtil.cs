using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace de.w3is.jdial.protocol
{
    static class XMLUtil
    {
        public static String getTextFromSub(this XmlDocument element, String tagName)
        {

            XmlNodeList elementsByTagName = element.GetElementsByTagName(tagName);

            if (elementsByTagName.Count >= 1)
            {
                return elementsByTagName.Item(0).InnerText;
            }

            return "";
        }
    }
}
