using Drivious.Enums;

namespace Drivious.Extensions
{
    public static class FileExtension
    {
        public static bool CheckFileType(this IFormFile file, string type)
        {
            if (file.ContentType.Contains(type))
            {
                return true;
            }
            return false;
        }

        public static bool CheckFileSize(this IFormFile file, FileSize fileSize, long size)
        {
            switch (fileSize)
            {
                case FileSize.KB:
                    return file.Length <= size * 1024;
                case FileSize.MB:
                    return file.Length <= size * 1024 * 1024;
                case FileSize.GB:
                    return file.Length <= size * 1024 * 1024 * 1024;
            }
            return false;
        }

        public static async Task<string> CreateFileAsync(this IFormFile file, params string[] roots)
        {
            string fileName = string.Concat(Guid.NewGuid().ToString(), file.FileName);

            string path = string.Empty;

            for (int i = 0; i < roots.Length; i++)
            {
                path = Path.Combine(path, roots[i]);
            }

            path = Path.Combine(path, fileName);

            using (FileStream fileStream = new(path, FileMode.Create))
            {
                await file.CopyToAsync(fileStream);
            }

            return fileName;
        }

        public static void DeleteFile(this string fileName, params string[] roots)
        {
            string path = string.Empty;

            for (int i = 0; i < roots.Length; i++)
            {
                path = Path.Combine(path, roots[i]);
            }

            path = Path.Combine(path, fileName);

            if (File.Exists(path)) File.Delete(path);
        }
    }
}
