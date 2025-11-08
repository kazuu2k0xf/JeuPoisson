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
    
    private bool _joueurAPoisson; 
    
    Camera _camera;
    
    private KeyboardState _previousKeyboardState;
    
    

    public myGame()
    {
        _graphics = new GraphicsDeviceManager(this);
        Content.RootDirectory = "Content";
        IsMouseVisible = true;
        
        Window.AllowUserResizing = true;
     
    }

    protected override void Initialize()
    {
       
        //_graphics.PreferredBackBufferWidth = tileWidth * 2; 
        //_graphics.PreferredBackBufferHeight = tileHeight * 2; 
        _graphics.ApplyChanges();
        
        _camera = new Camera();
        
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
         
         
         string xmlPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Niveau1.xml");

         XmlDocument doc = new XmlDocument();
         doc.Load(xmlPath);
         
         
         var joueurNode = doc.GetElementsByTagName("Joueur")[0];
         int joueurX = int.Parse(joueurNode.Attributes["x"].Value);
         int joueurY = int.Parse(joueurNode.Attributes["y"].Value);
         
         var poissonNode = doc.GetElementsByTagName("Poisson")[0];
         int poissonX = int.Parse(poissonNode.Attributes["x"].Value);
         int poissonY = int.Parse(poissonNode.Attributes["y"].Value);
         
         var finNode = doc.GetElementsByTagName("Fin")[0];
         _finX = int.Parse(finNode.Attributes["x"].Value);
         _finY = int.Parse(finNode.Attributes["y"].Value);
         
         var pasNode = doc.GetElementsByTagName("Pas")[0];
         _mouvementsRestants = int.Parse(pasNode.InnerText);
         
         
         Sprite playerSprite = new Sprite(personnageTexture, Vector2.Zero, 70, 7, 4);
         Sprite fishSprite = new Sprite(poissonTexture, Vector2.Zero, 70, 1, 1);
         
         _pecheur = new Pecheur(playerSprite, joueurX, joueurY, tileWidth, tileHeight);
         _poisson = new Poisson(fishSprite, poissonX, poissonY, tileWidth, tileHeight);
         
         _camera.follow(_pecheur.Sprite);

         _etatJeu = EtatJeu.Playing;
         
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


       
protected override void Update(GameTime gameTime) 
    {
        if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed ||
            Keyboard.GetState().IsKeyDown(Keys.Escape))
            Exit();

        KeyboardState currentKeyboardState = Keyboard.GetState();

        
        switch (_etatJeu)
        {
            
            case EtatJeu.Playing:
                UpdateJeu(gameTime, currentKeyboardState);
                break;
            
           
            case EtatJeu.GameOver:
                if (currentKeyboardState.IsKeyDown(Keys.Enter) && _previousKeyboardState.IsKeyUp(Keys.Enter))
                {
                    LoadContent();
                }
                break;
            
            case EtatJeu.GameWon:
                
                if (currentKeyboardState.IsKeyDown(Keys.Enter) && _previousKeyboardState.IsKeyUp(Keys.Enter))
                {
                    LoadContent();
                }
                break;
        }
        
        
        _previousKeyboardState = currentKeyboardState;
        base.Update(gameTime);
    }


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
                    _camera.follow(_pecheur.Sprite); 
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

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);
            
            
            _spriteBatch.Begin(
                transformMatrix: _camera.Transform, 
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
            
            

            _spriteBatch.Begin(samplerState: SamplerState.PointClamp);
            
            string textePas = $"Pas restants : {_mouvementsRestants}";
            _spriteBatch.DrawString(_policeScore, textePas, new Vector2(10, 10), Color.White);

            if (_etatJeu == EtatJeu.GameOver)
            {
                string texteFin = "VOUS AVEZ PLUS DE FORCE";
                string texteRelancer = "Appuyez sur Entree pour recommencer";
                
                Vector2 posFin = new Vector2(
                    _graphics.PreferredBackBufferWidth / 2f - _policeScore.MeasureString(texteFin).X / 2f,
                    _graphics.PreferredBackBufferHeight / 2f - 50
                );
                Vector2 posRelancer = new Vector2(
                    _graphics.PreferredBackBufferWidth / 2f - _policeScore.MeasureString(texteRelancer).X / 2f,
                    _graphics.PreferredBackBufferHeight / 2f
                );

                _spriteBatch.DrawString(_policeScore, texteFin, posFin, Color.Red);
                _spriteBatch.DrawString(_policeScore, texteRelancer, posRelancer, Color.White); 
            }
            else if (_etatJeu == EtatJeu.GameWon)
            {
                string texteFin = "GAGNE !";
                string texteRelancer = "Appuyez sur Entree pour recommencer";

                Vector2 posFin = new Vector2(
                    _graphics.PreferredBackBufferWidth / 2f - _policeScore.MeasureString(texteFin).X / 2f,
                    _graphics.PreferredBackBufferHeight / 2f - 50
                );
                Vector2 posRelancer = new Vector2(
                    _graphics.PreferredBackBufferWidth / 2f - _policeScore.MeasureString(texteRelancer).X / 2f,
                    _graphics.PreferredBackBufferHeight / 2f
                );

                _spriteBatch.DrawString(_policeScore, texteFin, posFin, Color.LawnGreen);
                _spriteBatch.DrawString(_policeScore, texteRelancer, posRelancer, Color.White);
            }
            
            _spriteBatch.End(); 
            
            base.Draw(gameTime);
        }
}