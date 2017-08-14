using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Enumeration;
using Windows.Devices.I2c;
using Windows.Devices.SerialCommunication;

namespace ProtractorLib
{
    public sealed class Protractor
    {
        // Constants
        const byte SERIALCOMM = 1;
        const byte I2CCOMM = 2;
        const byte MAXOBJECTS = 4;
        const byte SHOWOBJ = 1;
        const byte SHOWPATH = 2;
        const byte LEDOFF = 3;
        const byte MINDUR = 15;

        // PROTRACTOR COMMANDS
        const byte REQUESTDATA = 0x15;
        const byte SCANTIME = 0x20;
        const byte I2CADDR = 0x24;
        const byte BAUDRATE = 0x26;
        const byte LEDUSAGE = 0x30;

        byte[] _buffer = new byte[1 + 4 * MAXOBJECTS]; // store data received from Protractor.
        byte _numdata; // Number of data points requested from sensor during most recent read
        byte _comm; // Tracks whether we are using I2C or Serial for communication


        private I2cDevice protractorI2C;
        private SerialDevice protractorSerial;

        /// <summary>
        /// Initializes the Protractor using I2C
        /// </summary>
        /// <param name="i2CController">The I2C Controller to use.</param>
        /// <param name="i2cAddress">The I2C Address</param>
        /// <param name="I2CFastMode">Enable FastMode(Not implemented yet)</param>
        public Protractor(I2cController i2CController, int i2cAddress, bool I2CFastMode)
        {
            var settings = new I2cConnectionSettings(i2cAddress);
            //if(I2CFastMode) settings.BusSpeed = I2cBusSpeed.FastMode;
            protractorI2C = i2CController.GetDevice(settings);
            _comm = I2CCOMM;
        }

        /// <summary>
        /// Initializes the Protractor using Serial
        /// </summary>
        /// <param name="comport">The comport name the Protractor is on</param>
        /// <param name="baudrate">The baudrate of the Protractor</param>
        public Protractor(string comport, int baudrate)
        {
            string deviceSelector = SerialDevice.GetDeviceSelector(comport);
            var dis = DeviceInformation.FindAllAsync(deviceSelector).GetResults();
            DeviceInformation entry = (DeviceInformation) dis[0];
            protractorSerial = SerialDevice.FromIdAsync(entry.Id).GetResults();
            if (protractorSerial == null) return;


            // Configure serial settings
            protractorSerial.WriteTimeout = TimeSpan.FromMilliseconds(1000);
            protractorSerial.ReadTimeout = TimeSpan.FromMilliseconds(1000);
            protractorSerial.BaudRate = (uint)baudrate;
            protractorSerial.Parity = SerialParity.None;
            protractorSerial.StopBits = SerialStopBitCount.One;
            protractorSerial.DataBits = 8;
            protractorSerial.Handshake = SerialHandshake.None;
            
            _comm = SERIALCOMM;
        }
        /// <summary>
        /// Reads all the objects from the Protractor
        /// </summary>
        /// <returns>Reading successful</returns>
        public bool Read()
        { // get all of the data from the protractor.
            return Read(MAXOBJECTS);
        }
        /// <summary>
        /// Reads a number of objects from the Protractor
        /// </summary>
        /// <param name="obs">The amound of objects to read</param>
        /// <returns>Reading successful</returns>
        public bool Read(byte obs)
        { // gets obs number of objects and obs number of paths from protractor. Returns the most visible objects and most open pathways. Minimizes data transfer for time sensitive applications.
            if (obs > MAXOBJECTS) obs = MAXOBJECTS;
            _numdata = obs;
            byte numBytes = (byte) (1 + obs * 4);
            _requestData(numBytes); // Request bytes from the obstacle sensor
            int i = 0;
            while (i < numBytes)
            {
                _buffer[i] = _read();
                i++;
            }
            if (i == 0)
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        /// <summary>
        /// Get the amount of objects in the buffer
        /// </summary>
        /// <returns>The amount of objects in the buffer</returns>
        public int ObjectCount()
        { // returns the number of objects detected
            return (int)(_buffer[0] >> 4); // number of objects detected is the high nibble of _buffer[0]
        }

        /// <summary>
        /// Get the amount of paths detected
        /// </summary>
        /// <returns>The amount of paths</returns>
        public int PathCount()
        { // returns the number of paths detected
            return (int)(_buffer[0] &= 0b00001111); // number of paths detected is the low nibble of _buffer[0]
        }

        /// <summary>
        /// Get the object angle of the first object
        /// </summary>
        /// <returns>Object angle in degrees</returns>
        public int ObjectAngle()
        { // returns the angle to the most visible object
            return ObjectAngle(0);
        }
        
        /// <summary>
        /// Get the object angle of a specific object
        /// </summary>
        /// <param name="ob">The object number of which you want to know the angle</param>
        /// <returns>Object angle in degrees</returns>
        public int ObjectAngle(int ob)
        { // returns the angle to the object ob in the object list, indexed from 1. Left most object is 1. If ob exceeds number of objects detected, return zero.
            if (ob >= ObjectCount() || ob < 0)
            {
                return -1;
            }
            else
            {
                int angle = map(_buffer[1 + 4 * ob], 0, 255, 0, 180);  // ob0->_buffer[1], ob1->_buffer[5], etc.
                return angle;
            }
        }

        /// <summary>
        /// Get the visibility of the most visible object
        /// </summary>
        /// <returns>idk what this returns exactly</returns>
        public int ObjectVisibility()
        { // returns the visibility of the most visible object
            return ObjectVisibility(0);
        }

        /// <summary>
        /// Get the visibility of a object
        /// </summary>
        /// <param name="ob">The object you want to see the visibility of</param>
        /// <returns>idk what this returns</returns>
        public int ObjectVisibility(int ob)
        { // returns the visibility of the object ob in the object list, indexed from 1. Left most object is 1. If ob exceeds number of objects detected, return zero.
            if (ob >= ObjectCount() || ob < 0)
            {
                return -1;
            }
            else
            {
                int vis = _buffer[2 + 4 * ob];  // ob0->_buffer[2], ob1->_buffer[6], etc.
                return vis;
            }
        }

        /// <summary>
        /// Get the angle of the most open pathway
        /// </summary>
        /// <returns>Path angle in degrees</returns>
        public int PathAngle()
        { // returns the angle to the most open pathway
            return PathAngle(0);
        }

        /// <summary>
        /// Get the angle of a specific path
        /// </summary>
        /// <param name="pa">The path you want to get the angle off</param>
        /// <returns>Path angle in degrees</returns>
        public int PathAngle(int pa)
        { // returns the angle to the path pa in the pathway list, indexed from 1. Left most path is 1. If pa exceeds number of pathways detected, return zero.
            if (pa >= PathCount() || pa < 0)
            {
                return -1;
            }
            else
            {
                int angle = map(_buffer[3 + 4 * pa], 0, 255, 0, 180);  // pa0->_buffer[3], pa1->_buffer[7], etc.
                return angle;
            }
        }

        /// <summary>
        /// Get the path visibility of the first path
        /// </summary>
        /// <returns>idk what this returns</returns>
        public int PathVisibility()
        {
            return PathVisibility(0);
        }

        /// <summary>
        /// Get the path visibility of a path
        /// </summary>
        /// <param name="pa">The path you want to know the visibility of</param>
        /// <returns>idk what this returns</returns>
        public int PathVisibility(int pa)
        { // returns the angle to the path pa in the pathway list, indexed from 1. Left most path is 1. If pa exceeds number of pathways detected, return zero.
            if (pa >= PathCount() || pa < 0)
            {
                return -1;
            }
            else
            {
                int vis = _buffer[4 + 4 * pa];  // pa0->_buffer[4], pa1->_buffer[8], etc.
                return vis;
            }
        }

        /////// SETTINGS ///////
        /// <summary>
        /// Set the scantime
        /// </summary>
        /// <param name="milliSeconds">Scan time in milliseconds. Min 15, Max 32767, default 30, 0 Scan only when called</param>
        public void ScanTime(int milliSeconds)
        {//0 = scan only when called. 1 to 15 = rescan every 15ms, >15 = rescan every time_ms milliseconds.  Default time_ms is set to 30ms.
            if (milliSeconds >= 1 && milliSeconds <= MINDUR - 1)
            {  // Values within 1 and 14 milliSeconds aren't allowed, the sensor requires a minimum 15 seconds to complete a scan.
                byte[] sendData = { SCANTIME, MINDUR, 0x10 };
                _write(sendData, 3); // Send a signal (char SCANTIME) to tell Protractor that it needs to change its time between scans to milliSeconds.
            }
            else if (milliSeconds >= 0 && milliSeconds <= 32767)
            {  // Values less than 0 or greater than 32767 aren't allowed.
                byte[] sendData = { SCANTIME, (byte)(milliSeconds & 0x00FF), (byte)(milliSeconds >> 8), 0x10 };
                _write(sendData, 4); // Send a signal (char SCANTIME) to tell Protractor that it needs to change its time between scans to milliSeconds. 
            }
        }


        /// <summary>
        /// Sets a new I2C Address
        /// </summary>
        /// <param name="newAddress">I2C address. Min 2, Max 127. This will be active after a reboot. See the manual on how to restore this</param>
        public void SetNewI2Caddress(int newAddress)
        { // change the I2C address. Will be stored after shutdown. See manual for instructions on restoring defaults. Default = 0x45 (69d).
            if (newAddress >= 2 && newAddress <= 127)
            {
                byte[] sendData = { I2CADDR, (byte)newAddress, 0x10 };
                _write(sendData, 3); // Send a signal (char I2CADDR) to tell Protractor that it needs to change its I2C address to the newAddress
            }
        }

        /// <summary>
        /// Sets a new Serial baudrate
        /// </summary>
        /// <param name="newBaudRate">Baudrate, Min 1200, Max 1000000. This will be active after a reboot. See the manual on how to restore this</param>
        public void SetNewSerialBaudRate(int newBaudRate)
        { // change the Serial Bus baud rate. Will be stored after shutdown. See manual for instructions on restoring defaults. Default = 9600 baud. 0 = 1200, 1 = 2400, 2 = 4800, 3 = 9600, 4 = 19200, 5 = 28800, 6 = 38400, 7 = 57600, 8 = 115200, 9 = 230400
            if (newBaudRate >= 1200 && newBaudRate <= 1000000)
            {
                byte[] sendData = { BAUDRATE, (byte)(newBaudRate & 0x00FF), (byte)(newBaudRate >> 8), (byte)(newBaudRate >> 16), 0x10 };
                _write(sendData, 5); // Send a signal (char BAUDRATE) to tell Protractor that it needs to change its baudrate to the newBaudRate
            }
        }

        /// <summary>
        /// Set the feedback leds to follow the most visible objects detected
        /// </summary>
        public void LEDshowObject()
        { // Set the feedback LEDs to follow the most visible Objects detected
            byte[] sendData = { LEDUSAGE, SHOWOBJ, 0x10 };
            _write(sendData, 3); // Send a signal (char LEDUSAGE) to tell Protractor that it needs to SHOWOBJ
        }

        /// <summary>
        /// Set the feedback LEDS to follow the most open pathway detected
        /// </summary>
        public void LEDshowPath()
        { // Set the feedback LEDs to follow the most open pathway detected
            byte[] sendData = { LEDUSAGE, SHOWPATH, 0x10 };
            _write(sendData, 3); // Send a signal (char LEDUSAGE) to tell Protractor that it needs to SHOWPATH
        }

        /// <summary>
        /// Turn the feedback LEDs off
        /// </summary>
        public void LEDoff()
        { // Turn off the feedback LEDs
            byte[] sendData = { LEDUSAGE, LEDOFF, 0x10 };
            _write(sendData, 3); // Send a signal (char LEDUSAGE) to tell Protractor that it needs to turn the feedback LEDOFF
        }

        // PRIVATES //
        
        private int map(int x, int in_min, int in_max, int out_min, int out_max)
        {
            return (x - in_min) * (out_max - out_min) / (in_max - in_min) + out_min;
        }

        private byte _read()
        {
            byte[] buff = new byte[1];
            if (_comm == I2CCOMM)
            {
                protractorI2C.Read(buff);
                return buff[0];
            }
            if (_comm == SERIALCOMM)
            {
                protractorSerial.InputStream.AsStreamForRead().Read(buff, 0, 1);
                return buff[0];
            }
            return 0;
        }
        

        private void _write(byte[] arrayBuffer, byte arrayLength)
        {
            if (_comm == I2CCOMM)
            {
                protractorI2C.Write(arrayBuffer);
            }
            else if (_comm == SERIALCOMM)
            {
                protractorSerial.OutputStream.AsStreamForWrite().Write(arrayBuffer, 0, arrayLength);
            }
        }

        private void _requestData(byte numBytes)
        {
            if (_comm == SERIALCOMM)
            {
                byte[] sendData = { REQUESTDATA, numBytes, 0x10 };
                _write(sendData, 3); // Send a signal (char SENDDATA) to tell Protractor that it needs to send data, tell it the number of data points to send.
            }
        }
    }
}
