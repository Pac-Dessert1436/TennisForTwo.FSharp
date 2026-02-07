namespace TennisForTwo.FSharp

open Microsoft.Xna.Framework
open Microsoft.Xna.Framework.Graphics
open Microsoft.Xna.Framework.Input

type GameMain() as this =
    inherit Game()

    let _graphics: GraphicsDeviceManager = new GraphicsDeviceManager(this)
    let mutable _spriteBatch: SpriteBatch option = None

    let mutable _font: SpriteFont option = None
    let mutable player1: Actor option = None
    let mutable player2: Actor option = None
    let mutable tennis: Actor option = None

    // Game state
    let mutable score1: int = 0
    let mutable score2: int = 0
    let mutable _gameState: GameState = GameState.Title

    // Game constants
    [<Literal>]
    let COURT_HEIGHT: int = 200

    [<Literal>]
    let NET_WIDTH: int = 10

    [<Literal>]
    let PLAYER_SPEED: float32 = 5.0f

    [<Literal>]
    let BALL_SPEED: float32 = 6.0f

    [<Literal>]
    let GRAVITY: float32 = 0.2f

    [<Literal>]
    let WINNING_SCORE: int = 7

    [<Literal>]
    let COURT_OFFSET: int = 80

    let (screenWidth: int, screenHeight: int) = 800, 600

    // Net rectangle for collision and drawing
    let netRect: Rectangle =
        let centerX: float32 = float32 screenWidth / 2.0f
        let centerY: float32 = float32 screenHeight / 2.0f
        let courtTop: float32 = centerY - float32 (COURT_HEIGHT / 2) + float32 COURT_OFFSET
        Rectangle(int (centerX - float32 NET_WIDTH / 2.0f), int courtTop, NET_WIDTH, COURT_HEIGHT)

    do
        this.Content.RootDirectory <- "Content"
        this.IsMouseVisible <- true

    override _.Initialize() =
        _graphics.PreferredBackBufferWidth <- screenWidth
        _graphics.PreferredBackBufferHeight <- screenHeight
        _graphics.ApplyChanges()
        this.Window.Title <- "Tennis for Two (F#)"

        base.Initialize()

    member _.resetGame() =
        // Reset scores
        score1 <- 0
        score2 <- 0
        let centerY: float32 = float32 screenHeight / 2.0f
        let courtTop: float32 = centerY - float32 (COURT_HEIGHT / 2) + float32 COURT_OFFSET

        // Player 1 on the left, and player 2 on the right
        player1.Value.Position <- Vector2(float32 100, courtTop + float32 (COURT_HEIGHT + COURT_OFFSET) / 2.0f)

        player2.Value.Position <-
            Vector2(float32 (screenWidth - 200), courtTop + float32 (COURT_HEIGHT + COURT_OFFSET) / 2.0f)

        // Tennis ball in the center
        tennis.Value.Position <- Vector2(float32 screenWidth / 2.0f, float32 screenHeight / 2.0f)

        // Initial ball velocity with slight upward angle for parabolic motion
        tennis.Value.Velocity <- Vector2(-BALL_SPEED, -BALL_SPEED * 0.5f)

    override _.LoadContent() =
        _spriteBatch <- Some(new SpriteBatch(this.GraphicsDevice))
        player1 <- Some(Actor(this.Content.Load<Texture2D> "player1", true))
        player2 <- Some(Actor(this.Content.Load<Texture2D> "player2", true))
        tennis <- Some(Actor(this.Content.Load<Texture2D> "tennis", false))
        _font <- Some(this.Content.Load<SpriteFont> "game_font")
        this.resetGame ()
        base.LoadContent()

    override _.Update(gameTime: GameTime) =
        if
            GamePad.GetState(PlayerIndex.One).Buttons.Back = ButtonState.Pressed
            || Keyboard.GetState().IsKeyDown Keys.Escape
        then
            this.Exit()

        let keyboardState: KeyboardState = Keyboard.GetState()
        let centerY: float32 = float32 screenHeight / 2.0f
        let courtTop: float32 = centerY - float32 (COURT_HEIGHT / 2) + float32 COURT_OFFSET

        let courtBottom: float32 =
            centerY + float32 (COURT_HEIGHT / 2) + float32 COURT_OFFSET

        let centerX: float32 = float32 screenWidth / 2.0f
        let netHalfWidth: float32 = float32 NET_WIDTH / 2.0f

        // Game state management
        match _gameState with
        | GameState.Title ->
            // Start game when space is pressed
            if keyboardState.IsKeyDown Keys.Space then
                _gameState <- GameState.Playing

        | GameState.Playing ->
            // Pause game when P is pressed
            if keyboardState.IsKeyDown Keys.P then
                _gameState <- GameState.Paused

            // Player 1 controls (W/S keys) - vertical movement
            if keyboardState.IsKeyDown Keys.W then
                if player1.Value.Position.Y > courtTop then
                    player1.Value.Position <- player1.Value.Position - Vector2(0.0f, PLAYER_SPEED)

            if keyboardState.IsKeyDown Keys.S then
                if player1.Value.Position.Y < courtBottom - float32 player1.Value.Height then
                    player1.Value.Position <- player1.Value.Position + Vector2(0.0f, PLAYER_SPEED)

            // Player 1 horizontal movement - can't go past net to the right
            if keyboardState.IsKeyDown Keys.A then
                if player1.Value.Position.X > 0.0f then
                    player1.Value.Position <- player1.Value.Position - Vector2(PLAYER_SPEED, 0.0f)

            if keyboardState.IsKeyDown Keys.D then
                if player1.Value.Position.X < centerX - netHalfWidth - float32 player1.Value.Width then
                    player1.Value.Position <- player1.Value.Position + Vector2(PLAYER_SPEED, 0.0f)

            // Player 2 controls (Up/Down arrow keys) - vertical movement
            if keyboardState.IsKeyDown Keys.Up then
                if player2.Value.Position.Y > courtTop then
                    player2.Value.Position <- player2.Value.Position - Vector2(0.0f, PLAYER_SPEED)

            if keyboardState.IsKeyDown Keys.Down then
                if player2.Value.Position.Y < courtBottom - float32 player2.Value.Height then
                    player2.Value.Position <- player2.Value.Position + Vector2(0.0f, PLAYER_SPEED)

            // Player 2 horizontal movement - can't go past net to the left
            if keyboardState.IsKeyDown Keys.Left then
                if player2.Value.Position.X > centerX + netHalfWidth then
                    player2.Value.Position <- player2.Value.Position - Vector2(PLAYER_SPEED, 0.0f)

            if keyboardState.IsKeyDown Keys.Right then
                if player2.Value.Position.X < float32 (screenWidth - player2.Value.Width) then
                    player2.Value.Position <- player2.Value.Position + Vector2(PLAYER_SPEED, 0.0f)

            // Ensure players don't cross the net
            // Player 1 on left side
            if player1.Value.Position.X + float32 player1.Value.Width > centerX - netHalfWidth then
                player1.Value.Position <-
                    Vector2(centerX - netHalfWidth - float32 player1.Value.Width, player1.Value.Position.Y)

            // Player 2 on right side
            if player2.Value.Position.X < centerX + netHalfWidth then
                player2.Value.Position <- Vector2(centerX + netHalfWidth, player2.Value.Position.Y)

            // Apply gravity to ball
            tennis.Value.Velocity <- Vector2(tennis.Value.Velocity.X, tennis.Value.Velocity.Y + GRAVITY)

            // Update ball position
            tennis.Value.Move()

            // Ball collision with court boundaries (without energy loss)
            if tennis.Value.Position.Y > courtBottom - float32 tennis.Value.Height / 2.0f then
                tennis.Value.Position <-
                    Vector2(tennis.Value.Position.X, courtBottom - float32 tennis.Value.Height / 2.0f)

                tennis.Value.Velocity <- Vector2(tennis.Value.Velocity.X, -tennis.Value.Velocity.Y * 0.8f)

            // Ball collision with net - proper collision detection with net volume
            let netLeft: float32 = centerX - netHalfWidth
            let netRight: float32 = centerX + netHalfWidth

            // Check if ball is colliding with net's horizontal bounds
            if
                tennis.Value.Position.X < netRight
                && tennis.Value.Position.X + float32 tennis.Value.Width / 2.0f > netLeft
            then
                // Ball can pass over the net if ballTop is above netTop (with some margin)
                // If ball is within the net's height range, it hits the net
                if netRect.Intersects tennis.Value.Bounds then
                    // Ball hits the net - bounce back horizontally
                    if tennis.Value.Velocity.X > 0.0f then
                        tennis.Value.Position <-
                            Vector2(netLeft - float32 tennis.Value.Width / 2.0f, tennis.Value.Position.Y)

                        tennis.Value.Velocity <-
                            Vector2(-abs tennis.Value.Velocity.X * 0.8f, tennis.Value.Velocity.Y * 0.7f)
                    else
                        tennis.Value.Position <- Vector2(netRight, tennis.Value.Position.Y)

                        tennis.Value.Velocity <-
                            Vector2(abs tennis.Value.Velocity.X * 0.8f, tennis.Value.Velocity.Y * 0.7f)

            // Ball collision with players - fix clipping by using circle-to-rectangle collision
            let ballRadius: float32 = float32 tennis.Value.Width / 4.0f

            let ballCenter: Vector2 =
                Vector2(tennis.Value.Position.X + ballRadius, tennis.Value.Position.Y + ballRadius)

            // Player 1 collision - circle to rectangle
            let player1Rect: Rectangle = player1.Value.Bounds

            let closestX: float32 =
                max (float32 player1Rect.X) (min ballCenter.X (float32 (player1Rect.X + player1Rect.Width)))

            let closestY: float32 =
                max (float32 player1Rect.Y) (min ballCenter.Y (float32 (player1Rect.Y + player1Rect.Height)))

            let distanceX: float32 = ballCenter.X - closestX
            let distanceY: float32 = ballCenter.Y - closestY
            let distanceSquared: float32 = distanceX * distanceX + distanceY * distanceY

            if distanceSquared < ballRadius * ballRadius then
                // Ensure ball is outside player bounds
                if tennis.Value.Velocity.X < 0.0f then // Ball is moving left, ensure it's to the right of player
                    tennis.Value.Position <-
                        Vector2(player1.Value.Position.X + float32 player1.Value.Width, tennis.Value.Position.Y)

                // Calculate hit position relative to player center
                let playerCenterY: float32 =
                    player1.Value.Position.Y + float32 player1.Value.Height / 2.0f

                let hitPosition: float32 = ballCenter.Y - playerCenterY
                let normalizedHit: float32 = hitPosition / (float32 player1.Value.Height / 2.0f)

                // Create parabolic trajectory with strong upward velocity to clear net
                // Always provide enough upward force to go over the net
                let upwardVelocity: float32 = -BALL_SPEED * 1.2f - normalizedHit * BALL_SPEED * 0.5f
                tennis.Value.Velocity <- Vector2(BALL_SPEED, upwardVelocity)

            // Player 2 collision - circle to rectangle
            let player2Rect: Rectangle = player2.Value.Bounds

            let closestX2: float32 =
                max (float32 player2Rect.X) (min ballCenter.X (float32 (player2Rect.X + player2Rect.Width)))

            let closestY2: float32 =
                max (float32 player2Rect.Y) (min ballCenter.Y (float32 (player2Rect.Y + player2Rect.Height)))

            let distanceX2: float32 = ballCenter.X - closestX2
            let distanceY2: float32 = ballCenter.Y - closestY2
            let distanceSquared2: float32 = distanceX2 * distanceX2 + distanceY2 * distanceY2

            if distanceSquared2 < ballRadius * ballRadius then
                // Ensure ball is outside player bounds
                if tennis.Value.Velocity.X > 0.0f then // Ball is moving right, ensure it's to the left of player
                    tennis.Value.Position <-
                        Vector2(player2.Value.Position.X - float32 tennis.Value.Width / 2.0f, tennis.Value.Position.Y)

                // Calculate hit position relative to player center
                let playerCenterY: float32 =
                    player2.Value.Position.Y + float32 player2.Value.Height / 2.0f

                let hitPosition: float32 = ballCenter.Y - playerCenterY
                let normalizedHit: float32 = hitPosition / (float32 player2.Value.Height / 2.0f)

                // Create parabolic trajectory with strong upward velocity to clear net
                // Always provide enough upward force to go over the net
                let upwardVelocity: float32 = -BALL_SPEED * 1.2f - normalizedHit * BALL_SPEED * 0.5f
                tennis.Value.Velocity <- Vector2(-BALL_SPEED, upwardVelocity)

            // Scoring
            if tennis.Value.Position.X < 0.0f then
                // Player 2 scores
                score2 <- score2 + 1

                // Check for winning condition
                if score2 >= WINNING_SCORE then
                    _gameState <- GameState.Result
                else
                    // Reset ball with parabolic trajectory
                    tennis.Value.Position <- Vector2(float32 screenWidth / 2.0f, float32 screenHeight / 2.0f)
                    tennis.Value.Velocity <- Vector2(BALL_SPEED, -BALL_SPEED * 0.5f)

            if tennis.Value.Position.X > float32 screenWidth then
                // Player 1 scores
                score1 <- score1 + 1

                // Check for winning condition
                if score1 >= WINNING_SCORE then
                    _gameState <- GameState.Result
                else
                    // Reset ball with parabolic trajectory
                    tennis.Value.Position <- Vector2(float32 screenWidth / 2.0f, float32 screenHeight / 2.0f)

                    tennis.Value.Velocity <- Vector2(-BALL_SPEED, -BALL_SPEED * 0.5f)

        | GameState.Paused ->
            if keyboardState.IsKeyDown Keys.Space then
                _gameState <- GameState.Playing
            // Restart game when 'R' is pressed
            if keyboardState.IsKeyDown Keys.R then
                _gameState <- GameState.Title
                this.resetGame ()
        | GameState.Result ->
            // Restart game when R is pressed
            if keyboardState.IsKeyDown Keys.R then
                _gameState <- GameState.Title
                this.resetGame ()
        | _ -> ()

        base.Update gameTime

    override _.Draw(gameTime: GameTime) =
        this.GraphicsDevice.Clear Color.CornflowerBlue

        match _spriteBatch with
        | Some(batch: SpriteBatch) ->
            batch.Begin(samplerState = SamplerState.PointClamp)

            let centerX: float32 = float32 screenWidth / 2.0f
            let centerY: float32 = float32 screenHeight / 2.0f

            match _gameState with
            | GameState.Title ->
                // Draw title screen
                let titleText: string = "TENNIS FOR TWO"
                let startText: string = "PRESS SPACE TO START"

                let titlePosition: Vector2 =
                    Vector2(centerX - _font.Value.MeasureString(titleText).X / 2.0f, centerY - 50.0f)

                let startPosition: Vector2 =
                    Vector2(centerX - _font.Value.MeasureString(startText).X / 2.0f, centerY + 20.0f)

                batch.DrawString(_font.Value, titleText, titlePosition, Color.White)
                batch.DrawString(_font.Value, startText, startPosition, Color.White)

            | GameState.Playing
            | GameState.Paused ->
                // Draw game elements for playing and paused states
                let rectTexture: Texture2D = new Texture2D(this.GraphicsDevice, 1, 1)
                rectTexture.SetData [| Color.White |]

                // Calculate court bounds with offset
                let courtBottom: float32 = centerY + float32 (COURT_HEIGHT / 2 + COURT_OFFSET)

                // Draw court bottom boundary
                let courtRect: Rectangle = Rectangle(0, int courtBottom, screenWidth, 10)
                batch.Draw(rectTexture, courtRect, Color.LightGreen)
                batch.Draw(rectTexture, netRect, Color.Chartreuse)

                // Draw players and ball
                player1.Value.Draw batch
                player2.Value.Draw batch
                tennis.Value.Draw batch

                // Draw score
                let scoreText: string = sprintf "%d - %d" score1 score2

                let scorePosition: Vector2 =
                    Vector2(centerX - _font.Value.MeasureString(scoreText).X / 2.0f, 10.0f)

                batch.DrawString(_font.Value, scoreText, scorePosition, Color.White)

                // Draw pause screen overlay if paused
                if _gameState = GameState.Paused then
                    // Semi-transparent overlay
                    let overlayRect: Rectangle = Rectangle(0, 0, screenWidth, screenHeight)

                    batch.Draw(rectTexture, overlayRect, Color.Black * 0.5f)

                    // Pause text
                    let pauseText: string = "GAME PAUSED"
                    let resumeText: string = "Press SPACE to resume"
                    let restartText: string = "Press \"R\" to restart"

                    let pausePosition: Vector2 =
                        Vector2(centerX - _font.Value.MeasureString(pauseText).X / 2.0f, centerY - 30.0f)

                    let resumePosition: Vector2 =
                        Vector2(centerX - _font.Value.MeasureString(resumeText).X / 2.0f, centerY + 10.0f)

                    let restartPosition: Vector2 =
                        Vector2(centerX - _font.Value.MeasureString(restartText).X / 2.0f, centerY + 40.0f)

                    batch.DrawString(_font.Value, pauseText, pausePosition, Color.White)
                    batch.DrawString(_font.Value, resumeText, resumePosition, Color.White)
                    batch.DrawString(_font.Value, restartText, restartPosition, Color.White)

            | GameState.Result ->
                // Draw game result screen
                let rectTexture: Texture2D = new Texture2D(this.GraphicsDevice, 1, 1)
                rectTexture.SetData [| Color.White |]

                // Semi-transparent overlay
                let overlayRect: Rectangle = Rectangle(0, 0, screenWidth, screenHeight)
                batch.Draw(rectTexture, overlayRect, Color.Black * 0.5f)

                // Determine winner
                let winnerText: string =
                    if score1 >= WINNING_SCORE then
                        "PLAYER 1 WINS!"
                    else
                        "PLAYER 2 WINS!"

                let finalScoreText = sprintf "Final Score: %d - %d" score1 score2
                let restartText: string = "Press \"R\" to restart"

                let winnerPosition: Vector2 =
                    Vector2(centerX - _font.Value.MeasureString(winnerText).X / 2.0f, centerY - 40.0f)

                let scorePosition =
                    Vector2(centerX - _font.Value.MeasureString(finalScoreText).X / 2.0f, centerY + 10.0f)

                let restartPosition =
                    Vector2(centerX - _font.Value.MeasureString(restartText).X / 2.0f, centerY + 50.0f)

                // Draw winner text in gold color
                batch.DrawString(_font.Value, winnerText, winnerPosition, Color.Gold)
                batch.DrawString(_font.Value, finalScoreText, scorePosition, Color.White)
                batch.DrawString(_font.Value, restartText, restartPosition, Color.White)
            | _ -> ()

            batch.End()
        | None -> ()

        base.Draw gameTime


module GameMain =
    [<EntryPoint>]
    let main _ =
        use game: GameMain = new GameMain()
        game.Run()

        0
