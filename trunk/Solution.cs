using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Diagnostics;

namespace DepCharter
{

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
      writer.WriteLine(this.name + " [shape=box,style=filled,color=olivedrab1];");
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

    public ArrayList dependencyIds = new ArrayList();   // project id's (strings)
    public ArrayList dependencies = new ArrayList();    // project objects
    public ArrayList users = new ArrayList();           // project objects

    public string name;
    public string id;
    public bool ignore;
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

    public void markIgnoredProjects()
    {
      if (Settings.ignoreEndsWithList.Count > 0)
      {
        foreach (Project project in projects.Values)
        {
          foreach (string endString in Settings.ignoreEndsWithList)
          {
            if (project.name.ToLower().EndsWith(endString.ToLower()))
            {
              Console.WriteLine(project.name + " ignored.");
              project.ignore = true;
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
      Console.WriteLine("Writing dependencies for " + this.name + " to dot file");
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
        if (project.name.ToLower() == projectName.ToLower())
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

    public string name;
    public string fullname;
  }

}