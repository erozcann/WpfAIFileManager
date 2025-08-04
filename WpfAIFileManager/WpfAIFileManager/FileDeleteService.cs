using System;
using System.IO;
using System.Linq;

namespace WpfAIFileManager
{
    public static class FileDeleteService
    {
        public static string DeleteFiles(string baseDir, string? extension, string? createdAfter, string? createdBefore, string? fileNameOrFolder, string? target = null)
        {
            if (string.IsNullOrWhiteSpace(baseDir) || !Directory.Exists(baseDir))
                return "Klasör bulunamadı.";

            string targetDir = baseDir;

            // Eğer bir alt klasör belirtilmişse, onun yolunu belirle
            if (!string.IsNullOrWhiteSpace(fileNameOrFolder))
            {
                string potentialSubDir = Path.Combine(baseDir, fileNameOrFolder);
                if (Directory.Exists(potentialSubDir))
                {
                    targetDir = potentialSubDir;
                }
            }

            var files = Directory.GetFiles(targetDir, "*", SearchOption.TopDirectoryOnly)
                                 .Select(f => new FileInfo(f));

            // Belirli uzantı varsa filtrele
            if (!string.IsNullOrWhiteSpace(extension) && extension != "*")
            {
                var extensions = extension.Split('.', ',', '|', ';')
                                          .Select(ext => ext.Trim().TrimStart('.').ToLower())
                                          .Where(ext => !string.IsNullOrWhiteSpace(ext))
                                          .ToList();

                files = files.Where(f => extensions.Contains(f.Extension.TrimStart('.').ToLower()));
            }

            // Tarihe göre filtrele
            if (DateTime.TryParse(createdAfter, out var after))
                files = files.Where(f => f.CreationTime >= after);

            if (DateTime.TryParse(createdBefore, out var before))
                files = files.Where(f => f.CreationTime <= before);

            // Klasör silme işlemi
            if (target?.ToLower() == "folder" && !string.IsNullOrWhiteSpace(fileNameOrFolder))
            {
                string folderPath = Path.Combine(baseDir, fileNameOrFolder);
                if (Directory.Exists(folderPath))
                {
                    try
                    {
                        Directory.Delete(folderPath, recursive: true);
                        return $"'{fileNameOrFolder}' klasörü ve içindeki tüm dosyalar başarıyla silindi.";
                    }
                    catch (Exception ex)
                    {
                        return $"Klasör silinemedi: {ex.Message}";
                    }
                }
                else
                {
                    return $"'{fileNameOrFolder}' klasörü bulunamadı.";
                }
            }

            // Dosya adı eşleşmesi varsa, sadece onu sil
            if (!string.IsNullOrWhiteSpace(fileNameOrFolder))
            {
                // Eğer tam dosya adı verilmişse (örn: test.txt)
                if (fileNameOrFolder.Contains("."))
                {
                    files = files.Where(f => f.Name.Equals(fileNameOrFolder, StringComparison.OrdinalIgnoreCase));
                }
                // Eğer klasör adı verilmişse
                else if (Directory.Exists(Path.Combine(targetDir, fileNameOrFolder)))
                {
                    // Klasör içindeki dosyaları sil
                    var subDir = Path.Combine(targetDir, fileNameOrFolder);
                    var subFiles = Directory.GetFiles(subDir, "*", SearchOption.TopDirectoryOnly);
                    files = subFiles.Select(f => new FileInfo(f));
                }
                // Eğer sadece dosya adı verilmişse (uzantısız)
                else
                {
                    files = files.Where(f => f.Name.Equals(fileNameOrFolder, StringComparison.OrdinalIgnoreCase));
                }
            }

            var toDelete = files.ToList();
            int count = 0;

            var errors = new List<string>();
            
            foreach (var file in toDelete)
            {
                try
                {
                    file.Delete();
                    count++;
                }
                catch (Exception ex)
                {
                    errors.Add($"'{file.Name}': {ex.Message}");
                }
            }

            var result = $"{count} dosya başarıyla silindi.";
            if (errors.Count > 0)
            {
                result += $"\n\nHata alınan dosyalar:\n{string.Join("\n", errors)}";
            }

            return result;
        }
    }
}

