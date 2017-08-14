using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Devices.I2c;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using ProtractorLib;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace Change_Serial_Baudrate
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private MainViewModel viewModel;
        private Protractor myProtractor;

        int currentBaudRate = 9600; // Whatever the Protractor's current baud rate is, default is 9600
        int newBaudRate = 57600; // Pick a new baud rate for the Protractor.
        

        public MainPage()
        {
            this.InitializeComponent();
            viewModel = (MainViewModel)this.DataContext;
            myProtractor = new Protractor("COM1", currentBaudRate);
            PollProtractor();
        }

        private async void PollProtractor()
        {
            await Task.Delay(500); // Wait for Protractor to boot
            var connected = myProtractor.Read();
            if (connected) ConsoleWriteLine("Connected to Protractor.");
            else
            {
                ConsoleWriteLine("Could not connect to Protractor. Check if your wiring is correct and that you used the correct I2C address");
                while (true) ;
            }

            myProtractor.SetNewSerialBaudRate(newBaudRate); // Next time the Protractor is rebooted, serial communication will be at the new baud rate
            ConsoleWrite("New Baud Rate Set: ");
            ConsoleWriteLine(newBaudRate);

            while (true)
            {
                myProtractor.Read(); // Communicate with the sensor to get the data

                ConsoleWrite("Number of Objects: ");
                ConsoleWriteLine(myProtractor.ObjectCount());

                if (myProtractor.ObjectCount() > 0)
                {
                    ConsoleWrite("Angle to most visible Object = ");
                    ConsoleWrite(myProtractor.ObjectAngle());
                    ConsoleWriteLine(" degrees");
                }

                ConsoleWrite("Number of Paths: ");
                ConsoleWriteLine(myProtractor.PathCount());

                if (myProtractor.PathCount() > 0)
                {
                    ConsoleWrite("Angle to the Path = ");
                    ConsoleWrite(myProtractor.PathAngle());
                    ConsoleWriteLine(" degrees");
                }
                ConsoleWriteLine();
                await Task.Delay(1000);
            }
        }

        private void ConsoleWriteLine(string text, bool newLine = true)
        {
            viewModel.ConsoleLog += $"{text}";
            if (newLine) viewModel.ConsoleLog += "\r\n";
        }
        private void ConsoleWriteLine(int text, bool newLine = true)
        {
            viewModel.ConsoleLog += $"{text}";
            if (newLine) viewModel.ConsoleLog += "\r\n";
        }
        private void ConsoleWriteLine()
        {
            viewModel.ConsoleLog += "\r\n";
        }

        private void ConsoleWrite(string text)
        {
            viewModel.ConsoleLog += $"{text}";
        }
        private void ConsoleWrite(int text)
        {
            viewModel.ConsoleLog += $"{text}";
        }
    }
}
