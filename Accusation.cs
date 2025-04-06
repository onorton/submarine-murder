

using System.Collections.Generic;
using System.Linq;

namespace LudumDare57;


public class Accusation(HashSet<Clue> clues, string text, string dialogueText)
{

    public string Text => text;
    public string DialogueText => dialogueText;


    public bool Valid(HashSet<Clue> cluesFound)
    {
        return clues.IsSubsetOf(cluesFound);
    }
}