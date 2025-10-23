using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace FishGame.Content;

public class Tile
{
    public Texture2D Texture { get; }
    public Rectangle SourceRectangle { get; }
    public Vector2 Position { get; }
    public int Size { get; }

    public Tile(Texture2D texture, Rectangle sourceRect, Vector2 position, int size)
    {
        Texture = texture;
        SourceRectangle = sourceRect;
        Position = position;
        Size = size;
    }

    public void Draw(SpriteBatch spriteBatch)
    {
        spriteBatch.Draw(Texture, Position, SourceRectangle, Color.White);
    }
}


