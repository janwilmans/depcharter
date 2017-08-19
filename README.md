# depcharter

[![Build status](https://ci.appveyor.com/api/projects/status/4of6ag1kxjadtaln/branch/master?svg=true)](https://ci.appveyor.com/project/janwilmans/depcharter/branch/master)

creates project dependency diagrams from visual studio solution files in Graphviz DOT format
this project was inspired by:

http://www.codeproject.com/KB/trace/dependencygraph.aspx
and
http://jamiepenney.co.nz/2009/02/10/viewing-dependencies-between-projects-in-visual-studio/

but both projects depend on internal visual studio API's to read the .sln files. (and so they rely on professional versions of visual studio to build).
This project performs simular functions (and hopefully more) and also works with the express editions.

Tested with:
- visual studio 2010 (C++ and C#)
- visual studio 2013 (C++ and C#), vsproj and vcxproj format.

Untested, but should work just as well: vs2015 / vs2017 (all editions)
