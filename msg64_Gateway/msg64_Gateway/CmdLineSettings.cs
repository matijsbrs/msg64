using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Toolkits;
using static Toolkits.CommandLineParser;

namespace msg64_Gateway
{
    partial class Gateway
    {
        public class CmdLineSettings : Settings
        {
            public CommandLineParser.Option Device = new CommandLineParser.Option("--device,-d", msg64_Gateway.Properties.Settings.Default.ComPort, "Upload all channels", true);

            public CommandLineParser.Option Baudrate = new CommandLineParser.Option("--baudrate,-b", "115200", "Retransmission count", true);

            public CommandLineParser.Option Background = new CommandLineParser.Option("--background,-B", "", "Configure for background execution", false);
            public CommandLineParser.Option WinTerm = new CommandLineParser.Option("--winterm", "", "Use windows terminal options", false);
            public CommandLineParser.Option NoNotice = new CommandLineParser.Option("--nonotice", "", "Do not show notice messages", false);
            public CommandLineParser.Option NoWarming = new CommandLineParser.Option("--nowarning", "", "Do not show warning messages", false);
            public CommandLineParser.Option NoError = new CommandLineParser.Option("--noerror", "", "Do not show error messages", false);
            public CommandLineParser.Option NoCritical = new CommandLineParser.Option("--nocritical", "", "Do not show critical messages", false);



            public CommandLineParser.Option BrokerAddress = new CommandLineParser.Option("--host,-h",
                "localhost",
                "Address of the MQTT broker",
                true);
            public CommandLineParser.Option ServerTopic = new CommandLineParser.Option("--topic,-t",
                $"MSG64/",
                "MQTT topic path of the MSG64 Gateway",
                true);

            public CmdLineSettings()
            {
                // Set the program details
                ProgramDescription =
                    "Author: ing. M. Behrens. 2019,2020 \n" +
                    "MSG64 gateway application.\n" +
                    "Creates a bridge between a physical interface and a Mosquitto broker.";
                ProgramName = Assembly.GetExecutingAssembly().GetName().Name;
                ProgramVersion = Assembly.GetExecutingAssembly().GetName().Version.ToString();

                // Add all to the main option list.
                OptionList.Add(Device);
                OptionList.Add(Baudrate);
                OptionList.Add(Background);
                OptionList.Add(WinTerm);
                OptionList.Add(NoNotice);
                OptionList.Add(NoWarming);
                OptionList.Add(NoError);
                OptionList.Add(NoCritical);

                OptionList.Add(BrokerAddress);
                OptionList.Add(ServerTopic);

                AddStandardVersionOption(); // add default show version
                AddStandardHelpOption(); // add default help
                AddStandardShowConfigOption(); // add default showconfig
            }
        }
    }
}
