using System.Collections.Generic;

namespace Assets.Scripts.MastersOfOrionII
{
    [System.Serializable]
    public class RaceConfigData
    {
        public int TotalPoints;
        public string RaceName = "Terran Alliance";
        public List<string> TraitIds = new List<string>();
    }
}