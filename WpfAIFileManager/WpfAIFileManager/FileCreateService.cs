using System;
using System.IO;

namespace WpfAIFileManager
{
    public static class FileCreateService
    {
        public static string CreateFolder(string baseDir, string? folderName)
        {
            if (string.IsNullOrWhiteSpace(folderName))
                return "Oluşturulacak klasör adı belirtilmedi.";

            string fullPath = Path.IsPathRooted(folderName)
                ? folderName
                : Path.Combine(baseDir, folderName);

            try
            {
                if (!Directory.Exists(fullPath))
                    Directory.CreateDirectory(fullPath);

                return $"'{folderName}' klasörü başarıyla oluşturuldu.";
            }
            catch (Exception ex)
            {
                return $"Klasör oluşturulamadı: {ex.Message}";
            }
        }
        public static string CreateItem(string baseDir, string? target, string? name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return "Oluşturulacak ad belirtilmedi.";

            // Eğer name zaten tam yol ise kullan, değilse BASE ile birleştir
            string fullPath = Path.IsPathRooted(name) ? name : Path.Combine(baseDir, name);

            try
            {
                if (target?.ToLower() == "folder")
                {
                    if (!Directory.Exists(fullPath))
                        Directory.CreateDirectory(fullPath);

                    return $"'{name}' klasörü başarıyla oluşturuldu.";
                }
                else if (target?.ToLower() == "file")
                {
                    if (!File.Exists(fullPath))
                        File.WriteAllText(fullPath, ""); // boş dosya oluştur

                    return $"'{name}' dosyası başarıyla oluşturuldu.";
                }
                else
                {
                    return "Geçersiz hedef türü: file ya da folder olmalı.";
                }
            }
            catch (Exception ex)
            {
                return $"Oluşturma işlemi başarısız: {ex.Message}";
            }
        }

    }
}
