using System.ComponentModel;
using System.Diagnostics;
using System.Reflection.Metadata.Ecma335;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;


namespace LudumDare57;

public class MouseController : GameComponent
{
    public MouseController(Game game) : base(game)
    {
    }

    private static MouseController _instance;

    private MouseState _previousState;
    private bool _pressRegisteredThisFrame = false;
    public bool BlockedCurrentFrame {get; set;}

    public override void Update(GameTime gameTime)
    {
        BlockedCurrentFrame = false;
        _pressRegisteredThisFrame = false;
        _previousState = Mouse.GetState();
        base.Update(gameTime);
    }

    public bool PressedThisFrame()
    {
        var pressAttempt = Mouse.GetState().LeftButton == ButtonState.Pressed && _previousState.LeftButton == ButtonState.Released;

        var successfulPress = pressAttempt && !_pressRegisteredThisFrame;

        _pressRegisteredThisFrame = successfulPress || _pressRegisteredThisFrame;

        return successfulPress;
    }

    public Point Position { get { return Mouse.GetState().Position; } }

    public static void Instantiate(Game game)
    {
        if (_instance == null)
        {
            _instance = new MouseController(game);
        }
    }

    public static MouseController Instance => _instance;
}
