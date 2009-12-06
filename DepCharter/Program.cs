using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;

namespace DepCharter
{
    class ProjectDictionary : Dictionary<string, Project> {}

    class Settings
    {
        public static bool verbose;
        public static string input;
        public static string workdir;
    }

    class Project
    {
        public Project(Solution aSolution, StringReader reader, string firstLine)
        {
            solution = aSolution;
            string part = firstLine.Substring(firstLine.IndexOf(")") + 1);
            string startOfName = part.Substring(part.IndexOf("\"") + 1);
            this.name = startOfName.Substring(0, startOfName.IndexOf("\""));
            string startOfId = startOfName.Substring(startOfName.IndexOf("{"));

            // dependency-id's _must_ be treated case-insensitive
            this.id = startOfId.ToLower().Substring(0, startOfId.IndexOf("}") +1);
            
            //Console.WriteLine("project: " + this.name + " " + this.id);

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
                        string depId = depLine.Substring(0, depLine.IndexOf("}") +1);
                        dependencyIds.Add(depId);
                        //Console.WriteLine("dependency: " + depId);
                    }
                }
                if (line.StartsWith("EndProject")) break;
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

        public ArrayList dependencyIds = new ArrayList();   // project id's (strings)
        public ArrayList dependencies = new ArrayList();    // project objects
        public ArrayList users = new ArrayList();           // project objects

        public string name;
        public string id;
        Solution solution;

    }

    class Solution
    {
        public ProjectDictionary projects = new ProjectDictionary();
        void Add(Project project)
        {
            if (projects.ContainsKey(project.id))
            {
                Console.WriteLine("error in solution, project '" + project.name + "' listed multiple times! (ignored)");
            }
            else
            {
                projects.Add(project.id, project);
            }
        }

        public Solution(string aFullname)
        {
            fullname = aFullname.Trim();
            name = Path.GetFileNameWithoutExtension(fullname);
            StringReader reader = new StringReader(File.ReadAllText(fullname));

            string line = "";
            while ((line = reader.ReadLine()) != null)
            {
                if (line == "") continue;
                line = line.Trim().ToLower();
                if (line.StartsWith("project"))
                {
                    Project project = new Project(this, reader, line);
                    this.Add(project);
                }
            }        
        }

        public void resolveIds()
        {
            foreach (Project project in projects.Values)
            {
                project.resolveIds();
            }
        }

        public void writeDepsInDotCode(StreamWriter writer)
        {
            Console.WriteLine("Writing dependencies for " + this.name + " to dot file");

            foreach (Project project in projects.Values)
            {
                writer.WriteLine(project.name + " [shape=box,style=filled,color=olivedrab1];");
                foreach (Project depProject in project.dependencies)
                {
                    writer.WriteLine(project.name + " -> " + depProject.name);
                }
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

        public string name;
        public string fullname;
    }

    class Program
    {
        static void Execute()
        {
            if (Settings.verbose)
            {
                Console.WriteLine("verbose mode enabled.");
            }
            Settings.workdir = Path.GetDirectoryName(Settings.input) + "\\";
            Solution solution = new Solution(Settings.input);
            solution.resolveIds();

            Console.WriteLine("Found " + solution.projects.Values.Count + " projects");
            Console.WriteLine("Found " + solution.DepCount + " dependencies");

            string dotFileName = Settings.workdir + "dep.txt";
            if (File.Exists(dotFileName))
            {
                File.Delete(dotFileName);
            }

            Console.WriteLine("Created " + dotFileName);

            StreamWriter dotFile = File.CreateText(dotFileName);
            dotFile.WriteLine("digraph G {");   //the first line for the .dot file

            // write project-deps
            solution.writeDepsInDotCode(dotFile);

            dotFile.WriteLine("}");
            dotFile.Flush();
            dotFile.Close();

            string pngFile = Settings.workdir + solution.name + "_dep.png";
            string cmdLine = " -Tpng " + dotFileName + " -o " + pngFile;
            System.Diagnostics.Process proc = new System.Diagnostics.Process();
            proc.StartInfo.FileName = "dot.exe";
            proc.StartInfo.Arguments = cmdLine;
            proc.Start();
            proc.WaitForExit();

            proc.StartInfo.FileName = pngFile;
            proc.StartInfo.Arguments = "";
            proc.Start();
            proc.WaitForExit();

        }

        static void Main(string[] args)
        {
            foreach (string arg in args)
            {
                string arglower = arg.ToLower();
                if (arglower.StartsWith("/v"))
                {
                    Settings.verbose = true;
                    continue;
                }
                Settings.input = Path.GetFullPath(arglower);
            }

            if (Settings.input != null && File.Exists(Settings.input))
            {
                Execute();
            }
            else
            {
                Console.WriteLine("DepCharter [/v] <filename>\n");
            }
        }
    }
}
