using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Diagnostics;
using System.Globalization;

namespace DepCharter
{
    enum Parser { notmatched, project, projectArgument, ignore, ignoreArgument, verbose, reduce, help, font, fontsize, fontsizeArgument, aspect, aspectArgument, hide, config, userProperties, searchDirs, restrictToSolution }

    class Settings
    {
        static public void Initialize()
        {
            Directory.CreateDirectory(Settings.TempDirectory);

            optionList.Add(new Option("/?", Parser.help, "  /?  : display usage information"));
            optionList.Add(new Option("/r", Parser.reduce, "  /r  : reduce edges implied by transitivity"));
            optionList.Add(new Option("/p", Parser.project, Parser.projectArgument, "  /p <project> : include project recursively (may be specified more then once)"));
            optionList.Add(new Option("/i", Parser.ignore, Parser.ignoreArgument, "  /i <project> : ignore specific project (may be specified more then once)"));
            optionList.Add(new Option("/f", Parser.font, "  /f  : use truetype font, better for readability but much slower)"));
            optionList.Add(new Option("/fs", Parser.fontsize, Parser.fontsizeArgument, "  /fs <size> : fontsize"));
            optionList.Add(new Option("/a", Parser.aspect, Parser.aspectArgument, "  /a <0.x> : aspect ratio for the graph (set zero for none)"));
            optionList.Add(new Option("/h", Parser.hide, "  /h  : hide console window"));
            optionList.Add(new Option("/c", Parser.config, "  /c  : show config window"));
            optionList.Add(new Option("/v", Parser.verbose, "  /v  : be verbose"));
            optionList.Add(new Option("/u", Parser.userProperties, "  /u  : read UserProperties from Project files to establish relationships"));
            optionList.Add(new Option("/o", Parser.restrictToSolution, "  /o  : only show projects that are actually in the solution"));
            //optionList.Add(new Option("/sd", Parser.searchDirs, "  /sd : search directories to find related projects"));
        }

        public static void ProcessOption(Parser action, string arg)
        {
            // we are processing an option (can be either the option itself or its argument)
            //Console.WriteLine(action + ", arg: " + arg);
            bool optionDone = true;
            switch (action)
            {
                case Parser.config:
                    Settings.configwindow = true;
                    break;
                case Parser.hide:
                    Settings.hide = true;
                    break;
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
                case Parser.aspectArgument:
                    if (!Double.TryParse(arg, out Settings.aspectratio))
                    {
                        Settings.aspectratio = 0.0;
                    }
                    break;
                case Parser.userProperties:
                    Settings.userProperties = true;
                    break;
                case Parser.restrictToSolution:
                    Settings.restrictToSolution = true;
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

        public static string TempDirectory
        {
            get { return System.IO.Path.GetTempPath() + "DepCharter\\"; }
        }

        public static string input;
        public static string workdir;
        public static bool verbose;
        public static bool reduce;
        public static bool truetypefont;
        public static int fontsize;
        public static bool hide;
        public static bool configwindow;
        public static double aspectratio = 0.7;     // fill the page by default
        public static bool userProperties;
        public static bool restrictToSolution;

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

}