﻿using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Diagnostics;
using System.Globalization;
using System.Runtime.InteropServices;

namespace DepCharter
{
  class Program
  {
    static void Execute()
    {

      Solution solution = new Solution(Settings.input);
      solution.resolveIds();
      solution.markIgnoredProjects();

      Console.WriteLine("Found " + solution.projects.Values.Count + " projects");
      int deps = solution.DepCount;
      Console.WriteLine("Found " + deps + " dependencies");

      if (Settings.configwindow)
      {
        ConfigForm aConfigForm = new ConfigForm();
        aConfigForm.cbReduce.Checked = Settings.reduce;
        aConfigForm.cbTrueType.Checked = Settings.truetypefont;

        if (Settings.fontsize > 0)
        {
          aConfigForm.cbFontsize.Checked = true;
          aConfigForm.tbFontsize.Text = Settings.fontsize.ToString();
        }
        
        foreach (Project project in solution.projects.Values)
        {
            int index = aConfigForm.projectsBox.Items.Add(project.name);
            if (!project.ignore)
            {
              aConfigForm.projectsBox.SetSelected(index, true);
            }
        }
        DialogResult result = aConfigForm.ShowDialog();
        if (result != DialogResult.OK)
        {
          // no nothing if the config-window is close with the top-right 'x' button
          return;
        }
        Settings.reduce = aConfigForm.cbReduce.Checked;
        Settings.truetypefont = aConfigForm.cbTrueType.Checked;

        Settings.fontsize = 0;
        if (aConfigForm.cbFontsize.Checked)
        {
          Int32.TryParse(aConfigForm.tbFontsize.Text, out Settings.fontsize);
        }

        Settings.aspectratio = 0.7;
        if (aConfigForm.cbAspect.Checked)
        {
          Double.TryParse(aConfigForm.tbAspect.Text, out Settings.aspectratio);
        }

        Settings.projectsList.Clear();
        Settings.ignoreEndsWithList.Clear();
       
        foreach (Project project in solution.projects.Values)
        {
            project.ignore = true;
        }

        foreach (string selectedProject in aConfigForm.projectsBox.SelectedItems)
        {
          solution.projectsByName[selectedProject].ignore = false;
        }
        
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
      if (File.Exists(dotFileName))
      {
        File.Delete(dotFileName);
      }

      StreamWriter dotFile = File.CreateText(dotFileName);
      Console.WriteLine("Created " + dotFileName);

      dotFile.WriteLine("digraph G {");   // the first line for the .dot file

      if (Settings.aspectratio > 0.01)
      {
        // prevent any regional settings from interfering with number formatting
        String ratio = String.Format(CultureInfo.CreateSpecificCulture("en-us"), "aspect={0: #0.0}", Settings.aspectratio); 
        dotFile.WriteLine(ratio);
      }

      if (Settings.truetypefont)
      {
        Console.WriteLine("Using truetype font (calibri)");
        dotFile.WriteLine("fontname=calibri");
      }

      if (Settings.fontsize > 0)
      {
        Console.WriteLine("Using node fontsize = " + Settings.fontsize);
        dotFile.WriteLine("node [fontsize=" + Settings.fontsize + "]");
      }
             
      //dotFile.WriteLine("ranksep=1.5");
      //dotFile.WriteLine("edge [style=\"setlinewidth(4)\"]");
      //dotFile.WriteLine("graph [fontsize=32];");
      //dotFile.WriteLine("edge [fontsize=32];");
      //dotFile.WriteLine("nodesep=0.5");
      //dotFile.WriteLine("arrowsize=16.0");
      //dotFile.WriteLine("size=1024,700;");
      //dotFile.WriteLine("ratio=expand;"); // expand/fill/compress/auto

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

    [DllImport("user32.dll")]
    public static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

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
        Console.WriteLine("DepCharter " + optionString+ "<filename.sln>\n");
        foreach (Option option in Settings.optionList)
        {
          Console.WriteLine(option.description);
        }
      }
    }
  }
}