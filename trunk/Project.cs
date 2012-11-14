// Project.cs read Visual Studio Project files, unlike the .sln files, these are XML files.

using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.Xml.XPath;

namespace DepCharter
{

  enum ConfigurationType
  {
      Makefile = 0, 
      Application = 1,
      DynamicLibrary = 2,
      StaticLibrary = 4,
      Utility = 10,
      Unexpected = -1,
  }

  class Project
  {
    public Project(Solution aSolution, string firstLine)
    {
      solution = aSolution;
      SolutionLine solutionLine = new SolutionLine(firstLine);
      this.id = solutionLine[2][1].ToLower();       // store in lower-case, _must_ be treated case-insensitive
      this.name = solutionLine[0][3];
      this.filename = Path.GetFullPath(Path.GetDirectoryName(solution.fullname) + @"\" + solutionLine[1][1]);

      if (Settings.verbose) Console.WriteLine("project: " + this.name + " " + this.id);
    }

    public Project(Solution aSolution)
    {
      solution = aSolution;
    }

    static public Project createDummyProject(Solution solution, string name)
    {
      Project dummy = new Project(solution);
      dummy.id = Guid.NewGuid().ToString();
      dummy.name = name;
      dummy.filename = name + ".dummy";      
      return dummy;
    }

    // read project-dependencies part from the .sln file
    public void readDependencies(StringReader reader)
    {
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
        case ConfigurationType.Application:
          color = "red";
          break;
        case ConfigurationType.DynamicLibrary:
          color = "lightblue";
          break;
        case ConfigurationType.StaticLibrary:
          color = "lightgray";
          break;
      }

      String objectString = String.Format("\"{0}\" [shape=box,style=filled,color={1}];", this.name, color);
      writer.WriteLine(objectString);
      
      foreach (Project depProject in this.dependencies)
      {
        if (depProject.ignore) continue;
        writer.WriteLine("\"" + this.name + "\" -> \"" + depProject.name + "\"");
      }

      /*
      /// THIS IS TEST-ONLY CODE, the projects should be created already, before writeDepsInDotCode() is called
      // write dependencies from FEI User Properties
      if (this.userProperties.ContainsKey("ProjectUses"))
      {
          string[] list = userProperties["ProjectUses"].Split(';');
          foreach (string projectName in list )
          {
            if (string.IsNullOrEmpty(projectName)) continue;
            //if (depProject.ignore) continue;
            writer.WriteLine("\"" + this.name + "\" -> \"" + projectName + "\"");
          }      
      }
      */

    }

    public void resolveIds()
    {
      if (!Settings.userProperties)
      {
        // resolve project-relationship from solution
        foreach (string anId in dependencyIds)
        {
          //Console.WriteLine("resolve: " + anId);
          Project dependentProject = solution.projects[anId];
          dependencies.Add(dependentProject);
          dependentProject.users.Add(this);
        }
      }
      else
      {
        // read relationships from FEI specific UserProperties
        if (this.userProperties.ContainsKey("ProjectUses"))
        {
          string[] list = userProperties["ProjectUses"].Split(';');
          foreach (string projectName in list)
          {
            Project dependentProject = solution.findProject(projectName);
            if (dependentProject == null)
            {
              string projectdir = Path.GetDirectoryName(filename);
              string cocabasedir = Path.GetFullPath(projectdir + @"\..\..\..") + @"\";
              dependentProject = solution.createCoCaProject(cocabasedir, projectName);
            }
            dependencies.Add(dependentProject);
            dependentProject.users.Add(this);
          }
        }
      }    
    }

    public void readProjectFile()
    {
      // defaults
      outputName = "";
      outputType = ConfigurationType.Unexpected;
      outputTypeName = "";
      projectType = "";

      bool informationCollected = false;
      
      FileInfo projectFile = new FileInfo(this.filename);

      // if the project file does not exist, we accept that no
      // additional information is available and continue.
      if (!projectFile.Exists) return;

      XmlContext doc = new XmlContext();
      if (projectFile.Name.EndsWith(".csproj") || projectFile.Name.EndsWith(".vcproj") || projectFile.Name.EndsWith(".vcxproj"))
      {
        XmlTextReader reader = new XmlTextReader(this.filename);
        reader.Read();
        doc.Load(reader);
        reader.Close();
        doc.SetNamespace("vs", doc.DocumentElement.NamespaceURI);
        // Xpath: // selects at any level in the document
        //        @label='foo' 
      }

      if (projectFile.Name.EndsWith(".csproj"))
      {
        // its a C# project
        projectType = "C#";

        XmlNodeList outputList = doc.GetNodes("/vs:Project/vs:PropertyGroup/vs:OutputType");
        if (outputList.Count > 0)
        {
            outputTypeName = outputList[0].FirstChild.Value;
        }
        XmlNodeList outputNameList = doc.GetNodes("/vs:Project/vs:PropertyGroup/vs:AssemblyName");
        if (outputNameList.Count > 0)
        {
          outputName = outputNameList[0].FirstChild.Value;
        }
        XmlNodeList rootNameSpaceList = doc.GetNodes("/vs:Project/vs:PropertyGroup/vs:AssemblyName");
        if (rootNameSpaceList.Count > 0)
        {
          rootNamespace = rootNameSpaceList[0].FirstChild.Value;
        }
      } // end C# part
      else if (projectFile.Name.EndsWith(".vcproj") || projectFile.Name.EndsWith(".vcxproj"))
      {
        // its a C++ project
        projectType = "C++";

        rootNamespace = doc.GetNodeContent("//vs:PropertyGroup[@Label='Globals']/vs:RootNamespace", "");
        Console.WriteLine("Project '" + this.name + "' RootNamespace: '" + rootNamespace + "'");

        string projectGuid = doc.GetNodeContent("//vs:PropertyGroup[@Label='Globals']/vs:ProjectGuid", "").ToLower();
        if (!string.IsNullOrEmpty(projectGuid))
        {
            Console.WriteLine("GUID: " + id + " S: " + projectGuid);
            if ((!string.IsNullOrEmpty(id)) && !id.Equals(projectGuid))
            {
                Console.WriteLine("Project's guid (" + projectGuid + ") not equal to solution's project-guid (" + id + "), we assue the project is right about it's own guid");
            }
            id = projectGuid;
        }

        // first try to read .vcxproj -style
        XmlNodeList configurations = doc.GetNodes("//vs:PropertyGroup[@Label='Configuration']");
        if (configurations.Count > 0)
        {
            ReadVCXProjStyle(doc, configurations);
            informationCollected = true;
        }
        else
        {
            XmlNodeList configs = doc.GetNodes("/vs:VisualStudioProject/vs:Configurations/vs:Configuration");
            if (configs.Count > 0)
            {
                ReadVCProjStyle(configs); // before vs2010 the project xml layout was different.
                informationCollected = true;
            }
        }
      } // end c++ part


      // FEI (2012) specific extention
      XmlNodeList depsList = doc.GetNodes("//vs:ProjectExtensions/vs:VisualStudio/vs:UserProperties");
      if (depsList.Count > 0)
      {
        foreach (XmlAttribute a in depsList[0].Attributes)
        {
          if (userProperties.ContainsKey(a.Name))
          {
            if (Settings.verbose) Console.WriteLine("FEI UserProperties contains duplicated attibute '" + a.Name + " (ignored)");
            continue;
          }
          if (!a.Value.Contains(";"))
          {
            if (Settings.verbose) Console.WriteLine("FEI UserProperty[" + a.Name + "]: " + a.Value);
          }
          else
          {
            // assume it is a ; separated list.
            if (Settings.verbose) Console.WriteLine("FEI UserProperty[" + a.Name + "]:");
            foreach (string attr in a.Value.Split(';'))
            {
              if (Settings.verbose) Console.WriteLine("  " + attr);
            }
          }

          userProperties[a.Name] = a.Value;
        }
      }
      // FEI (2012) specific extention

      if (!informationCollected)
      {
        projectType = "unknown";
        outputTypeName = "unknown";
        outputName = projectFile.Name;
      }
    }

    // received a list of 'PropertyGroup' Nodes with an Attribute: 'Label="Configuration"'
    void ReadVCXProjStyle(XmlContext doc, XmlNodeList configs)
    {
        XmlElement configuration = null;
        foreach (XmlElement config in configs)
        {
            if (config.GetAttribute("Condition").ToLower().Contains("'release|"))
            {
                configuration = config;
                break;
            }
        }
        
        if (configuration == null)
        {
            foreach (XmlElement config in configs)
            {
                if (config.GetAttribute("Condition").ToLower().Contains("=='r"))
                {
                    configuration = config;
                    Console.WriteLine("WARNING: No 'release' configuration found, fallback to configuration: '" + configuration.GetAttribute("Condition") + "'");
                    break;
                }
            }
        }
        if (configuration == null)
        {
            configuration = (XmlElement) configs[0];
            Console.WriteLine("WARNING: No 'r*' configuration found, fallback to first configuration: '" + configuration.GetAttribute("Condition") + "'");
        }

        if (doc.GetNodeContent(configuration, "CLRSupport", "false") == "true")
        {
          projectType = "C++/CLI";
        }

        outputTypeName = doc.GetNodeContent(configuration, "ConfigurationType", ConfigurationType.Unexpected.ToString());
        outputType = ConfigurationType.Unexpected;
        if (outputTypeName.ToLower() == ConfigurationType.Makefile.ToString().ToLower())
        {
            outputType = ConfigurationType.Makefile;
        }
        else if (outputTypeName.ToLower() == ConfigurationType.Application.ToString().ToLower())
        {
            outputType = ConfigurationType.Application;
        }
        else if (outputTypeName.ToLower() == ConfigurationType.DynamicLibrary.ToString().ToLower())
        {
            outputType = ConfigurationType.DynamicLibrary;
        }
        else if (outputTypeName.ToLower() == ConfigurationType.StaticLibrary.ToString().ToLower())
        {
            outputType = ConfigurationType.StaticLibrary;
        }
        else if (outputTypeName.ToLower() == ConfigurationType.Utility.ToString().ToLower())
        {
            outputType = ConfigurationType.Utility;
        }
    }

    void ReadVCProjStyle(XmlNodeList configs)
    {
        // some dirty things to be flexible about the names of Configurations
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
            if (configuration == null)
            {
                foreach (XmlElement config in configs)
                {
                    if (config.GetAttribute("Name").ToLower().StartsWith("r"))
                    {
                        configuration = config;
                        Console.WriteLine("WARNING: No 'release' configuration found, fallback to configuration: '" + configuration.GetAttribute("Name") + "'");
                        break;
                    }
                }
            }
            if (configuration == null)
            {
                configuration = (XmlElement) configs[0];
                Console.WriteLine("WARNING: No 'r*' configuration found, fallback to first configuration: '" + configuration.GetAttribute("Name") + "'");
            }

            // found configuration
            if (configuration.GetAttribute("ManagedExtentions") == "1")
            {
                projectType = "C++/CLI";
            }

            string configurationTypeString = configuration.GetAttribute("ConfigurationType");
            int configurationType = Convert.ToInt32(configurationTypeString);
            outputType = ConfigurationType.Unexpected;
            switch (configurationType)
            {
                case (int)ConfigurationType.Makefile:
                    outputType = ConfigurationType.Makefile;
                    break;
                case (int)ConfigurationType.Application:
                    outputType = ConfigurationType.Application;
                    break;
                case (int)ConfigurationType.DynamicLibrary:
                    outputType = ConfigurationType.DynamicLibrary;
                    break;
                case (int)ConfigurationType.StaticLibrary:
                    outputType = ConfigurationType.StaticLibrary;
                    break;
                case (int)ConfigurationType.Utility:
                    outputType = ConfigurationType.Utility;
                    break;
                default:
                    outputTypeName = configurationTypeString;
                    break;
            }

            if (outputType != ConfigurationType.Unexpected)
            {
                outputTypeName = outputType.ToString();
            }

            //String name = String.Format("{0}.{1}", this.name, outputTypeName);
            //this.name = name;

            // add GenerateDebugInformation
            Console.WriteLine("projectType: {0}", projectType);
            Console.WriteLine("outputTypeName: {0}", outputTypeName);
            Console.WriteLine("outputName: {0}", outputName);
        }
        else
        {
            Console.WriteLine("ERROR: Configuration sections not found!");
        }
    }

    public ArrayList dependencyIds = new ArrayList();   // project id's (strings) from the .sln file
    public ArrayList dependencies = new ArrayList();    // project objects I use
    public ArrayList users = new ArrayList();           // project objects that use me
    public StringMap userProperties = new StringMap();  // FEI specific Project UserProperties

    public string rootNamespace;
    public string outputName;               // filename
    public ConfigurationType outputType;    
    public string outputTypeName;           // String representation of outputTypeName
    public string projectType;              // C#,C++,C++/CLI

    public string name;
    public string filename;
    public string id;
    public bool ignore;
    Solution solution;
    
  }
}