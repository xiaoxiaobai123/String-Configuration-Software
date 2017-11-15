using BurnerWinform;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Burner_AdvanTech
{
    class AdcantechCAnFIFO
    {
        //AdvCANIO Device = new AdvCANIO();

        public bool EnableProc;

        public const int REC_MSG_BUF_MAX = 0x2710;

        public AdvCan.canmsg_t[] gRecMsgBuf;
        public uint gRecMsgBufHead;
        public uint gRecMsgBufTail;

        public AdvCan.canmsg_t[] gRecMsgBuf2;
        public uint gRecMsgBufHead2;
        public uint gRecMsgBufTail2;

        public const int SEND_MSG_BUF_MAX = 0x2710;

        public AdvCan.canmsg_t[] gSendMsgBuf;
        public uint gSendMsgBufHead;
        public uint gSendMsgBufTail;

        public AdvCan.canmsg_t[] gSendMsgBuf2;
        public uint gSendMsgBufHead2;
        public uint gSendMsgBufTail2;

        public Timer _RecTimer;
        private Timer _SendTimer;

        public AutoResetEvent RecEvent;
        public TimerCallback RecTimerDelegate;
        private AutoResetEvent SendEvent;
        private TimerCallback SendTimerDelegate;



        public AdcantechCAnFIFO()
        {


            this.gSendMsgBuf = new AdvCan.canmsg_t[SEND_MSG_BUF_MAX];
            this.gSendMsgBufHead = 0;
            this.gSendMsgBufTail = 0;

            this.gSendMsgBuf2 = new AdvCan.canmsg_t[SEND_MSG_BUF_MAX];
            this.gSendMsgBufHead2 = 0;
            this.gSendMsgBufTail2 = 0;

            this.gRecMsgBuf = new AdvCan.canmsg_t[REC_MSG_BUF_MAX];
            this.gRecMsgBufHead = 0;
            this.gRecMsgBufTail = 0;

            this.gRecMsgBuf2 = new AdvCan.canmsg_t[REC_MSG_BUF_MAX];
            this.gRecMsgBufHead2 = 0;
            this.gRecMsgBufTail2 = 0;


            this.EnableProc = false;
            //this.RecEvent = new AutoResetEvent(false);
            //this.RecTimerDelegate = new TimerCallback(this.RecTimer_Tick);
        //    this._RecTimer = new Timer(this.RecTimerDelegate, this.RecEvent, 0, 5);
            this.SendEvent = new AutoResetEvent(false);
            this.SendTimerDelegate = new TimerCallback(this.SendTimer_Tick);
            this._SendTimer = new Timer(this.SendTimerDelegate, this.SendEvent, 0, 50);

        }




        private void ReadMessages()
        {
            AdvCan.canmsg_t[] mMsg = new AdvCan.canmsg_t[REC_MSG_BUF_MAX];
            Deviceinfo di = new Deviceinfo();
            int sCount = 0;
            uint pulNumberofRead = 0;
            int nRet = 0;
            do
            {               
                MainWindow.Device.acCanRead(mMsg, 100, ref pulNumberofRead);
                if (MainWindow.Device.acClearRxFifo() == 0)
                    ;
                else
                {
                    Console.WriteLine("shit can!!");
                }
                //if (pulNumberofRead == 0)
                //    return;
                if (nRet == AdvCANIO.TIME_OUT)
                {
                    Console.WriteLine("Package receiving timeout!");

                }
                else if (nRet == AdvCANIO.OPERATION_ERROR)
                {
                    Console.WriteLine("Package error!");
                }


                else
                {
                    for (int i = 0; i < pulNumberofRead; i++)
                    {
                        di.DeviceStruct.RawID = mMsg[0].id;
                        Console.WriteLine(di.DeviceStruct.StringID.ToString());
                        if (di.DeviceStruct.MessageID == 0x78 || di.DeviceStruct.MessageID == 0x79 || di.DeviceStruct.MessageID == 0x80)
                        {
                            this.gRecMsgBuf[this.gRecMsgBufHead].id = mMsg[i].id;
                            this.gRecMsgBuf[this.gRecMsgBufHead].length = mMsg[i].length;
                            this.gRecMsgBuf[this.gRecMsgBufHead].data = mMsg[i].data;
                            this.gRecMsgBuf[this.gRecMsgBufHead].flags = mMsg[i].flags;
                            this.gRecMsgBuf[this.gRecMsgBufHead].cob = mMsg[i].cob;
                            this.gRecMsgBufHead += 1;

                        }
                    }
                }
                if (this.gRecMsgBufHead >= REC_MSG_BUF_MAX)
                {
                    this.gRecMsgBufHead = 0;
                }
                sCount++;
            }
            while (sCount < 1);
        }



        private void ReadMessages2()
        {
             
        }


        private void SendMessages()
        {
            int ssCount = 10;
            int sCount = 0;
            uint pulNumberofWritten = 0;
            Deviceinfo di = new Deviceinfo();
            AdvCan.canmsg_t[] mMsg = new AdvCan.canmsg_t[500];
            uint mMsgc = 0;
            mMsg[0].flags = 4;
            uint i = 0;
            uint SendSSCount = 0;
            while(this.gSendMsgBufHead != this.gSendMsgBufTail )
            {
                mMsg[i] = this.gSendMsgBuf[this.gSendMsgBufTail];
                mMsg[i].data = this.gSendMsgBuf[this.gSendMsgBufTail].data;
                this.gSendMsgBufTail += 1;
                i++;
                if (this.gSendMsgBufTail >= SEND_MSG_BUF_MAX)
                {
                    this.gSendMsgBufTail = 0;
                }
                if (i >= 3)
                    break;
            }

            if(MainWindow.Device.acCanWrite(mMsg, i, ref pulNumberofWritten) != 0)//
             {
                                                                                    /*send error*/
                
            }
             else
            {
                Deviceinfo.devicecanstatus = true;
                 
            }
            //#region
            //do
            //{
            //    if (this.gSendMsgBufHead == this.gSendMsgBufTail)
            //        break;
            //    mMsgc = this.gSendMsgBufHead - this.gSendMsgBufTail;

            //    if(mMsgc < 100)
            //    {
            //        SendSSCount = mMsgc;
            //    }
            //    else
            //    {
            //        SendSSCount = 100;
            //    }
            //    for (uint i = 0; i < SendSSCount; i++)
            //    {
            //        mMsg[i] = this.gSendMsgBuf[this.gSendMsgBufTail];
            //        mMsg[i].data = this.gSendMsgBuf[this.gSendMsgBufTail].data;
            //        this.gSendMsgBufTail += 1;

            //        if (this.gSendMsgBufTail >= SEND_MSG_BUF_MAX)
            //        {
            //            this.gSendMsgBufTail = 0;
            //        }
            //    }
 
                
            //    while (ssCount > 0)
            //    {
                  
            //        if (MainWindow.Device.acCanWrite(mMsg, SendSSCount, ref pulNumberofWritten) != 0)//
            //        {                        
            //            ssCount--;                                                                     /*send error*/
            //            Thread.Sleep(50);
            //        }
            //        else
            //        {
            //            Deviceinfo.devicecanstatus = true;
            //            break;
            //        }
            //        if (ssCount == 0)
            //        { 
            //            Deviceinfo.devicecanstatus = false;
            //            break;
            //        }
            //    }
            //    sCount++;         
            //}
            //while (sCount < 1);
            //#endregion
        }
        private void SendMessages2()
        {
 
        }


        public void RecTime()
        {
            if (this.EnableProc)
            {
                this.ReadMessages();
                this.ReadMessages2();
            }
        }

        private void RecTimer_Tick(object mObject)
        {
            this.RecTime();
        }

        private void SendTimer_Tick(object mObject)
        {
            this.SendTime();
        }


        public void SendTime()
        {
            if (this.EnableProc)
            {
                this.SendMessages();
              //  this.SendMessages2();

            }
        }




        public void Close()
        {
        }

    }
}
