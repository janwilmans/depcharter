using System;
using System.IO;
using System.Collections.Generic;

namespace DepCharter
{
    class ProjectDictionary : Dictionary<string, Project> { }
    class SolutionDictionary : Dictionary<Solution, bool> { }   //.net2.0 does not have HashSet<>, we work around it using the keys of a Dictionary

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
            if (m_solutionProjects.ContainsKey(project.Id))
            {
                Console.WriteLine("Ignoring duplicate project id from: " + project.Filename);
                return;
            }

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