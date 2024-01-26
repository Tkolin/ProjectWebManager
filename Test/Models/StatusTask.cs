namespace Test.Models
{
    public class StatusTask
    {
        public int Id { get; set; }

        public string Name { get; set; } = null!;

        public virtual ICollection<Task> Status { get; set; } = new List<Task>();

    }
}