// Dependency.cs is a reference to a Project with its origin 

using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.Xml.XPath;

namespace DepCharter
{
    enum Origin
    {
        Solution,
        ProjectToProject,
        UserProperty
    }

    class DependencyCollection
    {
        public void Add(Project project, Origin origin)
        {
            if (dependencies.ContainsKey(project))
            {
                dependencies[project].Add(origin);
            }
            else
            {
                dependencies.Add(project, new List<Origin>{origin});
            }
        }

        public List<Origin> this[Project key]
        {
            get 
            {
                return dependencies[key];
            }
        }

        public List<Project> Keys
        {
            get
            {
                return new List<Project>(dependencies.Keys);
            }
        }

        public int Count
        {
            get
            {
                return dependencies.Count;
            }
        }

        private Dictionary<Project, List<Origin>> dependencies = new Dictionary<Project, List<Origin>>();
    }
}