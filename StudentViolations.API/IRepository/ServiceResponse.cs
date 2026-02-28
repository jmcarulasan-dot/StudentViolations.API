namespace StudentViolations.API.IRepository
{
    public class ServiceResponse<T>
    {
        public int Status { get; set; }
        public string? Message { get; set; }
        public T? Data { get; set; }
    }
}
