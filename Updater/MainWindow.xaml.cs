using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Updater
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private CancellationTokenSource _cancellationTokenSource = new();

        public MainWindow()
        {
            InitializeComponent();
        }

        private void BrowseSourcePath_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            if (openFileDialog.ShowDialog() == true)
            {
                SourcePathTextBox.Text = openFileDialog.FileName;
            }
        }

        private void BrowseDestinationPath_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            if (openFileDialog.ShowDialog() == true)
            {
                DestinationPathTextBox.Text = openFileDialog.FileName;
            }
        }

        private async void StartStopButton_Click(object sender, RoutedEventArgs e)
        {
            if (_cancellationTokenSource == null)
            {
                _cancellationTokenSource = new CancellationTokenSource();
                StartStopButton.Content = "Stop";

                string sourcePath = SourcePathTextBox.Text;
                string destinationPath = DestinationPathTextBox.Text;

                OutputTextBox.Clear();

                await CopyFileAsync(sourcePath, destinationPath, _cancellationTokenSource.Token);
            }
            else
            {
                _cancellationTokenSource.Cancel();
                _cancellationTokenSource = null!;
                StartStopButton.Content = "Start";
            }
        }

        private async Task CopyFileAsync(string sourcePath, string destinationPath, CancellationToken cancellationToken)
        {
            try
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    try
                    {
                        File.Copy(sourcePath, destinationPath, true);
                        AppendOutput($"File copied from {sourcePath} to {destinationPath}");
                        break;
                    }
                    catch (Exception ex)
                    {
                        AppendOutput($"Error copying file: {ex.Message}");
                        await Task.Delay(5000, cancellationToken);
                    }
                }
            }
            catch (TaskCanceledException)
            {
                AppendOutput("Operation cancelled.");
            }
            finally
            {
                Dispatcher.Invoke(() =>
                {
                    _cancellationTokenSource = null!;
                    StartStopButton.Content = "Start";
                });
            }
        }

        private void AppendOutput(string message)
        {
            Dispatcher.Invoke(() =>
            {
                OutputTextBox.AppendText($"{DateTime.Now}: {message}{Environment.NewLine}");
                OutputTextBox.ScrollToEnd();
            });
        }
    }
}
