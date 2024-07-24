using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using System.Text;
using DataFormats = System.Windows.DataFormats;
using DragDropEffects = System.Windows.DragDropEffects;
using System.Windows.Media;
using System.Windows.Input;

namespace Replacer
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private CancellationTokenSource _cancellationTokenSource;
        string _filename;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void BrowseSourcePath_Click(object sender, RoutedEventArgs e)
        {
            _filename = "";
            var openFileDialog = new Microsoft.Win32.OpenFileDialog();
            if (openFileDialog.ShowDialog() == true)
            {
                SourcePathTextBox.Text = openFileDialog.FileName;
                _filename = openFileDialog.SafeFileName;
            }
        }

        private void BrowseDestinationPath_Click(object sender, RoutedEventArgs e)
        {
            FolderBrowserDialog dlg = new FolderBrowserDialog();
            if (dlg.ShowDialog() != System.Windows.Forms.DialogResult.OK) return;
            DestinationPathTextBox.Text = dlg.SelectedPath;
        }

        private async void StartStopButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(SourcePathTextBox.Text) && string.IsNullOrEmpty(DestinationPathTextBox.Text))
            {
                OutputTextBox.Text = "Не указаны пути";
                return;
            }
            else if (string.IsNullOrEmpty(SourcePathTextBox.Text) && !string.IsNullOrEmpty(DestinationPathTextBox.Text)) 
            {
                OutputTextBox.Text = "Не указан исходный путь";
                return;
            }
            else if (!string.IsNullOrEmpty(SourcePathTextBox.Text) && string.IsNullOrEmpty(DestinationPathTextBox.Text)) 
            {
                OutputTextBox.Text = "Не указан путь назначения";
                return;
            }


            if (_cancellationTokenSource == null)
            {
                _cancellationTokenSource = new CancellationTokenSource();
                ProgressBar.Visibility = Visibility.Visible;
                IProgress<string> progress = new Progress<string>(m =>
                {
                    OutputTextBox.AppendText($"{Environment.NewLine}[{DateTime.Now:dd.MM.yy HH:mm:ss.fffff}]: {m}{Environment.NewLine}");
                    OutputTextBox.ScrollToEnd();
                });
                StartStopButton.Content = "Прекратить";

                string sourcePath = SourcePathTextBox.Text;
                string destinationPath = DestinationPathTextBox.Text;
                _filename = Path.GetFileName(sourcePath);
                var sb = new StringBuilder();
                var spaces = (_filename.Length + 2) / 2;
                sb.AppendFormat("[{0}]\n", _filename)
                  .Append('─', spaces)
                  .Append('┬')
                  .Append('─', _filename.Length % 2 == 0 ? spaces - 1 : spaces)
                  .AppendLine()
                  .Append(' ', spaces)
                  .AppendFormat("├──◄ [{0}]\n", sourcePath)
                  .Append(' ', spaces)
                  .AppendLine("│")
                  .Append(' ', spaces)
                  .AppendFormat("└──► [{0}]\n", destinationPath);
                OutputTextBox.Text = sb.ToString();
                await CopyFileAsync(sourcePath, destinationPath, _cancellationTokenSource.Token, progress);
            }
            else
            {
                _cancellationTokenSource.Cancel();
                _cancellationTokenSource = null;
                StartStopButton.Content = "Начать";
                ProgressBar.Visibility = Visibility.Visible;
            }
        }

        private async Task CopyFileAsync(string sourcePath, string destinationPath, CancellationToken cancellationToken, IProgress<string> progress)
        {
            
            try
            {
                if (!File.Exists(sourcePath))
                {
                    progress.Report("Исходный путь не существует.");
                    return;
                }
                if (!File.Exists(destinationPath) && !Directory.Exists(destinationPath))
                {
                    progress.Report("Целевой путь не существует.");
                    return;
                }
                else if (Directory.Exists(destinationPath)) destinationPath = Path.Combine(destinationPath, _filename);
                
                while (!cancellationToken.IsCancellationRequested)
                {
                    try
                    {
                        progress.Report($"Начало копирования...");
                        await Task.Run(() => File.Copy(sourcePath, destinationPath, true), cancellationToken);
                        progress.Report($"Копирование завершено.");
                        break;
                    }
                    catch (Exception ex)
                    {
                        progress.Report($"Ошибка копирования:\n{ex.Message}");
                        await Task.Delay(5000, cancellationToken);
                    }
                }
            }
            catch (TaskCanceledException)
            {
                progress.Report("Операция отменена.");
            }
            finally
            {
                _cancellationTokenSource = null;
                Dispatcher.Invoke(() =>
                {
                    StartStopButton.Content = "Начать";
                    ProgressBar.Visibility= Visibility.Hidden;
                });
            }
        }

        private void SourcePathTextBox_PreviewDragOver(object sender, System.Windows.DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                e.Effects = DragDropEffects.Copy;
                SourcePathTextBox.Background = (Brush)FindResource("TextBoxMouseDragOverColor");
                e.Handled = true;
            }
            else
            {
                e.Effects = DragDropEffects.None;
            }
        }

        private void SourcePathTextBox_Drop(object sender, System.Windows.DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                var files = (string[])e.Data.GetData(DataFormats.FileDrop);
                if (files.Length > 0)
                {
                    SourcePathTextBox.Text = files[0];
                }
            }
            SourcePathTextBox.ClearValue(BackgroundProperty);
        }

        private void SourcePathTextBox_DragLeave(object sender, System.Windows.DragEventArgs e)
        {
            SourcePathTextBox.ClearValue(BackgroundProperty);
        }        

        private void DestinationPathTextBox_PreviewDragOver(object sender, System.Windows.DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                e.Effects = DragDropEffects.Copy;
                DestinationPathTextBox.Background = (Brush)FindResource("TextBoxMouseDragOverColor");
                e.Handled = true;
            }
            else
            {
                e.Effects = DragDropEffects.None;
            }
        }

        private void DestinationPathTextBox_PreviewDrop(object sender, System.Windows.DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                var files = (string[])e.Data.GetData(DataFormats.FileDrop);
                if (files.Length > 0)
                {
                    DestinationPathTextBox.Text = files[0];
                }
            }
            DestinationPathTextBox.ClearValue(BackgroundProperty);
        }

        private void DestinationPathTextBox_PreviewDragLeave(object sender, System.Windows.DragEventArgs e)
        {
            DestinationPathTextBox.ClearValue(BackgroundProperty);
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void MinimizeButton_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        private void MaximizeButton_Click(object sender, RoutedEventArgs e)
        {
            if (WindowState == WindowState.Maximized)
            {
                MaximizeButton.Content = "🗗";
                WindowState = WindowState.Normal;
            }
            else
            {
                MaximizeButton.Content = "🗖";
                WindowState = WindowState.Maximized;
            }
        }

        private void TitleBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount == 2)
            {
                MaximizeButton_Click(sender, e);
            }
            else
            {
                DragMove();
            }
        }
    }
}
