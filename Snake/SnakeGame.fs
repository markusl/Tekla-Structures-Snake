module SnakeGame
open System

// Represents either a part of wall, snake or food
type GameItem(X : int, Y : int, size : int) =
    member this.X = X
    member this.Y = Y
    member this.Point = X, Y
    member this.size = size
    member this.eq (a: GameItem) = a.X = X && a.Y = Y
    

let snakeAreaSize = 20.
let private snakeOriginX, snakeOriginY = int snakeAreaSize / 2, int snakeAreaSize / 2
let private random = new Random()
let private snakeStartPosition = [GameItem(snakeOriginX, snakeOriginY, 0);
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

let rec private createRandomElement size exclude =
    let randI() = random.Next(int snakeAreaSize)
    let x, y = randI(), randI()
    if List.exists (fun (e : GameItem) -> e.X = x && e.Y = y) exclude then
        createRandomElement size exclude
    else
        GameItem(x, y, size)

/// Creates walls around the game area
let private initWalls exclude =
    let mutable borders = []
    for i in [0.. int snakeAreaSize - 1] do
        borders <- GameItem(0, i, 0) ::
                   GameItem(i, 0, 0) ::
                   GameItem(int snakeAreaSize - 1, i, 0) ::
                   GameItem(i, int snakeAreaSize - 1, 0) :: borders

    for i in [0..5] do
        borders <- createRandomElement 0 (exclude @ borders) :: borders

    borders

let initFoods exclude =
    [for i in {1..5} -> createRandomElement 3 exclude]

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
            let revbody = List.rev game.snake
            { game with snake = revbody.Tail |> List.rev }
        | _ -> { game with snakeToGrown = game.snakeToGrown - 1 }

let private maybeEatFood (game : SnakeGame) (head : GameItem) =
    let (eated, survived) = List.partition head.eq game.foods
    match eated with
        | h :: _ -> { game with snakeToGrown = game.snakeToGrown + h.size;
                                foods = createRandomElement 3 (game.snake @ game.walls) :: survived }
        | [] -> game

let snakeMove (game : SnakeGame) =
    let game = growSnake game
    let oldHead = game.snake.Head
    let newHead = match game.snakeDirection with 
                    | Up    -> GameItem(oldHead.X, oldHead.Y - 1, 0)
                    | Down  -> GameItem(oldHead.X, oldHead.Y + 1, 0)
                    | Left  -> GameItem(oldHead.X - 1, oldHead.Y, 0)
                    | Right -> GameItem(oldHead.X + 1, oldHead.Y, 0)

    let game = maybeEatFood game newHead

    let isHeadCrashing =  game.snake @ game.walls |> List.exists newHead.eq
    { game with snake = newHead :: game.snake;
                gameOver = not isHeadCrashing }
