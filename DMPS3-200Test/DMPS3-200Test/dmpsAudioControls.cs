using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using Crestron.SimplSharpPro;
using Crestron.SimplSharpPro.DM;

namespace DMPS3_200Test
{
    public class dmpsAudioControls
    {
        //Fields
        private Crestron.SimplSharpPro.DM.Cards.Card.Dmps3ProgramOutput programOut;
        private Crestron.SimplSharpPro.CrestronCollection<ICardInputOutputType> switcherInputs;
        
        // Properties
        public bool AudioMuteStats { get { return programOut.MasterMuteOnFeedBack.BoolValue; } }
        public bool AmpPowerStatus { get { return programOut.AmpPowerOnFeedback.BoolValue; } }
        

        //Constructors
        public dmpsAudioControls(Crestron.SimplSharpPro.DM.Cards.Card.Dmps3ProgramOutput _programOut, Crestron.SimplSharpPro.CrestronCollection<ICardInputOutputType> _inputs)
        {
            try
            {
                programOut = _programOut;
                switcherInputs = _inputs;
                MonoOrStereo(true);

            }
            catch (Exception e)
            {
                CrestronConsole.PrintLine("Error is DMAudioCOnstructor is: " + e);
            }

        }

        // Delegates and events
        public delegate void AudioLevelChangeEvent(ushort level,bool muteStatus);
        public event AudioLevelChangeEvent AudioLevelChange;

        public delegate void AudioSourceChangeEvent(uint source);
        public event AudioSourceChangeEvent AudioSourceChange;

        public delegate void AmpPowerChangeEvent(bool status);
        public event AmpPowerChangeEvent AmpPowerChange;

        public delegate void AmpMixerChangeEvent(bool status);
        public event AmpMixerChangeEvent AmpMixerChange;



        public void DMOutputChange(Switch device, DMOutputEventArgs args)
        {
            try
            {
                switch (args.EventId)
                {
                    case DMOutputEventIds.AudioOutEventId:
                        DMOutput output = programOut;//[args.Number] as DMOutput;
                        if (output.AudioOutFeedback != null)
                        {
                            DMInput input = output.AudioOutFeedback as DMInput;
                            AudioSourceChange(input.Number);
                        }
                        else
                        {
                            AudioSourceChange(0);
                        }

                        break;
                    case DMOutputEventIds.MasterVolumeFeedBackEventId:
                        AudioLevelChange(programOut.MasterVolumeFeedBack.UShortValue, programOut.MasterMuteOnFeedBack.BoolValue);
                        break;
                    case DMOutputEventIds.MasterMuteOnFeedBackEventId:
                        AudioLevelChange(programOut.MasterVolumeFeedBack.UShortValue, programOut.MasterMuteOnFeedBack.BoolValue);
                        break;
                    case DMOutputEventIds.MasterMuteOffFeedBackEventId:
                        AudioLevelChange(programOut.MasterVolumeFeedBack.UShortValue, programOut.MasterMuteOnFeedBack.BoolValue);
                        break;
                    case DMOutputEventIds.AmpPowerOnFeedBackEventId:
                        AmpPowerChange(AmpPowerStatus);
                        break;
                    case DMOutputEventIds.AmpPowerOffFeedBackEventId:
                        AmpPowerChange(AmpPowerStatus);
                        break;
                    case DMOutputEventIds.MonoOutputFeedBackEventId:
                        AmpMixerChange(programOut.OutputMixer.MonoOutputFeedback.BoolValue);
                        break;
                    case DMOutputEventIds.StereoOutputFeedBackEventId:
                        AmpMixerChange(programOut.OutputMixer.MonoOutputFeedback.BoolValue);
                        break;

                }
            }
            catch (Exception e)
            {
                CrestronConsole.PrintLine("Error in DMAudioOutputChange is: " + e);
            }
        }

       // Methods
        public void AudioRoute(int inputNumber)
        {

                if (inputNumber == 0)
                {
                    programOut.AudioOut = null;
                }
                else
                {
                    programOut.AudioOut = switcherInputs[(uint)inputNumber] as DMInput;
                }
        }

        public void AmpPower(bool status)
        {
            if (status)
                programOut.AmpPowerOn();
            else
                programOut.AmpPowerOff();
        }

        public void VolumeChange(ushort value)
        {
            try
            {
                int oldRange = 65535;
                int newRange = 900;
                int oldMin = 0;
                int newMin = -800;
                var math = (value * newRange / oldRange) + (newMin - oldMin);
                short level = Convert.ToInt16(math);
                programOut.MasterVolume.ShortValue = level;
            }
            catch (Exception e)
            {
                CrestronConsole.PrintLine("CMPS Audio Ramp Error is: " + e);
            }

        }

        public void MuteProgram(bool status)
        {
            if (status)
                programOut.MasterMuteOn();
            else
                programOut.MasterMuteOff();

        }

        public void MonoOrStereo(bool stereo)
        {
            try
            {
                if (stereo)
                    programOut.OutputMixer.StereoOutput();
                else
                    programOut.OutputMixer.MonoOutput();
            }
            catch (Exception e)
            {
                CrestronConsole.PrintLine("DMPSAudio MonoStereo Erreo is : " + e);
            }

        }

        private int GetStartupAudio()
        {
            int level = Convert.ToInt16(programOut.MasterVolumeFeedBack); ;
            int newMax = 65535;
            int newMin = 0;
            int oldMax = 100;
            int oldMin = -800;
            var newvalue = (level - oldMin) * (newMax - newMin) / (oldMax - oldMin);
            return newvalue;
        }
    }
}