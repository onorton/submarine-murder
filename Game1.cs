using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Timers;
using LudumDare57.Dialogue;
using LudumDare57.UI;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

namespace LudumDare57;

public class Game1 : Game
{
    private GraphicsDeviceManager _graphics;
    private SpriteBatch _spriteBatch;

    private TextureButton _backButton;

    private Canvas _victoryUI;
    private Canvas _gameOverUI;

    private SoundEffect _victorySoundEffect;
    private SoundEffect _gameOverSoundEffect;
    private SoundEffect _clueFoundSoundEffect;
    private Room _currentRoom;

    private HashSet<Clue> _foundClues;

    private DialogueRunner _dialogueRunner;

    private Character _guiltyCharacter;

    private AccusationManger _accusationManager;


    private double _timer = 0.0;
    private double _accusationTimer = -1.0f;
    public const int SpriteScaling = 2;

    private TextBox _clueTextBox;

    private TextureButton _mutedButton;
    private TextureButton _unmutedButton;


    public Game1()
    {
        _graphics = new GraphicsDeviceManager(this);
        Content.RootDirectory = "Content";
        IsMouseVisible = true;
        _graphics.PreferredBackBufferWidth = 800;
        _graphics.PreferredBackBufferHeight = 600;
    }


    protected override void Initialize()
    {
        _guiltyCharacter = null;

        MouseController.Instantiate(this);

        var uiFont = Content.Load<SpriteFont>("MyMenuFont");



        var defaultRect = new Texture2D(GraphicsDevice, 1, 1);
        defaultRect.SetData([Color.White]);

        _victorySoundEffect = Content.Load<SoundEffect>("victory");
        _gameOverSoundEffect = Content.Load<SoundEffect>("game_over");
        _clueFoundSoundEffect = Content.Load<SoundEffect>("clue_found");

        var panelRect = new Rectangle(GraphicsDevice.Viewport.Bounds.Center.X - 400 / 2, GraphicsDevice.Viewport.Bounds.Center.Y - 300 / 2, 400, 300);

        _victoryUI = new Canvas(new List<UIElement>()
        {
            new Panel(GraphicsDevice, GraphicsDevice.Viewport.Bounds, new Color(1, 1, 1, 0)), // Empty panel to prevent clicking outside of panel
            new Panel(GraphicsDevice, panelRect, Color.DarkGray),
            new TextBox(new Vector2(panelRect.Center.X, panelRect.Top + 20), uiFont) { Text = "Well done!"},
            new TextBox(new Vector2(panelRect.Center.X, panelRect.Top + 90), uiFont) { Text = "You've managed to solve the case!"},
            new TextBox(new Vector2(panelRect.Center.X, panelRect.Top + 140), uiFont) { Text = "Thank you for playing!"},
            new Button(GraphicsDevice, new Rectangle(panelRect.Center.X - 100 / 2, panelRect.Bottom - 60, 100, 50), Color.Gray, Color.White, () => Exit(), "Exit", uiFont)
        });

        _victoryUI.Enabled = false;

        _gameOverUI = new Canvas(new List<UIElement>()
        {
            new Panel(GraphicsDevice, GraphicsDevice.Viewport.Bounds, new Color(1, 1, 1, 0)), // Empty panel to prevent clicking outside of panel
            new Panel(GraphicsDevice, panelRect, Color.DarkGray),
            new TextBox(new Vector2(panelRect.Center.X, panelRect.Top + 20), uiFont) { Text = "Game Over!"},
            new TextBox(new Vector2(panelRect.Center.X, panelRect.Top + 90), uiFont) { Text = "Unfortunately, you accused the wrong person"},
            new Button(GraphicsDevice, new Rectangle(panelRect.Center.X - 100 / 2, panelRect.Bottom - 130, 100, 50), Color.Gray, Color.White, Initialize, "Restart", uiFont),
            new Button(GraphicsDevice, new Rectangle(panelRect.Center.X - 100 / 2, panelRect.Bottom - 60, 100, 50), Color.Gray, Color.White, Exit, "Exit", uiFont)
        });

        _gameOverUI.Enabled = false;

        _clueTextBox = new TextBox(new Vector2(GraphicsDevice.Viewport.Bounds.Center.X, GraphicsDevice.Viewport.Bounds.Bottom - 50), uiFont);
        _clueTextBox.Enabled = false;

        // Characters
        var cook = new Character(Content.Load<Texture2D>("Characters/cook"), new Point(420, 0), "Cook");
        var captain = new Character(Content.Load<Texture2D>("Characters/captain"), new Point(250, 100), "Captain");
        var scientist = new Character(Content.Load<Texture2D>("Characters/scientist"), new Point(300, 50), "Dr. Robinson");
        var engineer = new Character(Content.Load<Texture2D>("Characters/engineer"), new Point(500, 80), "Engineer");

        // Clues
        var victim = new Clue(Content.Load<Texture2D>("Clues/victim"), new Point(150, 177), "Victim", "Yep. He's definitely dead.");
        var poison = new Clue(Content.Load<Texture2D>("Clues/poison"), new Point(110, 150), "Poison", "This must have been used in the murder.");
        var letter = new Clue(Content.Load<Texture2D>("Clues/letter"), new Point(440, 250), "Letter", "A letter. Seems like the captain was having an affair with Dr. Jones' wife.");
        var researchPaper = new Clue(Content.Load<Texture2D>("Clues/research_paper"), new Point(180, 160), "Research Paper", "A research paper. Dr. Jones' name is the first name on it followed by Dr. Robinson's.");
        var overWorked = new Clue(defaultRect, Point.Zero, "Overworked", "Dr. Robinson has been overworked in the lab.");
        var cabinetKeys = new Clue(Content.Load<Texture2D>("Clues/keys"), new Point(600, 400), "Keys", "Some keys. Looks like they would fit the cabinets in the lab.");
        var cabinet = new Clue(Content.Load<Texture2D>("Clues/cabinet"), new Point(542, -2), "Cabinet", "No signs of forced entry.");
        var victimHatedCooking = new Clue(defaultRect, Point.Zero, "Overworked", "Dr. Jones hated the cook's food.");

        // Accusations
        var captainAccusations = new List<Accusation>() { new Accusation([letter, cabinetKeys, cabinet], "Affair", "You were having an affair with his wife!") };
        var cookAccusations = new List<Accusation>() { new Accusation([poison], "He hated your cooking", "He insulted your cooking and you couldn't take it!") };
        var scientistAccusations = new List<Accusation>() { new Accusation([researchPaper, overWorked, cabinet], "He took credit", "You did all the work but he was taking the credit!") };


        // Rooms
        var sleepingQuarters = new Room(Content.Load<Texture2D>("sleeping_quarters"),
            [],
            [scientist],
            [],
            "Sleeping Quarters"
            );
        var sleepingQuartersDoor = new Door(Content.Load<Texture2D>("Doors/sleeping_quarters_door"), sleepingQuarters, new Point(258, 73));


        var lab = new Room(Content.Load<Texture2D>("lab"),
        [],
        [],
        [researchPaper, cabinet],
        "Lab"
        );
        var labDoor = new Door(Content.Load<Texture2D>("Doors/lab_door"), lab, new Point(492, 73));

        var engineRoom = new Room(Content.Load<Texture2D>("engine_room"),
        [],
        [engineer],
        [],
        "Engine Room"
        );
        var engineRoomDoor = new Door(Content.Load<Texture2D>("Doors/engine_room_door"), engineRoom, new Point(334, 93));

        var secondCorridor = new Room(Content.Load<Texture2D>("second_corridor"),
    [sleepingQuartersDoor, labDoor, engineRoomDoor],
    [],
    [],
    "Second Corridor"
    );
        var secondCorridorDoor = new Door(Content.Load<Texture2D>("Doors/second_corridor_door"), secondCorridor, new Point(675, 80));
        engineRoom.Parent = secondCorridor;
        lab.Parent = secondCorridor;
        sleepingQuarters.Parent = secondCorridor;


        var kitchen = new Room(Content.Load<Texture2D>("kitchen"),
        [],
        [cook],
        [poison],
        "Kitchen"
        );

        var kitchenDoor = new Door(Content.Load<Texture2D>("Doors/kitchen_door"), kitchen, new Point(638, 68));



        var messHall = new Room(Content.Load<Texture2D>("mess_hall"),
      [kitchenDoor],
      [],
      [victim],
      "Mess Hall"
      );
        var messHallDoor = new Door(Content.Load<Texture2D>("Doors/mess_hall_door"), messHall, new Point(532, 73));
        kitchen.Parent = messHall;


        var captainsQuarters = new Room(Content.Load<Texture2D>("captain_quarters"),
      [],
      [captain],
      [letter, cabinetKeys],
      "Captain's Quarters"
      );

        var captainsQuartersDoor = new Door(Content.Load<Texture2D>("Doors/captains_quarters_door"), captainsQuarters, new Point(45, 52));


        _guiltyCharacter = scientist;

        var corridor = new Room(Content.Load<Texture2D>("corridor"),
        [messHallDoor, secondCorridorDoor, captainsQuartersDoor],
        [],
        [],
        "Main Corridor"
        );

        messHall.Parent = corridor;
        secondCorridor.Parent = corridor;
        captainsQuarters.Parent = corridor;

        _currentRoom = corridor;
        _backButton = new TextureButton(Content.Load<Texture2D>("back_button"), new Point(GraphicsDevice.Viewport.Bounds.Center.X - Content.Load<Texture2D>("back_button").Bounds.Size.X / 4, GraphicsDevice.Viewport.Bounds.Bottom - 250), GoBack);


        _unmutedButton = new TextureButton(Content.Load<Texture2D>("unmuted"), new Point(GraphicsDevice.Viewport.Bounds.Right - Content.Load<Texture2D>("unmuted").Bounds.Size.X / 2 - 10, 10), ToggleMute);
        _mutedButton = new TextureButton(Content.Load<Texture2D>("muted"), new Point(GraphicsDevice.Viewport.Bounds.Right - Content.Load<Texture2D>("muted").Bounds.Size.X / 2 - 10, 10), ToggleMute) { Enabled = false };


        _foundClues = new HashSet<Clue>();


        var scientistDialogueOptions = new List<DialogueOption>()
        {
            new DialogueOption(GraphicsDevice, "Who can access the poison?", "Only myself and Dr. Jones have keys. Oh, and the captain of course.", uiFont),
            new DialogueOption(GraphicsDevice, "You look tired", "Yeah. I've been working ten hours a day in the lab. It's tough work but rewarding.", uiFont, overWorked)
        };

        var engineerDialogueOptions = new List<DialogueOption>()
        {
            new DialogueOption(GraphicsDevice, "How's the engine?", "Still keeping on. What a terrible thing that happened to Dr. Jones.", uiFont),
            new DialogueOption(GraphicsDevice, "Dr. Jones' death", "It's tough. His ex-wife is going to be devastated once she hears the news.", uiFont, overWorked)
        };


        var cookDialogueOptions = new List<DialogueOption>()
        {
            new DialogueOption(GraphicsDevice, "Dr. Jones", "He might consider himself lucky. He always said he hated my cooking.", uiFont, victimHatedCooking),
            new DialogueOption(GraphicsDevice, "What are you cooking?", "Just some stew for us all. Some of us have taken Dr. Jones' death hard.", uiFont)
        };


        var captainDialogueOptions = new List<DialogueOption>()
        {
            new DialogueOption(GraphicsDevice, "Dr. Jones", "It's unfortunate, but we still must complete the mission. At least Dr. Robinson can carry on with the research.", uiFont),
        };

        var dialogueOptions = new Dictionary<string, List<DialogueOption>>
        {
            [captain.Name] = captainDialogueOptions,
            [scientist.Name] = scientistDialogueOptions,
            [engineer.Name] = engineerDialogueOptions,
            [cook.Name] = cookDialogueOptions

        };

        _dialogueRunner = new DialogueRunner(uiFont, new Vector2(GraphicsDevice.Viewport.Bounds.Center.X, 10), GraphicsDevice, dialogueOptions);
        _dialogueRunner.Start();





        _accusationManager = new AccusationManger(_foundClues, new Dictionary<string, List<Accusation>>
        {
            [captain.Name] = captainAccusations,
            [cook.Name] = cookAccusations,
            [scientist.Name] = scientistAccusations

        });


        base.Initialize();
    }

    protected override void LoadContent()
    {
        _spriteBatch = new SpriteBatch(GraphicsDevice);
        var ambiance = Content.Load<Song>("ambiance");

        MediaPlayer.IsRepeating = true;
        MediaPlayer.Play(ambiance);

    }

    protected override void Update(GameTime gameTime)
    {

        if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
            Exit();

        _victoryUI.Update();
        _victoryUI.TryPress();

        _gameOverUI.Update();
        _gameOverUI.TryPress();

        _mutedButton.Update();
        _mutedButton.TryPress();


        _unmutedButton.Update();
        _unmutedButton.TryPress();


        var optionSelected = _dialogueRunner.Update();

        if (optionSelected != null && optionSelected.Clue != null)
        {
            _foundClues.Add(optionSelected.Clue);
        }

        if (_dialogueRunner.CharacterAccused)
        {
            if (_accusationTimer == 0.0f)
            {
                _dialogueRunner.Enabled = false;
                if (_dialogueRunner.Character == _guiltyCharacter)
                {
                    if (!_victoryUI.Enabled)
                    {
                        _victorySoundEffect.Play();
                    }
                    _victoryUI.Enabled = true;

                }
                else
                {
                    if (!_gameOverUI.Enabled)
                    {
                        _gameOverSoundEffect.Play();
                    }
                    _gameOverUI.Enabled = true;
                }
            }
            else if (_accusationTimer < 0.0f)
            {
                _accusationTimer = 2.0f;
            }
        }

        if (_dialogueRunner.Character != null)
        {
            _timer = 0.0f;
            _dialogueRunner.UpdateAccusations(_accusationManager.AccusationsAvailable(_dialogueRunner.Character));
        }

        _currentRoom.Update();
        var nextRoom = _currentRoom.NextRoom();
        if (!nextRoom.Equals(_currentRoom))
        {
            _clueTextBox.Enabled = false;
        }
        _currentRoom = nextRoom;
        var characterSelected = _currentRoom.CharacterSelected();
        if (characterSelected != null)
        {
            _dialogueRunner.SetCharacter(characterSelected);
        }

        var foundClue = _currentRoom.FoundClue();
        if (foundClue != null)
        {
            _clueFoundSoundEffect.Play();
            _foundClues.Add(foundClue);
            _clueTextBox.Text = foundClue.Description;
            _clueTextBox.Enabled = true;
            _timer = 5.0;
        }


        var previousRoom = _currentRoom;
        _backButton.Enabled = _currentRoom.Parent != null;
        _backButton.Update();
        _backButton.TryPress();

        if (previousRoom != _currentRoom)
        {
            _clueTextBox.Enabled = false;
        }



        MouseController.Instance.Update(gameTime);


        _timer = Math.Max(_timer - gameTime.ElapsedGameTime.TotalSeconds, 0.0);

        if (_accusationTimer >= 0.0f)
        {
            _accusationTimer = Math.Max(_accusationTimer - gameTime.ElapsedGameTime.TotalSeconds, 0.0);
        }
        if (_timer == 0.0)
        {
            _clueTextBox.Enabled = false;
        }

        base.Update(gameTime);


    }

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(Color.CornflowerBlue);


        _currentRoom.Draw(_spriteBatch);
        _backButton.Draw(_spriteBatch);

        _dialogueRunner.Draw(_spriteBatch);
        _victoryUI.Draw(_spriteBatch);
        _gameOverUI.Draw(_spriteBatch);

        _clueTextBox.Draw(_spriteBatch);

        _mutedButton.Draw(_spriteBatch);


        _unmutedButton.Draw(_spriteBatch);

        base.Draw(gameTime);
    }

    public void GoBack()
    {
        _currentRoom = _currentRoom.Parent;
    }

    public void ToggleMute()
    {
        var currentlyMuted = MediaPlayer.IsMuted;

        MediaPlayer.IsMuted = !currentlyMuted;
        SoundEffect.MasterVolume = currentlyMuted ? 1.0f : 0.0f;
        _mutedButton.Enabled = !currentlyMuted;
        _unmutedButton.Enabled = currentlyMuted;

    }
}

