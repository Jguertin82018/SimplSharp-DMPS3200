using System;
using Crestron.SimplSharp;                          	// For Basic SIMPL# Classes
using Crestron.SimplSharpPro;                       	// For Basic SIMPL#Pro classes
using Crestron.SimplSharpPro.CrestronThread;        	// For Threading
using Crestron.SimplSharpPro.Diagnostics;		    	// For System Monitor Access
using Crestron.SimplSharpPro.DeviceSupport;         	// For Generic Device Support
using Crestron.SimplSharpPro.DM;


namespace DMPS3_200Test
{
    public class dmps3200General
    {
        // Fields
        private Dmps3SystemControl dmControl;

        // Properties
        public bool IsLocked { get { return dmControl.FrontPanelLockOnFeedback.BoolValue;}}
        public bool isPowered { get{return dmControl.SystemPowerOnFeedBack.BoolValue ;}}

        // Constructors
        public dmps3200General(CrestronControlSystem device)
        {
            try
            {
                device.DMSystemChange += new DMSystemEventHandler(device_DMSystemChange);
                dmControl = (Dmps3SystemControl)device.SystemControl;
                CrestronConsole.PrintLine("Dmps General Constructor and the lock status of the front panel is: " + dmControl.FrontPanelLockOnFeedback);
            }
            catch (Exception e)
            {
                CrestronConsole.PrintLine("DMPSRelay Constructor error is: " + e);
            }

        }




        // Delegates and Events
        public delegate void DMPS3FrontPanelEventHandler(bool status);
        public event DMPS3FrontPanelEventHandler DMPS3FrontpanelChange;

        public delegate void DMPSSystemPowerEventHandler(bool status);
        public event DMPSSystemPowerEventHandler DMPS3PowerEventChange;


        //Methods

        void device_DMSystemChange(Switch device, DMSystemEventArgs args)
        {
            switch (args.EventId)
            {

                case DMSystemEventIds.FrontPanelLockOnEventId:
                CrestronConsole.PrintLine("Dmps General DM System Change FrontpanelLockOn ID");
                DMPS3FrontpanelChange(IsLocked);
                break;
                case DMSystemEventIds.FrontPanelLockOffEventId:
                CrestronConsole.PrintLine("Dmps General DM System Change FrontpanelLockOff ID");
                DMPS3FrontpanelChange(IsLocked);
                break;
                case DMSystemEventIds.SystemPowerOnEventId:
                DMPS3PowerEventChange(true);
                break;
                case DMSystemEventIds.SystemPowerOffEventId:
                DMPS3PowerEventChange(false);
                break;
            }
        }

        public void SetFrontPanelLock(bool power)
        {
            if (power)
            {
                dmControl.FrontPanelLockOn();
            }
            else
            {
                dmControl.FrontPanelLockOff();
            }
        }

        public void SetDMPSPower(bool power)
        {
            if (power)
            {
                dmControl.SystemPowerOn();
            }
            else
            {
                dmControl.SystemPowerOff();
            }
        }

 
    }
}