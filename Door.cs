
using System.Diagnostics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace LudumDare57;

public class Door(Texture2D sprite, Room room, Point position)
{
    private bool _hovering;
    private bool _pressed;

    public Room Room { get; } = room;

    private Rectangle _bounds = new Rectangle(position, (sprite.Bounds.Size.ToVector2() / Game1.SpriteScaling).ToPoint());

    public void Update()
    {

        var mousePosition = MouseController.Instance.Position;

        _hovering = !MouseController.Instance.BlockedCurrentFrame && _bounds.Contains(new Point(mousePosition.X, mousePosition.Y));
    }

    public bool TryPress()
    {
        if (!_hovering)
        {
            return false;
        }


        _pressed = MouseController.Instance.PressedThisFrame();
        return _pressed;
    }

    public void Draw(SpriteBatch spriteBatch)
    {
        spriteBatch.Begin();
        spriteBatch.Draw(sprite, _bounds, (_hovering && !_pressed) ? Color.White : new Color(180, 180, 180));
        spriteBatch.End();
    }

}