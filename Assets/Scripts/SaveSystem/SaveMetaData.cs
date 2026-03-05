// Done
using System;

namespace SaveSystem
{
    [Serializable]
    public class SaveMetadata
    {
        public string characterName;
        public string location;
        public string lastSaved;      // ISO 8601 format
        public float playTimeSeconds;
        public int gold;
        public int crystals;
        
        public string FormattedPlayTime
        {
            get
            {
                var span = TimeSpan.FromSeconds(playTimeSeconds);
                return span.TotalHours >= 1 
                    ? $"{(int)span.TotalHours}h {span.Minutes}m" 
                    : $"{span.Minutes}m {span.Seconds}s";
            }
        }
        
        public string FormattedDateTime
        {
            get
            {
                if (DateTime.TryParse(lastSaved, out var dt))
                    return dt.ToString("MMM dd, yyyy  HH:mm");
                return lastSaved;
            }
        }
    }
}