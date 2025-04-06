

using System;
using System.Diagnostics;
using System.Net.Mime;
using Microsoft.VisualBasic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace LudumDare57.UI;

public class Button(GraphicsDevice graphicsDevice, Rectangle bounds, Color colour, Color hoverColour, Action onClick, string text, SpriteFont font) : UIElement, IDrawable
{

    private bool _hovering;
    private bool _pressed;

    public Rectangle Bounds { get; set; } = bounds;

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

        var rect = new Texture2D(graphicsDevice, 1, 1);
        rect.SetData([Color.White]);
        spriteBatch.Begin();
        spriteBatch.Draw(rect, Bounds, _hovering ? hoverColour : colour);
        spriteBatch.DrawString(font, text, Bounds.Center.ToVector2() - font.MeasureString(text) / 2, Color.Black);
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