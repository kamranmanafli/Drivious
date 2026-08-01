namespace Drivious.Models
{
    public class NotificationSettings
    {
        /// <summary>
        /// How far ahead the generator looks for a date that is about to pass, and
        /// how far back it looks for one that already has.
        /// </summary>
        public int LeadDays { get; set; } = 30;

        /// <summary>Gap between two scans. The first scan runs at startup.</summary>
        public double ScanIntervalHours { get; set; } = 12;

        /// <summary>Set to false to keep the background scan from running at all.</summary>
        public bool Enabled { get; set; } = true;
    }
}
