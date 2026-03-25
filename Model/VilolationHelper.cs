namespace StudentViolations.API.Helpers
{
    // Shared helper used by GuardController, StudentController, GuidanceController, and SAOController
    public static class ViolationHelper
    {
        // Returns a color-coded warning level based on the number of violations
        public static string GetWarningLevel(int violationCount)
        {
            if (violationCount >= 3) return "red";
            else if (violationCount == 2) return "orange";
            else if (violationCount == 1) return "yellow";
            else return "green";
        }
    }
}