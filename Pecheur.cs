using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace FishGame;

public class Pecheur : Personnage
{
    public bool aPoisson { get; private set; }
    


    public Pecheur(Sprite sprite, int gridX, int gridY, int tileWidth, int tileHeight)
        : base(sprite, gridX, gridY, tileWidth, tileHeight)
    {
        aPoisson = false;
        Sprite.SetFrame(0);
    }

    public void attraperPoisson()
    {
        aPoisson = true;
        Sprite.SetFrame(3);
    }

    public void lacherPoisson()
    {
        aPoisson = false;
        Sprite.SetFrame(0);
    }


    public override void Update(GameTime gameTime)
    {

    }
}