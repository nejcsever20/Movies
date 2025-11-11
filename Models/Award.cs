using System.Collections.Generic;

namespace Movies.Models
{
    public enum AwardRarity
    {
        Common,
        Uncommon,
        Rare,
        Epic,
        Legendary,
        Mythic,
        Divine
    }

    public class Award
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string IconUrl { get; set; } = string.Empty;
        public int RequirementLevel { get; set; } = 1;
        public string HowToUnlock { get; set; } = string.Empty;
        public AwardRarity Rarity { get; set; } = AwardRarity.Common;

        // Navigation property to UserAwards
        public virtual ICollection<UserAward> UserAwards { get; set; } = new List<UserAward>();
    }
}
