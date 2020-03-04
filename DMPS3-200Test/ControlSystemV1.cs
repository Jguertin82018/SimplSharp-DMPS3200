using System;
using Crestron.SimplSharp;                          	// For Basic SIMPL# Classes
using Crestron.SimplSharpPro;                       	// For Basic SIMPL#Pro classes
using Crestron.SimplSharpPro.CrestronThread;        	// For Threading
using Crestron.SimplSharpPro.Diagnostics;		    	// For System Monitor Access
using Crestron.SimplSharpPro.DeviceSupport;         	// For Generic Device Support
using Crestron.SimplSharpPro.UI;
using Crestron.SimplSharpPro.DM;

namespace DMPS3_200Test
{
    public class ControlSystem : CrestronControlSystem
    {

        private Tsw760 userInterface;
        private dmpsAudioControls dmAudioControl;
        private dmpsAudioRouting dmAudioRouting;
        private dmps3200General dmGeneralControl;
        private DMPSIOS dmIo;
        private DMPSRelays dmRelays;
        private DmpsRMC dmRMC;
        private dmpsVideoInput dmVideoInputs;
        private dmpsVideoRouting dmVideoRouting;
        private DmpxTS dmTX;
        private Rs232 dm232;
        private TouchPanel uiLogic;
        private int? dmSource;
        private int dmRoute;
        

        public ControlSystem()
            : base()
        {
            try
            {
                Thread.MaxNumberOfUserThreads = 20;

                //Subscribe to the controller events (System, Program, and Ethernet)
                CrestronEnvironment.SystemEventHandler += new SystemEventHandler(ControlSystem_ControllerSystemEventHandler);
                CrestronEnvironment.ProgramStatusEventHandler += new ProgramStatusEventHandler(ControlSystem_ControllerProgramEventHandler);
                CrestronEnvironment.EthernetEventHandler += new EthernetEventHandler(ControlSystem_ControllerEthernetEventHandler);
            }
            catch (Exception e)
            {
                ErrorLog.Error("Error in the constructor: {0}", e.Message);
            }
        }


        public override void InitializeSystem()
        {
            try
            {

                if (SupportsSwitcherOutputs)
                {
                    try
                    {
                        dmVideoRouting = new dmpsVideoRouting(SwitcherInputs[Convert.ToUInt16(eDmps3200cInputs.Hdmi1)], SwitcherOutputs[Convert.ToUInt16(eDmps3200cOutputs.Dm)]);
                        DMOutputChange += new Crestron.SimplSharpPro.DM.DMOutputEventHandler(dmVideoRouting.DMOutputChange);
                        dmVideoRouting.DMVideoEvent += new dmpsVideoRouting.DMVideoChangeEventHandler(dmVideoRouting_DMVideoEvent);
                    }
                    catch (Exception e)
                    {
                        CrestronConsole.PrintLine("Error is SwitcherOutputs Constructor is: " + e);
                    }
                    


                }

                uiLogic = new TouchPanel();
                uiLogic.AnalogChangeEvent += new TouchPanel.AnalogChangeEventHandler(uiLogic_AnalogChangeEvent);
                uiLogic.DigitalChangeEvent += new TouchPanel.DigitalChangeEventHandler(uiLogic_DigitalChangeEvent);
                uiLogic.SerialChangeEvent += new TouchPanel.SerialChangeEventHandler(uiLogic_SerialChangeEvent);

                userInterface = new Tsw760(0x03, this);
                userInterface.SigChange += new SigEventHandler(uiLogic.userInterface_SigChange);
                userInterface.Register();
                CrestronConsole.PrintLine("Touchpanel Registration Feedback is: " + userInterface.Registered);

                dmGeneralControl = new dmps3200General(this);
                dmGeneralControl.DMPS3FrontpanelChange += new dmps3200General.DMPS3FrontPanelEventHandler(dmGeneralControl_DMPS3FrontpanelChange);
                dmGeneralControl.DMPS3PowerEventChange += new dmps3200General.DMPSSystemPowerEventHandler(dmGeneralControl_DMPS3PowerEventChange);

                dmRelays = new DMPSRelays(this);
                dmRelays.RelayChangeEvent += new DMPSRelays.RelayChangeEventHandler(dmRelays_RelayChangeEvent);

                dmIo = new DMPSIOS(this);
                dmIo.IOChangeEvent += new DMPSIOS.IOChangeEventHandler(dmIo_IOChangeEvent);
                
            }
            catch (Exception e)
            {
                ErrorLog.Error("Error in InitializeSystem: {0}", e.Message);
            }
        }





        #region DMVideo and DMAudio

        void dmVideoRouting_DMVideoEvent(Crestron.SimplSharpPro.DM.DMOutput output, Crestron.SimplSharpPro.DM.DMInput input)
        {
            if (input == null)
            {
                
            }
            else
            {

            }
        }

        #endregion




        #region Relays and IO's
        void dmRelays_RelayChangeEvent(int relay, bool status)
        {
            uiLogic.DMPSRelays((uint)relay, status, userInterface);
        }

        void dmIo_IOChangeEvent(int relay, bool status)
        {
            uiLogic.DMPSIO((uint)relay, status, userInterface);
        }
        #endregion


        #region Front Panel Events
        void dmGeneralControl_DMPS3PowerEventChange(bool status)
        {
            if (status)
            {
                uiLogic.SetFeedback(14, 15, 14, userInterface);
                
            }
            else
            {
                uiLogic.SetFeedback(14, 15, 15, userInterface);
            }
        }

        void dmGeneralControl_DMPS3FrontpanelChange(bool status)
        {
            if (status)
            {
                uiLogic.SetFeedback(16, 17, 16, userInterface);
            }
            else
            {
                uiLogic.SetFeedback(16, 17, 17, userInterface);
            }
        }
        #endregion

        #region TouchPanelLogic
        void uiLogic_SerialChangeEvent(uint deviceId, SigEventArgs args)
        {
            throw new NotImplementedException();
        }

        void uiLogic_DigitalChangeEvent(uint deviceId, SigEventArgs args)
        {
            #region DMPS Power Joins
            if (args.Sig.Number >= 14 && args.Sig.Number <= 15)
            {
                if (args.Sig.Number == 14)
                {
                    dmGeneralControl.SetDMPSPower(true);
                }
                else
                {
                    dmGeneralControl.SetDMPSPower(false);
                }
            }
            #endregion
            #region DMPS Lock JOins
            else if (args.Sig.Number >= 16 && args.Sig.Number <= 17)
            {
                if (args.Sig.Number == 16)
                {
                    dmGeneralControl.SetFrontPanelLock(true);
                }
                else if(args.Sig.Number == 17)
                {
                    dmGeneralControl.SetFrontPanelLock(false);
                }
            }
            else if (args.Sig.Number >= 6 && args.Sig.Number <= 9)
            {
                if (args.Sig.Number == 6)
                {
                    dmRelays.DMPSRelayTrigger((int)args.Sig.Number - 5, dmRelays.getRelay1);
                }
                else if (args.Sig.Number == 7)
                {
                    dmRelays.DMPSRelayTrigger((int)args.Sig.Number - 5, dmRelays.getRelay2);
                }
                else if (args.Sig.Number == 8)
                {
                    dmRelays.DMPSRelayTrigger((int)args.Sig.Number - 5, dmRelays.getRelay3);
                }
                else
                {
                    dmRelays.DMPSRelayTrigger((int)args.Sig.Number - 5, dmRelays.getRelay4);
                }
            }
            #endregion
            else if (args.Sig.Number == 20)
            {
                try
                {
                    var yesOrNo = dmGeneralControl.isPowered ? "Yes" : "No";
                    userInterface.StringInput[3].StringValue = yesOrNo;
                    CrestronConsole.PrintLine("ControlSystem TouchpanelRelayJoins Triggering Power Poll");
                }
                catch (Exception e)
                {
                    CrestronConsole.PrintLine("Join20 ControlSystem, the error is: " + e);
                }

            }
            #region DM Video
            else if (args.Sig.Number >= 18 && args.Sig.Number <= 24 || args.Sig.Number == 59 || args.Sig.Number == 61 || args.Sig.Number == 55  )
            {
                try
                {
                    if (args.Sig.Number >= 18 && args.Sig.Number <= 24)
                    {
                        dmSource = Convert.ToInt16(args.Sig.Number - 17);
                    }
                    else if (args.Sig.Number == 59 || args.Sig.Number == 61)
                    {
                        dmRoute = Convert.ToInt16(args.Sig.Number - 55);
                        if (dmGeneralControl.isPowered)
                        {
                            dmVideoRouting.DMVideoRoute(dmSource, dmRoute);
                        }
                        else
                        {
                            dmSource = 0;
                            dmRoute = 0;
                        }
                    }
                    else
                    {
                        dmSource = null;
                    }
                    
                }
                catch (Exception e)
                {
                    CrestronConsole.PrintLine("ControlSystem DMRoutingJoin Error Is: " + e);
                }
 
            }
            #endregion
 
        }

        void uiLogic_AnalogChangeEvent(uint deviceId, SigEventArgs args)
        {
            throw new NotImplementedException();
        }
        #endregion


        /// <summary>
        /// Event Handler for Ethernet events: Link Up and Link Down. 
        /// Use these events to close / re-open sockets, etc. 
        /// </summary>
        /// <param name="ethernetEventArgs">This parameter holds the values 
        /// such as whether it's a Link Up or Link Down event. It will also indicate 
        /// wich Ethernet adapter this event belongs to.
        /// </param>
        void ControlSystem_ControllerEthernetEventHandler(EthernetEventArgs ethernetEventArgs)
        {
            switch (ethernetEventArgs.EthernetEventType)
            {//Determine the event type Link Up or Link Down
                case (eEthernetEventType.LinkDown):
                    //Next need to determine which adapter the event is for. 
                    //LAN is the adapter is the port connected to external networks.
                    if (ethernetEventArgs.EthernetAdapter == EthernetAdapterType.EthernetLANAdapter)
                    {
                        //
                    }
                    break;
                case (eEthernetEventType.LinkUp):
                    if (ethernetEventArgs.EthernetAdapter == EthernetAdapterType.EthernetLANAdapter)
                    {

                    }
                    break;
            }
        }

        /// <summary>
        /// Event Handler for Programmatic events: Stop, Pause, Resume.
        /// Use this event to clean up when a program is stopping, pausing, and resuming.
        /// This event only applies to this SIMPL#Pro program, it doesn't receive events
        /// for other programs stopping
        /// </summary>
        /// <param name="programStatusEventType"></param>
        void ControlSystem_ControllerProgramEventHandler(eProgramStatusEventType programStatusEventType)
        {
            switch (programStatusEventType)
            {
                case (eProgramStatusEventType.Paused):
                    //The program has been paused.  Pause all user threads/timers as needed.
                    break;
                case (eProgramStatusEventType.Resumed):
                    //The program has been resumed. Resume all the user threads/timers as needed.
                    break;
                case (eProgramStatusEventType.Stopping):
                    //The program has been stopped.
                    //Close all threads. 
                    //Shutdown all Client/Servers in the system.
                    //General cleanup.
                    //Unsubscribe to all System Monitor events
                    break;
            }

        }

        /// <summary>
        /// Event Handler for system events, Disk Inserted/Ejected, and Reboot
        /// Use this event to clean up when someone types in reboot, or when your SD /USB
        /// removable media is ejected / re-inserted.
        /// </summary>
        /// <param name="systemEventType"></param>
        void ControlSystem_ControllerSystemEventHandler(eSystemEventType systemEventType)
        {
            switch (systemEventType)
            {
                case (eSystemEventType.DiskInserted):
                    //Removable media was detected on the system
                    break;
                case (eSystemEventType.DiskRemoved):
                    //Removable media was detached from the system
                    break;
                case (eSystemEventType.Rebooting):
                    //The system is rebooting. 
                    //Very limited time to preform clean up and save any settings to disk.
                    break;
            }

        }
    }
}