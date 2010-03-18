using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.Xml.XPath;

namespace DepCharter
{
  class Project
  {
    public Project(Solution aSolution, StringReader reader, string firstLine)
    {
      solution = aSolution;
      SolutionLine solutionLine = new SolutionLine(firstLine);
      this.id = solutionLine[2][1].ToLower();
      this.name = solutionLine[0][3];
      this.filename = solutionLine[1][1];

      if (Settings.verbose) Console.WriteLine("project: " + this.name + " " + this.id);

      bool addDependencies = false;
      while (true)
      {
        string line = reader.ReadLine();
        if (line == null)
        {
          Console.WriteLine("Unexpected end of solution file! (EndProject line not found)");
          break;
        }
        line = line.Trim();
        if (line.StartsWith("ProjectSection(ProjectDependencies)"))
        {
          addDependencies = true;
        }

        if (addDependencies)
        {
          string depLine = line.ToLower();      // dependency-id's _must_ be treated case-insensitive
          if (depLine.StartsWith("{"))
          {
            string depId = depLine.Substring(0, depLine.IndexOf("}") + 1);
            dependencyIds.Add(depId);
            if (Settings.verbose) Console.WriteLine("dependency: " + depId);
          }
          if (line.StartsWith("EndProjectSection"))
          {
            if (Settings.verbose) Console.WriteLine("EndProjectSection found.\n");
            break;
          }
        }
        if (line.ToLower().Trim() == "endproject")
        {
          if (Settings.verbose) Console.WriteLine("EndProject found.\n");
          break;
        }
      }
    }

    public void writeDepsInDotCodeRecursive(StreamWriter writer)
    {
      writeDepsInDotCode(writer);
      foreach (Project depProject in this.dependencies)
      {
        depProject.writeDepsInDotCodeRecursive(writer);
      }
    }

    public void writeDepsInDotCode(StreamWriter writer)
    {
      if (this.ignore) return;
      String color = "olivedrab1";
      switch (outputType)
      {
        case "exe":
          color = "red";
          break;
        case "dll":
          color = "lightblue";
          break;
        case "lib":
          color = "lightgray";
          break;
      }

      String objectString = String.Format("{0} [shape=box,style=filled,color={1}];", this.name, color);
      writer.WriteLine(objectString);
      foreach (Project depProject in this.dependencies)
      {
        if (depProject.ignore) continue;
        writer.WriteLine(this.name + " -> " + depProject.name);
      }
    }

    public void resolveIds()
    {
      foreach (string id in dependencyIds)
      {
        //Console.WriteLine("resolve: " + id);
        Project dependendProject = solution.projects[id];
        dependencies.Add(dependendProject);
        dependendProject.users.Add(this);
      }
    }

    public void readProjectFile()
    {
      // defaults
      outputName = "";
      outputType = "";
      projectType = "";
      
      FileInfo projectFile = new FileInfo(this.filename);
      if (!projectFile.Exists) return;

      if (projectFile.Name.EndsWith(".csproj"))
      {
        // its a C# project
        projectType = "C#";
        XmlDocument doc = new XmlDocument();
        {
          XmlTextReader reader = new XmlTextReader(this.filename);
          reader.Read();
          doc.Load(reader);
          reader.Close();
        }

        XmlNamespaceManager nsmgr = new XmlNamespaceManager(doc.NameTable);
        nsmgr.AddNamespace("vs", doc.DocumentElement.NamespaceURI);
        outputType = doc.DocumentElement.SelectNodes("/vs:Project/vs:PropertyGroup/vs:OutputType", nsmgr)[0].FirstChild.Value;
        outputName = doc.DocumentElement.SelectNodes("/vs:Project/vs:PropertyGroup/vs:AssemblyName", nsmgr)[0].FirstChild.Value;

      }
      else if (projectFile.Name.EndsWith(".vcproj"))
      {
        // its a C++ project
        projectType = "C++";
        outputName = projectFile.Name.Substring(0, projectFile.Name.IndexOf("."));
        XmlDocument doc = new XmlDocument();
        {
          XmlTextReader reader = new XmlTextReader(this.filename);
          reader.Read();
          doc.Load(reader);
          reader.Close();
        }

        XmlNamespaceManager nsmgr = new XmlNamespaceManager(doc.NameTable);
        nsmgr.AddNamespace("vs", doc.DocumentElement.NamespaceURI);
        XmlNodeList configs = doc.DocumentElement.SelectNodes("/vs:VisualStudioProject/vs:Configurations/vs:Configuration", nsmgr);

        if (configs.Count > 0)
        {
          XmlElement configuration = null;
          foreach (XmlElement config in configs)
          {
            if (config.GetAttribute("Name").ToLower().StartsWith("release|"))
            {
              configuration = config;
              break;
            }
          }
          if (configuration == null)  // no 'release' configuration?, try first configuration that starts with 'r'
          {
            foreach (XmlElement config in configs)
            {
              if (config.GetAttribute("Name").ToLower().StartsWith("r"))
              {
                configuration = config;
                break;
              }
            }
          }
          if (configuration == null)  // no 'r*' configuration?, just default to the first configuration
          {
            configuration = (XmlElement)configs[0];
          }

          // found configuration
          if (configuration.GetAttribute("ManagedExtentions") == "1")
          {
            projectType = "C++/CLI";
          }

          string configurationTypeString = configuration.GetAttribute("ConfigurationType");
          int configurationType = Convert.ToInt32(configurationTypeString);
          switch (configurationType)
          {
            case 0:
              outputType = "makefile";
              break;
            case 1:
              outputType = "exe";
              break;
            case 2:
              outputType = "dll";
              break;
            case 4:
              outputType = "lib"; //static
              break;
            case 10:
              outputType = "utility";
              break;
            default:
              outputType = configurationTypeString;
              break;
          }

          //String name = String.Format("{0}.{1}", this.name, outputType);
          //this.name = name;

          // add GenerateDebugInformation
          Console.WriteLine("projectType: {0}", projectType);
          Console.WriteLine("outputType: {0}", outputType);
          Console.WriteLine("outputName: {0}", outputName);
        }
      }
      else
      {
        projectType = "unknown";
        outputType = "unknown";
        outputName = projectFile.Name;
      }
    }

    public ArrayList dependencyIds = new ArrayList();   // project id's (strings)
    public ArrayList dependencies = new ArrayList();    // project objects
    public ArrayList users = new ArrayList();           // project objects

    public string outputName;   // filename
    public string outputType;   // EXE/DLL/LIB
    public string projectType;  // C#,C++,C++/CLI

    public string name;
    public string filename;
    public string id;
    public bool ignore;
    Solution solution;
  }
}