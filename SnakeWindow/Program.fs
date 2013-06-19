// Learn more about F# at http://fsharp.net
// See the 'F# Tutorial' project for more help.
open System.Windows

[<EntryPoint>]
[<System.STAThread>]
let main argv =
    new SnakeWindow.SnakeWindow()
        |> Application().Run
