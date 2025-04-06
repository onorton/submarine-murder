
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace LudumDare57;

public class AccusationManger(HashSet<Clue> cluesFound, Dictionary<string, List<Accusation>> accusations)
{
    public List<Accusation> AccusationsAvailable(Character character)
    {
        var accusationsFound = accusations.TryGetValue(character.Name, out var accusationsForCharacter);

        if (!accusationsFound)
        {
            return new List<Accusation>();
        }

        return accusationsForCharacter.Where(accusation => accusation.Valid(cluesFound)).ToList();
    }



}