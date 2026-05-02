using System.Collections.Generic;

namespace Assets.Scripts.MastersOfOrionII
{
    public class ColonizedSystem
    {
        public string CustomName { get; set; }
        public int PopulationMillions { get; set; }
        public List<int> InOrbit { get; set; }
    }
}