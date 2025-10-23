using FishGame.Content;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace FishGame;

public class Tileset
{
    private Texture2D _texture;
    private int _tileWidth;
    private int _tileHeight;
    private int _columns;

    public Tileset(Texture2D texture, int tileWidth, int tileheight)
    {
        _texture = texture;
        _tileWidth = tileWidth;
        _tileHeight = tileheight;
        _columns = _texture.Width / _tileWidth;
    }

    public Tile GetTile(int index, Vector2 position)
    {
        int col = (index % _columns);
        int row = (index / _columns);
        Rectangle sourceRect = new Rectangle(col * _tileWidth, row * _tileHeight, _tileWidth, _tileHeight);
        return new Tile(_texture, sourceRect, position, _tileWidth);
    }
}