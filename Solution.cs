// Solution.cs read Visual Studio Solution (.sln) files, these are plain-text files in a undocumented line-based format.

using System;
using System.IO;
using System.Collections; // ArrayList
using System.Collections.Generic; // Dictionary <>
using System.Windows.Forms;
using System.Diagnostics;

namespace DepCharter
{
    class Solution
    {
        public ProjectDictionary projects = new ProjectDictionary();

        public void Add(Project project)
        {
            if (projects.ContainsKey(project.Id))
            {
                Console.WriteLine("error in solution, project '" + project.Name + "' listed multiple times! (ignored)");
            }
            else
            {
                projects.Add(project.Id, project);
            }
        }

        public bool containsProjectId(string id)
        {
            return projects.ContainsKey(id);
        }

        // no error handle here, assume the file exists is checked.
        public void read(string aFullname)
        {
            Fullname = Path.GetFullPath(aFullname.Trim());
            Name = Path.GetFileNameWithoutExtension(Fullname);
            StringReader reader = new MyStringReader(File.ReadAllText(Fullname));

            string line = "";
            while ((line = reader.ReadLine()) != null)
            {
                if (line == "") continue;
                line = line.Trim();
                if (line.StartsWith("Project"))
                {
                    Project project = new Project(this);
                    project.InitializeFromSolutionLine(line);
                    project.readDependencies(reader);
                    Program.Model.Add(this, project);
                }
            }

            // collect info from project xml files
            foreach (Project project in projects.Values)
            {
                project.readProjectFile();
            }
        }

        public string ReplaceFirst(string text, string search, string replace)
        {
            int pos = text.IndexOf(search);
            if (pos < 0)
            {
                return text;
            }
            return text.Substring(0, pos) + replace + text.Substring(pos + search.Length);
        }

        // a CoCa project name looks like "subdirname.projectname" 
        // for example 'tem.mdlsmc', means:
        // a project 'basedir\tem\mdlsmc\mdlsmc.vcxproj' OR 'basedir\tem\mdlsmc\mdlsmc.csproj' OR
        //           'basedir\tem\mdlsmc\src\mdlsmc.vcxproj' OR 'basedir\tem\mdlsmc\src\mdlsmc.csproj' exists
        public Project createCoCaProject(string basedir, string CoCaProjectName)
        {
            string projectname = CoCaProjectName.Substring(CoCaProjectName.LastIndexOf(".") + 1);
            string basename = basedir + ReplaceFirst(CoCaProjectName, ".", "\\").Trim();

            if (!Directory.Exists(basename))
            {
                Console.WriteLine("Project: " + CoCaProjectName + " not found because base direcory: " + basedir + " does not exist!");
                return createDummyProject(CoCaProjectName);
            }

            if (Directory.Exists(basename + "src\\"))
            {
                basename = basename + "src\\";
            }

            List<string> files = new List<string>();
            if (files.Count == 0) files.AddRange(Directory.GetFiles(basename, "*.csproj"));
            if (files.Count == 0) files.AddRange(Directory.GetFiles(basename, "*.vcxproj"));
            if (files.Count == 0) files.AddRange(Directory.GetFiles(basename, "*.vcproj"));

            if (files.Count == 0)
            {
                Console.WriteLine("Project: " + CoCaProjectName + " not found at base direcory: " + basedir + "!");
                return createDummyProject(CoCaProjectName);
            }

            if (files.Count > 1)
            {
                Console.WriteLine("Multiple suspicious projects!");
                foreach (var file in files)
                {
                    Console.WriteLine("  " + file);
                }
                Console.WriteLine("Defaulting to: " + files[0]);
            }
            return readProject(files[0], CoCaProjectName);
        }

        public Project readProject(string aFullname, string coCaProjectName)
        {
            var result = new Project(this);
            result.Name = coCaProjectName;
            result.Filename = aFullname;
            result.readProjectFile();
            this.Add(result);
            return result;
        }

        public void resolveIds()
        {
            if (Settings.userProperties)
            {
                // copy the list before iterating, resolveIds may add dummy-projects in the process
                foreach (Project project in new ArrayList(projects.Values))
                {
                    project.recursivelyAddUserPropertyProjects();
                }
            }

            foreach (Project project in new ArrayList(projects.Values))
            {
                project.resolveIds();
            }
        }

        public void markIgnoredProjects()
        {
            if (Settings.ignoreEndsWithList.Count > 0)
            {
                foreach (Project project in projects.Values)
                {
                    project.Ignore = false;
                    foreach (string endString in Settings.ignoreEndsWithList)
                    {
                        if (project.Name.ToLower().EndsWith(endString.ToLower()))
                        {
                            Console.WriteLine(project.Name + " ignored.");
                            project.Ignore = true;
                            break;
                        }
                    }
                }
            }
        }

        public void writeDepsInDotCode(StreamWriter writer)
        {
            if (Settings.projectsList.Count > 0)
            {
                foreach (String project in Settings.projectsList)
                {
                    writeDepsInDotCodeForProject(writer, project);
                }
            }
            else
            {
                writeDepsInDotCodeForSolution(writer);
            }
        }

        public void writeDepsInDotCodeForSolution(StreamWriter writer)
        {
            Console.WriteLine("Writing dependencies for " + this.Name + " to dot file");
            foreach (Project project in projects.Values)
            {
                // use the non-recursive method
                project.writeDepsInDotCode(writer);
            }
        }

        public void writeDepsInDotCodeForProject(StreamWriter writer, string projectName)
        {
            bool found = false;
            foreach (Project project in projects.Values)
            {
                if (project.Name.ToLower() == projectName.ToLower())
                {
                    Console.WriteLine("Writing dependencies for project " + projectName + " to dot file");
                    project.writeDepsInDotCodeRecursive(writer);
                    found = true;
                    break;
                }
            }
            if (!found)
            {
                Console.WriteLine("project " + projectName + " not found!");
            }
        }

        public int DepCount
        {
            get
            {
                int depCount = 0;
                foreach (Project project in projects.Values)
                {
                    depCount += project.dependencies.Count;
                }
                return depCount;
            }
        }

        public Project findProject(string name)
        {
            foreach (Project project in projects.Values)
            {
                //todo: match this more accurately, now 'foo' will match 'tem.foo' and/or 'bar.foo'
                if (name.ToLower().EndsWith(project.Name.ToLower()) || project.Name.ToLower().EndsWith(name.ToLower()))
                {
                    return project;
                }

            }
            return null;
        }

        public Project createDummyProject(string name)
        {
            Project dummyProject = Project.createDummyProject(this, name);
            Add(dummyProject);
            return dummyProject;
        }

        public string Name;
        public string Fullname;
    }

}