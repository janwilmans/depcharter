using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.Xml.XPath;
using System.Runtime.InteropServices;

namespace DepCharter
{
    class SolutionParticle
    {
        public SolutionParticle(string content)
        {
            char[] charSeparators = { '"', '(', ')' };
            string[] result = content.Split(charSeparators, StringSplitOptions.RemoveEmptyEntries);
            values = new ArrayList(result);
        }

        public string this[int index]
        {
            get
            {
                return (string)values[index];
            }
        }
        public ArrayList values = new ArrayList();
    }

    class SolutionLine
    {
        public SolutionLine(string line)
        {
            char[] charSeparators = { ',' };
            string[] result = line.Split(charSeparators, StringSplitOptions.RemoveEmptyEntries);
            foreach (string content in result)
            {
                particles.Add(new SolutionParticle(content));
            }
        }

        public SolutionParticle this[int index]
        {
            get
            {
                return (SolutionParticle)particles[index];
            }
        }

        //public SolutionProjectType Type
        //{
        //    get { return Project(\"{2150E333-8FDC-42A3-9474-1A3956D46DE8}\")}
        //}

        public ArrayList particles = new ArrayList();
    }

    class MyStringReader : StringReader
    {
        public MyStringReader(string input) : base(input) { }

        static int linenumber = 1;
        public override string ReadLine()
        {
            string line = base.ReadLine();
            //if (Settings.verbose) Console.WriteLine(linenumber + ": " + line);
            linenumber++;
            return line;
        }
    }

    class XmlContext : XmlDocument
    {
        [DllImport("kernel32.dll")]
        public static extern void OutputDebugString(string lpOutputString);

        public XmlContext()
            : base()
        {
        }

        public void SetNamespace(string prefix, string uri)
        {
            NamespaceManager = new XmlNamespaceManager(this.NameTable);
            NamespaceManager.AddNamespace(prefix, uri);
        }

        // mask the normal SelectNode method and replace it 
        // with one the incorporates a pre-set namespace
        public List<XmlNode> GetNodes(string xpath)
        {
            XmlNodeList list = null;
            try
            {
                if (NamespaceManager == null)
                {
                    list = base.SelectNodes(xpath);
                }
                else
                {
                    list = base.SelectNodes(xpath, NamespaceManager);
                }
            }
            catch (System.Xml.XPath.XPathException)
            {
                OutputDebugString("xpath: " + xpath + " not found.");
            }

            List<XmlNode> result = new List<XmlNode>();
            if (list == null) return result;
            foreach (XmlNode xmlnode in list)
            {
                result.Add(xmlnode);
            }
            return result;
        }

        public string GetNodeContent(string xpath, string defaultvalue)
        {
            string result = defaultvalue;
            var list = GetNodes(xpath);
            if (list.Count > 0)
            {
                result = list[0].InnerXml;
            }
            return result;
        }

        public string GetNodeContent(XmlElement list, string nodename, string defaultvalue)
        {
            string result = defaultvalue;
            XmlNode node = list[nodename];
            if (node != null)
            {
                result = node.InnerText;
            }
            return result;
        }

        public XmlNamespaceManager NamespaceManager;
    }

    class StringMap : Dictionary<string, string> { }


    public static class ExtensionMethods
    {
        public static List<XmlNode> ToList(this IEnumerable list)
        {
            List<XmlNode> result = new List<XmlNode>();
            foreach (XmlNode item in list)
            {
                result.Add(item);
            }
            return result;
        }
    }
}

namespace System.Runtime.CompilerServices
{
    [AttributeUsage(AttributeTargets.Assembly | AttributeTargets.Class
         | AttributeTargets.Method)]
    public sealed class ExtensionAttribute : Attribute { }
}

