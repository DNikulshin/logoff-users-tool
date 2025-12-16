
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using LogoffUsersTool.Models;
using LogoffUsersTool.Services;

namespace LogoffUsersTool.UI
{
    public partial class MainForm : Form
    {
        private readonly SessionService _sessionService;
        private readonly SettingsService _settingsService;
        private FullAppSettings _fullAppSettings;
        private CancellationTokenSource _cancellationTokenSource;

        public MainForm()
        {
            InitializeComponent();
            _sessionService = new SessionService();
            _settingsService = new SettingsService();
            _cancellationTokenSource = new CancellationTokenSource();
            _fullAppSettings = _settingsService.LoadSettings();
            LoadSettings();
        }

        private void LoadSettings()
        {
            var lastUsed = _fullAppSettings.LastUsedSettings;
            serverTextBox.Text = lastUsed.Server;
            timerNumericUpDown.Value = lastUsed.TimerSeconds;
            intervalNumericUpDown.Value = lastUsed.NotificationInterval;
            messageTextBox.Text = lastUsed.Message;
        }

        private void SaveSettings()
        {
            var lastUsed = _fullAppSettings.LastUsedSettings;
            lastUsed.Server = serverTextBox.Text;
            lastUsed.TimerSeconds = (int)timerNumericUpDown.Value;
            lastUsed.NotificationInterval = (int)intervalNumericUpDown.Value;
            lastUsed.Message = messageTextBox.Text;
            _settingsService.SaveSettings(_fullAppSettings);
        }

        private async void startButton_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(serverTextBox.Text))
            {
                MessageBox.Show("Пожалуйста, введите имя сервера.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            startButton.Enabled = false;
            stopButton.Enabled = true;
            statusLabel.Text = "Выполняется...";
            progressBar.Value = 0;

            var server = serverTextBox.Text;
            var timer = (int)timerNumericUpDown.Value;
            var interval = (int)intervalNumericUpDown.Value;
            var message = messageTextBox.Text;

            _cancellationTokenSource = new CancellationTokenSource();
            var progress = new Progress<string>(update => outputRichTextBox.AppendText(update + "\n"));

            try
            {
                await HandleSessionResetAsync(server, timer, interval, message, progress, _cancellationTokenSource.Token);
            }
            catch (OperationCanceledException)
            {
                ((IProgress<string>)progress).Report("Операция прервана пользователем.");
            }
            catch (Exception ex)
            {
                ((IProgress<string>)progress).Report($"Ошибка: {ex.Message}");
            }
            finally
            {
                startButton.Enabled = true;
                stopButton.Enabled = false;
                statusLabel.Text = "Готово";
                progressBar.Value = 0;
            }
        }

        private void stopButton_Click(object sender, EventArgs e)
        {
            _cancellationTokenSource.Cancel();
        }

        private void clearButton_Click(object sender, EventArgs e)
        {
            outputRichTextBox.Clear();
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            SaveSettings();
        }

        private async Task HandleSessionResetAsync(string server, int timer, int interval, string message, IProgress<string> progress, CancellationToken cancellationToken)
        {
            progress.Report("Скрипт принудительного завершения сеансов");
            progress.Report($"Сервер: {server}");
            progress.Report($"Таймер: {timer} секунд");
            progress.Report($"Интервал оповещений: {interval} секунд\n");

            var initialSessions = _sessionService.GetActiveSessions(server);
            if (!initialSessions.Any())
            {
                progress.Report("Нет активных сессий.");
                return;
            }

            progress.Report($"Начальное количество сессий: {initialSessions.Count}");
            foreach (var session in initialSessions)
            {
                progress.Report($"  ID: {session.Id}, Пользователь: {session.UserName}");
            }
            progress.Report("\n");

            progress.Report("Начинаю обратный отсчет...");
            var remaining = timer;

            while (remaining > 0)
            {
                if (remaining == timer || (remaining % interval == 0 && remaining < timer))
                {
                    var currentSessions = _sessionService.GetActiveSessions(server);
                    if (currentSessions.Any())
                    {
                        var minutes = (int)Math.Ceiling(remaining / 60.0);
                        var formattedMessage = $"{message} (Осталось: {minutes} мин.)";

                        foreach (var session in currentSessions)
                        {
                            var sessionCopy = session;
                            _ = Task.Run(() => _sessionService.SendMessage(server, sessionCopy.Id, formattedMessage, interval), cancellationToken);
                        }
                        progress.Report($"[{DateTime.Now:HH:mm:ss}] Оповещение отправлено {currentSessions.Count} сессиям. Осталось: {remaining} секунд ({minutes} мин.)");
                    }
                }

                var percentComplete = 100 - (int)Math.Round((double)remaining / timer * 100);
                var statusText = $"Осталось: {remaining} секунд (примерно {(int)Math.Ceiling(remaining / 60.0)} мин.)";

                progressBar.Value = percentComplete;
                statusLabel.Text = statusText;

                await Task.Delay(1000, cancellationToken);
                remaining--;
            }

            progressBar.Value = 100;
            statusLabel.Text = "Завершение сеансов";
            progress.Report("\nВремя истекло! Завершаю сеансы...");

            var finalSessions = _sessionService.GetActiveSessions(server);
            if (finalSessions.Any())
            {
                foreach (var session in finalSessions)
                {
                    progress.Report($"Завершаю сеанс ID: {session.Id}, Пользователь: {session.UserName}");
                    _sessionService.LogoffSession(server, session.Id);
                }
            }
            else
            {
                progress.Report("Нет активных сессий для завершения.");
            }

            progress.Report("\nГотово! Все операции завершены.");
        }

        private void settingsButton_Click(object sender, EventArgs e)
        {
            using (var settingsForm = new SettingsForm())
            {
                if (settingsForm.ShowDialog() == DialogResult.OK)
                {
                    LoadSettings();
                }
            }
        }
        
        private void defaultSettingsButton_Click(object sender, EventArgs e)
        {
            var defaultSettings = _fullAppSettings.DefaultSettings;
            serverTextBox.Text = defaultSettings.Server;
            timerNumericUpDown.Value = defaultSettings.TimerSeconds;
            intervalNumericUpDown.Value = defaultSettings.NotificationInterval;
            messageTextBox.Text = defaultSettings.Message;
        }
    }
}
