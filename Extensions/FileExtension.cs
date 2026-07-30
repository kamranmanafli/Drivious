using Drivious.Enums;

namespace Drivious.Extensions
{
    public static class FileExtension
    {
        public static bool CheckFileType(this IFormFile file, string type)
        {
            return file.ContentType.Contains(type, StringComparison.OrdinalIgnoreCase);
        }

        public static bool CheckFileSize(this IFormFile file, FileSize fileSize, long size)
        {
            return fileSize switch
            {
                FileSize.KB => file.Length <= size * 1024,
                FileSize.MB => file.Length <= size * 1024 * 1024,
                FileSize.GB => file.Length <= size * 1024 * 1024 * 1024,
                _ => false
            };
        }

        public static async Task<string> CreateFileAsync(this IFormFile file, params string[] roots)
        {
            // Only the extension is taken from the client - the rest of file.FileName is
            // attacker controlled and could contain path segments such as "../".
            string extension = Path.GetExtension(file.FileName);

            string fileName = $"{Guid.NewGuid()}{extension}";

            string directory = Path.Combine(roots);

            Directory.CreateDirectory(directory);

            string path = Path.Combine(directory, fileName);

            using (FileStream fileStream = new(path, FileMode.Create))
            {
                await file.CopyToAsync(fileStream);
            }

            return fileName;
        }

        public static void DeleteFile(this string? fileName, params string[] roots)
        {
            if (string.IsNullOrWhiteSpace(fileName)) return;

            // Guard against a stored value that escapes the target directory.
            string safeName = Path.GetFileName(fileName);

            string path = Path.Combine(Path.Combine(roots), safeName);

            if (File.Exists(path)) File.Delete(path);
        }
    }
}
