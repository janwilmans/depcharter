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

    public Solution()
    {
    }

    // no error handle here, assume the file exists is checked.
    public void read(string aFullname)
    {
      fullname = Path.GetFullPath(aFullname.Trim());
      name = Path.GetFileNameWithoutExtension(fullname);
      StringReader reader = new MyStringReader(File.ReadAllText(fullname));

      string line = "";
      while ((line = reader.ReadLine()) != null)
      {
        if (line == "") continue;
        line = line.Trim();
        if (line.StartsWith("Project"))
        {
          Project project = new Project(this, line);
          project.readDependencies(reader);
          this.Add(project);
        }
      }

      // collect info from project xml files
      foreach (Project project in projects.Values)
      {
        project.readProjectFile();
      }
    }
    
    // a CoCa project name looks like "subdirname.projectname" 
    // for example 'tem.mdlsmc', it means a project 'basedir\tem\mdlsmc.vcxproj' OR 'basedir\tem\mdlsmc.csproj' exists
    public Project createCoCaProject(string basedir, string CoCaProjectName)
    {
      Project result;
      string projectname = CoCaProjectName.Substring(CoCaProjectName.LastIndexOf(".")+1);
      string basename = basedir + CoCaProjectName.Trim().Replace(".", @"\") + @"\src\";
      string fullname = basename + projectname + ".csproj";

      if (!File.Exists(fullname))
      {
        fullname = basename + projectname + ".vcxproj";
      }
      if (File.Exists(fullname))
      {
        result = readProject(fullname);
      }
      else
      {
        Console.WriteLine("Project: " + CoCaProjectName + " not found at base direcory: " + basedir + "!");
        result = createDummyProject(CoCaProjectName);
      }
      return result;
    }

    string getCocaProjectName(string aFullname)
    {
      string projectname = Path.GetFileNameWithoutExtension(fullname);
      string basedir = Path.GetFullPath(Path.GetDirectoryName(fullname) + @"\..");
      string projectdir = basedir.Substring(0, basedir.LastIndexOf(@"\"));
      string componentname = projectdir.Substring(projectdir.LastIndexOf(@"\") + 1);
      return componentname + "." + projectname;
    }

    public Project readProject(string aFullname)
    {
      Project result;
      fullname = aFullname.Trim();
      // name = Path.GetFileNameWithoutExtension(fullname);
      name = getCocaProjectName(fullname);
      StringReader reader = new MyStringReader(File.ReadAllText(fullname));

      result = new Project(this);
      result.name = name;
      result.filename = fullname;
      result.readProjectFile();
      this.Add(result);
      return result;
    }
    
    public void resolveIds()
    {
      // copy the list before iterating, resolveIds may add dummy-projects in the process
      ArrayList projectList = new ArrayList(projects.Values);
      foreach (Project project in projectList)
      {
        project.recursivelyAddAllProjects();
      }

      projectList = new ArrayList(projects.Values);
      foreach (Project project in projectList)
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
          project.ignore = false;
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

    public Project findProject(string name)
    {
      foreach (Project project in projects.Values)
      {
        //todo: match this more accurately, now 'foo' will match 'tem.foo' and/or 'bar.foo'
        if (name.ToLower().EndsWith(project.name.ToLower()) || project.name.ToLower().EndsWith(name.ToLower()))
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

    public string name;
    public string fullname;
  }

}