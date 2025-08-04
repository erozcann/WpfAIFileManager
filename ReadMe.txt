# 🧠 WpfAIFileManager

**WpfAIFileManager**, kullanıcıların doğal dilde verdiği yazılı komutları anlayarak dosya sistemi üzerinde işlem yapabilen, yapay zeka destekli bir masaüstü (WPF) uygulamasıdır. Bu proje, Microsoft Semantic Kernel teknolojisi ile Semantic Function kullanarak komutları anlamlandırır ve sistemdeki işlemleri tetikler.

---

## 🎯 Proje Amacı

Bu uygulama, kullanıcıların aşağıdaki işlemleri yalnızca doğal dilde komut yazarak gerçekleştirebilmesini sağlar:

- 📂 Dosya ve klasör listeleme
- 🧬 Dosya ve klasör kopyalama
- 🚚 Dosya ve klasör taşıma
- ❌ Dosya ve klasör silme
- 🆕 Dosya veya klasör oluşturma
- 🔎 Filtreleme (uzantıya, tarihe göre)
- 📦 ZIP dosyası çıkarma (extract)

Tüm işlemler, uygulamanın yanına konumlandırılmış `BASE` isimli bir klasör içinde gerçekleşir. Sistem dosyalarına zarar verilmez.

---

## 🚀 Kullanılan Teknolojiler

| Teknoloji         | Açıklama                                   |
|------------------|---------------------------------------------|
| .NET 8           | Uygulama altyapısı                          |
| WPF              | Masaüstü kullanıcı arayüzü                  |
| C#               | Yazılım dili                                |
| Semantic Kernel  | Yapay zeka destekli komut yorumlama         |
| OpenRouter AI    | LLM sağlayıcısı (GPT tabanlı)               |
| JSON             | Komutların makine formatı                   |

---

## 🧩 Klasör ve Dosya Yapısı

WpfAIFileManager/
├── App.xaml
├── MainWindow.xaml
├── MainWindow.xaml.cs
├── AppPaths.cs
├── SemanticProcessor.cs
├── SemanticCommandResult.cs
├── FileCreateService.cs
├── FileCopyService.cs
├── FileMoveService.cs
├── FileDeleteService.cs
├── FileRenameService.cs
├── FileListerService.cs
├── ZipExtractorService.cs
├── SemanticFunctions/
│ ├── fileCommand_function.json
│ └── skprompt.txt
└── BASE/ ← Uygulamanın çalıştığı dizin


---

## 💬 Örnek Komutlar

> ✅ “Son 1 ay içinde oluşturulan PDF dosyalarını göster.”  
> ✅ “Test isimli klasördeki tüm TXT dosyalarını sil.”  
> ✅ “Temp klasörünü Arşiv olarak yeniden isimlendir.”  
> ✅ “A, B klasörlerindeki tüm JPG dosyalarını JPGs klasörüne kopyala.”

---

## 🧪 Kurulum ve Çalıştırma

### 1️⃣ Gerekli Araçlar

- [Visual Studio 2022+](https://visualstudio.microsoft.com/)
- [.NET 8.0 SDK](https://dotnet.microsoft.com/en-us/download)
- OpenRouter veya OpenAI API anahtarı

### 2️⃣ Repo'yu Klonlayın

```bash
git clone https://github.com/kullaniciadi/WpfAIFileManager.git
cd WpfAIFileManager

4️⃣ API Key Bilgilerini Girin
SemanticProcessor.cs dosyasındaki şu kısmı kendi API bilgilerinizle güncelleyin:

csharp
Kopyala
Düzenle
private const string ApiKey = "YOUR_API_KEY";
private const string Model = "gryphe/mythomax-l2-13b";
private const string Endpoint = "https://openrouter.ai/api/v1";
İpucu: OpenRouter kullanıyorsanız, https://openrouter.ai üzerinden ücretsiz key oluşturabilirsiniz.

5️⃣ Uygulamayı Başlatın
Visual Studio üzerinden WpfAIFileManager.sln dosyasını açın.
F5 tuşuna basarak uygulamayı başlatın.

🖥️ Uygulama Kullanımı
Açılan arayüzde komut kutusuna doğal dilde talimat yazın.

"Çalıştır" butonuna tıklayın.

Sonuçlar ekranda gösterilir.

İşlem geçmişi ve sistem mesajları da arayüzde listelenir.

⚙️ Semantic Kernel Yapısı
SemanticFunctions/skprompt.txt
Komutların nasıl algılanması gerektiğini tanımlayan temel prompt.

SemanticFunctions/fileCommand_function.json
LLM'den dönecek çıktının JSON şeması. Örn:

json
Kopyala
Düzenle
{
  "operation": "delete",
  "target": "file",
  "extension": "pdf",
  "createdBefore": "2024-08-01",
  "source": ["BASE/test"]
}
Bu JSON SemanticCommandResult.cs sınıfına parse edilerek ilgili servis yönlendirilir.

🛡️ Güvenlik
Sistem klasörlerine erişim engellenmiştir.

Yalnızca BASE klasörü altında işlem yapılabilir.

Hatalı komutlar kullanıcıya anlamlı hata mesajıyla gösterilir.

📌 Notlar
Sesli komut özelliği ileride eklenebilir.

Komut yorumlama gücü kullanılan modele göre artabilir.

Kod modülerdir, yeni servisler kolaylıkla eklenebilir.

👨‍💻 Geliştirici
Erenalp Özcan
📍 PAVOTEK - Web/UI Yazılım Departmanı
📅 Temmuz–Ağustos 2025 Staj Projesi
📫 [https://www.linkedin.com/in/erenalp%C3%B6zcan/]

📜 Lisans
Bu proje eğitim amaçlı olarak geliştirilmiştir ve açık kaynak değildir.
Kurumsal kullanım veya dağıtım için izne tabidir.