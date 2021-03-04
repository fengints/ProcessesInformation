using Data;
using GetProcesses;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Imaging;
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
using WPF.Tools;
using WPF.ViewModels;

namespace WPF
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly IProcessInfoViewModel service;
        public ObservableCollection<ProcessInfo> ProcCollection { get => service.ProcessInfos; }

        public MainWindow()
        {
            InitializeComponent();
            service = new ProcessInfoViewModel();
        }

        private bool updateStarted;
        private CancellationTokenSource updateCtokenS;


        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            var button = (Button)sender;
            await UpdateButton(button);
        }
        private async Task UpdateButton(Button button)
        {
            if (!updateStarted)
            {
                updateStarted = true;
                SetButtonContent(button, "Прервать");

                updateCtokenS = new CancellationTokenSource();
                var ctoken = updateCtokenS.Token;

                // Update code
                var statusInfo = new StatusInfo();
                var updating = service.UpdateAsync(statusInfo, ctoken);

                //Send statusinfo to progressbar
                try
                {
                    await HandleProgressBar(UpdateProgress, statusInfo, ctoken);
                }
                finally
                {
                    updateStarted = false;
                    SetButtonContent(button, "Обновить");
                }

                await updating;
            }
            else
            {
                button.IsEnabled = false;
                SetButtonContent(button, "Прерывание.."); // if too long

                updateCtokenS.Cancel();

                button.IsEnabled = true;
            }
        }
        private async Task HandleProgressBar(ProgressBar bar, StatusInfo si, CancellationToken token)
        {
            try
            {
                bar.Minimum = si.StatusMin;
                bar.Maximum = si.StatusMax;
                while (si.CurrentStatus < 100)
                {
                    if (token.IsCancellationRequested)
                        return;
                    bar.Value = si.CurrentStatus;
                    await Task.Delay(500);
                }
            }
            finally
            {
                bar.Value = si.StatusMin;
            }
        }
        private void SetButtonContent(Button button, string content)
        {
            button.Content = content;
        }

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            //initialze view
            await UpdateButton(this.update_button);
        }
    }



}
