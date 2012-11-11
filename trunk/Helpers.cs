using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.Xml.XPath;

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

    public ArrayList particles = new ArrayList();
    }

    class ProjectDictionary : Dictionary<string, Project> { }

    class MyStringReader : StringReader
    {
    public MyStringReader(string input) : base(input) { }

    static int linenumber = 1;
    public override string ReadLine()
    {
      string line = base.ReadLine();
      if (Settings.verbose) Console.WriteLine(linenumber + ": " + line);
      linenumber++;
      return line;
    }
  }

  class XmlContext : XmlDocument
  {
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
    public XmlNodeList GetNodes(string xpath)         
    {
        XmlNodeList result = null;
        if (NamespaceManager == null)
        {
            result = base.SelectNodes(xpath);
        }
        else
        {
            result = base.SelectNodes(xpath, NamespaceManager);
        }
        return result;
    }

    public string GetNodeContent(string xpath, string defaultvalue)         
    {
        string result = defaultvalue;
        XmlNodeList list = GetNodes(xpath);
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
}