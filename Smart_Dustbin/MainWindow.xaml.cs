using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.IO.Ports; // Add this using directive for serial communication
using System.Windows.Threading;
using System;

namespace Smart_Dustbin
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    /// 
    public partial class MainWindow : Window
    {
        private SerialPort arduinoPort;

        // Serial port for communication with Arduino

        public MainWindow()
        {
            InitializeComponent();
            PopulateComPorts(); // Call the method to populate COM ports in the ComboBox
            UpdateConnectButtonState(false); // Initialize the connect button state

        }
        private void ConnectButton_Click(object sender, RoutedEventArgs e)
        {
            ConnectToArduino(); // Call the method to connect to Arduino
        }
        private void DisconnectButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (arduinoPort != null && arduinoPort.IsOpen)
                {
                    arduinoPort.Close(); // Close the serial port
                    arduinoPort.Dispose(); // Dispose of the SerialPort object
                    MessageBox.Show("Disconnected from Arduino.", "Connection Status");
                    //clear the progress bar and status text
                    dustbinProgressBar.Value = 0; // Reset the ProgressBar value
                    dustbinStatusText.Text = "0%"; // Reset the status text
                    UpdateConnectButtonState(false); // Update the connect button state to reflect disconnection status
                }
                else
                {
                    MessageBox.Show("Arduino is not connected.", "Connection Status");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error disconnecting from Arduino: {ex.Message}", "Connection Error");
            }
        }
        // method to initialize and open the port
        private void ConnectToArduino()
        {
            try
            {
                // Replace "COMx" with your Arduino's actual COM port
                //arduinoPort = new SerialPort("COM6", 9600); // 9600 is a common baud rate, match your Arduino's serial.begin()
                //arduinoPort.DataReceived += new SerialDataReceivedEventHandler(ArduinoPort_DataReceived);
                //arduinoPort.Open();
                //MessageBox.Show("Connected to Arduino successfully!", "Connection Status");

                //get the selected COM port from the ComboBox
                string selectedPort = PortComboBox.SelectedItem as string;
                if (string.IsNullOrEmpty(selectedPort))
                {
                    MessageBox.Show("Please select a COM port.", "Connection Error");
                    return;
                }
                arduinoPort = new SerialPort(selectedPort, 9600); // Initialize the SerialPort with the selected COM port and baud rate
                arduinoPort.DataReceived += new SerialDataReceivedEventHandler(ArduinoPort_DataReceived); // Attach the DataReceived event handler
                arduinoPort.Open(); // Open the serial port for communication
                MessageBox.Show("Connected to Arduino successfully!", "Connection Status");
                UpdateConnectButtonState(true); // Update the connect button state to reflect connection status

            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error connecting to Arduino: {ex.Message}", "Connection Error");
            }
        }
        // Method to populate the ComboBox with available COM ports
        private void PopulateComPorts()
        {
            try
            {
                string[] ports = SerialPort.GetPortNames(); // Get available COM ports
                PortComboBox.ItemsSource = ports; // Set the ComboBox items source to the available ports
                if (ports.Length > 0)
                {
                    PortComboBox.SelectedIndex = 0; // Select the first port by default
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error retrieving COM ports: {ex.Message}", "Error");
            }
        }
        // Event handler for data received from Arduino
        private void ArduinoPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            try
            {
                string data = arduinoPort.ReadLine(); // Read a line of data (assuming Arduino sends newline after each value)
                if (int.TryParse(data.Trim(), out int level)) // Parse the data to an integer
                {
                    // Update UI on the UI thread using Dispatcher
                    Dispatcher.Invoke(() =>
                    {
                        UpdateDustbinUI(level); // Call a method to update your UI elements
                    });
                }
            }
            catch (Exception ex)
            {
                // Handle error (e.g., log it, show a message)
                Console.WriteLine($"Error reading serial data: {ex.Message}");
            }
        }
        // Method to update your dustbin visualization in XAML
        private void UpdateDustbinUI(int level)
        {
            // ProgressBar named 'dustbinProgressBar'
            // In XAML: <ProgressBar Name="dustbinProgressBar" Minimum="0" Maximum="100" />
            int clampedLevel = Math.Max(0, Math.Min(100, level)); // Ensure level is within 0-100
            dustbinProgressBar.Value = clampedLevel; // Update the ProgressBar value
            dustbinStatusText.Text = $"{clampedLevel}%";

            if(clampedLevel == 100)
            {
                messagee.Text = "Dustbin is full!\nPlease Empty \nthe bin";
                dustbinProgressBar.Foreground = new SolidColorBrush(Colors.Red); // Change color to red when full
            }
            else
            {
                dustbinProgressBar.Foreground = new SolidColorBrush(Colors.Green); // Change color to green when not full
                messagee.Text = " ";

            }

            //dustbinProgressBar.Value = level;
        }
        // In your MainWindow.xaml.cs
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (arduinoPort != null && arduinoPort.IsOpen)
            {
                arduinoPort.Close();
                arduinoPort.Dispose();
            }
        }

        private void UpdateConnectButtonState(bool isConnected)
        {
            ConnectButton.IsEnabled = !isConnected; // Disable the connect button if connected
        }
    }
}