using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using LudumDare57.UI;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace LudumDare57.Dialogue;

public class DialogueRunner(SpriteFont font, Vector2 position, GraphicsDevice graphicsDevice, Dictionary<string, List<DialogueOption>> dialogueOptions)
{
    public bool Enabled { get; set; } = false;

    public Character Character { get; set; }

    private bool _accusing = false;

    public bool CharacterAccused { get; set; } = false;

    private List<DialogueOption> _currentOptions;


    private List<Accusation> _availableAccusations;

    private List<DialogueOption> _accusationButtons = new List<DialogueOption>();

    private int _padding = 80;

    private Rectangle _panel;
    private TextBox _textBox;

    private Button _closeButton;
    private DialogueOption _accuseButton;
    private Button _backButton;

    private Panel _blockingPanel;
    private TextBox _nameTextBox;
    private Dictionary<string, List<DialogueOption>> _dialogueOptions = dialogueOptions;

    public void Start()
    {
        Enabled = false;
        _accusing = false;

        var closeButtonPadding = 5;
        _accuseButton = new DialogueOption(graphicsDevice, "Accuse", "You killed Dr. Jones because...", font);
        _backButton = new Button(graphicsDevice, new Rectangle(10, 10, 300, 50), Color.Gray, Color.White, () => { }, "Never mind...", font);
        _backButton.Enabled = false;
        _accusationButtons = new List<DialogueOption>();
        _availableAccusations = new List<Accusation>();
        _panel = new Rectangle(graphicsDevice.Viewport.Bounds.Center.X - 400 / 2, graphicsDevice.Viewport.Bounds.Center.Y - 450 / 2, 400, 450);
        _textBox = new TextBox(new Vector2(position.X, _panel.Bottom - font.MeasureString("A").Y * 1.5f), font);
        _closeButton = new Button(graphicsDevice, new Rectangle(new Point(_panel.Right - 30 - closeButtonPadding, _panel.Top + closeButtonPadding), new Point(30, 30)), Color.Red, Color.LightPink, Close, "X", font);
        _blockingPanel = new Panel(graphicsDevice, graphicsDevice.Viewport.Bounds, new Color(1, 1, 1, 0)); // Empty panel to prevent clicking outside of panel
        _nameTextBox = new TextBox(new Vector2(position.X, _panel.Top + 20), font);
        _currentOptions = new List<DialogueOption>();

    }

    public DialogueOption Update()
    {
        if (Character != null)
        {
            _nameTextBox.Text = Character.Name;
        }

        if (!Enabled)
        {
            return null;
        }

        if (_accusing)
        {
            foreach (var accusation in _accusationButtons)
            {

                var pressSucceeds = accusation.TryPress();
                if (pressSucceeds)
                {
                    _textBox.Text = $"{Character.Name}: {accusation.DialogueText}";
                    CharacterAccused = true;
                }
            }

            _backButton.Update();
            if (_backButton.TryPress())
            {
                _accusing = false;
                _textBox.Text = "";

            }

        }
        else
        {
            foreach (var option in _currentOptions)
            {
                var pressSucceeds = option.TryPress();
                if (pressSucceeds)
                {
                    _textBox.Text = $"{Character.Name}: {option.DialogueText}";
                    return option;
                }
            }

            if (_accuseButton.TryPress())
            {
                _textBox.Text = _accuseButton.DialogueText;
                _accusing = true;
                _backButton.Enabled = _accusing;
            }
        }

        _closeButton.Update();
        _closeButton.TryPress();

        _blockingPanel.InBounds();

        return null;
    }


    public void Draw(SpriteBatch spriteBatch)
    {
        if (!Enabled)
        {
            return;
        }

        // Draw panel
        var rect = new Texture2D(graphicsDevice, 1, 1);
        rect.SetData([Color.DarkGray]);
        spriteBatch.Begin();
        spriteBatch.Draw(rect, _panel, Color.White);
        spriteBatch.End();

        // Draw name
        _nameTextBox.Draw(spriteBatch);

        // Draw Options / Accusations

        var yPosition = _panel.Top + 70;


        if (_accusing)
        {
            foreach (var accusation in _accusationButtons)
            {

                accusation.Draw(spriteBatch, (int)position.X, yPosition);
                yPosition += _padding;
                //TODO: Add back button
            }

            _backButton.Bounds = new Rectangle(new Point((int)position.X - _backButton.Bounds.Size.X / 2, yPosition), _backButton.Bounds.Size);
            _backButton.Draw(spriteBatch);
        }
        else
        {
            foreach (var option in _currentOptions)
            {
                option.Draw(spriteBatch, (int)position.X, yPosition);
                yPosition += _padding;
            }

            _accuseButton.Draw(spriteBatch, (int)position.X, yPosition);

        }

        _closeButton.Draw(spriteBatch);

        _textBox.Draw(spriteBatch);
    }

    public void Close()
    {
        Debug.WriteLine("Closing");
        Enabled = false;
        _accusing = false;
        _textBox.Text = "";
        _nameTextBox.Text = "";
        Character = null;
    }

    public void UpdateAccusations(List<Accusation> accusations)
    {
        if (!accusations.ToHashSet().SetEquals(_availableAccusations.ToHashSet()))
        {
            Debug.WriteLine($"{accusations.Count}, {_availableAccusations.Count}");
            Debug.WriteLine("Updating accusations");
            _accusationButtons = accusations.Select(a => new DialogueOption(graphicsDevice, a.Text, a.DialogueText, font)).ToList();

        }

        _availableAccusations = accusations;
    }

    public void SetCharacter(Character character)
    {
        Character = character;
        if (_dialogueOptions.ContainsKey(character.Name))
        {
            _currentOptions = _dialogueOptions[character.Name];
        }
        else
        {
            _currentOptions = new List<DialogueOption>();
        }
        Enabled = true;
    }



}