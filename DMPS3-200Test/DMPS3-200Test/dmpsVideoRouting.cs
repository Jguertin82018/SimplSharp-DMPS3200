using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using Crestron.SimplSharpPro;
using Crestron.SimplSharpPro.DM;

namespace DMPS3_200Test
{
    public class dmpsVideoRouting
    {

        //Fields
        private Crestron.SimplSharpPro.CrestronCollection<ICardInputOutputType> switcherInputs;
        private Crestron.SimplSharpPro.CrestronCollection<ICardInputOutputType> switcherOutputs;
        private Crestron.SimplSharpPro.DM.Cards.Card.Dmps3DmInput dmInput;
        private Crestron.SimplSharpPro.DM.Cards.Card.Dmps3HdmiOutput hdmiOutput;
        private Dmps3SystemControl _device;
        private string[] sourceName = new string[] { "HDMI1", "HDMI2", "HDMI3", "HDMI4", "HDMI5", "DM1" };


        // Properties

        //Constructors
        public dmpsVideoRouting(Crestron.SimplSharpPro.CrestronCollection<ICardInputOutputType> SwitcherInputs,
            Crestron.SimplSharpPro.CrestronCollection<ICardInputOutputType> SwitcherOutputs, Dmps3SystemControl Device,
           Crestron.SimplSharpPro.DM.Cards.Card.Dmps3HdmiOutput HDMIOutput,
            Crestron.SimplSharpPro.DM.Cards.Card.Dmps3DmInput DMInput)
        {
            try
            {
                switcherInputs = SwitcherInputs;
                switcherOutputs = SwitcherOutputs;
                _device = (Dmps3SystemControl)Device;
                _device.FrontPanelPassword.StringValue = "1111";
                 for (uint i = 1; i <= switcherInputs.Count; i++)
                {
                    DMInput test = switcherInputs[i] as DMInput;
                    test.Name.StringValue = sourceName[i - 1];
                }
                 hdmiOutput = HDMIOutput;
                 dmInput = DMInput;
                 hdmiOutput.HdmiOutputPort.OutputDisable();
                
            }
            catch (Exception e)
            {
                CrestronConsole.PrintLine("DMPsVideo Constructor error is: " + e);
            }

        }



        // Delegates and events
        public delegate void DMPSVideoOutChangeEventhandler(uint output, uint input, string sourceName);
        public event DMPSVideoOutChangeEventhandler DMPSVideoEvent;

        public delegate void DMPSDmInputChangeEvent(bool status);
        public event DMPSDmInputChangeEvent DMPSDmInputChange;

        public delegate void DMPSVideoSyncChangeEvent(bool status);
        public event DMPSVideoSyncChangeEvent DMPSVideoSyncChange;

        public delegate void DMPSHDMiMuteChangeEvene(bool status);
        public event DMPSHDMiMuteChangeEvene DMPSHDMIChange;

        public void DMSystemChange(Switch device, DMSystemEventArgs args)
        {
            
        }

        public void DMOutputChange(Switch device, DMOutputEventArgs args)
        {
            try
            {
                switch (args.EventId)
                {
                    case DMOutputEventIds.VideoOutEventId:
                        DMOutput output = switcherOutputs[args.Number] as DMOutput;
                        if (output.VideoOutFeedback != null)
                        {
                            DMInput input = output.VideoOutFeedback as DMInput;
                            DMPSVideoEvent(Convert.ToUInt16(output.Number), Convert.ToUInt16(input.Number),input.Name.StringValue);
                        }
                        else
                        {
                           DMPSVideoEvent(Convert.ToUInt16(output.Number), 0,"");
                        }
                        break;
                    case DMOutputEventIds.OutputDisabledEventId:
                        CrestronConsole.PrintLine("HDMI1 Output Enabled Status is: " + hdmiOutput.HdmiOutputPort.OutputEnabledFeedback.BoolValue);
                        DMPSHDMIChange(hdmiOutput.HdmiOutputPort.OutputEnabledFeedback.BoolValue);
                        break;
                }
            }
            catch (Exception e)
            {
                CrestronConsole.PrintLine("Error in DMOUtputChange is: " + e);
            }

        }

        public void DMInputChange(Switch device, DMInputEventArgs args)
        {
            switch (args.EventId)
            {
                case DMInputEventIds.SourceSyncEventId:
                    try
                    {
                        
                        DMPSDmInputChange(dmInput.DmInputPort.SyncDetectedFeedback.BoolValue);

                    }
                    catch (Exception e)
                    {
                        CrestronConsole.PrintLine("Error in Source Sync Feedback Event is: " + e);
                    }

                    break;
                case DMInputEventIds.VideoDetectedEventId:
                    DMInput input2 = switcherInputs[args.Number] as DMInput;
                    DMPSVideoSyncChange(input2.VideoDetectedFeedback.BoolValue);
                    break;
            }
        }


       // Methods
        public void DMVideoRoute(int inputNumber, int outputNumber)
        {
            try
            {
                DMOutput output = switcherOutputs[(uint)outputNumber] as DMOutput;
                if (inputNumber == 0)
                {
                    output.VideoOut = null;
                }
                else
                {
                    output.VideoOut = switcherInputs[(uint)inputNumber] as DMInput;
                }
            }
            catch (Exception e)
            {
                CrestronConsole.PrintLine("DmPSVideoRouting DMVideoRoute Error is: " + e);
            }
        }

        public void HDMIMute(bool status)
        {
            try
            {
                if (status)
                {
                    hdmiOutput.HdmiOutputPort.OutputEnable();
                }
                else
                {
                    hdmiOutput.HdmiOutputPort.OutputDisable();
                }
            }
            catch (Exception e)
            {
                CrestronConsole.PrintLine("DMPSVideo HDMIMute Error is: " + e);
            }


                
        }
    }
}