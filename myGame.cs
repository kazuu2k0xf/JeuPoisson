using System;
using System.Collections.Generic;
using FishGame.Content; 
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.IO;
using System.Xml.Linq;

namespace FishGame;

public enum GameState
{
    Playing,
    GameOver,
    GameWon
}
public class myGame : Game
{
    public static GraphicsDeviceManager _graphics;
    private SpriteBatch _spriteBatch;
    private Sprite _personnage;
    private Sprite _poisson;
    private Texture2D _tilesetTexture;
    private Tile[,] _tileMap;
    
    private Tileset _tileset;
    private int[,] _mapLayout;


    
  
    private const int GridColumns = 8;
    private const int GridRows = 8;

    private int tileWidth = 225;
    private int tileHeight = 130;
    
    private bool[,] arbreCollision;

  
    private int columns;
    private int rows;

    private int _joueurX;
    private int _joueurY;

    private int _poissonX;
    private int _poissonY;
    private bool _poissonVisible;
    
    private int _mouvementsRestants;
    
    private GameState _currentState;
    private SpriteFont _policeScore;
    
    private bool _joueurAPoisson; 
    private int _finX;          
    private int _finY;          
    
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
         Texture2D personnage = Content.Load<Texture2D>("pecheur");
         _tilesetTexture = Content.Load<Texture2D>("wood");
         Texture2D poisson = Content.Load<Texture2D>("poisson");
         
         _policeScore = Content.Load<SpriteFont>("PoliceScore");
         
         _tileset = new Tileset(_tilesetTexture, tileWidth, tileHeight);
         
         
         // lecture du fichier xml
         string xmlPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Niveau1.xml");
         XDocument doc = XDocument.Load(xmlPath);
         
         var general = doc.Root.Element("General");
         // recupere les information des attribut  du fichier xml et les assigne a des variables 
         _joueurX = (int)general.Element("Joueur").Attribute("x");
         _joueurY = (int)general.Element("Joueur").Attribute("y");
     
         _poissonX = (int)general.Element("Poisson").Attribute("x");
         _poissonY = (int)general.Element("Poisson").Attribute("y");
     
         _finX = (int)general.Element("Fin").Attribute("x");
         _finY = (int)general.Element("Fin").Attribute("y");
         
         _mouvementsRestants = (int)general.Element("Pas");
         
         Vector2 startPosition = new Vector2(_joueurX * tileWidth, _joueurY * tileHeight);
         _personnage = new Sprite(personnage, startPosition, 70,7,4);
         
         
 
         Vector2 startPositionPoisson = new Vector2(_poissonX * tileWidth, _poissonY * tileHeight);
         _poisson = new Sprite(poisson, startPositionPoisson, 70, 1, 1);
         
         _poissonVisible = true;
         _joueurAPoisson = false;
         

         _personnage.SetFrame(0);
         _currentState = GameState.Playing;
         
         InitializeTileMap(doc);
    } 
    private void InitializeTileMap(XDocument doc)
    {
        columns = GridColumns; 
        rows = GridRows;       
    
        _tileMap = new Tile[columns, rows]; 
        arbreCollision = new bool[columns, rows];
        
        var lignes = doc.Root.Element("Carte").Element("Collision").Elements("Ligne");
    
        int yCourant = 0;
        foreach (var ligne in lignes)
        {

            if (yCourant >= rows) break; 
        
            string data = ligne.Value;
            int x = 0;
            foreach (char c in data)
            {

                if (x >= columns) break; 
            
                if (c == 'C')
                {
                    arbreCollision[x, yCourant] = true;
                }
                x++;
            }
            yCourant++;
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

        
        switch (_currentState)
        {
            
            case GameState.Playing:
                UpdateJeu(gameTime, currentKeyboardState);
                break;
            
           
            case GameState.GameOver:
                if (currentKeyboardState.IsKeyDown(Keys.Enter) && _previousKeyboardState.IsKeyUp(Keys.Enter))
                {
                    LoadContent();
                }
                break;
            
            case GameState.GameWon:
                
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
        
        int targetGridX = _joueurX;
        int targetGridY = _joueurY;

        if (currentKeyboardState.IsKeyDown(Keys.Up) && _previousKeyboardState.IsKeyUp(Keys.Up)) targetGridY--;
        if (currentKeyboardState.IsKeyDown(Keys.Down) && _previousKeyboardState.IsKeyUp(Keys.Down)) targetGridY++;
        if (currentKeyboardState.IsKeyDown(Keys.Left) && _previousKeyboardState.IsKeyUp(Keys.Left)) targetGridX--;
        if (currentKeyboardState.IsKeyDown(Keys.Right) && _previousKeyboardState.IsKeyUp(Keys.Right)) targetGridX++;
        
        // SI Deplacement
        if (targetGridX != _joueurX || targetGridY != _joueurY)
        {
            if (targetGridX >= 0 && targetGridX < GridColumns && targetGridY >= 0 && targetGridY < GridRows)
            {
                if (arbreCollision[targetGridX, targetGridY] == false)
                {
                    _joueurX = targetGridX;
                    _joueurY = targetGridY;
                    _camera.follow(_personnage);
                    _mouvementsRestants--;
                }
            }
        }

        float centrerX = (_joueurX * tileWidth) + (tileWidth / 2f);
        float centrerY = (_joueurY * tileHeight) + (tileHeight / 2f);
        _personnage.Position = new Vector2(centrerX, centrerY);
        _camera.follow(_personnage);
        _personnage.Update(gameTime); 

       //victoire ou défaite

        if (_poissonVisible)
        {
            if (_joueurX == _poissonX && _joueurY == _poissonY)
            {
                _poissonVisible = false;
                _joueurAPoisson = true;
                _personnage.SetFrame(3);
            }
        } 
            

        if (_joueurAPoisson)
        {
            if (_joueurX == _finX && _joueurY == _finY)
            {
                _currentState = GameState.GameWon;
                _joueurAPoisson = false;
            }
        }
            
       //PERDU
        if (_mouvementsRestants <= 0 && _currentState == GameState.Playing)
        {
            _currentState = GameState.GameOver; 
        }
        
    }

protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(Color.CornflowerBlue);
        
        
        //_spriteBatch.Begin(samplerState: SamplerState.PointClamp, blendState: BlendState.AlphaBlend);
        _spriteBatch.Begin(transformMatrix: _camera.Transform, samplerState: SamplerState.PointClamp);
        
        // Boucles pour dessiner la grille  8 PAR 8
        for (int x = 0; x < columns; x++)
        {
            for (int y = 0; y < rows; y++)
                
            {
                _tileMap[x, y].Draw(_spriteBatch);
            }
        }
            
        _personnage.Draw(_spriteBatch);
        
        if (_poissonVisible)
        {
            _poisson.Draw(_spriteBatch);
        }
        
        _spriteBatch.End();
        
        _spriteBatch.Begin(samplerState: SamplerState.PointClamp);
        
        
        string textePas = $"Pas restants : {_mouvementsRestants}";
        _spriteBatch.DrawString(_policeScore, textePas, new Vector2(10, 10), Color.White);

       
        if (_currentState == GameState.GameOver)
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
        else if (_currentState == GameState.GameWon)
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