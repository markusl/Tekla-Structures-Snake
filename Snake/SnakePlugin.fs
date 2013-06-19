/// Contains Snake plugin to be loaded by Tekla Structures.
/// Wouldn't be loaded from module (static class).
/// After plugin is loaded, it can be placed on a toolbar and started.
namespace SnakePlugin

open System.Threading.Tasks
open System.Collections.Generic

open Tekla.Structures
open Tekla.Structures.Plugins

type SnakeData() = class end

[<Plugin("Play Snakes")>]
type SnakePlugin(data : SnakeData) =
    inherit PluginBase()
    do
        System.Diagnostics.Debug.WriteLine("Loading Snake plugin")

    /// Snakes does not need any input from user
    override x.DefineInput() = List<PluginBase.InputDefinition>()

    /// When running plugin, launch the UI in separate AppDomain to have its own
    /// message pump thread to not mess with Tekla Structures one.
    override x.Run(inputs) =
        try
            let domain = System.AppDomain.CreateDomain "$nakesssDomain"
            let assembly = typeof<SnakeWindow.SnakeWindow>.Assembly.Location
            let className = typeof<SnakeWindow.Launcher>.FullName
            domain.CreateInstanceFrom (assembly, className) |> ignore
        with | e ->
            // If something goes wrong, add a breakpoint here.
            System.Diagnostics.Debug.WriteLine(e.Message)
        true
