// Copyright (c) Microsoft. All rights reserved.

using System;
using Windows.Devices.Gpio;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Media;

namespace DashWall
{
    public sealed partial class MainPage : Page
    {
        private const int CLOCK_PIN = 12;
        private const int TEMP_PIN = 34;
        private const int PHOTO_PIN = 33;
        private const int START_PIN = 36;
        private const int WATER_PIN = 69;

        private GpioPin clockPin;
        private GpioPinValue clockPinValue;
        private GpioPin tempPin;
        private GpioPinValue tempPinValue;
        private GpioPin photoPin;
        private GpioPinValue photoPinValue;
        private GpioPin startPin;
        private GpioPinValue startPinValue;
        private GpioPin waterPin;
        private GpioPinValue waterPinValue;

        private DispatcherTimer timer;
        private DispatcherTimer apiTimer;

        private GpioController gpio;

        private Brush red = new SolidColorBrush(Windows.UI.Colors.Red);
        private Brush white = new SolidColorBrush(Windows.UI.Colors.White);

        public MainPage()
        {
            InitializeComponent();

            apiTimer = new DispatcherTimer();
            apiTimer.Interval = TimeSpan.FromSeconds(10);
            apiTimer.Tick += ApiTimer_Tick;

            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromMilliseconds(10);
            timer.Tick += Timer_Tick;

            InitGpio();
            timer.Start();
            apiTimer.Start();
        }

        private void InitGpio()
        {
            gpio = GpioController.GetDefault();

            clockPin = gpio.OpenPin(CLOCK_PIN);
            clockPin.SetDriveMode(GpioPinDriveMode.Input);

            tempPin = gpio.OpenPin(TEMP_PIN);
            tempPin.SetDriveMode(GpioPinDriveMode.Input);

            photoPin = gpio.OpenPin(PHOTO_PIN);
            photoPin.SetDriveMode(GpioPinDriveMode.Input);

            waterPin = gpio.OpenPin(WATER_PIN);
            waterPin.SetDriveMode(GpioPinDriveMode.Input);

            startPin = gpio.OpenPin(START_PIN);
            startPin.SetDriveMode(GpioPinDriveMode.Input);
        }

        bool isReading = false;
        bool bitRead = false;
        uint curPhoto;
        uint curTemp;
        uint curWater;
        uint bitCount = 0;
        bool start;
        bool clock;
        bool photo;
        bool temp;
        bool water;
        private void Timer_Tick(object sender, object e)
        {
            handleInput();

            if (start && clock && !isReading)
            {
                isReading = true;
                curPhoto = 0;
                curTemp = 0;
                curWater = 0;
            }

            if (isReading)
            {
                if (clock && !bitRead)
                {
                    ReadBit();
                    bitRead = true;
                }
                if (!clock)
                {
                    bitRead = false;
                }
            }
        }

        private void ReadBit()
        {
            uint photoBit = 0;
            uint tempBit = 0;
            uint waterBit = 0;
            if (photo) { photoBit = 1; }
            if (temp) { tempBit = 1; }
            if (water) { waterBit = 1; }

            curPhoto = curPhoto << 1;
            curPhoto = curPhoto + photoBit;

            curTemp = curTemp << 1;
            curTemp = curTemp + tempBit;

            curWater = curWater << 1;
            curWater = curWater + waterBit;

            bitCount++;

            if (bitCount >= 8)
            {
                isReading = false;
                bitCount = 0;

                
                if (curWater > 15)
                {
                    Time.Text = "FLOOD WARNING";
                    Time.FontSize = 150;
                    Time.Foreground = red;
                }
                else
                {
                    Time.Text = DateTime.Now.ToString("h:mm tt");
                    Time.FontSize = 250;
                    Time.Foreground = white;
                }

                Temp.Text = "" + (int)((255.0 - curTemp) * .166 + 60.0) + "°";

                Fade.Opacity = .4 - (255.0 - curPhoto) / 255.0;
            }
        }

        private void handleInput()
        {
            clockPinValue = clockPin.Read();
            clock = clockPinValue == GpioPinValue.High;

            startPinValue = startPin.Read();
            start = startPinValue == GpioPinValue.High;

            photoPinValue = photoPin.Read();
            photo = photoPinValue == GpioPinValue.High;

            tempPinValue = tempPin.Read();
            temp = tempPinValue == GpioPinValue.High;

            waterPinValue = waterPin.Read();
            water = waterPinValue == GpioPinValue.High;
        }


        private void ApiTimer_Tick(object sender, object e)
        {

        }

    }
}
