module SnakeGame
open System

let snakeAreaSize = 20.
let snakeOriginX, snakeOriginY = int snakeAreaSize / 2, int snakeAreaSize / 2
let random = new Random()

// Represents either a part of wall, snake or food
type GameItem(X : int, Y : int, size : int) =
    member this.X = X
    member this.Y = Y
    member this.Point = X, Y
    member this.size = size
    member this.eq (a: GameItem) = a.X = X && a.Y = Y
    
let snakeStartPosition = [GameItem(snakeOriginX, snakeOriginY, 0);
                          GameItem(snakeOriginX, snakeOriginY + 1, 0);
                          GameItem(snakeOriginX, snakeOriginY + 2, 0)]

/// Where the snake is going?
type SnakeDirection = Up | Down | Left | Right

/// Snake game state.
type SnakeGame = {
    walls : GameItem list;
    foods : GameItem list;
    snake : GameItem list;
    snakeDirection : SnakeDirection;
    snakeToGrown : int;
    /// True if snake hit itself or a wall
    gameOver : bool;
}

let rec private randomize size except = begin
    let randI() = random.Next(int snakeAreaSize) in
    let x, y = randI(), randI()
    match List.tryFind (fun (e : GameItem) -> e.X = x && e.Y = y) except with 
        | Some _ -> randomize size except
        | _      -> GameItem(x, y, size)
    end

/// Creates walls around the game area
let private initWalls exclude =
    let mutable borders = []
    for i in [0.. int snakeAreaSize - 1] do
        borders <- GameItem(0, i, 0) ::
                   GameItem(i, 0, 0) ::
                   GameItem(int snakeAreaSize - 1, i, 0) ::
                   GameItem(i, int snakeAreaSize - 1, 0) :: borders

    for i in [0..5] do
        borders <- randomize 0 (exclude @ borders) :: borders

    borders

let initFoods exclude =
    [for i in {1..5} -> randomize 3 exclude]

let defaultWalls = initWalls snakeStartPosition

let initializeGame =
    { snake = snakeStartPosition;
      snakeDirection = Up;
      snakeToGrown = 5;
      walls = defaultWalls;
      foods = initFoods (snakeStartPosition @ defaultWalls);
      gameOver = false }

let private growSnake (game : SnakeGame) =
    match game.snakeToGrown with 
        | 0 ->
            let revbody = List.rev game.snake in
            { game with snake = revbody.Tail |> List.rev }
        | _ -> { game with snakeToGrown = game.snakeToGrown - 1 }

let private maybeEatFood (game : SnakeGame) (head : GameItem) =
    let (eated, survived) = List.partition head.eq game.foods in
    match eated with
        | h :: _ -> { game with snakeToGrown = game.snakeToGrown + h.size;
                                foods = randomize 3 (game.snake @ game.walls) :: survived }
        | [] -> game

let snakeMove (game : SnakeGame) =
    let game = growSnake game in
    let old_head = game.snake.Head in
    let new_head = match game.snakeDirection with 
                    | Up    -> GameItem(old_head.X, old_head.Y - 1, 0)
                    | Down  -> GameItem(old_head.X, old_head.Y + 1, 0)
                    | Left  -> GameItem(old_head.X - 1, old_head.Y, 0)
                    | Right -> GameItem(old_head.X + 1, old_head.Y, 0)

    let game = maybeEatFood game new_head

    let isHeadCrashing =  match game.snake @ game.walls |> List.tryFind new_head.eq with
                            | Some _  -> false
                            | None    -> true
    { game with snake = new_head :: game.snake;
                gameOver = not isHeadCrashing }
