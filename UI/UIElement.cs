

namespace LudumDare57.UI;

public abstract class UIElement() {


    public bool Enabled {get; set;} = true;

    public bool BlocksRays {get; set;} = true;


    public abstract bool InBounds();
}