    using System;
    using System.Collections.Generic;
    using FishGame.Content; 
    using Microsoft.Xna.Framework;
    using Microsoft.Xna.Framework.Graphics;
    using Microsoft.Xna.Framework.Input;
    using System.IO;
    using System.Xml;

    namespace FishGame;

    public enum EtatJeu
    {
        Menu, // <--- AJOUTÉ
        Playing,
        GameOver,
        GameWon
    }
    public class myGame : Game
    {
        public static GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;
        private Texture2D _tilesetTexture;
        private Tile[,] _tileMap;
        
        private Tileset _tileset;
        private int[,] _mapLayout;

        private Pecheur _pecheur;
        private Poisson _poisson;
        
        private int _finX;          
        private int _finY;
        
        private const int GridColumns = 8;
        private const int GridRows = 8;

        private int tileWidth = 225;
        private int tileHeight = 130;
        
        private bool[,] arbreCollision;

      
        private int columns;
        private int rows;
        
        
        private int _mouvementsRestants;
        
        private EtatJeu _etatJeu;
        private SpriteFont _policeScore;
        
        // Inutile, le pêcheur a déjà cette info
        // private bool _joueurAPoisson; 
        
        private KeyboardState _previousKeyboardState;
        
        
        private Texture2D _background;
        private Texture2D _btnJouer;
        private Texture2D _btnQuitter;

        private Rectangle _btnJouerRect;
        private Rectangle _btnQuitterRect;
        private MouseState _previousMouseState;

        private Color _jouerColor = Color.White;
        private Color _quitterColor = Color.White;
        

        public myGame()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
            
            Window.AllowUserResizing = true;
         
        }

        protected override void Initialize()
        {
           
            _graphics.PreferredBackBufferWidth = 1280;
            _graphics.PreferredBackBufferHeight = 720;
            _graphics.ApplyChanges();
            
            _previousKeyboardState = Keyboard.GetState();
            
            base.Initialize();
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);
             Texture2D personnageTexture = Content.Load<Texture2D>("pecheur");
             _tilesetTexture = Content.Load<Texture2D>("wood");
             Texture2D poissonTexture = Content.Load<Texture2D>("poisson");
             _policeScore = Content.Load<SpriteFont>("PoliceScore");
             _tileset = new Tileset(_tilesetTexture, tileWidth, tileHeight);
             
             _background = Content.Load<Texture2D>("wood");
             _btnJouer = Content.Load<Texture2D>("Jouer");
             _btnQuitter = Content.Load<Texture2D>("Quitter");
             
             
             string xmlPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Niveau1.xml");

             XmlDocument doc = new XmlDocument();
             doc.Load(xmlPath);
             
             
             var joueurXML = doc.GetElementsByTagName("Joueur")[0];
             int joueurX = int.Parse(joueurXML.Attributes["x"].Value);
             int joueurY = int.Parse(joueurXML.Attributes["y"].Value);
             
             var poissonXML = doc.GetElementsByTagName("Poisson")[0];
             int poissonX = int.Parse(poissonXML.Attributes["x"].Value);
             int poissonY = int.Parse(poissonXML.Attributes["y"].Value);
             
             var finXML = doc.GetElementsByTagName("Fin")[0];
             _finX = int.Parse(finXML.Attributes["x"].Value);
             _finY = int.Parse(finXML.Attributes["y"].Value);
             
             var pasXML = doc.GetElementsByTagName("Pas")[0];
             _mouvementsRestants = int.Parse(pasXML.InnerText);
             
             
             Sprite playerSprite = new Sprite(personnageTexture, Vector2.Zero, 70, 7, 4);
             Sprite fishSprite = new Sprite(poissonTexture, Vector2.Zero, 70, 1, 1);
             
             _pecheur = new Pecheur(playerSprite, joueurX, joueurY, tileWidth, tileHeight);
             _poisson = new Poisson(fishSprite, poissonX, poissonY, tileWidth, tileHeight);
             

             // L'état initial est le Menu !
             _etatJeu = EtatJeu.Menu; // <--- MODIFIÉ
             
             InitializeTileMap(doc);
        }
        
        
        private void InitializeTileMap(XmlDocument doc)
        {
            columns = GridColumns;
            rows = GridRows;
            
            _tileMap = new Tile[columns, rows]; 
            arbreCollision = new bool[columns, rows]; 
            
            XmlNodeList lignes = doc.GetElementsByTagName("Ligne"); 
            
            int currentY = 0;
            foreach (XmlNode ligne in lignes)
            {
                if (currentY >= rows) break;
                
                string data = ligne.InnerText;
                int x = 0;
                foreach (char c in data)
                {
                    if (x >= columns) break;
                    
                    if (c == 'C')
                    {
                        arbreCollision[x, currentY] = true;
                    }
                    x++;
                }
                currentY++;
            }

            
            for (int x = 0; x < columns; x++)
            {
                for (int y = 0; y < rows; y++) 
                {
                    int tileIndex = (y * columns) + x;
                    Vector2 position = new Vector2(x * tileWidth, y * tileHeight);
                    _tileMap[x, y] = _tileset.GetTile(tileIndex, position);
                }
            }
        }


        // --- UPDATE LOGIC (RÉORGANISÉ) ---

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed ||
                Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            //Mets la fenetre en grand écran en appuyant sur f11
            if (Keyboard.GetState().IsKeyDown(Keys.F11))
            {
                _graphics.ToggleFullScreen();
            }

            MouseState currentMouseState = Mouse.GetState();
            KeyboardState currentKeyboardState = Keyboard.GetState();

            // Machine à états principale
            switch (_etatJeu)
            {
                case EtatJeu.Menu:
                    UpdateMenu(currentMouseState);
                    break;

                case EtatJeu.Playing:
                    UpdateJeu(gameTime, currentKeyboardState);
                    break;

                case EtatJeu.GameOver:
                    if (currentKeyboardState.IsKeyDown(Keys.Enter) && _previousKeyboardState.IsKeyUp(Keys.Enter))
                    {
                        LoadContent(); // Recharge le jeu (et retourne au menu)
                    }
                    break;

                case EtatJeu.GameWon:
                    if (currentKeyboardState.IsKeyDown(Keys.Enter) && _previousKeyboardState.IsKeyUp(Keys.Enter))
                    {
                        LoadContent(); // Recharge le jeu (et retourne au menu)
                    }
                    break;
            }


            // Mémorise les états précédents à la fin
            _previousKeyboardState = currentKeyboardState;
            _previousMouseState = currentMouseState;

            base.Update(gameTime);
        }

        /// <summary>
        /// Gère la logique quand on est dans le menu
        /// </summary>
        private void UpdateMenu(MouseState currentMouseState)
        {
            // --- Calcule des positions et tailles ---
            int screenWidth = GraphicsDevice.Viewport.Width;
            int screenHeight = GraphicsDevice.Viewport.Height;

            int jouerWidth = _btnJouer.Width;
            int jouerHeight = _btnJouer.Height;
            int quitterWidth = _btnQuitter.Width;
            int quitterHeight = _btnQuitter.Height;

            int posJouerX = (screenWidth / 2) - (jouerWidth / 2);
            int posQuitterX = (screenWidth / 2) - (quitterWidth / 2);

            int posYJouer = (screenHeight / 5) - (jouerHeight / 5);
            int posYQuitter = (screenHeight) - (quitterHeight) - 20;

            _btnJouerRect = new Rectangle(posJouerX, posYJouer, jouerWidth, jouerHeight);
            _btnQuitterRect = new Rectangle(posQuitterX, posYQuitter, quitterWidth, quitterHeight);

            // --- Logique Bouton Jouer ---
            _jouerColor = Color.White; // Réinitialise la couleur
            if (_btnJouerRect.Contains(currentMouseState.Position))
            {
                _jouerColor = Color.LightGray;
                if (currentMouseState.LeftButton == ButtonState.Pressed &&
                    _previousMouseState.LeftButton == ButtonState.Released)
                {
                    // La seule action est de changer d'état !
                    _etatJeu = EtatJeu.Playing; 
                }
            }

            // --- Logique Bouton Quitter ---
             _quitterColor = Color.White; // Réinitialise la couleur
            if (_btnQuitterRect.Contains(currentMouseState.Position))
            {
                _quitterColor = Color.LightGray;
                if (currentMouseState.LeftButton == ButtonState.Pressed &&
                    _previousMouseState.LeftButton == ButtonState.Released)
                {
                    Exit();
                }
            }
        }


        /// <summary>
        /// Gère la logique quand on est en train de jouer
        /// </summary>
        private void UpdateJeu(GameTime gameTime, KeyboardState currentKeyboardState)
        {
            int targetGridX = _pecheur.GridX;
            int targetGridY = _pecheur.GridY;
            

            if (currentKeyboardState.IsKeyDown(Keys.Up) && _previousKeyboardState.IsKeyUp(Keys.Up)) targetGridY--;
            if (currentKeyboardState.IsKeyDown(Keys.Down) && _previousKeyboardState.IsKeyUp(Keys.Down)) targetGridY++;
            if (currentKeyboardState.IsKeyDown(Keys.Left) && _previousKeyboardState.IsKeyUp(Keys.Left)) targetGridX--;
            if (currentKeyboardState.IsKeyDown(Keys.Right) && _previousKeyboardState.IsKeyUp(Keys.Right)) targetGridX++;
            

            if (targetGridX != _pecheur.GridX || targetGridY != _pecheur.GridY)
            {
                if (targetGridX >= 0 && targetGridX < GridColumns && targetGridY >= 0 && targetGridY < GridRows)
                {
                    if (arbreCollision[targetGridX, targetGridY] == false)
                    {
                        _pecheur.SetGridPosition(targetGridX, targetGridY);
                        _mouvementsRestants--;
                    }
                }
            }

            _pecheur.Update(gameTime); 
            _poisson.Update(gameTime); 


            
            // attraper
            if (_poisson.estVisible && _pecheur.GridX == _poisson.GridX && _pecheur.GridY == _poisson.GridY)
            {
                _pecheur.attraperPoisson(); 
                _poisson.poissonAttraper();
                System.Diagnostics.Debug.WriteLine("POISSON ATTRAPÉ !");
            }
                
            // gagner
            if (_pecheur.aPoisson && _pecheur.GridX == _finX && _pecheur.GridY == _finY)
            {
                _etatJeu = EtatJeu.GameWon;
                _pecheur.lacherPoisson(); // 
            }
                
            // perdu
            if (_mouvementsRestants <= 0 && _etatJeu == EtatJeu.Playing)
            {
                _etatJeu = EtatJeu.GameOver;
            }
        }


        // --- DRAW LOGIC (RÉORGANISÉ) ---

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.White);

            // Machine à états pour le dessin
            switch (_etatJeu)
            {
                case EtatJeu.Menu:
                    DrawMenu();
                    break;
                
                // Les 3 états suivants dessinent le jeu
                case EtatJeu.Playing:
                case EtatJeu.GameOver:
                case EtatJeu.GameWon:
                    DrawJeu(); // Dessine le monde du jeu (avec caméra)
                    DrawUI();  // Dessine l'interface (score, messages, sans caméra)
                    break;
            }
                
            base.Draw(gameTime);
        }

        /// <summary>
        /// Dessine l'écran de menu (sans caméra)
        /// </summary>
        private void DrawMenu()
        {
            // Démarre un batch SANS transformation de caméra
            _spriteBatch.Begin(samplerState: SamplerState.PointClamp);
                
            _spriteBatch.Draw(_background, GraphicsDevice.Viewport.Bounds, Color.White);        
            _spriteBatch.Draw(_btnJouer, _btnJouerRect, _jouerColor);
            _spriteBatch.Draw(_btnQuitter, _btnQuitterRect, _quitterColor);
                
            _spriteBatch.End(); 
        }

      /// <summary>
    /// Dessine le monde du jeu
    /// </summary>
    private void DrawJeu()
    {

        float totalMapWidth = (float)GridColumns * tileWidth;
        float totalMapHeight = (float)GridRows * tileHeight;

        float screenWidth = (float)GraphicsDevice.Viewport.Width;  // <--- MODIFIÉ
        float screenHeight = (float)GraphicsDevice.Viewport.Height; // <--- MODIFIÉ

       
        float scaleX = screenWidth / totalMapWidth;
        float scaleY = screenHeight / totalMapHeight;

       
        Matrix transformMatrix = Matrix.CreateScale(scaleX, scaleY, 1.0f);



        _spriteBatch.Begin(
            transformMatrix: transformMatrix, 
            samplerState: SamplerState.PointClamp, 
            blendState: BlendState.AlphaBlend
        );
    
        // Boucles pour dessiner la grille 8 PAR 8
        for (int x = 0; x < columns; x++)
        {
            for (int y = 0; y < rows; y++)
            {
                _tileMap[x, y].Draw(_spriteBatch);
            }
        }
    
        _pecheur.Draw(_spriteBatch);
        _poisson.Draw(_spriteBatch); 
    
        _spriteBatch.End(); 
    }

    /// <summary>
    /// Dessine l'interface utilisateur (score, messages de fin) (SANS caméra)
    /// </summary>
    private void DrawUI()
    {
        _spriteBatch.Begin(samplerState: SamplerState.PointClamp);
            
        string textePas = $"Pas restants : {_mouvementsRestants}";
        _spriteBatch.DrawString(_policeScore, textePas, new Vector2(10, 10), Color.White);

        if (_etatJeu == EtatJeu.GameOver)
        {
            string texteFin = "VOUS AVEZ PLUS DE FORCE";
            string texteRelancer = "Appuyez sur Entree pour recommencer";
                
            Vector2 posFin = new Vector2(
                GraphicsDevice.Viewport.Width / 2f - _policeScore.MeasureString(texteFin).X / 2f,  
                GraphicsDevice.Viewport.Height / 2f - 50                                          
            );
            Vector2 posRelancer = new Vector2(
                GraphicsDevice.Viewport.Width / 2f - _policeScore.MeasureString(texteRelancer).X / 2f, 
                GraphicsDevice.Viewport.Height / 2f                                                    
            );

            _spriteBatch.DrawString(_policeScore, texteFin, posFin, Color.Red);
            _spriteBatch.DrawString(_policeScore, texteRelancer, posRelancer, Color.White); 
        }
        else if (_etatJeu == EtatJeu.GameWon)
        {
            string texteFin = "GAGNE !";
            string texteRelancer = "Appuyez sur Entree pour recommencer";

            Vector2 posFin = new Vector2(
                GraphicsDevice.Viewport.Width / 2f - _policeScore.MeasureString(texteFin).X / 2f,  
                GraphicsDevice.Viewport.Height / 2f - 50                                          
            );
            Vector2 posRelancer = new Vector2(
                GraphicsDevice.Viewport.Width / 2f - _policeScore.MeasureString(texteRelancer).X / 2f, 
                GraphicsDevice.Viewport.Height / 2f                                                   
            );

            _spriteBatch.DrawString(_policeScore, texteFin, posFin, Color.LawnGreen);
            _spriteBatch.DrawString(_policeScore, texteRelancer, posRelancer, Color.White);
        }
            
        _spriteBatch.End(); 
    }
    }