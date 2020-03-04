using System;
using Crestron.SimplSharp;                          	// For Basic SIMPL# Classes
using Crestron.SimplSharpPro;                       	// For Basic SIMPL#Pro classes
using Crestron.SimplSharpPro.CrestronThread;        	// For Threading
using Crestron.SimplSharpPro.Diagnostics;		    	// For System Monitor Access
using Crestron.SimplSharpPro.DeviceSupport;         	// For Generic Device Support
using Crestron.SimplSharpPro.UI;
using Crestron.SimplSharpPro.DM;
using Crestron.SimplSharpPro.DM.Endpoints;
using Crestron.SimplSharpPro.DM.Endpoints.Transmitters;
using Crestron.SimplSharpPro.DM.Endpoints.Receivers;

namespace DMPS3_200Test
{
    public class ControlSystem : CrestronControlSystem
    {

        private Tsw760 userInterface;
        private dmpsAudioControls dmAudioControl;
        private dmps3200General dmGeneralControl;
        private DMPSIOS dmIo;
        private DMPSRelays dmRelays;
        private dmpsVideoRouting dmVideoRouting;
        private DmpxTS dmTX;
        private TouchPanel uiLogic;
        private int dmSource;
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
                        dmVideoRouting = new dmpsVideoRouting(SwitcherInputs, SwitcherOutputs, this.SystemControl as Dmps3SystemControl, SwitcherOutputs[(uint)eDmps3200cOutputs.Hdmi] as Crestron.SimplSharpPro.DM.Cards.Card.Dmps3HdmiOutput, SwitcherInputs[(uint)eDmps3200cInputs.Dm] as Crestron.SimplSharpPro.DM.Cards.Card.Dmps3DmInput);
                        DMOutputChange += new Crestron.SimplSharpPro.DM.DMOutputEventHandler(dmVideoRouting.DMOutputChange);
                        DMSystemChange += new DMSystemEventHandler(dmVideoRouting.DMSystemChange);
                        DMInputChange += new DMInputEventHandler(dmVideoRouting.DMInputChange);
                        dmVideoRouting.DMPSVideoEvent += new dmpsVideoRouting.DMPSVideoOutChangeEventhandler(dmVideoRouting_DMPSVideoEvent);
                        dmVideoRouting.DMPSVideoSyncChange += new dmpsVideoRouting.DMPSVideoSyncChangeEvent(dmVideoRouting_DMPSVideoSyncChange);
                        dmVideoRouting.DMPSDmInputChange += new dmpsVideoRouting.DMPSDmInputChangeEvent(dmVideoRouting_DMPSDmInputChange);
                        dmVideoRouting.DMPSHDMIChange += new dmpsVideoRouting.DMPSHDMiMuteChangeEvene(dmVideoRouting_DMPSHDMIChange);


                        dmAudioControl = new dmpsAudioControls(SwitcherOutputs[(uint)eDmps3200cOutputs.Program] as Crestron.SimplSharpPro.DM.Cards.Card.Dmps3ProgramOutput, SwitcherInputs);
                        DMOutputChange += new DMOutputEventHandler(dmAudioControl.DMOutputChange);
                        dmAudioControl.AudioLevelChange += new dmpsAudioControls.AudioLevelChangeEvent(dmAudioControl_AudioLevelChange);
                        dmAudioControl.AudioSourceChange += new dmpsAudioControls.AudioSourceChangeEvent(dmAudioControl_AudioSourceChange);
                        dmAudioControl.AmpMixerChange += new dmpsAudioControls.AmpMixerChangeEvent(dmAudioControl_AmpMixerChange);
                        dmAudioControl.AmpPowerChange += new dmpsAudioControls.AmpPowerChangeEvent(dmAudioControl_AmpPowerChange);

                        DmTx201C _dmTX201c = new DmTx201C(0x08,SwitcherInputs[(uint)eDmps3200cInputs.Dm] as DMInput);
                        dmTX = new DmpxTS(_dmTX201c);
                        dmTX.DmTXAudioChangeEvent += new DmpxTS.DmTxAudioChangeEventHandler(dmTX_DmTXAudioChangeEvent);
                        dmTX.DmTXVideoChangeEvent += new DmpxTS.DmTxVideoChangeEventHandler(dmTX_DmTXVideoChangeEvent);
                    }
                    catch (Exception e)
                    {
                        CrestronConsole.PrintLine("Error is SwitcherOutputs Constructor is: " + e);
                    }

                }

                uiLogic = new TouchPanel();
                uiLogic.AnalogChangeEvent += new TouchPanel.AnalogChangeEventHandler(uiLogic_AnalogChangeEvent);
                uiLogic.DigitalChangeEvent += new TouchPanel.DigitalChangeEventHandler(uiLogic_DigitalChangeEvent);

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

                dmVideoRouting.HDMIMute(true);
                
            }
            catch (Exception e)
            {
                ErrorLog.Error("Error in InitializeSystem: {0}", e.Message);
            }
        }




        #region DMTX
        void dmTX_DmTXVideoChangeEvent(DmTx200Base.eSourceSelection videoSource)
        {
            CrestronConsole.PrintLine("Control System DMBideo Change Event Number is: " + videoSource);
            uiLogic.SetFeedback(31, 34, (uint)videoSource + 31, userInterface);
        }

        void dmTX_DmTXAudioChangeEvent(DmTx200Base.eSourceSelection audioSource)
        {
            uiLogic.SetFeedback(35, 38, (uint)audioSource + 35, userInterface);
            CrestronConsole.PrintLine("Control System DMAudio Change Event Number is: " + audioSource);
        }
        #endregion


        #region DMVideo and DMAudio
        void dmVideoRouting_DMPSVideoEvent(uint output, uint input, string sourceName)
        {
            if (output == 1)
                userInterface.StringInput[4].StringValue = sourceName;
            else
                userInterface.StringInput[5].StringValue = sourceName;
            CrestronConsole.PrintLine("ControlSystem DMVideoChange Output is:{0} and input is{1} and name is {2}", output, input, sourceName);
        }
        void dmAudioControl_AudioSourceChange(uint source)
        {
            if (source > 0)
                uiLogic.SetFeedback(25, 30, source + 24, userInterface);
            else
                uiLogic.SetFeedback(25, 30, 0, userInterface);
        }
        void dmAudioControl_AudioLevelChange(ushort level, bool muteStatus)
        {
                int newMax = 65535;
                int newMin = 0;
                int oldMax = 100;
                int oldMin = -800;
                var newvalue = (level - oldMin) * (newMax - newMin) / (oldMax - oldMin);
                var knoblevel = Convert.ToUInt16(newvalue);
                if (muteStatus)
                    uiLogic.SetFeedback(47, 48, 47, userInterface);
                else
                    uiLogic.SetFeedback(47, 48, 48, userInterface);
                userInterface.UShortInput[1].UShortValue = knoblevel;

        }
        void dmAudioControl_AmpPowerChange(bool status)
        {
            if (status)
                uiLogic.SetFeedback(45, 46, 45, userInterface);
            else
                uiLogic.SetFeedback(45, 46, 46, userInterface);
        }

        void dmAudioControl_AmpMixerChange(bool status)
        {
            if (status)
                uiLogic.SetFeedback(43, 44, 44, userInterface);
            else
                uiLogic.SetFeedback(43, 44, 43, userInterface);
        }
        void dmVideoRouting_DMPSVideoSyncChange(bool status)
        {
            if (status)
                uiLogic.SetFeedback(41, 42, 41, userInterface);
            else
                uiLogic.SetFeedback(41, 42, 42, userInterface);
        }
        void dmVideoRouting_DMPSDmInputChange(bool status)
        {
            if (status)
                uiLogic.SetFeedback(39, 40, 39, userInterface);
            else
                uiLogic.SetFeedback(39, 40, 40, userInterface);
        }
        void dmVideoRouting_DMPSHDMIChange(bool status)
        {
            if (status)
                uiLogic.SetFeedback(52, 53, 52, userInterface);
            else
                uiLogic.SetFeedback(52, 53, 53, userInterface);
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
            #region PowerPoll
            else if (args.Sig.Number == 24)
            {

                    var yesOrNo = dmGeneralControl.isPowered ? "System is Powered" : "System Is Off";
                    userInterface.StringInput[3].StringValue = yesOrNo;
                    CrestronConsole.PrintLine("ControlSystem TouchpanelRelayJoins Triggering Power Poll");
            }
            #endregion
            #region DM Video
            else if (args.Sig.Number >= 18 && args.Sig.Number <= 23 || args.Sig.Number == 59 || args.Sig.Number == 60 || args.Sig.Number == 55  )
            {

                    if (args.Sig.Number >= 18 && args.Sig.Number <= 23)
                    {
                        dmSource = Convert.ToInt16(args.Sig.Number - 17);
                        uiLogic.SetFeedback(18, 24, args.Sig.Number, userInterface);
                    }
                    else if (args.Sig.Number == 59 || args.Sig.Number == 60)
                    {
                        dmRoute = Convert.ToInt16(args.Sig.Number - 58);
                        if (dmGeneralControl.isPowered)
                        {
                            dmVideoRouting.DMVideoRoute(dmSource, dmRoute);
                        }
                        else
                        {
                            dmGeneralControl.SetDMPSPower(true);
                            dmVideoRouting.DMVideoRoute(dmSource, dmRoute);
                            dmVideoRouting.DMVideoRoute(dmSource, dmRoute);
                        }
                    }
                    else
                    {
                        dmSource = 0;
                        uiLogic.SetFeedback(18, 24, 0, userInterface);
                    }
                    

 
            }
            #endregion
            #region DM Audio
            else if (args.Sig.Number >= 25 && args.Sig.Number <= 30 || args.Sig.Number == 56)
            {
                try
                {
                    if (args.Sig.Number >= 25 && args.Sig.Number <= 30)
                    {
                        dmAudioControl.AudioRoute((int)args.Sig.Number - 24);
                    }
                    else
                    {
                        dmAudioControl.AudioRoute(0);
                    }
                }
                catch (Exception e)
                {
                    CrestronConsole.PrintLine("ControlSystem AudioRoute Join error is: " + e);
                }

            }     
            #endregion 
            #region DMTXAudioandVideo

            else if (args.Sig.Number >= 31 && args.Sig.Number <= 34)
            {
                try
                {
                    dmTX.SetVideoRoute(args.Sig.Number - 31);
                    CrestronConsole.PrintLine("Sending video Route DMTX: " + (args.Sig.Number - 31));
                }
                catch (Exception e)
                {
                    CrestronConsole.PrintLine("ControlSystem DMTX Video Change Error is: " + e);
                }

            }
            else if (args.Sig.Number >= 35 && args.Sig.Number <= 38 )
            {
                try
                {
                    dmTX.SetAudioRoute(args.Sig.Number - 35);
                    CrestronConsole.PrintLine("Sending Audio Route DMTX " + (args.Sig.Number - 35));
                }
                catch (Exception e)
                {
                    CrestronConsole.PrintLine("Control System DmTX Audio Change Error is: " + e);
                }

            }
            
            #endregion 
#region DMPSProgramAudioControls
 
            else if (args.Sig.Number >= 43 && args.Sig.Number <= 44)
            {
                if (args.Sig.Number == 43)
                    dmAudioControl.MonoOrStereo(true);
                else
                    dmAudioControl.MonoOrStereo(false);
            }
            else if (args.Sig.Number >= 45 && args.Sig.Number <= 46)
            {
                if (args.Sig.Number == 45)
                    dmAudioControl.AmpPower(true);
                else
                    dmAudioControl.AmpPower(false);

            }
            else if (args.Sig.Number >= 47 && args.Sig.Number <= 48)
            {
                if (args.Sig.Number == 47)
                {
                    dmAudioControl.MuteProgram(true);
                }
                else if (args.Sig.Number == 48)
                {
                    dmAudioControl.MuteProgram(false);
                }
                   
            }



#endregion 
            #region HDMIMuteCOntrol
            else if (args.Sig.Number >= 52 && args.Sig.Number <= 53)
            {
                if (args.Sig.Number == 52)
                    dmVideoRouting.HDMIMute(true);
                else
                    dmVideoRouting.HDMIMute(false);
            }
            #endregion

        }

        void uiLogic_AnalogChangeEvent(uint deviceId, SigEventArgs args)
        {
                if (args.Sig.Number == 1)
                {
                    if (dmGeneralControl.isPowered)
                    {
                        if (dmAudioControl.AudioMuteStats)
                        {
                            dmAudioControl.MuteProgram(false);
                            dmAudioControl.VolumeChange(args.Sig.UShortValue);
                        }
                        else
                        {
                            dmAudioControl.VolumeChange(args.Sig.UShortValue);
                        }

                    }
                }

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