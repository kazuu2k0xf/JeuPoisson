using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace FishGame;

public class Personnage
{
    // Propriétés pour la position sur la GRILLE
    public int GridX { get; protected set; }
    public int GridY { get; protected set; }
    
    public Sprite Sprite { get; protected set; }
    
    protected int _tileWidth;
    protected int _tileHeight;
    
    
    
    // Constucteur qui instancie le sprite la position X et Y du perso et la taille des tuile.
    public Personnage(Sprite sprite, int gridX, int gridY, int tileWidth, int tileHeight)
    {
        Sprite = sprite;
        GridX = gridX;
        GridY = gridY;
        _tileWidth = tileWidth;
        _tileHeight = tileHeight;
        

        UpdatePixelPosition();
    }

    // Déplacement
    public void SetGridPosition(int x, int y)
    {
        GridX = x;
        GridY = y;
        UpdatePixelPosition();
    }

    // convertion postion grille en pixel pour centrer le personnage
    protected void UpdatePixelPosition()
    {
        if (Sprite != null)
        {
            float centerX = (GridX * _tileWidth) + (_tileWidth / 2f);
            float centerY = (GridY * _tileHeight) + (_tileHeight / 2f);
            
            
            Sprite.Position = new Vector2(centerX, centerY); // Permet de centrer le perso sur la tuile
        }
    }


    public virtual void Update(GameTime gameTime)
    {
        if (Sprite != null)
            Sprite.Update(gameTime);
    }

    public virtual void Draw(SpriteBatch spriteBatch)
    {
        if (Sprite != null)
            Sprite.Draw(spriteBatch);
    }
}