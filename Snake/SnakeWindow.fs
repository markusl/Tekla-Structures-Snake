/// WPF display for the snake.
module SnakeWindow
open System
open System.Windows
open System.Windows.Controls
open System.Windows.Data
open System.Windows.Media
open System.Windows.Shapes

open System.Threading

open SnakeGame

let snakeColor          = new SolidColorBrush(Colors.GreenYellow)
let snakeHeadColor      = new SolidColorBrush(Colors.Green)
let wallColor           = new SolidColorBrush(Colors.Gray)
let foodColor           = new SolidColorBrush(Colors.Red)
let whiteColor          = new SolidColorBrush(Colors.White)

let screenWidth      = 520.
let screenHeight     = 520.
let blockSize        = (screenWidth - 20.) / snakeAreaSize

let title     = "$nake for Tekla Structures"

/// Draws rectangle or ellipse on the canvas
let drawGameItem<'TShape when 'TShape :> Shape and 'TShape: (new: unit -> 'TShape)> brush (canvas : Canvas) (item : GameItem) =
    let x, y = float item.X * blockSize, float item.Y * blockSize
    let width, height = blockSize - 1., blockSize - 1.
    let shape = new 'TShape()
    shape.Width <- width
    shape.Height <- height
    shape.Fill <- brush
    canvas.Children.Add(shape) |> ignore
    Canvas.SetTop(shape, y); Canvas.SetLeft(shape, x)

type SnakeWindow() as this =
    inherit Window(Width=screenWidth, Height=screenHeight, Title=title)
    let canvas = new Canvas(Background=whiteColor, VerticalAlignment=VerticalAlignment.Top)
    let timer = new Threading.DispatcherTimer(Interval = TimeSpan.FromMilliseconds(50.00), IsEnabled=true)

    let drawGame game =
        canvas.Children.Clear()
        TeklaSnake.redrawGame game
        game.foods      |> List.iter (drawGameItem<Ellipse> foodColor canvas)
        game.walls      |> List.iter (drawGameItem<Rectangle> wallColor canvas)
        game.snake.Tail |> List.iter (drawGameItem<Rectangle> snakeColor canvas)
        drawGameItem<Ellipse> snakeHeadColor canvas game.snake.Head

    let handleKeyPress (arg : Input.KeyEventArgs) game =
        match arg.Key, game.snakeDirection with
            | Input.Key.Up, SnakeGame.Down
            | Input.Key.Down, SnakeGame.Up
            | Input.Key.Left, SnakeGame.Right
            | Input.Key.Right, SnakeGame.Left -> game
            | Input.Key.Up, _ -> { game with snakeDirection = SnakeGame.Up }
            | Input.Key.Down, _ -> { game with snakeDirection = SnakeGame.Down }
            | Input.Key.Left, _ -> { game with snakeDirection = SnakeGame.Left }
            | Input.Key.Right, _ -> { game with snakeDirection = SnakeGame.Right }
            | Input.Key.Space, _ -> SnakeGame.initializeGame
            | _ -> game

    let moveSnake game =
        if game.gameOver then game
        else
            let next = SnakeGame.snakeMove game
            if next.gameOver then
                MessageBox.Show("Game over!", title) |> ignore;
            next

    do
        this.Content <- canvas
        let move = timer.Tick |> Observable.map (fun f -> moveSnake)
        let turn = this.KeyDown |> Observable.map (fun f -> handleKeyPress f)
        Observable.merge move turn
            |> Observable.scan (fun a b -> b a) SnakeGame.initializeGame
            |> Observable.add drawGame

/// Launches the SnakeWindow in another thread in single-threaded apartment mode.
type Launcher() =
    let runPluginVersion =
        let ts = ThreadStart(fun f -> SnakeWindow() |> Application().Run |> ignore)
        Thread(ts, ApartmentState = ApartmentState.STA).Start()

    do
        if not (Application.Current = null) then runPluginVersion
