using UnityEngine;

namespace Workshop.GURPS
{
    public class GURPSCharacterSkill
    {
        public GURPSSkillDefinition Definition;
        public float PointsInvested = 0.5f;

        // GURPS 3e Skill Calculation Logic
        public int GetRelativeLevelOffset()
        {
            int step = 0;
            if (PointsInvested >= 0.5f) step = 1;
            if (PointsInvested >= 1.0f) step = 2;
            if (PointsInvested >= 2.0f) step = 3;
            if (PointsInvested >= 4.0f) step = 4;
            if (PointsInvested >= 8.0f) step = 4 + Mathf.FloorToInt((PointsInvested - 4f) / 2f); // +1 per 2 pts after 4

            int difficultyPenalty = Definition.Difficulty switch
            {
                GURPSDifficulty.Easy => -2,
                GURPSDifficulty.Average => -3,
                GURPSDifficulty.Hard => -4,
                GURPSDifficulty.VeryHard => -5,
                _ => -3
            };

            // Physical skills advance slightly differently in 3e, but for simplicity, we use the unified offset table
            return difficultyPenalty + step;
        }

        public int GetActualLevel(GURPS3eCharacterData character)
        {
            int baseAttr = Definition.BaseAttribute switch
            {
                GURPSAttributeType.ST => character.ST,
                GURPSAttributeType.DX => character.DX,
                GURPSAttributeType.IQ => character.IQ,
                GURPSAttributeType.HT => character.HT,
                _ => 10
            };
            return baseAttr + GetRelativeLevelOffset();
        }

        public string RelativeLevelString
        {
            get
            {
                int offset = GetRelativeLevelOffset();
                string attr = Definition.BaseAttribute.ToString();
                if (offset == 0) return attr;
                return offset > 0 ? $"{attr}+{offset}" : $"{attr}{offset}";
            }
        }
    }
}