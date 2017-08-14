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

namespace Basic_Serial
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
            viewModel = (MainViewModel) this.DataContext;
            myProtractor = new Protractor("COM1", 9600);
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
            while (true)
            {
                myProtractor.Read(); // Communicate with the sensor to get the data

                // How many objects are within view?
                int numObjects = myProtractor.ObjectCount();
                ConsoleWrite("Number of Objects: ");
                ConsoleWrite(numObjects);
                ConsoleWrite(", ");

                // If at least one object is in view, print the angle of the most visible object to the Serial Port
                if (numObjects > 0)
                {
                    int objectAngle = myProtractor.ObjectAngle(); // store the angle to the object in a variable
                    ConsoleWrite("Angle of Most Visible Object = ");
                    ConsoleWrite(objectAngle); // Print the Angle of the object
                    ConsoleWriteLine(" degrees");
                }

                // Print the angles to all objects within view. Objects are in rank order from most visible to least visible.
                ConsoleWriteLine("Angles, Visibility");
                for (int i = 0; i < numObjects; i++)
                {
                    ConsoleWrite("   ");
                    if (myProtractor.ObjectAngle(i) < 100) ConsoleWrite(" ");
                    if (myProtractor.ObjectAngle(i) < 10) ConsoleWrite(" ");
                    ConsoleWrite(myProtractor.ObjectAngle(i));
                    ConsoleWrite(", ");
                    if (myProtractor.ObjectVisibility(i) < 100) ConsoleWrite(" ");
                    if (myProtractor.ObjectVisibility(i) < 10) ConsoleWrite(" ");
                    ConsoleWriteLine(myProtractor.ObjectVisibility(i));
                }
                ConsoleWriteLine();

                // How many pathways are within view?
                int numPaths = myProtractor.PathCount();
                ConsoleWrite("Number of Paths: ");
                ConsoleWrite(numPaths);
                ConsoleWrite(", ");

                // If at least one pathway is in view, print the angle of the most visibile pathway to the Serial Port
                if (numPaths > 0)
                {
                    int path = myProtractor.PathAngle(); // store the angle to the object in a variable
                    ConsoleWrite("Angle of Most Visible Path = ");
                    ConsoleWrite(path); // Print the Angle
                    ConsoleWriteLine(" degrees");
                }

                // Print the angles to all paths within view. Paths are in rank order from most open to least open.
                ConsoleWriteLine("Angles, Visibility");
                for (int i = 0; i < numPaths; i++)
                {
                    ConsoleWrite("   ");
                    if (myProtractor.PathAngle(i) < 100) ConsoleWrite(" ");
                    if (myProtractor.PathAngle(i) < 10) ConsoleWrite(" ");
                    ConsoleWrite(myProtractor.PathAngle(i));
                    ConsoleWrite(", ");
                    if (myProtractor.PathVisibility(i) < 100) ConsoleWrite(" ");
                    if (myProtractor.PathVisibility(i) < 10) ConsoleWrite(" ");
                    ConsoleWriteLine(myProtractor.PathVisibility(i));
                }

                ConsoleWriteLine();
                await Task.Delay(1000);
            }
        }


        private void ConsoleWriteLine(string text)
        {
            viewModel.ConsoleLog += $"{text}";
            viewModel.ConsoleLog += "\r\n";
        }

        private void ConsoleWriteLine(int text)
        {
            viewModel.ConsoleLog += $"{text}";
            viewModel.ConsoleLog += "\r\n";
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
