using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace LudumDare57.UI;

public class Panel(GraphicsDevice graphicsDevice, Rectangle bounds, Color colour) : UIElement, IDrawable
{
    public void Draw(SpriteBatch spriteBatch)
    {
        if (!Enabled)
        {
            return;
        }

        var rectTexture = new Texture2D(graphicsDevice, 1, 1);
        rectTexture.SetData([colour]);
        spriteBatch.Begin();
        spriteBatch.Draw(rectTexture, bounds, Color.White);
        spriteBatch.End();

    }

    public override bool InBounds()
    {
        if (MouseController.Instance.BlockedCurrentFrame)
        {
            return false;
        }

        var inBounds = bounds.Contains(MouseController.Instance.Position);

        if (BlocksRays && inBounds)
        {
            MouseController.Instance.BlockedCurrentFrame = true;
        }

        return inBounds;
    }
}