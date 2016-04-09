// Program.cs contains the Main() and DOT generation helper methods

using System;
using System.Threading;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Diagnostics;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Xml;
using System.Xml.XPath;

/*  // optimize for circulair diagram:
 *  digraph G {
 *  aspect="1.3";
 *  layout="twopi";
 *  ranksep="3.0";
 *  overlap="vpsc";
 */

namespace DepCharter
{
  class DotWriter
  {
    public StreamWriter dotFile;

    public DotWriter(string filename)
    {
      if (File.Exists(filename))
      {
        File.Delete(filename);
      }
      dotFile = File.CreateText(filename);
      Console.WriteLine("Created " + filename);
      dotFile.WriteLine("digraph G {");   // the first line for the .dot file
    }

    public void Close()
    {
      dotFile.WriteLine("}");
      dotFile.Close();      
    }

    public void WriteAspectRatio(double ratio)
    {
      if (ratio > 0.01)
      {
        // prevent any regional settings from interfering with number formatting
        String ratioString = String.Format(CultureInfo.CreateSpecificCulture("en-us"), "aspect={0: #0.0}", Settings.aspectratio); 
        dotFile.WriteLine(ratioString);
      }
    }

    public void WriteTTFont(string font)
    {
       Console.WriteLine("Using truetype font ({0})", font);
       dotFile.WriteLine("fontname={0}", font);
    }
     
    public void WriteFontSize(int fontSize)
    {
      if (fontSize > 0)
      {
        Console.WriteLine("Using node fontsize = " + fontSize);
        dotFile.WriteLine("node [fontsize=" + fontSize + "]");
      }
    }
    
    public void WriteLegend()
    {
      string legend = @"
      ""Legend""
      [shape=none]
      [label=<<TABLE border=""0"" cellborder=""1"" CELLSPACING=""0"">
      <TR><TD><b>Legend</b></TD></TR>
      <TR><TD bgcolor=""orange"">application</TD></TR>
      <TR><TD bgcolor=""olivedrab1"">utility</TD></TR>
      <TR><TD bgcolor=""lightblue"">dll</TD></TR>
      <TR><TD bgcolor=""lightgray"">static</TD></TR>
      <TR><TD bgcolor=""white"">not found</TD></TR>
      </TABLE>>];";
        dotFile.WriteLine(legend);
    }

    static public void reduceDotfile(string inputname, string outputname)
    {
      System.Diagnostics.Process tredProc = new System.Diagnostics.Process();
      Console.WriteLine("Using tred to create reduced dot file {0}", outputname);
      tredProc.StartInfo.FileName = "tred.exe";
      tredProc.StartInfo.Arguments = inputname;
      tredProc.StartInfo.UseShellExecute = false;
      tredProc.StartInfo.RedirectStandardOutput = true;

      string tredOutput;
      try
      {
          tredProc.Start();
          tredOutput = tredProc.StandardOutput.ReadToEnd();
          tredProc.WaitForExit();
          if (File.Exists(outputname)) File.Delete(outputname);
          StreamWriter writer = new StreamWriter(outputname);
          writer.Write(tredOutput);
          writer.Close();
      } 
        catch (Exception)
      {
          MessageBox.Show("Error during TRED execution (is Graphviz installed?)", Application.ProductName);
      }
    }

    static public void createPngFromDot(string dotInputname, string pngOutputname)
    {
      if (File.Exists(pngOutputname)) File.Delete(pngOutputname);
      System.Diagnostics.Process proc = new System.Diagnostics.Process();
      proc.StartInfo.FileName = "dot.exe";
      proc.StartInfo.Arguments = " -Tpng " + dotInputname + " -o " + pngOutputname;
      proc.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
      try
      {
          proc.Start();
          proc.WaitForExit();
      } 
        catch (Exception)
      {
          MessageBox.Show("Error during DOT execution (is Graphviz installed?)", Application.ProductName);
      }
    }

  }

  class Program
  {

    static public void shellExecute(string filename, string args)
    {
      System.Diagnostics.Process proc = new System.Diagnostics.Process();
      proc.StartInfo.FileName = filename;
      proc.StartInfo.Arguments = args;
      proc.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
      try
      {
          proc.Start();
      }
      catch (Exception)
      {
          MessageBox.Show(string.Format("Error starting: {0} {1}", proc.StartInfo.FileName, proc.StartInfo.Arguments), Application.ProductName);
      }
    }

    public delegate void Action();

    public static void PopulateSolutionTree(CharterForm control, string searchDir, string searchMask)
    {
      TreeNode rootNode = null;

      control.Invoke((Action)delegate()
      {
        control.progressBar.Style = ProgressBarStyle.Marquee;
        control.progressBar.MarqueeAnimationSpeed = 50;
        rootNode = control.solutionTree.Nodes.Add(searchDir);
      });

      ProcessDir(searchDir, searchMask, control, rootNode, 0);
      control.Invoke((Action)delegate()
      {
        control.progressBar.Style = ProgressBarStyle.Blocks;
        control.progressBar.Value = control.progressBar.Maximum;
      });
    }

    const int HowDeepToScan = 4;
    public static void ProcessDir(string searchDir, string searchMask, CharterForm control, TreeNode node, int recursionLvl)
    {
      if (recursionLvl <= HowDeepToScan)
      {

        string[] dirs = new string[0];
        try
        {
          dirs = Directory.GetFiles(searchDir, searchMask);
        }
        catch (Exception) 
        {
          dirs = new string[0];
        }

        // Process the list of files found in the directory. 
        foreach (string file in dirs)
        {
          control.Invoke((Action)delegate()
          {
            node.Nodes.Add(Path.GetFileName(file));
          });
        }
 
        // Recurse into subdirectories of this directory.
        string[] subdirEntries = new string[0];
        try
        {
          subdirEntries = Directory.GetDirectories(searchDir);
        }
        catch (Exception ) 
        {
          subdirEntries = new string[0];
        }

        foreach (string subdir in subdirEntries)
        {
          TreeNode subNode = null;
          control.Invoke((Action)delegate()
          {
            subNode = node.Nodes.Add(subdir.Substring(subdir.LastIndexOf("\\") + 1));
          });

          // Do not iterate through reparse points
          if ((File.GetAttributes(subdir) & FileAttributes.ReparsePoint) != FileAttributes.ReparsePoint)
          {
            ProcessDir(subdir, searchMask, control, subNode, recursionLvl + 1);
          }
          if (subNode.Nodes.Count == 0)
          {
            control.Invoke((Action) delegate() 
            {
              node.Nodes.Remove(subNode);
            });

          }
        }
      }
    }

    static void Execute()
    {
      Solution solution;
      if (Settings.input.ToLower().EndsWith(".sln"))
      {
        solution = new Solution();
        solution.read(Settings.input);
        solution.resolveIds();
        solution.markIgnoredProjects();
      }
      else
      {
        solution = new Solution(); 
        solution.readProject(Settings.input);
        solution.resolveIds();
        solution.markIgnoredProjects();
      }

      int deps = solution.DepCount;
      Console.WriteLine("Found " + solution.projects.Values.Count + " projects with " + deps + " relationships");

      if (Settings.configwindow)
      {
        CharterForm aCharterForm = new CharterForm();

        //PopulateSolutionTree(aCharterForm.solutionTree, "d:\\project", "*.sln");
 
        aCharterForm.cbReduce.Checked = Settings.reduce;
        aCharterForm.cbTrueType.Checked = Settings.truetypefont;

        if (Settings.fontsize > 0)
        {
          aCharterForm.cbFontsize.Checked = true;
          aCharterForm.tbFontsize.Text = Settings.fontsize.ToString();
        }
        
        foreach (Project project in solution.projects.Values)
        {
            int index = aCharterForm.projectsBox.Items.Add(project.name);
            if (!project.ignore)
            {
              aCharterForm.projectsBox.SetSelected(index, true);
            }
        }
        DialogResult result = aCharterForm.ShowDialog();
        if (result != DialogResult.OK)
        {
          // no nothing if the config-window is close with the top-right 'x' button
          return;
        }
        Settings.reduce = aCharterForm.cbReduce.Checked;
        Settings.truetypefont = aCharterForm.cbTrueType.Checked;

        Settings.fontsize = 0;
        if (aCharterForm.cbFontsize.Checked)
        {
          Int32.TryParse(aCharterForm.tbFontsize.Text, out Settings.fontsize);
        }

        Settings.aspectratio = 0.7;
        if (aCharterForm.cbAspect.Checked)
        {
          Double.TryParse(aCharterForm.tbAspect.Text, out Settings.aspectratio);
        }

        Settings.projectsList.Clear();
        Settings.ignoreEndsWithList.Clear();
       
        foreach (Project project in solution.projects.Values)
        {
            project.ignore = true;
        }

        foreach (string selectedProject in aCharterForm.projectsBox.SelectedItems)
        {
          foreach (Project project in solution.projects.Values)
          {
            if (project.name.Equals(selectedProject))
            {
              project.ignore = false;
            }
          }
        }
        
      }

      if (deps == 0)
      {
        Console.WriteLine("No dependencies, nothing to do...!");
        MessageBox.Show("No dependencies, nothing to do...!");
        Environment.Exit(0);
      }

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
      DotWriter dotWriter = new DotWriter(dotFileName);
      dotWriter.WriteAspectRatio(Settings.aspectratio);

      if (Settings.truetypefont)
      {
        dotWriter.WriteTTFont("calibri");
      }

      dotWriter.WriteFontSize(Settings.fontsize);
      dotWriter.WriteLegend();
      
             
      //dotFile.WriteLine("ranksep=1.5");
      //dotFile.WriteLine("edge [style=\"setlinewidth(4)\"]");
      //dotFile.WriteLine("graph [fontsize=32];");
      //dotFile.WriteLine("edge [fontsize=32];");
      //dotFile.WriteLine("nodesep=0.5");
      //dotFile.WriteLine("arrowsize=16.0");
      //dotFile.WriteLine("size=1024,700;");
      //dotFile.WriteLine("ratio=expand;"); // expand/fill/compress/auto

      solution.writeDepsInDotCode(dotWriter.dotFile);
      dotWriter.Close();

      string pngFile = Settings.workdir + solution.name + "_dep.png";
      string repngFile = Settings.workdir + solution.name + "_redep.png";
      
      if (Settings.reduce)
      {
        string redepFile = Settings.workdir + "redep.txt";
        DotWriter.reduceDotfile(dotFileName, redepFile);
        dotFileName = redepFile;
        pngFile = repngFile;
      }

      Console.WriteLine("Using dot to create bitmap");
      DotWriter.createPngFromDot(dotFileName, pngFile);
      Console.WriteLine("Done.");

      if (File.Exists(pngFile))
      {
        shellExecute(pngFile, "");
      }
      else
      {
        MessageBox.Show("Error generation diagram");
      }
    }

    [DllImport("user32.dll")]
    public static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

    [STAThread]
    static void Main(string[] args)
    {
      Settings.Initialize();
      foreach (string arg in args)
      {
        Settings.ProcessCommandline(arg);
      }

      if (Settings.hide)
      {
        IntPtr hWnd = Process.GetCurrentProcess().MainWindowHandle;
        ShowWindow(hWnd, 0);
      }

      if (Settings.input != null && File.Exists(Settings.input))
      {
        Execute();
      }
      else
      {
        string optionString = "";
        foreach (Option option in Settings.optionList)
        {
          optionString = optionString + "[" + option.text + "] ";
        }

        // display usage help
        Console.WriteLine("DepCharter " + optionString + "<filename.sln>\n");
        foreach (Option option in Settings.optionList)
        {
          Console.WriteLine(option.description);
        }
      }
    }
  }
}

// dependency: 'installations.TitanUtilities', installations\TitanUtilities\Utilities.csproj exists.
//todo: appearently a CoCa 'uses'-reference refers to a directory and not to a VS-project name (should we use the first or ALL projects we find in a directory?)

//todo: CoCa projects are not always found, maybe the project-base directory is not correctly determined?
//todo: implement 'searchDirs' options to search for related projects even if they are not found by following the CoCa uses-relations.
// how to interpret 'AUTOLOADER' as dependency?? see BhvSpecimenExchangePS probably just an errornous dependency
