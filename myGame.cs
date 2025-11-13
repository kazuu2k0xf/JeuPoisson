using System;
using System.Collections.Generic;
using FishGame.Content; 
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

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
    private Texture2D laCarte;
    private Tile[,] _tileMap;
    
    private Tileset _tileset;


    
  
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
    
    private GameState EtatCourant;
    
    private bool _joueurAPoisson; 
    private int _finX;          
    private int _finY;          
    
    //Camera _camera;
    
    
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
       

        _graphics.ApplyChanges();
        
        //_camera = new Camera();
        
        _previousKeyboardState = Keyboard.GetState();
        
        base.Initialize();
    }

    protected override void LoadContent()
    {
        _spriteBatch = new SpriteBatch(GraphicsDevice);
         Texture2D personnage = Content.Load<Texture2D>("pecheur");
         laCarte = Content.Load<Texture2D>("wood");
         Texture2D poisson = Content.Load<Texture2D>("poisson");
         
         
         
         _tileset = new Tileset(laCarte, tileWidth, tileHeight);
         
         _joueurX = 5; 
         _joueurY = 1; 
         
         _mouvementsRestants = 30;
         

         Vector2 posDepartJoueur = new Vector2(_joueurX * tileWidth, _joueurY * tileHeight);
         
         // cree le personnage à cette position
         _personnage = new Sprite(personnage, posDepartJoueur, 70,7,4);
         
         
         _poissonX = 7; 
         _poissonY = 2; 
         Vector2 posPoisson = new Vector2(_poissonX * tileWidth, _poissonY * tileHeight);
         

         
         _poisson = new Sprite(poisson, posPoisson, 70, 1, 1);
         _poissonVisible = true;
         _joueurAPoisson = false;
         
         _finX = 7;
         _finY = 7;

         _personnage.SetFrame(0);
         
         EtatCourant = GameState.Playing;
         
         InitializeTileMap();
    } 
    private void InitializeTileMap()
    {
        columns = GridColumns;
        rows = GridRows;
        
        _tileMap = new Tile[columns, rows]; 
        arbreCollision = new bool[columns, rows];
        
        
        arbreCollision[0, 0] = true;
        arbreCollision[1, 0] = true;
        arbreCollision[2, 0] = true;
        arbreCollision[3, 0] = true;
        arbreCollision[0, 1] = true;
        arbreCollision[0, 2] = true;
        arbreCollision[0, 6] = true;
        arbreCollision[1, 7] = true;
        arbreCollision[0, 7] = true;
        arbreCollision[6, 0] = true;
        arbreCollision[7, 0] = true;
        arbreCollision[1, 1] = true;
        arbreCollision[2, 1] = true;
        

        for (int x = 0; x < columns; x++)
        {
            for (int y = 0; y < rows; y++) 
            {
                int tileIndex = (y * GridRows) + x;
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

        
        switch (EtatCourant)
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
        
        int posJoueurX = _joueurX;
        int posJoueurY = _joueurY;
        //_camera.follow(_personnage);

        if (currentKeyboardState.IsKeyDown(Keys.Up) && _previousKeyboardState.IsKeyUp(Keys.Up)) posJoueurY--;
        if (currentKeyboardState.IsKeyDown(Keys.Down) && _previousKeyboardState.IsKeyUp(Keys.Down)) posJoueurY++;
        if (currentKeyboardState.IsKeyDown(Keys.Left) && _previousKeyboardState.IsKeyUp(Keys.Left)) posJoueurX--;
        if (currentKeyboardState.IsKeyDown(Keys.Right) && _previousKeyboardState.IsKeyUp(Keys.Right)) posJoueurX++;
        
        // SI Deplacement
        if (posJoueurX != _joueurX || posJoueurY != _joueurY)
        {
            if (posJoueurX >= 0 && posJoueurX < GridColumns && posJoueurY >= 0 && posJoueurY < GridRows)
            {
                if (arbreCollision[posJoueurX, posJoueurY] == false)
                {
                    _joueurX = posJoueurX;
                    _joueurY = posJoueurY;
                    //_camera.follow(_personnage);
                    _mouvementsRestants--;
                }
            }
        }
        
        //Centrer le joueur sur sa tuile actuel
        float centrerX = (_joueurX * tileWidth) + (tileWidth / 2f);
        float centrerY = (_joueurY * tileHeight) + (tileHeight / 2f);
        _personnage.Position = new Vector2(centrerX, centrerY);
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
                EtatCourant = GameState.GameWon;
                _joueurAPoisson = false;
            }
        }
            
       //PERDU
        if (_mouvementsRestants <= 0 && EtatCourant == GameState.Playing)
        {
            EtatCourant = GameState.GameOver; 
        }
        
    }

protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(Color.CornflowerBlue);
        
        
        
        _spriteBatch.Begin( samplerState: SamplerState.PointClamp);
        
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
        base.Draw(gameTime);
    }
}