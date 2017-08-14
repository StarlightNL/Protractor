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
/* PROTRACTOR - A Proximity Sensor that Measures Angles
This is an example for the Protractor Sensor. This example will demonstrate how to change the scan time 
of the Protractor.
 - By default, the Protractor scans 180 degrees every 15 milliseconds. This example will demonstrate how 
   to increase the time between scans to save power.
 - By default, the Protractor scans continuously. This example will demonstrate how to disable continuous 
   scanning so that scans occur only when data is requested. There will be approximately 15 milliseconds 
   delay between when the data is requested and when the Protractor responds.
Changes to the scan time take effect immediately. Changes are not remembered and must be set each time the 
Protractor is rebooted.
This example assumes I2C communication, but the same methods can be applied whether I2C or Serial communication 
is used.

For a complete tutorial on wiring up and using the Protractor go to:
    http://www.will-moore.com/protractor/ProtractorAngleProximitySensor_UserGuide.pdf
*/

namespace Change_Scan_Time
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private MainViewModel viewModel;
        private Protractor myProtractor;
        

        public MainPage()
        {
            this.InitializeComponent();
            viewModel = (MainViewModel)this.DataContext;
            myProtractor = new Protractor(I2cController.GetDefaultAsync().GetResults(), 69, false);
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

            myProtractor.ScanTime(300); // It will now take 300 milliSeconds to complete a 180 degree scan
            ConsoleWriteLine("New Scan Time Set");
            
            await Task.Delay(10000); // Play with the Protractor for 10 seconds. Note that the Blue LEDs don't update as fast or smoothly now.

            // Disable Continuous Scanning
            if (connected)
            {
                myProtractor.ScanTime(0); // A Scan Time of Zero will disable continuous scanning.
                ConsoleWriteLine("Continuous Scan Disabled"); // Note that the Blue LEDs will only update after a scan is requested
            }
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
                ConsoleWriteLine();
                await Task.Delay(3000);
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
