using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Diagnostics;

namespace DepCharter
{
  enum Parser { notmatched, project, projectArgument, ignore, ignoreArgument, verbose, reduce, help, font, fontsize, fontsizeArgument}

  class Settings
  {
    static public void Initialize()
    {
      optionList.Add(new Option("/?", Parser.help, "display usage information"));
      optionList.Add(new Option("/r", Parser.reduce, "reduce edges implied by transitivity"));
      optionList.Add(new Option("/p", Parser.project, Parser.projectArgument, "include project recursively (may be specified more then once)"));
      optionList.Add(new Option("/i", Parser.ignore, Parser.ignoreArgument, "ignore specific project (may be specified more then once)"));
      optionList.Add(new Option("/f", Parser.font, "use truetype font, better for readability but much slower)"));
      optionList.Add(new Option("/fs", Parser.fontsize,  Parser.fontsizeArgument, "fontsize)"));
      optionList.Add(new Option("/v", Parser.verbose, "be verbose"));
    }

    public static void ProcessOption(Parser action, string arg)
    {
      // we are processing an option (can be either the option itself or its argument)
      //Console.WriteLine(action + ", arg: " + arg);
      bool optionDone = true;
      switch (action)
      {
        case Parser.reduce:
          Settings.reduce = true;
          break;
        case Parser.verbose:
          Settings.verbose = true;
          break;
        case Parser.font:
          Settings.truetypefont = true;
          break;

        case Parser.ignoreArgument:
          Settings.ignoreEndsWithList.Add(arg.ToLower());
          break;

        case Parser.projectArgument:
          Settings.projectsList.Add(arg.ToLower());
          break;

        case Parser.fontsizeArgument:
          if (!Int32.TryParse(arg, out Settings.fontsize))
          {
            Settings.fontsize = 0;
          }
          break;
        default:
          // we are done process this option's arguments
          // if no action is takes (default), we are not done yet.
          optionDone = false;
          break;
      }

      if (optionDone)
      {
        currentOption = null;
      }
      parserAction = nextAction;
      nextAction = Parser.notmatched;
    }


    public static void ProcessCommandline(string arg)
    {
      nextAction = Parser.notmatched;
      if (currentOption != null)
      {
        ProcessOption(parserAction, arg);
        return;
      }

      foreach (Option option in optionList)
      {
        if (option.text.ToLower() == arg.ToLower().Trim())
        {
          //Console.WriteLine("Matched: " + option.argumentAction);
          currentOption = option;
          nextAction = option.argumentAction;
          ProcessOption(option.optionAction, arg);
          return;
        }
      }
      if (currentOption == null && File.Exists(arg))    // the Parser was not matched, asume it was a filename
      {
        Settings.input = Path.GetFullPath(arg).ToLower();
        Settings.workdir = Path.GetDirectoryName(Settings.input) + "\\";
        Console.WriteLine("Settings.input: " + Settings.input);
      }
      else
      {
        Console.WriteLine("Ignoring unknown argument " + arg);
        return;
      }
    }
    public static string input;
    public static string workdir;
    public static bool verbose;
    public static bool reduce;
    public static bool truetypefont;
    public static int fontsize;

    public static Parser parserAction;
    public static Parser nextAction;

    public static ArrayList ignoreEndsWithList = new ArrayList();
    public static ArrayList projectsList = new ArrayList();
    public static ArrayList optionList = new ArrayList();
    public static Option currentOption;

  }

  class Option
  {
    public string text;
    public Parser optionAction;
    public Parser argumentAction;
    public string description;

    public Option(string aText, Parser anOption, Parser anArgument, string aDescription)
    {
      text = aText;
      optionAction = anOption;
      argumentAction = anArgument;
      description = aDescription;
    }
    public Option(string aText, Parser anOption, string aDescription)
    {
      text = aText;
      optionAction = anOption;
      argumentAction = Parser.notmatched;
      description = aDescription;
    }
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

  class Program
  {
    static void Execute()
    {
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

      StreamWriter dotFile = File.CreateText(dotFileName);
      Console.WriteLine("Created " + dotFileName);

      dotFile.WriteLine("digraph G {");   // the first line for the .dot file
      dotFile.WriteLine("aspect=0.7");    // fill the page

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

      solution.markIgnoredProjects();
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
      Settings.Initialize();
      foreach (string arg in args)
      {
        Settings.ProcessCommandline(arg);
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
        Console.WriteLine("DepCharter " + optionString+ "<filename.sln>\n");
        foreach (Option option in Settings.optionList)
        {
          Console.WriteLine("  " + option.text + ": " + option.description);
        }
      }
    }
  }
}
