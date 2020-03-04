using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using Crestron.SimplSharpPro;
using Crestron.SimplSharpPro.DM;
using Crestron.SimplSharpPro.DM.Endpoints;
using Crestron.SimplSharpPro.DM.Endpoints.Transmitters;

namespace DMPS3_200Test
{
    public class DmpxTS
    {
       

        //Fields
        private DmTx201C dmTx201C;

        // Properties


        //Constructors
        public DmpxTS(DmTx201C DmTX201C)
        {
            try
            {
                dmTx201C = DmTX201C;
                dmTx201C.Register();
                dmTx201C.BaseEvent += new BaseEventHandler(dmTx201C_BaseEvent);
                CrestronConsole.PrintLine("DM TX Registration Status is" + dmTx201C.Registered);
                
            }
            catch (Exception e)
            {
                CrestronConsole.PrintLine("DMTX201 Registration error is: " + e);
            }

        }



        // Delegates and events
        public delegate void DmTxVideoChangeEventHandler(DmTx200Base.eSourceSelection videoSource);
        public event DmTxVideoChangeEventHandler DmTXVideoChangeEvent;

        public delegate void DmTxAudioChangeEventHandler(DmTx200Base.eSourceSelection audioSource);
        public event DmTxAudioChangeEventHandler DmTXAudioChangeEvent;

        void dmTx201C_BaseEvent(GenericBase device, BaseEventArgs args)
        {
            try
            {
                switch (args.EventId)
                {
                    case DmTx200Base.AudioSourceFeedbackEventId:
                        DmTXAudioChangeEvent(dmTx201C.AudioSourceFeedback);
                        break;
                    case DmTx200Base.VideoSourceFeedbackEventId:
                        DmTXVideoChangeEvent(dmTx201C.VideoSourceFeedback);
                        break;
                }
            }
            catch (Exception e)
            {
                CrestronConsole.PrintLine("DMPStx201Change Event Error is: " + e);
            }

        }

        // Methods

        public void SetAudioRoute(uint input)
        {
            try
            {
                DmTx200Base.eSourceSelection test = (DmTx200Base.eSourceSelection)input;
                dmTx201C.AudioSource = test;
                CrestronConsole.PrintLine("DMTXSetAudio Routing is: " + input);
            }
            catch (Exception e)
            {
                CrestronConsole.PrintLine("DMTXSetAudio Error IS: " + e);
            }

        }

        public void SetVideoRoute(uint input)
        {
            try
            {
                DmTx200Base.eSourceSelection test = (DmTx200Base.eSourceSelection)input;
                dmTx201C.VideoSource = test;
                CrestronConsole.PrintLine("DMTXSetVideo Routing is: " + input);
            }
            catch (Exception e)
            {
                CrestronConsole.PrintLine("DMTXSetVideo Error IS: " + e);
            }
            
        }

    }
}