namespace StudentViolations.API.Helpers
{
    public static class ViolationHelper
    {
        public static string GetWarningLevel(int violationCount)
        {
            if (violationCount >= 3) return "red";
            else if (violationCount == 2) return "orange";
            else if (violationCount == 1) return "yellow";
            else return "green";
        }

        public static string GetRecommendedAction(int violationCount)
        {
            return violationCount switch
            {
                0 => "No action needed",
                1 => "Issue written warning",
                2 => "Call parents / schedule counseling",
                _ => "Recommend for dismissal"
            };
        }
    }
}