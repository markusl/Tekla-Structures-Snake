/// This module contains Tekla Structures dependent parts.
///
/// Main functionality is to keep track of current SnakeGame state
/// and update Tekla Structures object visualizations to match accordingly.
///
module TeklaSnake
open SnakeGame
open Tekla.Structures.Model
open Tekla.Structures.Geometry3d

/// At each update, all snake foods are recreated in Tekla Structures model
let mutable private foods : Beam list = []
/// Keep track of maze objects. Maze is meant to be stable, so we only update
/// possible changes to Tekla Structures model.
let mutable private maze : (Beam * GameItem) list = []

/// Definitions
let private MAZE_MAX_Y = 30000.0
let private GAME_SCALE = 1000.0
let private MAZE_Z_LEVEL = 0.0

let private greenClass = "3"
let private redClass = "2"
let private grayClass = "666"
let private defaultMaterial = Material(MaterialString = "Steel_Undefined")
let private defaultPosition = Position(Depth = Position.DepthEnum.MIDDLE)
let private mazeProfile     = Profile(ProfileString = "PL600*600")
let private snakeProfile    = Profile(ProfileString = "D500")
let private foodProfile     = Profile(ProfileString = "D500")
let private model : Model = new Model()

/// Snake is represented by a polybeam with a round profile.
let private snake : PolyBeam = new PolyBeam(Material = defaultMaterial, Position = defaultPosition,
                                   Profile = snakeProfile, Class=greenClass)

/// Convert GameItem to Tekla Structures point
let private extractPoints (item:GameItem) = new Point(float item.X * GAME_SCALE,
                                                      MAZE_MAX_Y - float item.Y * GAME_SCALE,
                                                      MAZE_Z_LEVEL)

let private extractPointsZ z (item:GameItem) = new Point(float item.X * GAME_SCALE,
                                                         MAZE_MAX_Y - float item.Y * GAME_SCALE,
                                                         z)
let private contourPoints points = points |> List.map (fun e -> new ContourPoint(e, null))

let private gameItemToPoints (items:GameItem list) = items |> List.map extractPoints |> contourPoints

let private removeIfExists (teklaItem:Part) =
    if teklaItem.Identifier.IsValid() then
        teklaItem.Delete() |> ignore

let private updateTeklaItem (teklaItem:PolyBeam) items =
    removeIfExists teklaItem
    teklaItem.Contour <- new Contour(ContourPoints=new System.Collections.ArrayList(items |> gameItemToPoints |> Array.ofList))
    teklaItem.Insert() |> ignore
    model.CommitChanges() |> ignore

let private updateSnake = updateTeklaItem snake

let private createBeam color profile (startPoint, endPoint) =
    let food = new Tekla.Structures.Model.Beam(startPoint, endPoint,
                        Material = defaultMaterial, Position = defaultPosition,
                        Profile = profile, Class=color)
    food.Insert() |> ignore
    food

let private updateFoods (items:GameItem list) =
    foods |> List.map removeIfExists |> ignore
    let startAndEndPoints = List.zip (items |> List.map extractPoints)
                                     (items |> List.map (extractPointsZ 200.0))
    foods <- startAndEndPoints |> List.map (createBeam redClass foodProfile)

let private updateMaze (items:GameItem list) =
    let findGameItem items item = items |> List.exists (fun i -> i = snd item)
    let removedItems = maze |> List.filter (fun item -> not (findGameItem items item)) |> List.map fst
    let itemsLeft = maze |> List.filter (findGameItem items)
    removedItems |> List.map removeIfExists |> ignore
    let addedItems = items |> List.filter (fun item -> not (maze |> List.exists (fun i -> snd i = item)))
    let addedBeams = List.zip (addedItems |> List.map extractPoints)
                              (addedItems |> List.map (extractPointsZ 200.0)) |> List.map (createBeam grayClass mazeProfile)
    let addedMaze = List.zip addedBeams addedItems
    maze <- itemsLeft @ addedMaze

let redrawGame game =
    if model.GetConnectionStatus() then
        updateSnake game.snake
        updateFoods game.foods
        updateMaze game.walls

do
    System.Diagnostics.Debug.Assert(model.GetConnectionStatus(),
        "Correct version of Tekla Structures program not opened!")
