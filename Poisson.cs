using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace FishGame;

public class Poisson : Personnage
{
    public bool estVisible { get; private set; }

    public Poisson(Sprite sprite, int gridX, int gridY, int tileWidth, int tileHeight)
        : base(sprite, gridX, gridY, tileWidth, tileHeight)
    {
        estVisible = true;
    }

    public void poissonAttraper()
    {
        estVisible = false;
    }
    
    public void Reset()
    {
        estVisible = true;
    }


    public override void Draw(SpriteBatch spriteBatch)
    {
        if (estVisible)
        {
            base.Draw(spriteBatch);
        }
    }
    
}