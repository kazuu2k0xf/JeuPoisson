using Microsoft.Xna.Framework;
namespace FishGame;

public class Camera
{
    public Matrix Transform { get; private set; }

    public void follow(Sprite player)
    {
        var position = Matrix.CreateTranslation(-player.Position.X- (player.Size/2f), -player.Position.Y- (player.Size/2f), 0);

        var offset = Matrix.CreateTranslation(myGame._graphics.GraphicsDevice.Viewport.Width/2f, myGame._graphics.GraphicsDevice.Viewport.Height/2f, 0);
        
        Transform = position * offset;
    }
}