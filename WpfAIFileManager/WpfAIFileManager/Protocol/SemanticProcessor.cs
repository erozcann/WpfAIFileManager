#pragma warning disable SKEXP0010

using System;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using WpfAIFileManager.Models;

namespace WpfAIFileManager.Protocol
{
    public class SemanticProcessor
    {
        private const string ApiKey = "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";
        private const string Model = "gryphe/mythomax-l2-13b";
        private const string Endpoint = "https://openrouter.ai/api/v1";

        private readonly IChatCompletionService _chatService;

        public SemanticProcessor()
        {
            var builder = Kernel.CreateBuilder();
            builder.Services.AddSingleton<IChatCompletionService>(_ =>
                new OpenAIChatCompletionService(
                    modelId: Model,
                    endpoint: new Uri(Endpoint),
                    apiKey: ApiKey
                )
            );

            var kernel = builder.Build();
            _chatService = kernel.GetRequiredService<IChatCompletionService>();
        }

        public async Task<SemanticCommandResult?> ProcessAsync(string input)
        {
            var systemPrompt = @"
Sen SADECE geçerli bir JSON üret. Başka hiçbir şey yazma.

‼️ YASAK:
- Açıklama yazma
- Yorum yapma
- Kod bloğu kullanma
- Giriş konuşması yapma
- Ekstra örnek verme
- 'Output:' veya başka bir fazlalık yazma

🎯 Sadece şu formatta JSON üret (gereken alanlar varsa):

{
  ""action"": ""create | delete | move | copy | extract | rename | list"",
  ""target"": ""file | folder"",
  ""extension"": ""pdf | txt | zip | * | jpg,png"",
  ""createdBefore"": ""YYYY-MM-DD"",
  ""createdAfter"": ""YYYY-MM-DD"",
  ""destination"": ""hedef klasör adı"",
  ""oldName"": ""eski adı"",
  ""newName"": ""yeni adı"",
  ""source"": [""A"", ""B""]
}

GENEL KURALLAR:
- Boş alanları dahil etme (örneğin: createdAfter yoksa hiç yazma)
- source değeri varsa, mutlaka string listesi (dizi) olmalı → örnek: [""A"", ""B"", ""C""]
- target alanı her zaman ""file"" veya ""folder"" olmalı
- createdBefore / createdAfter alanları sadece YYYY-MM-DD formatında yazılmalı
- Dosya veya klasör isimlerini değiştirme, aynen kullan
- Hiçbir şekilde yorum, açıklama, 'Output:', 'Sample:' gibi ifadeler yazma

🚫 Sadece geçerli ve sade JSON çıktısı ver, başka hiçbir şey ekleme.
";



            var chat = new ChatHistory();
            chat.AddSystemMessage(systemPrompt);
            chat.AddUserMessage(input);

            try
            {
                var result = await _chatService.GetChatMessageContentAsync(chat);

                if (result is ChatMessageContent message)
                {
                    var content = message.Content?.Trim();

                    if (!string.IsNullOrWhiteSpace(content))
                    {
                        File.WriteAllText("debug_result.txt", "[GELEN YANIT]:\n" + content);

                        int jsonStart = content.IndexOf('{');
                        int jsonEnd = content.LastIndexOf('}');

                        if (jsonStart == -1 || jsonEnd == -1 || jsonEnd <= jsonStart)
                        {
                            // AI JSON vermedi, tekrar dene
                            var retryPrompt = "SADECE JSON VER. Başka hiçbir şey yazma. Komut: " + input;
                            chat.AddUserMessage(retryPrompt);

                            var retryResult = await _chatService.GetChatMessageContentAsync(chat);
                            if (retryResult is ChatMessageContent retryMessage)
                            {
                                content = retryMessage.Content?.Trim();
                                jsonStart = content.IndexOf('{');
                                jsonEnd = content.LastIndexOf('}');

                                if (jsonStart == -1 || jsonEnd == -1 || jsonEnd <= jsonStart)
                                    throw new Exception("AI JSON yanıtı vermedi:\n\n" + content);
                            }
                            else
                            {
                                throw new Exception("AI yanıtı alamadı");
                            }
                        }

                        string jsonPart = content.Substring(jsonStart, jsonEnd - jsonStart + 1).Trim();

                        // Temizleme (boş alanlar ve fazladan virgüller)
                        string[] cleanTargets = {
                    @"""createdBefore"": """",",
                    @"""createdAfter"": """",",
                    @"""destination"": """",",
                    @"""oldName"": """",",
                    @"""newName"": """",",
                    @"""extension"": """",",
                    @"""source"": """",",
                    @"""type"": """",",
                    @"""name"": """","
                };

                        foreach (var target in cleanTargets)
                        {
                            jsonPart = jsonPart.Replace(target, "");
                        }

                        // Sondaki virgül varsa kaldır
                        jsonPart = jsonPart.Trim().TrimEnd(',');
                        if (jsonPart.EndsWith(",}"))
                            jsonPart = jsonPart.Replace(",}", "}");

                        // Logla
                        File.AppendAllText("debug_result.txt", "\n\n[İŞLENMİŞ JSON]:\n" + jsonPart);

                        // Debug gösterim
                        MessageBox.Show(jsonPart, "JSON PARÇASI");

                        // Deserialize
                        var parsed = JsonSerializer.Deserialize<SemanticCommandResult>(jsonPart, new JsonSerializerOptions
                        {
                            AllowTrailingCommas = true,
                            ReadCommentHandling = JsonCommentHandling.Skip,
                            PropertyNameCaseInsensitive = true
                        });

                        // EK: Silme komutları için akıllı mantık
                        if (parsed?.Action == "delete")
                        {
                            // Belirli dosya adı kontrolü
                            if (!string.IsNullOrWhiteSpace(parsed.Destination))
                            {
                                // Eğer destination'da uzantı varsa (test.txt, dosya.pdf) → belirli dosya
                                if (parsed.Destination.Contains("."))
                                {
                                    parsed.FileName = parsed.Destination;
                                    parsed.Destination = null;
                                    parsed.Extension = null;
                                }
                                // Eğer destination'da uzantı yoksa ama txt/pdf gibi kelime varsa → belirli dosya
                                else if (parsed.Destination.Contains("txt") || parsed.Destination.Contains("pdf"))
                                {
                                    parsed.FileName = parsed.Destination;
                                    parsed.Destination = null;
                                    parsed.Extension = null;
                                }
                                // Eğer destination klasör adı ise → klasör içindeki dosyalar
                                else
                                {
                                    // Klasör içindeki dosyaları sil
                                    parsed.FileName = null;
                                    parsed.Extension = null;
                                }
                            }
                            // Eğer extension varsa → o uzantıdaki tüm dosyalar
                            else if (!string.IsNullOrWhiteSpace(parsed.Extension))
                            {
                                parsed.FileName = null;
                                parsed.Destination = null;
                            }
                        }

                        // Debug için log
                        if (parsed?.Action == "delete")
                        {
                            File.AppendAllText("debug_result.txt", $"\n\n[SİLME DEBUG]:\n" +
                                $"Action: {parsed.Action}\n" +
                                $"Target: {parsed.Target}\n" +
                                $"Extension: {parsed.Extension}\n" +
                                $"Destination: {parsed.Destination}\n" +
                                $"FileName: {parsed.FileName}\n");
                        }

                        return parsed;
                    }
                }

                return null;
            }
            catch (JsonException jex)
            {
                File.WriteAllText("debug_result.txt", "JSON AYRIŞTIRMA HATASI:\n" + jex.ToString());
                MessageBox.Show("YANIT JSON formatında çözümlenemedi:\n\n" + jex.Message, "JSON Hatası");
                return null;
            }
            catch (Exception ex)
            {
                File.WriteAllText("debug_result.txt", "GENEL HATA:\n" + ex.ToString());
                MessageBox.Show("HATA: " + ex.Message, "Genel Hata");
                return null;
            }
        }





    }
}

#pragma warning restore SKEXP0010
