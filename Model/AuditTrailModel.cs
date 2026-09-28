namespace StudentViolations.API.Model
{
    public sealed class AuditTrailEntry
    {
        public long AuditID { get; set; }
        public string Action { get; set; } = string.Empty;
        public string EntityType { get; set; } = string.Empty;
        public string EntityID { get; set; } = string.Empty;
        public string? StudentNo { get; set; }
        public string? PreviousValue { get; set; }
        public string? NewValue { get; set; }
        public string? Remarks { get; set; }
        public string ActorUsername { get; set; } = string.Empty;
        public string ActorRole { get; set; } = string.Empty;
        public DateTime CreatedAtUtc { get; set; }
    }
}
