using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using System.ComponentModel;
using System.Text;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;



namespace TetrisDemo
{
    public enum PlaceStates
    {
        CAN_PLACE,
        BLOCKED,
        OFFSCREEN
    }

    enum GameStates { TitleScreen, Playing, GameOver, Controls};

    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class Game1 : Microsoft.Xna.Framework.Game
    {

        System.Diagnostics.Stopwatch myStopWatch = new System.Diagnostics.Stopwatch();
        const int BoardWidth = 10;  // Board width in blocks
        const int BoardHeight = 20; // Board height in blocks
        const int BlockSize = 20;   // Block size in pixels

        GameStates gameState = GameStates.TitleScreen;

        Texture2D titleScreen;
        Texture2D gamebackground;
        Texture2D mainbackground;
        Texture2D optionsMenu;
        Texture2D controlsMenu;
        Rectangle mainFrame;
        Texture2D spriteSheet;
        Score score;
        SpriteFont pericles14;
        Song song;

        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;





        List<int[,]> pieces;

        int[,] Board;
        int[,] SpawnedPiece;
        int[,] NextSpawnedPiece;
        Vector2 NextSpawnedPieceLocation;
        Vector2 SpawnedPieceLocation;
        Vector2 BoardLocation;
        private Vector2 scoreLocation = new Vector2(540, 25);
        private Vector2 linesLocation = new Vector2(110, 25);
        private Vector2 levelLocation = new Vector2(110, 125);
        private Vector2 elapsedhoursLocation = new Vector2(110, 225);
        private Vector2 titleScreenLocation = new Vector2(-450, -480);
        float timer;
        int timecounter;


        
        int StepTime = 300;  // Time step between updates in ms
        int ElapsedTime = 0;  // Total elapsed time since the last update
        int KeyBoardElapsedTime = 0;  // Total elapsed time since handling the last keypress

        Random rand = new Random(System.Environment.TickCount);  // Seed a new random number generator

        Color[] TetronimoColors = {
                                    Color.Transparent,  /* 0 */
                                    Color.Orange,       /* 1 */
                                    Color.Blue,         /* 2 */
                                    Color.Red,          /* 3 */
                                    Color.LightSkyBlue, /* 4 */
                                    Color.Yellow,       /* 5 */
                                    Color.Magenta,      /* 6 */
                                    Color.LimeGreen,    /* 7 */
                                    Color.WhiteSmoke    /* 8 */
                                    
                                  };


        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";

            graphics.PreferredBackBufferHeight = 440;
            graphics.PreferredBackBufferWidth = 800;
            Board = new int[BoardWidth, BoardHeight];
            ElapsedTime = 0;

            score = new Score();
            
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            this.IsMouseVisible = true;

            pieces = new List<int[,]>();

            /* I Piece */
            pieces.Add(new int[4, 4] { 
                                         {0, 0, 0, 0},
                                         {1, 1, 1, 1},
                                         {0, 0, 0, 0},
                                         {0, 0, 0, 0}
                                     });

            /* J Piece */
            pieces.Add(new int[3, 3] { 
                                         {0, 0, 1},
                                         {1, 1, 1},
                                         {0, 0, 0}
                                     });

            /* O Piece */
            pieces.Add(new int[2, 2] { 
                                         {1, 1},
                                         {1, 1}
                                     });

            /* S Piece */
            pieces.Add(new int[3, 3] { 
                                         {0, 1, 1},
                                         {1, 1, 0},
                                         {0, 0, 0}
                                     });


            /* T Piece */
            pieces.Add(new int[3, 3] { 
                                         {0, 1, 0},
                                         {1, 1, 1},
                                         {0, 0, 0}
                                     });

            /* Z Piece */
            pieces.Add(new int[3, 3] { 
                                         {1, 1, 0},
                                         {0, 1, 1},
                                         {0, 0, 0}
                                     });

            /* L Piece */
            pieces.Add(new int[3, 3] { 
                                         {1, 0, 0},
                                         {1, 1, 1},
                                         {0, 0, 0}
                                     });


            // Reset the board
            InitializeBoard(Board);
            SpawnPiece();
            SpawnPiece();  //spawn on deck piece


            base.Initialize();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);

            // TODO: use this.Content to load your game content here
            spriteSheet = Content.Load<Texture2D>(@"TetrisSprites");
            titleScreen = Content.Load<Texture2D>(@"tetristitle17");
            pericles14 = Content.Load<SpriteFont>("Pericles14");
            mainbackground = Content.Load<Texture2D>("mainbackground");
            gamebackground = Content.Load<Texture2D>("gamebackground");
            optionsMenu = Content.Load<Texture2D>("optionsMenu");
            controlsMenu = Content.Load<Texture2D>("controlsMenu");
            song = Content.Load<Song>("gameMusic");
            mainFrame = new Rectangle(0, 0, GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height);


            


        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// all content.
        /// </summary>
        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }

        public void InitializeBoard(int[,] board)
        {
            BoardLocation = new Vector2(300, 20);

            // Reset all board grid locations to empty
            for (int x = 0; x < BoardWidth; x++)
                for (int y = 0; y < BoardHeight; y++)
                {
                    board[x, y] = 0;
                }
        }

        // Create a new piece
        public void SpawnPiece()
        {
            int colr = rand.Next(0, pieces.Count);

            SpawnedPiece = (int[,])pieces[colr].Clone();
            SpawnedPiece = NextSpawnedPiece;
            SpawnedPieceLocation = new Vector2(4, 0);
            NextSpawnedPiece = (int[,])pieces[colr].Clone();
            int dim = NextSpawnedPiece.GetLength(0);
            for (int x = 0; x < dim; x++)
                for (int y = 0; y < dim; y++)
                    NextSpawnedPiece[x, y] *= (colr + 1);

            NextSpawnedPieceLocation = new Vector2(14, 7); // Temporary
        }


        // Checks to see if piece can be placed at location x,y on the board
        // Returns PlaceStates.CAN_PLACE if it can exist there, otherwise reports a reason why it cannot
        public PlaceStates CanPlace(int[,] board, int[,] piece, int x, int y)
        {
            // First we'll need to know the dimensions of the piece
            // Since they are square it is sufficient to just get the dimension of the first row
            int dim = piece.GetLength(0);

            // All pieces are square, so let's use a nested loop to iterate through all the cells of the piece
            for (int px = 0; px < dim; px++)
                for (int py = 0; py < dim; py++)
                {
                    // Calculate where on the game board this segment should be placed
                    int coordx = x + px;
                    int coordy = y + py;

                    // Is this space empty?
                    if (piece[px, py] != 0)
                    {
                        // If the board location would be too far to the left or right then
                        // we are hitting a wall
                        if (coordx < 0 || coordx >= BoardWidth)
                            return PlaceStates.OFFSCREEN;

                        // If even one segment can't be placed because it is being blocked then
                        // we need to return the BLOCKED state
                        if (coordy >= BoardHeight || board[coordx, coordy] != 0)
                        {
                            return PlaceStates.BLOCKED;
                        }
                    }
                }

            // If we get this far we can place the piece!
            return PlaceStates.CAN_PLACE;

        }

        // Permanently write piece to the game board 
        // Note that this method assumes that the piece can actually be placed already and does not recheck
        // to make sure the piece can be placed
        public void Place(int[,] board, int[,] piece, int x, int y)
        {
            
            int dim = piece.GetLength(0);

            for (int px = 0; px < dim; px++)
                for (int py = 0; py < dim; py++)
                {
                    int coordx = x + px;
                    int coordy = y + py;

                    if (piece[px, py] != 0)
                    {
                        board[coordx, coordy] = piece[px, py];

                    }
                    
                }

            RemoveCompleteLines(board);
            score.PlayerScore += 50;

        }

        // Rotate the piece (this style of rotation for Tetris is called a super rotation as it doesn't follow the conventional
        // style of piece rotations)
        public int[,] Rotate(int[,] piece, bool left)
        {
            int dim = piece.GetLength(0);
            int[,] npiece = new int[dim, dim];

            for (int i = 0; i < dim; i++)
                for (int j = 0; j < dim; j++)
                {
                    if (left)
                        npiece[j, i] = piece[i, dim - 1 - j];
                    else
                        npiece[j, i] = piece[dim - 1 - i, j];
                }

            return npiece;
        }

        // Check to see if there are any already completed lines on the board, if yes remove them
        public void RemoveCompleteLines(int[,] board)
        {
            for (int y = BoardHeight - 1; y >= 0; y--)
            {
                bool isComplete = true;
                for (int x = 0; x < BoardWidth; x++)
                {
                    if (board[x, y] == 0)
                    {
                        isComplete = false;
                    }
                }
                if (isComplete)
                {
                    score.PlayerScore += 500;
                    score.PlayerLines += 1;
                    score.PlayerLevel = score.PlayerLines / 5;
                    StepTime = 300 - (int)score.PlayerLevel * 10;

                    // Row y needs to go bye bye
                    // Copy row y-1 to row y
                    for (int yc = y; yc > 0; yc--)
                    {
                        for (int x = 0; x < 10; x++)
                        {
                            board[x, yc] = board[x, yc - 1];
                        }
                    }

                    // Recheck this row
                    y++;


                }
            }
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {

            // Allows the game to exit
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
                this.Exit();
            // TODO: Add your update logic here

            ElapsedTime += gameTime.ElapsedGameTime.Milliseconds;
            KeyBoardElapsedTime += gameTime.ElapsedGameTime.Milliseconds;

            timer += (float)gameTime.ElapsedGameTime.TotalSeconds;
            timecounter += (int)timer;
            if (timer >= 1.0f) timer = 0f;

            


            KeyboardState ks = Keyboard.GetState();

            if (gameState == GameStates.TitleScreen)
            {
                if (ks.IsKeyDown(Keys.Q))
                {
                    this.Exit();
                }

                if (ks.IsKeyDown(Keys.Space))
                {
                    MediaPlayer.Play(song);
                    gameState = GameStates.Playing;
                    myStopWatch.Start();
                    MediaPlayer.Play(song);
                    InitializeBoard(Board);
                    SpawnPiece();
                    SpawnPiece();
                    gameState = GameStates.Playing;

                    score.PlayerLevel = 0;
                    score.PlayerLines = 0;
                    score.PlayerScore = 0;
                }
            }
            else if (gameState == GameStates.Playing)
            {
                if (ks.IsKeyDown(Keys.C))
                {
                    gameState = GameStates.Controls;
                }
                if (ks.IsKeyDown(Keys.Escape))
                {
                    gameState = GameStates.TitleScreen;
                    MediaPlayer.Stop();
                    myStopWatch.Reset();
                }

                if (ks.IsKeyDown(Keys.S))
                {
                    MediaPlayer.Stop();
                    MediaPlayer.Play(song);

                }

                if (ks.IsKeyDown(Keys.Q))
                {
                    this.Exit();
                }

                if (ks.IsKeyDown(Keys.R) && myStopWatch.Elapsed.Seconds >= 1)
                {
                    if (gameState == GameStates.Playing)
                    {
                        MediaPlayer.Stop();
                        MediaPlayer.Play(song);
                    }
                    InitializeBoard(Board);
                    SpawnPiece();
                    SpawnPiece();
                    gameState = GameStates.Playing;

                    score.PlayerLevel = 0;
                    score.PlayerLines = 0;
                    score.PlayerScore = 0;

                    myStopWatch.Restart();
                }

                if (KeyBoardElapsedTime > 200)
                {
                    if (ks.IsKeyDown(Keys.Left) || ks.IsKeyDown(Keys.Right))
                    {
                        // Create a new location that contains where we WANT to move the piece
                        Vector2 NewSpawnedPieceLocation = SpawnedPieceLocation + new Vector2(ks.IsKeyDown(Keys.Left) ? -1 : 1, 0);

                        // Next, check to see if we can actually place the piece there
                        PlaceStates ps = CanPlace(Board, SpawnedPiece, (int)NewSpawnedPieceLocation.X, (int)NewSpawnedPieceLocation.Y);
                        if (ps == PlaceStates.CAN_PLACE)
                        {
                            SpawnedPieceLocation = NewSpawnedPieceLocation;
                        }

                        KeyBoardElapsedTime = 0;
                    }

                    if (ks.IsKeyDown(Keys.Up))
                    {
                        int[,] newSpawnedPiece = Rotate(SpawnedPiece, true);

                        PlaceStates ps = CanPlace(Board, newSpawnedPiece, (int)SpawnedPieceLocation.X, (int)SpawnedPieceLocation.Y);
                        if (ps == PlaceStates.CAN_PLACE)
                        {
                            SpawnedPiece = newSpawnedPiece;
                        }

                        KeyBoardElapsedTime = 0;
                    }


                    if (ks.IsKeyDown(Keys.Down))
                    {
                        ElapsedTime = StepTime + 1;
                        KeyBoardElapsedTime = 175;
                        score.PlayerScore += 1;
                    }
                }

                // If the accumulated time over the last couple Update() method calls exceeds our StepTime variable
                if (ElapsedTime > StepTime)
                {
                    // Create a new location for this spawned piece to go to on the next update
                    Vector2 NewSpawnedPieceLocation = SpawnedPieceLocation + new Vector2(0, 1);

                    // Now check to see if we can place the piece at that new location
                    PlaceStates ps = CanPlace(Board, SpawnedPiece, (int)NewSpawnedPieceLocation.X, (int)NewSpawnedPieceLocation.Y);
                    if (ps != PlaceStates.CAN_PLACE)
                    {
                        // We can't move down any further, so place the piece where it is currently
                        Place(Board, SpawnedPiece, (int)SpawnedPieceLocation.X, (int)SpawnedPieceLocation.Y);
                        SpawnPiece();

                        // This is just a check to see if the newly spawned piece is already blocked, in which case the 
                        // game is over
                        ps = CanPlace(Board, SpawnedPiece, (int)SpawnedPieceLocation.X, (int)SpawnedPieceLocation.Y);
                        if (ps == PlaceStates.BLOCKED)
                        {
                            // Game over.. normally we would change a game state variable but for this tutorial we're just
                            // going to exit the app
                            gameState = GameStates.GameOver;
                            

                            
                        }
                    }
                    else
                    {
                        // We can move our piece into the new location, so update the existing piece location
                        SpawnedPieceLocation = NewSpawnedPieceLocation;
                    }

                    ElapsedTime = 0;
                }
            }

            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        /// 
        protected override void Draw(GameTime gameTime)
        {

            GraphicsDevice.Clear(Color.CornflowerBlue);

            if (gameState == GameStates.Controls)
            {
                KeyboardState ks = Keyboard.GetState();
                if (ks.IsKeyDown(Keys.G))
                {
                    gameState = GameStates.Playing;
                    MediaPlayer.Resume();
                }
                if (ks.IsKeyDown(Keys.Escape))
                {
                    gameState = GameStates.TitleScreen;
                    myStopWatch.Reset();
                }
                spriteBatch.Begin();
                spriteBatch.Draw(controlsMenu, mainFrame, Color.White);

                spriteBatch.DrawString(
                    pericles14,
                    "Up Arrow - Rotate Block",
                    new Vector2(240, 75),
                    Color.White);
                spriteBatch.DrawString(
                    pericles14,
                    "Down Arrow - Fast Drop",
                    new Vector2(240, 100),
                    Color.White);
                spriteBatch.DrawString(
                    pericles14,
                    "Left Arrow - Move Block Left",
                    new Vector2(240, 125),
                    Color.White);
                spriteBatch.DrawString(
                    pericles14,
                    "Right Arrow - Move Block Right",
                    new Vector2(240, 150),
                    Color.White);
                
                spriteBatch.DrawString(
                    pericles14,
                    "ESC - Return to Menu",
                    new Vector2(240, 175),
                    Color.White);
                spriteBatch.DrawString(
                    pericles14,
                    "G - Return to Game",
                    new Vector2(240, 275),
                    Color.White);
                spriteBatch.DrawString(
                    pericles14,
                    "S - Replay Song",
                    new Vector2(240, 200),
                    Color.White);
                spriteBatch.DrawString(
                    pericles14,
                    "Q - Quit Game",
                    new Vector2(240, 225),
                    Color.White);
                
                spriteBatch.DrawString(
                    pericles14,
                    "R - Restart Game",
                    new Vector2(240, 250),
                    Color.White);
                spriteBatch.End();

                
            }


            spriteBatch.Begin();

            if (gameState == GameStates.TitleScreen)
            {
                spriteBatch.Draw(mainbackground, mainFrame, Color.White);
                spriteBatch.Draw(titleScreen, titleScreenLocation, Color.White);
                
                spriteBatch.DrawString(
                    pericles14,
                    "P R E S S  S P A C E  T O  C O N T I N U E",
                    new Vector2(200, 225),
                    Color.White);

            }
            else if (gameState == GameStates.Playing)
            {
                myStopWatch.Start();
                // Draw the board first
                for (int y = 0; y < BoardHeight; y++)
                    for (int x = 0; x < BoardWidth; x++)
                    {
                        Color tintColor = TetronimoColors[Board[x, y]];

                        // Since for the board itself background colors are transparent, we'll go ahead and give this one
                        // a custom color.  This can be omitted if you draw a background image underneath your board
                        if (Board[x, y] == 0)
                            tintColor = Color.FromNonPremultiplied(50, 50, 50, 50);

                        spriteBatch.Draw(spriteSheet, new Rectangle((int)BoardLocation.X + x * BlockSize, (int)BoardLocation.Y + y * BlockSize, BlockSize, BlockSize), new Rectangle(0, 0, 32, 32), tintColor);
                        
                    }


                //Next draw the on deck piece
                int dim = NextSpawnedPiece.GetLength(0);

                for (int y = 0; y < dim; y++)
                    for (int x = 0; x < dim; x++)
                    {
                        if (NextSpawnedPiece[x, y] != 0)
                        {
                            Color tintColor = TetronimoColors[NextSpawnedPiece[x, y]];

                            spriteBatch.Draw(spriteSheet, new Rectangle((int)BoardLocation.X + ((int)NextSpawnedPieceLocation.X + x) * BlockSize, (int)BoardLocation.Y + ((int)NextSpawnedPieceLocation.Y + y) * BlockSize, BlockSize, BlockSize), new Rectangle(0, 0, 32, 32), tintColor);
                        }
                    }

                // Next draw the spawned piece
                dim = SpawnedPiece.GetLength(0);

                for (int y = 0; y < dim; y++)
                    for (int x = 0; x < dim; x++)
                    {
                        if (SpawnedPiece[x, y] != 0)
                        {
                            Color tintColor = TetronimoColors[SpawnedPiece[x, y]];

                            spriteBatch.Draw(spriteSheet, new Rectangle((int)BoardLocation.X + ((int)SpawnedPieceLocation.X + x) * BlockSize, (int)BoardLocation.Y + ((int)SpawnedPieceLocation.Y + y) * BlockSize, BlockSize, BlockSize), new Rectangle(0, 0, 32, 32), tintColor);
                        }
                    }
                spriteBatch.Draw(gamebackground, mainFrame, Color.White);
                    spriteBatch.DrawString(
                        pericles14,
                        "Score: " + score.PlayerScore.ToString(),
                        scoreLocation,
                        Color.White);


                spriteBatch.DrawString(
                    pericles14,
                    "Next Piece ",
                    new Vector2(540, 125),
                    Color.White);

                spriteBatch.DrawString(
                    pericles14,
                    "Time: " + myStopWatch.Elapsed.Hours + ":" + myStopWatch.Elapsed.Minutes + ":" + myStopWatch.Elapsed.Seconds.ToString(),
                    elapsedhoursLocation,
                    Color.White);
                spriteBatch.DrawString(
                    pericles14,
                    ("Lines: ") + score.PlayerLines.ToString(),
                    linesLocation,
                    Color.White);

                spriteBatch.DrawString(
                    pericles14,
                    ("Level: ") + score.PlayerLevel.ToString(),
                    levelLocation,
                    Color.White);
                spriteBatch.DrawString(
                    pericles14,
                    "Press C for Control List",
                    new Vector2(520, 250),
                    Color.White);
            }

            else if ((gameState == GameStates.GameOver))
            {

                myStopWatch.Stop();
                KeyboardState ks = Keyboard.GetState();

                if (ks.IsKeyDown(Keys.S))
                {
                    MediaPlayer.Stop();
                    MediaPlayer.Play(song);
                }

                spriteBatch.Draw(gamebackground, mainFrame, Color.White);
                if (ks.IsKeyDown(Keys.C))
                {
                    gameState = GameStates.Controls;
                }
                spriteBatch.DrawString(
                    pericles14,
                    "G A M E  O V E R !",
                    new Vector2(
                        this.Window.ClientBounds.Width / 2 -
                          pericles14.MeasureString("G A M E  O V E R !").X / 2,
                        50),
                    Color.White);
                if(score.PlayerLevel >= 0)
                spriteBatch.DrawString(
                pericles14,
                ("Level: ") + score.PlayerLevel.ToString(),
                levelLocation,
                Color.White);

                spriteBatch.DrawString(
                pericles14,
                ("Lines: ") + score.PlayerLines.ToString(),
                linesLocation,
                Color.White);

                spriteBatch.DrawString(
                pericles14,
                "Next Piece ",
                new Vector2(540, 125),
                Color.White);

                spriteBatch.DrawString(
                pericles14,
                "Time: " + myStopWatch.Elapsed.Hours + ":" + myStopWatch.Elapsed.Minutes + ":" + myStopWatch.Elapsed.Seconds.ToString(),
                elapsedhoursLocation,
                Color.White);

                spriteBatch.DrawString(
                pericles14,
                "Score: " + score.PlayerScore.ToString(),
                scoreLocation,
                Color.White);

                spriteBatch.DrawString(
                    pericles14,
                    "Press C for Control List",
                    new Vector2(520, 250),
                    Color.White);

                if (ks.IsKeyDown(Keys.Q))
                {
                    this.Exit();
                }

                if (ks.IsKeyDown(Keys.Escape))
                {
                    gameState = GameStates.TitleScreen;
                    MediaPlayer.Stop();
                    myStopWatch.Reset();
                }

                if (ks.IsKeyDown(Keys.R) && myStopWatch.Elapsed.Seconds >= 1)
                {
                    MediaPlayer.Play(song);
                    InitializeBoard(Board);
                    SpawnPiece();
                    SpawnPiece();
                    gameState = GameStates.Playing;

                    score.PlayerLevel = 0;
                    score.PlayerLines = 0;
                    score.PlayerScore = 0;

                    

                    myStopWatch.Restart();


                }
            }

            spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}

