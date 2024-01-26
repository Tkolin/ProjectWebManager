namespace Test.Models
{
    public class DocumentsToProject
    {

        public virtual int DocumentId { get; set; }

        public virtual int ProjectId { get; set; }

        public virtual Document Document { get; set; } = null!;
        public virtual Project Project { get; set; } = null!;
    }
}
