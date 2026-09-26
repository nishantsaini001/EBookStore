namespace BooksShoppingProjectMVC.Shared
{
    public class FileService : IFileService
    {
        private readonly IWebHostEnvironment _environment;

        public FileService(IWebHostEnvironment environment)
        {
           _environment = environment;  
        }

        public async Task<string> SaveFile(IFormFile file, string[] allowedExtensions)
        {
            var wwwPath = _environment.WebRootPath;
            var path = Path.Combine(wwwPath,"images");

            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
            var extension = Path.GetExtension(file.FileName);
            if (!allowedExtensions.Contains(extension))
            {
                throw new InvalidOperationException($"Only {string.Join(",",allowedExtensions)} files allowed");
            }

            string fileName = $"{Guid.NewGuid()}{extension}";
            string fileNameWithPath = Path.Combine(path,fileName);
            using var stream = new FileStream(fileNameWithPath,FileMode.Create);
            await file.CopyToAsync(stream);
            return fileName;
        }

        public void DeleteFile(string fileName)
        {
            var wwwPath = _environment.WebRootPath;
            var fileNameWithpath = Path.Combine(wwwPath, "images\\",fileName);
            if (!File.Exists(fileNameWithpath))
            {
                throw new FileNotFoundException(fileName);
            }
            File.Delete(fileNameWithpath);
        }

    }
}

namespace BooksShoppingProjectMVC
{
    public interface IFileService
    {
        Task<string> SaveFile(IFormFile file, string[] allowedExtensions);
        void DeleteFile(string fileName);
    }
}