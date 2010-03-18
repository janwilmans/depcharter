using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;

namespace DepCharter
{
  class SolutionParticle
  {
    public SolutionParticle(string content)
    {
      char[] charSeparators = { '"', '(', ')' };
      string[] result = content.Split(charSeparators, StringSplitOptions.RemoveEmptyEntries);
      values = new ArrayList(result);
    }

    public string this[int index]
    {
      get
      {
        return (string)values[index];
      }
    }
    public ArrayList values = new ArrayList();
  }

  class SolutionLine
  {
    public SolutionLine(string line)
    {
      char[] charSeparators = { ',' };
      string[] result = line.Split(charSeparators, StringSplitOptions.RemoveEmptyEntries);
      foreach (string content in result)
      {
        particles.Add(new SolutionParticle(content));
      }
    }

    public SolutionParticle this[int index]
    {
      get
      {
        return (SolutionParticle)particles[index];
      }
    }

    public ArrayList particles = new ArrayList();
  }

  class ProjectDictionary : Dictionary<string, Project> { }

  class MyStringReader : StringReader
  {
    public MyStringReader(string input) : base(input) { }

    static int linenumber = 1;
    public override string ReadLine()
    {
      string line = base.ReadLine();
      if (Settings.verbose) Console.WriteLine(linenumber + ": " + line);
      linenumber++;
      return line;
    }
  }
}