Tekla Structures Snake plugin
======================
This Tekla Structures plugin enables you to play Snakes in Tekla Structures model view.

This is what Snakes game looks like.
![Snakes game](/img/snake_window.png)

This is what Snakes game looks like in *Tekla Structures*.
![Snakes game](/img/ts_snake_view.png)

How to play Snakes
-------------
Click on the toolbar icon and use your arrow keys to turn the head into another direction.

How to install Snakes
-------------
1.  Build Snake.sln Visual Studio project using Visual Studio 2012
2.  `Snake.dll` and `FSharp.Core.dll` build to the plugins directory.

Tekla Structures log will notify you that Plug-in Play Snakes in `C:\Program Files\Tekla Structures\19.0\nt\bin\plugins\Tekla\Model` loaded successfully.`
Note: Tekla Structures 19.0 x64 is the default output directory. For other versions of Tekla Structures, you need to change the references in build settings.

Configuring Snakes in Tekla Structures toolbar
-------------
![Toolbar configuration](/img/ts_toolbar.png)

Technical stuff
-------------
This is a (proof of concept) **in-process** plugin that creates a new [AppDomain](http://msdn.microsoft.com/en-us/library/system.appdomain.aspx) in which a new WPF window is launched in a new STA-thread to handle user input for the Snake movement.
You *should not* create new windows when overriding `Tekla.Structures.Plugins.PluginBase.Run` method but you should do what is recommended in the documentation.

Implementation is written in [F#](http://fsharp.net/) programming language which works on .NET framework like C#.
Code contains Snake game logic functionality, WPF window for visualizing and controlling the Snake, Tekla Structures plugin stub to launch the window. Snake can also be played without Tekla Structures model visualization using the standalone application.