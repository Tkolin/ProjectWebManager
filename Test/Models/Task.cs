namespace Test.Models

{
    public class Task
    {
        public int Id { get; set; }
        public int ProjectId { get; set; }
        public int AuthorId { get; set; }
        public int? ExecutorId { get; set; }
        public string Description { get; set; }
        public int Priority { get; set; }
        public int StatusId { get; set; }

        public virtual Project Project { get; set; } = null!;
        public virtual Employee? Executor { get; set; } = null!;
        public virtual Employee Author { get; set; } = null!;
        public virtual StatusTask StatusTask {  get; set; } = null!;
}
}
