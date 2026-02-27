namespace StudentViolationsAPI.Model.Entities
{
    public class Student
    {
        public string Id { get; set; } = Guid.NewGuid().ToString("N").Substring(0, 10);
        public string QrCode { get; set; }
        public string Name { get; set; }
        public int ViolationCount { get; set; }
    }
}