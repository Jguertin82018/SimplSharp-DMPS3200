using System;
using Crestron.SimplSharp;                          	// For Basic SIMPL# Classes
using Crestron.SimplSharpPro;                       	// For Basic SIMPL#Pro classes
using Crestron.SimplSharpPro.CrestronThread;        	// For Threading
using Crestron.SimplSharpPro.Diagnostics;		    	// For System Monitor Access
using Crestron.SimplSharpPro.DeviceSupport;         	// For Generic Device Support
using Crestron.SimplSharpPro.DM;
using System.Collections;
using System.Collections.Generic;
using Crestron.SimplSharpPro.GeneralIO;
using Crestron.SimplSharpPro.Keypads;

namespace DMPS3_200Test
{
    public class DMPSIOS
    {
         //Fields
        private List<DigitalInput> ioPorts = new List<DigitalInput>();
        
       

        // Properties
        private bool getIO1 { get { return Convert.ToBoolean(ioPorts[0].State); } set { Convert.ToBoolean(ioPorts[0].State); } }
        private bool getIO2 { get { return Convert.ToBoolean(ioPorts[1].State); } set { Convert.ToBoolean(ioPorts[1].State); } }
        private bool getIO3 { get { return Convert.ToBoolean(ioPorts[2].State); } set { Convert.ToBoolean(ioPorts[2].State); } }
        private bool getIO4 { get { return Convert.ToBoolean(ioPorts[3].State); } set { Convert.ToBoolean(ioPorts[3].State); } }
        public ArrayList GetIO = new ArrayList(); // Just Because I felt like using an Array List in case I ever need to Get or Poll Status

        //Constructors
        public DMPSIOS(CrestronControlSystem device)
        {
            try
            {
                for (uint i = 1; i <= device.DigitalInputPorts.Count; i++)
                {
                    
                    ioPorts.Add(device.DigitalInputPorts[i]);
                    ioPorts[(int)i - 1] = device.DigitalInputPorts[i];
                    ioPorts[(int)i - 1].Register();
                    ioPorts[(int)i - 1].StateChange += new DigitalInputEventHandler(DMPSIOS_StateChange);
                    CrestronConsole.PrintLine("\nDMPSIO, Constructor Registering, regitsering IO #{0} and registration status is: {1}", i, ioPorts[(int)i - 1].Registered);
                }
                
                GetIO.Add(getIO1);
                GetIO.Add(getIO2);
                GetIO.Add(getIO3);
                GetIO.Add(getIO4);
                 
            }
            catch (Exception e)
            {
                CrestronConsole.PrintLine("In DMPSIO COnstructor Error is: " + e);
            }
             

        }




        // Delegates and events
        public delegate void IOChangeEventHandler(int relay, bool status);
        public event IOChangeEventHandler IOChangeEvent;


        void DMPSIOS_StateChange(DigitalInput digitalInput, DigitalInputEventArgs args)
        {
            var ioNumber = Int32.Parse(digitalInput.DeviceName.Substring(digitalInput.DeviceName.Length - 1,1));
            CrestronConsole.PrintLine("DMPSIO DigitalStateChange IO is:{0} , and the status is {1}", digitalInput.DeviceName, digitalInput.State);
            IOChangeEvent(ioNumber, digitalInput.State);
        }


       // Methods

    }
}