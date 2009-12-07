using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Diagnostics;

namespace DepCharter
{
  class Settings
  {
    public static bool reduce;
    public static string input;
    public static string workdir;
    public static bool verbose;
  }

  class ProjectDictionary : Dictionary<string, Project> { }

  class MyStringReader : StringReader
  {
    public MyStringReader(string input) : base(input) {}

    static int linenumber = 1;
    public override string ReadLine()
    {
      string line = base.ReadLine();
      if (Settings.verbose) Console.WriteLine(linenumber + ": " + line);
      linenumber++;
      return line;
    }
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
      this.id = startOfId.ToLower().Substring(0, startOfId.IndexOf("}") + 1);
      
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
      StringReader reader = new MyStringReader(File.ReadAllText(fullname));

      string line = "";
      while ((line = reader.ReadLine()) != null)
      {
        if (line == "") continue;
        line = line.Trim();
        if (line.StartsWith("Project"))
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
      Settings.workdir = Path.GetDirectoryName(Settings.input) + "\\";
      Solution solution = new Solution(Settings.input);
      solution.resolveIds();

      Console.WriteLine("Found " + solution.projects.Values.Count + " projects");
      int deps = solution.DepCount;
      Console.WriteLine("Found " + deps + " dependencies");

      if (deps > 100)
      {
        if (Settings.reduce)
        {
          Console.WriteLine("Processesing >100 dependencies, this may take several minutes!");
        }
        else
        {
          Console.WriteLine("Processesing >100 dependencies, consider using /r");
        }
      }

      string dotFileName = Settings.workdir + "dep.txt";
      if (File.Exists(dotFileName))
      {
        File.Delete(dotFileName);
      }

      Console.WriteLine("Created " + dotFileName);

      StreamWriter dotFile = File.CreateText(dotFileName);
      dotFile.WriteLine("digraph G {");   //the first line for the .dot file

      // style
      //dotFile.WriteLine("graph [fontsize=32];");
      //dotFile.WriteLine("edge [fontsize=32];");
      dotFile.WriteLine("node [fontsize=70]");

      dotFile.WriteLine("fontname=calibri");
      dotFile.WriteLine("aspect=0.7");
      dotFile.WriteLine("ranksep=1.5");
      //dotFile.WriteLine("nodesep=0.5");
      
      //dotFile.WriteLine("arrowsize=16.0");
      dotFile.WriteLine("edge [style=\"setlinewidth(4)\"]");

      //dotFile.WriteLine("size=1024,700;");
      //dotFile.WriteLine("ratio=expand;"); // expand/fill/compress/auto

      // write project-deps
      solution.writeDepsInDotCode(dotFile);

      dotFile.WriteLine("}");
      dotFile.Close();

      string pngFile = Settings.workdir + solution.name + "_dep.png";
      if (File.Exists(pngFile)) File.Delete(pngFile);
      string repngFile = Settings.workdir + solution.name + "_redep.png";
      if (File.Exists(repngFile)) File.Delete(repngFile);

      string redepFile = "redep.txt";
      if (File.Exists(redepFile)) File.Delete(redepFile);

      if (Settings.reduce)
      {
        System.Diagnostics.Process tredProc = new System.Diagnostics.Process();
        Console.WriteLine("Using tred to create reduced redep.txt");
        tredProc.StartInfo.FileName = "tred.exe";
        tredProc.StartInfo.Arguments = "dep.txt";
        tredProc.StartInfo.UseShellExecute = false;
        tredProc.StartInfo.RedirectStandardOutput = true;
        tredProc.Start();
        string tredOutput = tredProc.StandardOutput.ReadToEnd();
        tredProc.WaitForExit();

        StreamWriter writer = new StreamWriter(redepFile);
        writer.Write(tredOutput);
        writer.Close();

        dotFileName = redepFile;
        pngFile = repngFile;
      }

      Console.WriteLine("Using dot to create bitmap");

      System.Diagnostics.Process proc = new System.Diagnostics.Process();
      proc.StartInfo.FileName = "dot.exe";
      proc.StartInfo.Arguments = " -Tpng " + dotFileName + " -o " + pngFile;
      proc.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
      proc.Start();
      proc.WaitForExit();

      Console.WriteLine("Done.");

      if (File.Exists(pngFile))
      {
        proc.StartInfo.FileName = pngFile;
        proc.StartInfo.Arguments = "";
        proc.Start();
        //proc.WaitForExit();
      }
      else
      {
        MessageBox.Show("Error generation diagram");
      }
    }

    static void Main(string[] args)
    {
      foreach (string arg in args)
      {
        string arglower = arg.ToLower();
        if (arglower.StartsWith("/r"))
        {
          Settings.reduce = true;
          continue;
        }
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
        Console.WriteLine("DepCharter [/r] <filename>\n");
      }
    }
  }
}
