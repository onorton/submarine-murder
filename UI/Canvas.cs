



using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework.Graphics;

namespace LudumDare57.UI;

public class Canvas(List<UIElement> uiObjects)
{
    public bool Enabled { get; set; } = true;
    public void Draw(SpriteBatch spriteBatch)
    {
        if (!Enabled)
        {
            return;
        }

        foreach (var uiObject in uiObjects.OfType<IDrawable>())
        {
            uiObject.Draw(spriteBatch);
        }
    }

    public void Update()
    {
        if (!Enabled)
        {
            return;
        }

        foreach (var button in uiObjects.OfType<Button>())
        {
            button.Update();
        }
    }

    public bool TryPress()
    {
        if (!Enabled)
        {
            return false;
        }

        foreach (var button in uiObjects.OfType<Button>())
        {
            if (button.TryPress())
            {
                return true;
            }
        }

        return false;
    }
}