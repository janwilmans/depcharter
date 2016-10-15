// Solution.cs read Visual Studio Solution (.sln) files, these are plain-text files in a undocumented line-based format.

using System;
using System.IO;
using System.Collections; // ArrayList
using System.Collections.Generic; // Dictionary <>
using System.Windows.Forms;
using System.Diagnostics;

namespace DepCharter
{
    class ProjectDictionary : Dictionary<string, Project> { }
    class SolutionDictionary : Dictionary<Solution, bool> { }

    class BuildModel
    {
        public List<Solution> Solutions
        {
            get { return new List<Solution>(m_solutions.Keys); }
        }

        public ProjectDictionary SolutionProjects
        {
            get { return m_solutionProjects; }
        }

        public ProjectDictionary ReferencedProjects
        {
            get { return m_referencedProjects; }
        }

        public Project GetProjectById(string id)
        {
            if (m_solutionProjects.ContainsKey(id)) return m_solutionProjects[id];
            if (m_referencedProjects.ContainsKey(id)) return m_referencedProjects[id];
            throw new InvalidDataException("Project with id'" + id + "' unknown");
        }

        public void Add(Solution solution, Project project)
        {
            solution.Add(project);
            m_solutions[solution] = false;
            m_solutionProjects.Add(project.Id, project);
        }

        public void Add(Project project)
        {
            m_referencedProjects.Add(project.Id, project);
        }

        public bool IsSolutionProject(Project project)
        {
            return m_solutionProjects.ContainsKey(project.Id);
        }

        private SolutionDictionary m_solutions = new SolutionDictionary();
        private ProjectDictionary m_solutionProjects = new ProjectDictionary();
        private ProjectDictionary m_referencedProjects = new ProjectDictionary();   // not used yet...
    }

}