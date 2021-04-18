using System;
using System.Windows.Controls;
using System.Windows.Media;
using BankSync.Logging;

namespace BankSync.Windows
{
    public record ProgressMessage(string Text, Brush Brush);

    public class TextBoxLogger : IBankSyncLogger
    {
        private readonly IProgress<ProgressMessage> progress;

        public TextBoxLogger(ListBox textBox)
        {
            progress = new Progress<ProgressMessage>(message => textBox.Items.Add(new ListBoxItem(){Content = message.Text, Foreground = message.Brush}));
        }

        public void Debug(string message)
        {
            progress.Report(new ProgressMessage(message, Brushes.Gray));
        }

        public void Info(string message)
        {
            progress.Report(new ProgressMessage(message, Brushes.Black));
        }

        public void Warning(string message)
        {
            progress.Report(new ProgressMessage(message, Brushes.Orange));
        }

        public void Error(string message, Exception ex)
        {
            progress.Report(new ProgressMessage(message, Brushes.Red));
        }

        public void LogProgress(string progressMessage)
        {
            progress.Report(new ProgressMessage(progressMessage, Brushes.Blue));
        }

        public void EndLogProgress(string progressMessage)
        {
            progress.Report(new ProgressMessage(progressMessage, Brushes.Blue));
        }

        public void StartLogProgress(string progressMessage)
        {
            progress.Report(new ProgressMessage(progressMessage, Brushes.Blue));

        }
    }
}