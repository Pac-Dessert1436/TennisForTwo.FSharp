namespace TennisForTwo.FSharp

open Microsoft.Xna.Framework
open Microsoft.Xna.Framework.Graphics

type Actor(texture: Texture2D, isPlayer: bool) =
    let mutable _position: Vector2 = Vector2.Zero
    let mutable _velocity: Vector2 = Vector2.Zero

    member _.Position
        with get () = _position
        and set (value: Vector2) = _position <- value

    member _.Velocity
        with get () = _velocity
        and set (value: Vector2) = _velocity <- value

    member this.Move() =
        this.Position <- this.Position + this.Velocity

    member _.Draw(spriteBatch: SpriteBatch) =
        spriteBatch.Draw(
            texture,
            _position,
            new Rectangle(0, 0, texture.Width, texture.Height),
            Color.White,
            0.0f,
            Vector2.Zero,
            (if isPlayer then 1.0f else 0.5f),
            SpriteEffects.None,
            0.0f
        )

    member _.Width = texture.Width
    member _.Height = texture.Height

    member this.Bounds =
        new Rectangle(
            int this.Position.X,
            int this.Position.Y,
            (if isPlayer then this.Width else this.Width / 2),
            (if isPlayer then this.Height else this.Height / 2)
        )

type GameState =
    | Title = 0
    | Playing = 1
    | Paused = 2
    | Result = 3

// TODO: Press 'SPACE' to start the game, 'P' to pause the game, and 'R' to restart.
