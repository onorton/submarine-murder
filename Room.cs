using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace LudumDare57;

public class Room(Texture2D background, List<Door> doors, List<Character> characters, List<Clue> clues, string name)
{

    public Room Parent { get; set; }

    public void Draw(SpriteBatch spriteBatch)
    {
        spriteBatch.Begin();
        spriteBatch.Draw(
            background,
            Vector2.Zero,
            null,
            Color.White,
            0f,
            Vector2.Zero,
            scale: new Vector2(
                (float)background.GraphicsDevice.Viewport.Bounds.Size.X / background.Bounds.Size.X,
                (float)background.GraphicsDevice.Viewport.Bounds.Size.Y / background.Bounds.Size.Y),
            effects: SpriteEffects.None,
            0f);

        spriteBatch.End();
        foreach (var door in doors)
        {
            door.Draw(spriteBatch);
        }

        foreach (var clue in clues)
        {
            clue.Draw(spriteBatch);
        }

        foreach (var character in characters)
        {
            character.Draw(spriteBatch);
        }
    }

    public Room NextRoom()
    {

        foreach (var door in doors)
        {
            if (door.TryPress())
            {
                return door.Room;
            }
        }

        return this;
    }

    public void Update()
    {
        foreach (var door in doors)
        {
            door.Update();
        }


        foreach (var character in characters)
        {
            character.Update();
        }


        foreach (var clue in clues)
        {
            clue.Update();
        }
    }

    public Character CharacterSelected()
    {
        foreach (var character in characters)
        {
            if (character.TryPress())
            {
                return character;
            }
        }

        return null;
    }

    public Clue FoundClue()
    {
        foreach (var clue in clues)
        {
            if (clue.TryPress())
            {
                return clue;
            }
        }

        return null;
    }
}