using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using WpfAIFileManager.Context;
using WpfAIFileManager.Protocol;
using WpfAIFileManager.Services;

namespace WpfAIFileManager
{
    public partial class MainWindow : Window
    {
        private readonly SemanticProcessor _semanticProcessor = new();
        private readonly List<string> _commandHistory = new();

        public MainWindow()
        {
            InitializeComponent();
        }

        private async void RunCommand(object sender, RoutedEventArgs e)
        {
            var input = CommandInput.Text?.Trim();

            if (string.IsNullOrWhiteSpace(input))
            {
                ResultText.Text = "Lütfen bir komut girin.";
                return;
            }

            // Komutu geçmişe ekle
            if (!_commandHistory.Contains(input))
            {
                _commandHistory.Add(input);
                CommandHistory.Items.Add(input);
            }

            // Progress bar'ı göster
            ProgressBar.Visibility = Visibility.Visible;
            ProgressBar.IsIndeterminate = true;
            ResultText.Text = "Komut işleniyor...";

            var result = await _semanticProcessor.ProcessAsync(input);

            if (result == null)
            {
                ResultText.Text = "Komut anlaşılamadı veya işlenemedi.";
                ProgressBar.Visibility = Visibility.Collapsed;
                return;
            }

            // Klasör adı varsa BASE altındaki gerçek path'i ayarla
            string actualDir = AppPaths.BaseDirectory;

            if (!string.IsNullOrWhiteSpace(result.Destination))
            {
                string subDir = Path.Combine(AppPaths.BaseDirectory, result.Destination);
                if (Directory.Exists(subDir))
                {
                    actualDir = subDir;
                }
            }

            // Eğer dosya oluşturuluyorsa ve destination'da uzantı yoksa ekle
            if (result.Target?.ToLower() == "file" &&
                !string.IsNullOrWhiteSpace(result.Destination) &&
                !string.IsNullOrWhiteSpace(result.Extension) &&
                !result.Destination.Contains("."))
            {
                result.Destination = $"{result.Destination}.{result.Extension}";
            }

            switch (result.Action?.ToLower())
            {
                case "extract" when result.Extension?.ToLower() == "zip":
                    ResultText.Text = ZipExtractorService.ExtractAllZips(actualDir);
                    break;

                case "list":
                    var files = FileListerService.ListFiles(
                        actualDir,
                        result.Extension,
                        result.CreatedAfter,
                        result.CreatedBefore);
                    ResultText.Text = string.Join(Environment.NewLine, files);
                    break;

                case "delete":
                    ResultText.Text = FileDeleteService.DeleteFiles(
                        baseDir: actualDir,
                        extension: result.Extension,
                        createdAfter: result.CreatedAfter,
                        createdBefore: result.CreatedBefore,
                        fileNameOrFolder: result.FileName ?? result.Destination,
                        target: result.Target);
                    break;

                case "copy":
                    ResultText.Text = FileCopyService.CopyFiles(
                        actualDir,
                        result.Extension,
                        result.CreatedAfter,
                        result.CreatedBefore,
                        result.Destination,
                        result.Source);

                    break;

                case "move":
                    ResultText.Text = FileMoveService.MoveFiles(
                        actualDir,
                        result.Extension,
                        result.CreatedAfter,
                        result.CreatedBefore,
                        result.Destination,
                        result.Source);
                    break;


                case "create":
                    ResultText.Text = FileCreateService.CreateItem(
                        actualDir,
                        result.Target,
                        result.Destination);
                    break;

                case "rename":
                    ResultText.Text = FileRenameService.RenameItem(
                        AppPaths.BaseDirectory,
                        result.OldName,
                        result.NewName);
                    break;

                default:
                    ResultText.Text = $"İşlem: {result.Action} (henüz bu işlem için servis yazılmadı)";
                    break;
            }

            // Progress bar'ı gizle
            ProgressBar.Visibility = Visibility.Collapsed;
        }

        private void CommandHistory_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (CommandHistory.SelectedItem != null)
            {
                CommandInput.Text = CommandHistory.SelectedItem.ToString();
            }
        }

    }
}
