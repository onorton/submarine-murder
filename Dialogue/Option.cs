using System.Diagnostics;
using LudumDare57.UI;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace LudumDare57.Dialogue;

public class DialogueOption(GraphicsDevice graphicsDevice, string optionText, string dialogueText, SpriteFont font, Clue clue = null)
{
    public string DialogueText { get; } = dialogueText;

    public Clue Clue { get; } = clue;

    private Button _button = new Button(graphicsDevice, new Rectangle(10, 10, 320, 50), Color.Gray, Color.White, () => { }, optionText, font);


    public bool TryPress()
    {
        _button.Update();
        return _button.TryPress();
    }


    public void Draw(SpriteBatch spriteBatch, int x, int y)
    {
        _button.Bounds = new Rectangle(new Point(x - _button.Bounds.Size.X / 2, y), _button.Bounds.Size);


        _button.Draw(spriteBatch);
    }


}