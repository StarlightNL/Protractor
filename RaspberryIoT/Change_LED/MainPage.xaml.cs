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
This is an example for the Protractor Sensor. This example will demonstrate how to change the behavior 
of the Protractor's Feedback LEDs. The behaviors available are:
 - Show Object: Visually indicates the angle to the most visible object
 - Show Path: Visually indicates the angle to the most open path
 - Off: Turn off the feedback LEDs
Changes to the LED behavior take effect immediately. Changes are not remembered and must be set each time 
the Protractor is rebooted.

This example assumes I2C communication, but the same methods can be applied whether I2C or Serial 
communication is used.

For a complete tutorial on wiring up and using the Protractor go to:
    http://www.will-moore.com/protractor/ProtractorAngleProximitySensor_UserGuide.pdf
*/
namespace Change_LED
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private MainViewModel viewModel;
        private Protractor myProtractor;

        int newI2Caddress = 28; // Pick a number between 0 and 127 that is not already on the I2C bus

        public MainPage()
        {
            this.InitializeComponent();
            viewModel = (MainViewModel) this.DataContext;
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
                ConsoleWriteLine(
                    "Could not connect to Protractor. Check if your wiring is correct and that you used the correct I2C address");
                while (true) ;
            }
            myProtractor.LEDshowPath(); // LEDs will indicate the location of the most visible object
            ConsoleWriteLine("Showing Path");
            await Task.Delay(10000); // Play with the Protractor for 10 seconds.

            // LED OFF
            // Set the LEDs to Off. This may be useful to save power or avoid interference with other optical sensors.
            // NOTE: Setting the LEDs to off will also disable the Green status LED.
            myProtractor.LEDoff(); // LEDs are off
            ConsoleWriteLine("LEDs Off");
            await Task.Delay(2000); // Wait 2 seconds.

            // LED SHOW OBJECT
            // Set the LEDs to show the location of the most visible object.
            myProtractor.LEDshowObject(); // LEDs will indicate the location of the most visible object
            ConsoleWriteLine("Showing Objects");
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