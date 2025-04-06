using System.Diagnostics;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;


namespace LudumDare57.UI;

public class TextBox(Vector2 position, SpriteFont font, int maxWidth = 380) : UIElement, IDrawable
{
    public string Text { private get; set; } = "";


    public void Draw(SpriteBatch spriteBatch)
    {
        if (!Enabled)
        {
            return;
        }

        spriteBatch.Begin();

        var wrappedText = WrapText(font, Text, maxWidth);

        var linePosition = position - new Vector2(font.MeasureString(wrappedText).X / 2, 0);
        var lineNum = 0;
        foreach (var line in wrappedText.Split("\n").Reverse())
        {
            spriteBatch.DrawString(font, line, linePosition - new Vector2(0.0f, lineNum * font.MeasureString(line).Y), Color.Black);
            lineNum += 1;
        }

        spriteBatch.End();
    }

    public override bool InBounds()
    {
        return false;
    }

    private string WrapText(SpriteFont spriteFont, string text, float maxLineWidth)
    {
        string[] words = text.Split(' ');
        StringBuilder sb = new StringBuilder();
        float lineWidth = 0f;
        float spaceWidth = spriteFont.MeasureString(" ").X;


        foreach (string word in words)
        {
            Vector2 size = spriteFont.MeasureString(word);

            if (lineWidth + size.X < maxLineWidth)
            {
                sb.Append(word + " ");
                lineWidth += size.X + spaceWidth;
            }
            else
            {
                sb.Append("\n" + word + " ");
                lineWidth = size.X + spaceWidth;
            }
        }

        return sb.ToString();
    }
}