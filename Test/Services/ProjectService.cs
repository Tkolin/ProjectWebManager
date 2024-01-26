using Test.Models;

namespace Test.Services
{
    public class ProjectService
    {
        private readonly ProjectDbContext _context;
        public ProjectService(ProjectDbContext context)
        {
            _context = context;
        }
        public List<Company> GetCompanies()
        {
            return _context.Companies.ToList();
        }
        public List<Employee> GetEmployees()
        {
            return _context.Employees.ToList();
        }
        public int SaveProject(Project project)
        {
            if (project.Id == 0)
            {
                _context.Projects.Add(project);
            }

            _context.SaveChanges();

            return project.Id;
        }
        public void SaveDocument(List<Document> document)
        {
            foreach (Document documentItem in document) {
                    _context.Document.Add(documentItem);     
            }

            _context.SaveChanges();
        }
        public int SaveDocument(Document document)
        {
            _context.Document.Add(document);
            _context.SaveChanges();

            return document.Id;
        }
        public void BindingDocumentToProject(int projectId, List<int> documentsId) {

            foreach (var document in documentsId)
            {
                _context.DocumentsToProject.Add(new DocumentsToProject
                {
                    DocumentId = document,
                    ProjectId = projectId
                });
            }

            _context.SaveChanges();
        }
    }
}
