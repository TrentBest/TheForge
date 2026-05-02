using System.Collections.Generic;
using System.Text;

namespace Workshop.GURPS
{
    public class SpaceStarships
    {
        public Hull HullType { get; private set; } = Hull.Lifeboat;
    }

    public enum Hull
    {
        Lifeboat,
        Fighter,
        Scout,
        Corvette,
        Freighter,
        ColonyShip,
        Destroyer,
        Cruiser,
        Battleship,
        Dreadnaught,
        OrbitalFortress
    }

    public enum WorldFrequency
    {
        Common,
        Likely,
        Scattered,
        Scarce,
        Rare,
    }
}


//Note To Self:  Page 22 TechLevels Checklist -
//  Overall Tech Level (list of TL cards where selection of a TL card presents it's data in the view panel.
//  StarDrive Type
//  Starship Range

//I wsa thinking above was the opening of a new game, the first thing we need to know is what the player is looking for, 
//are they starting at the beginning where they have a colony and a scout?  Or do we start farther along with a more advanced start?

//But the second value is resulting of the TL choice, not a choice of itself, unless we wanted to allow an FTL galaxy with STL race(s).