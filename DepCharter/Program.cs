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
        public Project(StringReader reader, string line)
        {
            this.name = line.Substring(line.IndexOf(")")+1);
            Console.WriteLine("project: " + this.name);
        }
       

        public ArrayList dependencies = new ArrayList();
        public ArrayList users = new ArrayList();

        public string name;
        public string id;

    }

    class Solution
    {
        public ProjectDictionary projects = new ProjectDictionary();
        void Add(Project project)
        {
            if (projects.ContainsKey(project.name))
            {
                Console.WriteLine("error in solution, project '" + project.name + "' listed multiple times! (ignored)");
            }
            else
            {
                projects.Add(project.name, project);
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
                    Project project = new Project(reader, line);
                    this.Add(project);
                }
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

            string dotFileName = Settings.workdir + "dep.txt";
            if (File.Exists(dotFileName))
            {
                File.Delete(dotFileName);
            }

            StreamWriter dotFile = File.CreateText(dotFileName);
            dotFile.WriteLine("digraph G {");   //the first line for the .dot file

            // write project-deps

            dotFile.WriteLine("}");
            dotFile.Flush();
            dotFile.Close();

            string cmdLine = " -Tpng " + dotFileName + " -o " + Settings.workdir + solution.name + "_dep.png";
            System.Diagnostics.Process proc = new System.Diagnostics.Process();
            proc.StartInfo.FileName = "dot.exe";
            proc.StartInfo.Arguments = cmdLine;
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
                Settings.input = arglower;
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
