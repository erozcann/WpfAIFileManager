using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace WpfAIFileManager.Services
{
    public static class FileCopyService
    {
        public static string CopyFiles(string baseDir, string? extension, string? createdAfter, string? createdBefore, string? destinationFolder, List<string>? source = null)
        {
            if (string.IsNullOrWhiteSpace(destinationFolder))
                return "Hedef klasör belirtilmedi.";

            if (destinationFolder.Contains("*"))
                return "Geçersiz hedef klasör adı.";

            if (!Directory.Exists(baseDir))
                return "BASE klasörü bulunamadı.";

            // AI bazen "JPGs,jpg,png" ya da "JPGs/test.jpg" gibi şeyler gönderiyor, sadece ilk klasör adını al
            string cleanDestination = destinationFolder.Split(new[] { '/', '\\', ',', '.' }, StringSplitOptions.RemoveEmptyEntries).First().Trim();
            string fullDestination = Path.Combine(baseDir, cleanDestination);

            if (!Directory.Exists(fullDestination))
                Directory.CreateDirectory(fullDestination);

            var matched = new List<FileInfo>();

            // Uzantı filtresi
            List<string> extensions = new();

            if (!string.IsNullOrWhiteSpace(extension) && extension != "*")
            {
                extensions = extension.Split(',', '.', '|', ';')
                                      .Select(e => e.Trim().ToLower())
                                      .Where(e => !string.IsNullOrWhiteSpace(e))
                                      .ToList();
            }

            // Eğer source klasörleri geldiyse
            if (source != null && source.Any())
            {
                foreach (var folderName in source)
                {
                    string folderPath = Path.Combine(baseDir, folderName);

                    if (Directory.Exists(folderPath))
                    {
                        var files = Directory.GetFiles(folderPath, "*", SearchOption.AllDirectories)
                                             .Select(f => new FileInfo(f));

                        if (extensions.Any())
                            files = files.Where(f => extensions.Contains(f.Extension.TrimStart('.').ToLower()));

                        if (DateTime.TryParse(createdAfter, out var after))
                            files = files.Where(f => f.CreationTime >= after);

                        if (DateTime.TryParse(createdBefore, out var before))
                            files = files.Where(f => f.CreationTime <= before);

                        matched.AddRange(files);
                    }
                }
            }
            else
            {
                // Source yoksa BASE'de tüm dosyalar
                var files = Directory.GetFiles(baseDir, "*", SearchOption.AllDirectories)
                                     .Select(f => new FileInfo(f));

                if (extensions.Any())
                    files = files.Where(f => extensions.Contains(f.Extension.TrimStart('.').ToLower()));

                if (DateTime.TryParse(createdAfter, out var after))
                    files = files.Where(f => f.CreationTime >= after);

                if (DateTime.TryParse(createdBefore, out var before))
                    files = files.Where(f => f.CreationTime <= before);

                matched = files.ToList();
            }

            var errors = new List<string>();

            foreach (var file in matched)
            {
                string destPath = Path.Combine(fullDestination, file.Name);
                try
                {
                    File.Copy(file.FullName, destPath, overwrite: true);
                }
                catch (Exception ex)
                {
                    errors.Add($"'{file.Name}': {ex.Message}");
                }
            }

            if (errors.Count > 0)
                return $"Kopyalama tamamlandı. Hatalar:\n{string.Join("\n", errors)}";

            return matched.Count > 0
                ? $"{matched.Count} dosya '{cleanDestination}' klasörüne kopyalandı."
                : "Kopyalanacak dosya bulunamadı.";
        }
    }
}
