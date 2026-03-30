namespace StudentViolations.API.Helpers
{
    // Shared helper used by all controllers
    public static class ViolationHelper
    {
        public static string GetWarningLevel(int violationCount)
        {
            if (violationCount >= 3) return "red";
            else if (violationCount == 2) return "orange";
            else if (violationCount == 1) return "yellow";
            else return "green";
        }
    }
}