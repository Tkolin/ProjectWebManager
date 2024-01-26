namespace Test.Models
{
    public class Document
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public byte[] Data { get;  set; }
        public virtual ICollection<DocumentsToProject> DocumentsToProjects { get; set; } = new List<DocumentsToProject>();
    }
}
