using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Web.Script.Serialization;
using Toolkits;
using Toolkits.Networking.M2Mqtt;
using Toolkits.Networking.M2Mqtt.Messages;
using Toolkits.IO;
using Toolkits.Networking.IOT;
using System.Threading;
using Toolkits.Networking.IOT.BCom;


namespace msg64_Gateway
{
    partial class Gateway
    {
        CmdLineSettings Settings = new CmdLineSettings();

        private Serial _Serial;
        MqttClient client;

        /// <summary>
        /// Connector method for MQTT broker connection.
        /// </summary>
        public void ConnectToBroker(string BrokerAddress)
        {
            ConsoleEx.State(ConsoleEx.State_e.Start, "Connecting to Mosquitto broker.");
            client = new MqttClient(BrokerAddress);
            client.MqttMsgPublishReceived += MqttMessageReceivedEvent;

            string[] topics = { $"{Settings.ServerTopic}/tx" };
            byte[] qosLevels = { 0 };
            client.Subscribe(topics, qosLevels);
            // use a unique id as client id, each time we start the application
            try
            {
                client.Connect(Guid.NewGuid().ToString());
            }
            catch (Exception e)
            {

            }

            if (client.IsConnected)
                ConsoleEx.State(ConsoleEx.State_e.Ready, "");
            else
            {
                ConsoleEx.State(ConsoleEx.State_e.Error, $"Error connecting to the broker on: '{BrokerAddress}'");
                System.Environment.Exit(-1); // error
            }

        }


        /// <summary>
        /// Mqtt messages received event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e">mqtt payload</param>
        private void MqttMessageReceivedEvent(object sender, MqttMsgPublishEventArgs e)
        {
            try
            {
                //byte[] msgIn = Convert.FromBase64String(e.ToString().TrimEnd(new char[] { '\n', '\r' }).TrimStart(new char[] { '#' }));

                UInt32 Subscriber = (UInt32)(
                           ((e.Message[0] << 24))
                         | ((e.Message[1] << 16))
                         | ((e.Message[2] << 8))
                         | ((e.Message[3]))
                         );
                byte[] Payload = e.Message.GetRange(5, e.Message.Length - 5);

                if (!Settings.Background.Exist) ConsoleEx.Info($"[timestamp] [cyan]MQTT  -> MSG64:[reset] (#{Subscriber.ToString("X8")}) {Payload.ToHexString()}:{e.Message.ToBase64String()}:{System.Text.Encoding.UTF8.GetString(e.Message)}");

                _Serial.WriteLine($"#{System.Text.Encoding.UTF8.GetString(e.Message)}\n");

            }
            catch (Exception ex)
            {
                ConsoleEx.Error("Tx failed." + ex.ToString());
            }

        }
        /// <summary>
        /// Open connection to a serial port.
        /// </summary>
        /// <param name="Port"></param>
        /// <param name="BaudRate"></param>
        public void Connect(string Port, int BaudRate)
        {
            _Serial = new Serial(Port, BaudRate);
            _Serial.OnDataReceived += DataReceivedHandler;
            _Serial.Open();
        }


        private void DataReceivedHandler(object sender, SerialDataArgs e)
        {

            if (e.ToString().StartsWith("#"))
            {
                try
                {
                    byte[] msgIn = Convert.FromBase64String(e.ToString().TrimEnd(new char[] { '\n', '\r' }).TrimStart(new char[] { '#' }));

                    UInt32 Subscriber = (UInt32)(
                               ((msgIn[0] << 24))
                             | ((msgIn[1] << 16))
                             | ((msgIn[2] << 8))
                             | ((msgIn[3]))
                             );
                    byte[] Payload = msgIn.GetRange(5, msgIn.Length - 5);

                    if (!Settings.Background.Exist) ConsoleEx.Info($"[timestamp] [yellow]MQTT <-  MSG64:[reset] (#{Subscriber.ToString("X8")}) {Payload.ToHexString()}:{e.ToString().TrimEnd(new char[] { '\n', '\r' })}");

                    client.Publish($"{Settings.ServerTopic}/rx", msgIn);

                }
                catch (FormatException Fe)
                {
                    ConsoleEx.Error("[timestamp] Malformed BC Message, possible collision");
                }
            }
        }


        public Gateway(string[] args)
        {
            BCom bcom = new BCom();
            Settings.Parse(args);

            ConnectToBroker(Settings.BrokerAddress.ToString());
            Connect(Settings.Device.ToString(), int.Parse(Settings.Baudrate.ToString()));

            string version = Assembly.GetExecutingAssembly().GetName().Version.ToString();

            // ConsoleEx.State(ConsoleEx.State_e.Ready, $"{Assembly.GetExecutingAssembly().GetName().Name } v{version}");

            //var backgroundScheduler = TaskScheduler.Default;
            //Task.Factory.StartNew(delegate { Toggle(Bcs); }, backgroundScheduler);
            //.ContinueWith(delegate { CountDown(100); }, backgroundScheduler)
            //.ContinueWith(delegate { ShowText("Test"); }, backgroundScheduler)
            //.ContinueWith(delegate { ShowText("Ready"); }, backgroundScheduler);

            Task.Factory.StartNew(delegate { OnlineTest(100); }, TaskScheduler.Default).Wait();

        }

        /// <summary>
        /// Process a received BCom frame
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnBComDataReceived(object sender, DataArgs e)
        {
            e.frame.Dump();
            client.Publish($"{Settings.ServerTopic}/{e.frame.Source.ToString("X8")}/rx", e.frame.Export().ToByteArr());
        }

        private void OnlineTest(int interval)
        {
            while (client.IsConnected)
            {
                Thread.Sleep(interval);
            }
            ConsoleEx.Critical("Lost connection with MQTT broker. Closing application");
            // _Serial.Close();
            Environment.Exit(0); // error


        }

        static void Main(string[] args)
        {
            new Gateway(args);
        }


    }
}
