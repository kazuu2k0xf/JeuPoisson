using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace FishGame;

public class Sprite
{
    
    public Vector2 Position { get; set; }
    public Texture2D Texture { get; init; }
    public int Size { get; set; }
    private Color _color = Color.White;

    
    private readonly int _columns;
    private readonly int _rows;
    private readonly int _frameWidth;
    private readonly int _frameHeight;
    private int _currentFrame;

    
    public Rectangle DestinationRect
    {
        get
        {
           
            int x = (int)(Position.X - Size / 2f);
            int y = (int)(Position.Y - Size / 2f);
            return new Rectangle(x, y, Size, Size);
        }
    }

    
    public Rectangle SourceRect
    {
        get
        {
            int row = _currentFrame / _columns;
            int col = _currentFrame % _columns;
            return new Rectangle(
                _frameWidth * col,
                _frameHeight * row,
                _frameWidth,
                _frameHeight
            );
        }
    }

    
    public Sprite(Texture2D texture, Vector2 position, int size, int columns, int rows)
    {
        Texture = texture;
        Position = position;
        Size = size;
        _columns = columns;
        _rows = rows;

        
        _frameWidth = Texture.Width / _columns;
        _frameHeight = Texture.Height / _rows;

        _currentFrame = 0; 
    }


    public void SetFrame(int frame)
    {
        if (frame >= 0 && frame < _columns * _rows)
        {
            _currentFrame = frame;
        }
    }

    
    public void Update(GameTime gameTime)
    {
       
    }

    
    public void Draw(SpriteBatch spriteBatch)
    {
        spriteBatch.Draw(
            Texture,
            DestinationRect, 
            SourceRect,      
            _color
        );
    }
}