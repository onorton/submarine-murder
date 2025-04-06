

using System;
using System.Diagnostics;
using System.Net.Mime;
using Microsoft.VisualBasic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace LudumDare57.UI;

public class TextureButton(Texture2D sprite, Point position, Action onClick) : UIElement, IDrawable
{

    private bool _hovering;
    private bool _pressed;

    public Rectangle Bounds { get; set; } = new Rectangle(position, (sprite.Bounds.Size.ToVector2() / Game1.SpriteScaling).ToPoint());

    public void Update()
    {
        if (!Enabled)
        {
            _hovering = false;
            _pressed = false;
            return;
        }

        _hovering = InBounds();
    }

    public void Draw(SpriteBatch spriteBatch)
    {
        if (!Enabled)
        {
            return;
        }

        spriteBatch.Begin();
        spriteBatch.Draw(sprite, Bounds, _hovering ? Color.White : Color.LightGray);
        spriteBatch.End();
    }

    public bool TryPress()
    {
        if (!Enabled)
        {
            _pressed = false;
            return _pressed;
        }

        if (!_hovering)
        {
            return false;
        }

        _pressed = MouseController.Instance.PressedThisFrame();

        if (_pressed)
        {
            onClick.Invoke();
        }

        return _pressed;

    }

    public override bool InBounds()
    {
        if (MouseController.Instance.BlockedCurrentFrame)
        {
            return false;
        }

        var inBounds = Bounds.Contains(MouseController.Instance.Position);

        if (BlocksRays && inBounds)
        {
            MouseController.Instance.BlockedCurrentFrame = true;
        }

        return inBounds;
    }
}