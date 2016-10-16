// Project.cs read Visual Studio Project files, unlike the .sln files, these are XML files.

using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.Xml.XPath;

namespace DepCharter
{
    // values correspond to actual data in vcproj files
    enum ConfigurationType
    {
        Makefile = 0,
        Application = 1,
        DynamicLibrary = 2,
        StaticLibrary = 4,
        Utility = 10,
        Unexpected = -1,
        Library = -2,     // C#
        WinExe = -3,      // C#
    }

    class Project
    {
        public void InitializeFromSolutionLine(string firstLine)
        {
            SolutionLine solutionLine = new SolutionLine(firstLine);
            this.Id = solutionLine[2][1].ToLower();       // store in lower-case, _must_ be treated case-insensitive
            this.Name = solutionLine[0][3];
            this.Filename = Path.GetFullPath(Path.GetDirectoryName(Solution.Fullname) + @"\" + solutionLine[1][1]);
            if (Settings.verbose) Console.WriteLine("project: " + this.Name + " " + this.Id);
        }

        public Project(Solution aSolution)
        {
            Solution = aSolution;
        }

        public Project(string id)
        {
            this.Id = id;
            this.Name = Id;
        }

        static public Project createDummyProject(Solution solution, string name)
        {
            Project dummy = new Project(solution);
            dummy.Id = Guid.NewGuid().ToString();
            dummy.Name = name;
            dummy.Filename = name + ".dummy";
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
                        solutionDependencyIds.Add(depId);
                        if (Settings.verbose) Console.WriteLine("SLN dependency: " + depId);
                    }
                    if (line.StartsWith("EndProjectSection"))
                    {
                        if (Settings.verbose) Console.WriteLine("EndProjectSection\n");
                        break;
                    }
                }
                if (line.ToLower().Trim() == "endproject")
                {
                    if (Settings.verbose) Console.WriteLine("EndProject\n");
                    break;
                }
            }
        }

        public void writeDepsInDotCodeRecursive(StreamWriter writer)
        {
            writeDepsInDotCode(writer);
            foreach (var project in this.dependencies.Keys)
            {
                project.writeDepsInDotCodeRecursive(writer);
            }
        }

        public void writeDepsInDotCode(StreamWriter writer)
        {
            string extraInfo = "";
            if (this.Ignore) return;
            if (Settings.restrictToSolution && (!Program.Model.IsSolutionProject(this))) return;
            String color = "white";   // white so independend .sln files will look 'normal'
            switch (OutputType)
            {
                case ConfigurationType.Application:
                case ConfigurationType.WinExe:
                    color = "orange";
                    break;
                case ConfigurationType.DynamicLibrary:
                case ConfigurationType.Library:
                    color = "lightblue";
                    break;
                case ConfigurationType.StaticLibrary:
                    color = "lightgray";
                    break;
                case ConfigurationType.Makefile:
                case ConfigurationType.Utility:
                    color = "olivedrab1";
                    break;
                default:
                    // unknown type or missing
                    color = "white";
                    if (!string.IsNullOrEmpty(OutputTypeName))
                    {
                        extraInfo = " (outputTypeName: '" + OutputTypeName + "')";
                    }
                    break;
            }

            String objectString = String.Format("\"{0}\" [shape=box,style=filled,fillcolor={1},color=black];", this.Name + extraInfo, color);
            writer.WriteLine(objectString);

            foreach (var project in this.dependencies.Keys)
            {
                var origins = dependencies[project];
                if (project.Ignore) continue;
                if (Settings.restrictToSolution && (!Program.Model.IsSolutionProject(project))) continue;


                List<string> colors = new List<string>();
                foreach (var origin in origins)
                {
                    if (origin == Origin.ProjectToProject)
                    {
                        colors.Add("blue");     // fractured lines (multicolor in series) gives a pretty bad visual effect....
                    }
                    if (origin == Origin.UserProperty)
                    {
                        colors.Add("green");
                    }
                    if (origin == Origin.Solution)
                    {
                        colors.Add("red");
                    }
                }
                string arrowType = "[color=\"" + string.Join(":", colors.ToArray()) + "\"]";

                writer.WriteLine("\"" + this.Name + "\" -> \"" + project.Name + "\" " + arrowType);
            }
        }

        public List<string> GetUserPropertyEntries()
        {
            char[] charSeparators = { ';' };
            string result = "";
            if (this.userProperties.ContainsKey("ProjectUses"))
            {
                result = userProperties["ProjectUses"];
            }
            if (this.userProperties.ContainsKey("ProjectDependsOn"))
            {
                result = userProperties["ProjectDependsOn"];
            }
            return new List<string>(result.Split(charSeparators, StringSplitOptions.RemoveEmptyEntries));
        }

        public void recursivelyAddAllProjects()
        {
            if (!Settings.userProperties) return;
            var list = GetUserPropertyEntries();
            if (list.Count == 0) return;
            foreach (string projectName in list)
            {
                Project project = Solution.findProject(projectName);
                if (project == null)                                     // todo: this is wrong, it assumes a project can only be references once, which is stupid.
                {
                    string projectdir = Path.GetDirectoryName(Filename);
                    // assumption: the cocabasedir is four levels deep. ie. C:\sandbox\<stream_name>\<component>
                    string cocabasedir = String.Join("\\", Path.GetFullPath(projectdir).Split(new char[] { '\\' }), 0, 4) + @"\";
                    Console.WriteLine("createCoCaProject: " + projectName + " (for " + this.Name + ")");

                    project = Solution.createCoCaProject(cocabasedir, projectName);
                    project.recursivelyAddAllProjects();
                }
            }
        }

        private void AddDependency(Project project, Origin origin)
        {
            dependencies.Add(project, origin);
            project.users.Add(this, origin);
        }

        public void AddSolutionDependency(string id)
        {
            Project project = Solution.projects[id];
            AddDependency(project, Origin.Solution);
        }

        public Project GetProjectForId(string id)
        {
            Project result = null;
            if (Solution.projects.ContainsKey(id))
            {
                // project is the same solution 
                result = Solution.projects[id];
            }
            else
            {
                // todo: resolve the project outside the solution by scanning for it recursively down the directory-tree
                result = new Project(id);
            }
            return result;
        }

        public void AddProjectToProjectDependency(string id) 
        {
            // project can refer also to projects outside the solution
            AddDependency(GetProjectForId(id), Origin.ProjectToProject);
        }

        public void AddUserPropertyDependency(Project project)
        {
            AddDependency(project, Origin.UserProperty);
        }

        public void resolveIds()
        {
            if (!Settings.userProperties)
            {
                // resolve project-relationship from solution
                foreach (string id in solutionDependencyIds)
                {
                    AddSolutionDependency(id);
                }
                return;
            }

            // read relationships from FEI specific UserProperties
            foreach (string projectName in GetUserPropertyEntries())
            {
                if (string.IsNullOrEmpty(projectName)) continue;
                Project dependentProject = Solution.findProject(projectName);
                if (dependentProject == null)
                {
                    dependentProject = Solution.createDummyProject(projectName);
                }
                AddUserPropertyDependency(dependentProject);
            }
        }

        public void readProjectToProjectReferences(XmlContext doc)
        {
            var projectReferences = doc.GetNodes("//vs:ItemGroup/vs:ProjectReference/vs:Project");
            foreach (XmlElement projectId in projectReferences)
            {
                string id = projectId.InnerText;
                if (Settings.verbose) Console.WriteLine("  ProjectReference: " + id);
                if (Settings.restrictToSolution && (!Solution.containsProjectId(id))) continue;
                AddProjectToProjectDependency(id);
            }
        }

        public void readProjectFile()
        {
            // defaults
            OutputName = "";
            OutputType = ConfigurationType.Unexpected;
            OutputTypeName = "";
            ProjectType = "";

            bool informationCollected = false;

            FileInfo projectFile = new FileInfo(this.Filename);

            // if the project file does not exist, we accept that no
            // additional information is available and continue.
            if (!projectFile.Exists) return;

            XmlContext doc = new XmlContext();
            if (projectFile.Name.EndsWith(".csproj") || projectFile.Name.EndsWith(".vcproj") || projectFile.Name.EndsWith(".vcxproj"))
            {
                XmlTextReader reader = new XmlTextReader(this.Filename);
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
                Name = "[C#] " + Name;
                ProjectType = "C#";
                informationCollected = true;

                var outputList = doc.GetNodes("/vs:Project/vs:PropertyGroup/vs:OutputType").ToList();
                if (outputList.Count > 0)
                {
                    OutputTypeName = outputList[0].FirstChild.Value;
                }
                var outputNameList = doc.GetNodes("/vs:Project/vs:PropertyGroup/vs:AssemblyName").ToList();
                if (outputNameList.Count > 0)
                {
                    OutputName = outputNameList[0].FirstChild.Value;
                }
                var projectGuidList = doc.GetNodes("/vs:Project/vs:PropertyGroup/vs:ProjectGuid").ToList();
                if (projectGuidList.Count > 0)
                {
                    string projectGuid = projectGuidList[0].FirstChild.Value.ToLower();
                    //Console.WriteLine("GUID: " + id + " S: " + projectGuid);
                    if ((!string.IsNullOrEmpty(Id)) && !Id.Equals(projectGuid))
                    {
                        Console.WriteLine("Project's guid (" + projectGuid + ") not equal to solution's project-guid (" + Id + "), we assue the project is right about it's own guid");
                    }
                    Id = projectGuid;
                }
                readProjectToProjectReferences(doc);
            } // end C# part
            else if (projectFile.Name.EndsWith(".vcproj") || projectFile.Name.EndsWith(".vcxproj"))
            {
                // its a C++ project
                ProjectType = "C++";

                Console.WriteLine("Project '" + this.Name + "'");

                string projectGuid = doc.GetNodeContent("//vs:PropertyGroup[@Label='Globals']/vs:ProjectGuid", "").ToLower();
                if (!string.IsNullOrEmpty(projectGuid))
                {
                    //Console.WriteLine("GUID: " + id + " S: " + projectGuid);
                    if ((!string.IsNullOrEmpty(Id)) && !Id.Equals(projectGuid))
                    {
                        Console.WriteLine("Project's guid (" + projectGuid + ") not equal to solution's project-guid (" + Id + "), we assue the project is right about it's own guid");
                    }
                    Id = projectGuid;
                }

                // first try to read .vcxproj -style
                var configurations = doc.GetNodes("//vs:PropertyGroup[@Label='Configuration']");
                if (configurations.Count > 0)
                {
                    ReadVCXProjStyle(doc, configurations);
                    informationCollected = true;
                    readProjectToProjectReferences(doc);
                }
                else
                {
                    var configs = doc.GetNodes("/vs:VisualStudioProject/vs:Configurations/vs:Configuration");
                    if (configs.Count > 0)
                    {
                        ReadVCProjStyle(configs); // before vs2010 the project xml layout was different.
                        informationCollected = true;
                    }
                }
            } // end C++ part


            // FEI (2012) specific extention
            var depsList = doc.GetNodes("//vs:ProjectExtensions/vs:VisualStudio/vs:UserProperties");
            if (depsList.Count > 0)
            {
                foreach (XmlAttribute a in depsList[0].Attributes)
                {
                    if (userProperties.ContainsKey(a.Name))
                    {
                        //if (Settings.verbose) Console.WriteLine("FEI UserProperties contains duplicated attibute '" + a.Name + " (ignored)");
                        continue;
                    }
                    if (!a.Value.Contains(";"))
                    {
                        //if (Settings.verbose) Console.WriteLine("FEI UserProperty[" + a.Name + "]: " + a.Value);
                    }
                    else
                    {
                        // assume it is a ; separated list.
                        if (Settings.verbose) Console.WriteLine("FEI UserProperty[" + a.Name + "]:");
                        foreach (string attr in a.Value.Split(';'))
                        {
                            if (Settings.verbose) Console.WriteLine("  FBT reference: " + attr);
                        }
                    }

                    userProperties[a.Name] = a.Value;
                }
            }
            // FEI (2012) specific extention

            if (informationCollected)
            {
                OutputType = ConfigurationType.Unexpected;
                if (OutputTypeName.ToLower() == ConfigurationType.Makefile.ToString().ToLower())
                {
                    OutputType = ConfigurationType.Makefile;
                }
                else if (OutputTypeName.ToLower() == ConfigurationType.Application.ToString().ToLower())
                {
                    OutputType = ConfigurationType.Application;
                }
                else if (OutputTypeName.ToLower() == ConfigurationType.DynamicLibrary.ToString().ToLower())
                {
                    OutputType = ConfigurationType.DynamicLibrary;
                }
                else if (OutputTypeName.ToLower() == ConfigurationType.Library.ToString().ToLower())  // C# libraries are never static
                {
                    OutputType = ConfigurationType.Library;
                }
                else if (OutputTypeName.ToLower() == ConfigurationType.StaticLibrary.ToString().ToLower())
                {
                    OutputType = ConfigurationType.StaticLibrary;
                }
                else if (OutputTypeName.ToLower() == ConfigurationType.Utility.ToString().ToLower())
                {
                    OutputType = ConfigurationType.Utility;
                }
                else if (OutputTypeName.ToLower() == ConfigurationType.WinExe.ToString().ToLower())
                {
                    OutputType = ConfigurationType.WinExe;
                }
            }
            else
            {
                ProjectType = "unknown";
                OutputName = projectFile.Name;
                Console.WriteLine("File could not be identified as a project file: " + OutputName);
            }
        }

        // received a list of 'PropertyGroup' Nodes with an Attribute: 'Label="Configuration"'
        void ReadVCXProjStyle(XmlContext doc, List<XmlNode> configs)
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
                configuration = (XmlElement)configs[0];
                Console.WriteLine("WARNING: No 'r*' configuration found, fallback to first configuration: '" + configuration.GetAttribute("Condition") + "'");
            }

            if (doc.GetNodeContent(configuration, "CLRSupport", "false") == "true")
            {
                ProjectType = "C++/CLI";
            }

            OutputTypeName = doc.GetNodeContent(configuration, "ConfigurationType", ConfigurationType.Unexpected.ToString());
        }

        void ReadVCProjStyle(List<XmlNode> configs)
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
                    configuration = (XmlElement)configs[0];
                    Console.WriteLine("WARNING: No 'r*' configuration found, fallback to first configuration: '" + configuration.GetAttribute("Name") + "'");
                }

                // found configuration
                if (configuration.GetAttribute("ManagedExtentions") == "1")
                {
                    ProjectType = "C++/CLI";
                }

                OutputTypeName = configuration.GetAttribute("ConfigurationType");

                // add GenerateDebugInformation
                Console.WriteLine("projectType: {0}", ProjectType);
                Console.WriteLine("outputTypeName: {0}", OutputTypeName);
                Console.WriteLine("outputName: {0}", OutputName);
            }
            else
            {
                Console.WriteLine("ERROR: Configuration sections not found!");
            }
        }

        public ArrayList solutionDependencyIds = new ArrayList();   // project id's (strings) from the .sln file
        public DependencyCollection dependencies = new DependencyCollection();    // project objects I use
        public DependencyCollection users = new DependencyCollection();           // project objects that use me
        public StringMap userProperties = new StringMap();  // FEI specific Project UserProperties

        public string OutputName;               // filename
        public ConfigurationType OutputType = ConfigurationType.Unexpected;
        public string OutputTypeName;           // String representation of outputTypeName
        public string ProjectType;              // C#,C++,C++/CLI

        public string Name = "";
        public string Filename = "";
        public string Id = "";
        public bool Ignore;
        Solution Solution;

    }
}