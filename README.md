# depcharter

[![Build status](https://ci.appveyor.com/api/projects/status/4of6ag1kxjadtaln/branch/master?svg=true)](https://ci.appveyor.com/project/janwilmans/depcharter/branch/master) using VS2017

creates project dependency diagrams from visual studio solution files in Graphviz DOT format
this project was inspired by:

<http://www.codeproject.com/KB/trace/dependencygraph.aspx>
and
<http://jamiepenney.co.nz/2009/02/10/viewing-dependencies-between-projects-in-visual-studio/>

but both projects depend on internal visual studio API's to read the .sln files. (and so they rely on professional versions of visual studio to build).
This project performs simular functions (and hopefully more) and also works with the express editions.

Tested with:
 * visual studio 2010 (C++ and C#) 
 * visual studio 2013, 2015, 2017 (C++ and C#), vsproj and vcxproj format.

# references

This project offers a similar visualization, but uses clang source code analysis to determine wat
the dependencies actually should be, instead of visualizing what dependencies we have.
<https://github.com/tomtom-international/cpp-dependencies>

See also
<http://dependencyvisualizer.codeplex.com/> (similar project)
<http://quickgraph.codeplex.com/> ?
