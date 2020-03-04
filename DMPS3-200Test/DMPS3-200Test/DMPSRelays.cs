using System;
using Crestron.SimplSharp;                          	// For Basic SIMPL# Classes
using Crestron.SimplSharpPro;                       	// For Basic SIMPL#Pro classes
using Crestron.SimplSharpPro.CrestronThread;        	// For Threading
using Crestron.SimplSharpPro.Diagnostics;		    	// For System Monitor Access
using Crestron.SimplSharpPro.DeviceSupport;         	// For Generic Device Support
using Crestron.SimplSharpPro.DM;
using System.Collections;
using System.Collections.Generic;

namespace DMPS3_200Test
{
    public class DMPSRelays
    {

        //Fields
        private List<Crestron.SimplSharpPro.Relay> relays = new List<Relay>();

        // Properties
        public bool getRelay1 { get { return Convert.ToBoolean(relays[0].State); } private set { Convert.ToBoolean(relays[0].State); } }
        public bool getRelay2 { get { return Convert.ToBoolean(relays[1].State); } private set { Convert.ToBoolean(relays[1].State); } }
        public bool getRelay3 { get { return Convert.ToBoolean(relays[2].State); } private set { Convert.ToBoolean(relays[2].State); } }
        public bool getRelay4 { get { return Convert.ToBoolean(relays[3].State); } private set { Convert.ToBoolean(relays[3].State); } }
        // try List or Array prop For Getting info?
        //public bool[] getRelays { get; set; }


        //Constructors
        public DMPSRelays(CrestronControlSystem device)
        {
            try
            {
                for (uint i = 1; i <= device.RelayPorts.Count; i++)
                {
                    relays.Add(device.RelayPorts[i]);
                    relays[(int)i-1] = device.RelayPorts[i];
                    relays[(int)i-1].Register();
                    relays[(int) i - 1].StateChange += new RelayEventHandler(DMPSRelays_StateChange);
                    CrestronConsole.PrintLine("\nDMPs Relay, Constructor Registering, regitsering Relay #{0} and registration status is: {1}", i, relays[(int)i - 1].Registered);
                }

            }
            catch (Exception e)
            {
                CrestronConsole.PrintLine("DMPSRelay Constructor error is: " + e);
            }

        }






        // Delegates and events
        public delegate void RelayChangeEventHandler(int relay, bool status);
        public event RelayChangeEventHandler RelayChangeEvent;


        void DMPSRelays_StateChange(Relay relay, RelayEventArgs args)
        {
            CrestronConsole.PrintLine("\nDMPSRelays Relay Change, and Relay {0} is:{1} ",relay.DeviceName, relay.State);
            if (relay.DeviceName.Contains("1"))
            {
                getRelay1 = relays[0].State;
                RelayChangeEvent(1, relays[0].State);
            }
            else if (relay.DeviceName.Contains("2"))
            {
                getRelay2 = relays[1].State;
                RelayChangeEvent(2, relays[1].State);
            }
            else if (relay.DeviceName.Contains("3"))
            {
                getRelay3 = relays[2].State;
                RelayChangeEvent(3, relays[2].State);
            }
            else
            {
                getRelay4 = relays[3].State;
                RelayChangeEvent(4, relays[3].State);
            }
        }

        // Methods

        public void DMPSRelayTrigger(int relay, bool state)
        {
            try
            {
                if (state)
                {
                    relays[relay - 1].Open();
                    CrestronConsole.PrintLine("\nDMPSRelays RelayTrigger, and Relay {0} is:{1} ",relay, relays[relay - 1].State);
                }
                else
                {
                    relays[relay - 1].Close();
                    CrestronConsole.PrintLine("\nDMPSRelays RelayTrigger, and Relay {0} is:{1} ", relay, relays[relay - 1].State);
                }
            }
            catch (Exception e)
            {
                CrestronConsole.PrintLine("DMPSRelays Set Relay method error is: " + e);
            }

        }


    }
}