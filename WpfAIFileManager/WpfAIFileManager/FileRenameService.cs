using System;
using System.IO;

namespace WpfAIFileManager
{
    public static class FileRenameService
    {
        public static string RenameItem(string baseDir, string? oldName, string? newName)
        {
            if (string.IsNullOrWhiteSpace(oldName) || string.IsNullOrWhiteSpace(newName))
                return "Eski veya yeni isim belirtilmedi.";

            string oldPath = Path.Combine(baseDir, oldName);
            string newPath = Path.Combine(baseDir, newName);

            try
            {
                if (File.Exists(oldPath))
                {
                    File.Move(oldPath, newPath);
                    return $"Dosya '{oldName}' → '{newName}' olarak yeniden adlandırıldı.";
                }
                else if (Directory.Exists(oldPath))
                {
                    Directory.Move(oldPath, newPath);
                    return $"Klasör '{oldName}' → '{newName}' olarak yeniden adlandırıldı.";
                }
                else
                {
                    return $"'{oldName}' bulunamadı.";
                }
            }
            catch (Exception ex)
            {
                return $"Yeniden adlandırma işlemi başarısız: {ex.Message}";
            }
        }
    }
}
