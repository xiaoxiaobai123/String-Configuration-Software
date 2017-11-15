using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.ComponentModel;
using ECAN;

using System.Threading;
using BurnerWinform;
using System.Collections;
using Microsoft.Win32;
using System.IO;
using System.Windows.Threading;
using System.Text.RegularExpressions;

namespace Burner_AdvanTech
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        AdcantechCAnFIFO mCan = new AdcantechCAnFIFO();
        static object locker = new object();
        Thread Burn;
        Thread GReadVersion;
        List<VolTemp>[] ListVolTemp = new List<VolTemp>[Deviceinfo.BPNumberValue];

        List<VolTemp> ListVTvar = new List<VolTemp>();
        bool ReadMsgThreadFlag = true;
        bool QueryOnlineStringFlag = false;
        bool QueryThreadExitFlag = false;
        Queue myOnStringQ = new Queue();

        BinaryReader br = null;
        TimeCount CanTimer = new TimeCount();
        public static AdvCANIO Device ;

        AdvCan.canmsg_t[] msgReadTemp = new AdvCan.canmsg_t[Deviceinfo.MaxMsgCount];
        AdvCan.canmsg_t[] msgR = new AdvCan.canmsg_t[Deviceinfo.MaxMsgCount];


        List<StringFirmwareInfor> stringFirList = new List<StringFirmwareInfor>();

        List<StringFirmwareInfor> bpFirList = new List<StringFirmwareInfor>();


        Uptext UpT = new Uptext();

        bool broadcastburningstatus = false;

        List<stringInformation> ListStringInfor = new List<stringInformation>();
        System.Windows.Threading.DispatcherTimer SendQueryMsgTimer;

        uint stringnumber_selected;
        uint bpnumber_selected;

        int PageSelected = 0;
        public MainWindow()
        {
            InitializeComponent();
            Device = new AdvCANIO();
            mCan.EnableProc = true;
            
            InitStriList();

            InitilaFunc();
           

            AdvantechCANStart();
            Thread.Sleep(500);         
            QueryOnLineDevice(0x00, 0x00, (byte)UpgradeDataField.StringTypeMsg);
            GReadVersion = new Thread(ReadStringVersion);
            GReadVersion.Start();
            CanTimer.TimeStart = DateTime.Now;
            QueryOnlineStringFlag = true;
            Thread RdMsg = new Thread(ReadMessages);
            RdMsg.Start();

            System.Windows.Threading.DispatcherTimer dispatcherTimer = new System.Windows.Threading.DispatcherTimer();
            dispatcherTimer.Tick += new EventHandler(dispatcherTimer_Tick);
            dispatcherTimer.Interval = new TimeSpan(0, 0, 0,0,20);
            dispatcherTimer.Start();

            SendQueryMsgTimer = new DispatcherTimer();
            SendQueryMsgTimer.Tick += new EventHandler(SendQueryMsgTimer_Tick);
            SendQueryMsgTimer.Interval = new TimeSpan(0, 0, 0, 1);

             
            SCNumberselect.Loaded += (s, args) =>
            {
                SCNumberselect.SelectionChanged +=
                        new SelectionChangedEventHandler(SCNumberselect_SelectionChanged);
            };

            BpNumberselect.Loaded += (s, args) =>
            {
                SCNumberselect.SelectionChanged +=
                        new SelectionChangedEventHandler(BpNumberselect_SelectionChanged);
            };

            PageSelected = tabControl.SelectedIndex;
        }

        private void InitilaFunc()
        {

            for (int i = 0; i < Deviceinfo.BPNumberValue; i++)
            {
                ListVolTemp[i] = new List<VolTemp>();
                for (int j = 0;j < Deviceinfo.CellNumberValue; j++)
                {
                    ListVolTemp[i].Add(new VolTemp { CellNumber = (j + 1).ToString(), Vol = "NotUpdate", Temp = "NotUpdate", BalState = false });
                }
            }
            CellinfodataGridView.ItemsSource = ListVolTemp[0];

            for (int i = 0; i < Deviceinfo.StringNumberValue; i++)
            {
                ListStringInfor.Add(new stringInformation
                {
                    stringID = i + 1,
                    stringvoltage = "NotUpdate",
                    dcbusvoltage = "NotUpdate",
                    stringcurrent = "NotUpdate",
                    targetvalue = "NotUpdate",
                    kwh = "NotUpdate",
                    kw = "NotUpdate",
                    contactorclosedpermissionstatus = "NotUpdate",
                    pcontactorsattus = "NotUpdate",
                    ncontactorstatus = "NotUpdate",
                    highvoltage = "NotUpdate",
                    avevoltage = "NotUpdate",
                    lowvoltage = "NotUpdate",
                    hightemp = "NotUpdate",
                    avetemp = "NotUpdate",
                    lowtemp = "NotUpdate",
                });
            }

            this.DataContext = ListStringInfor;
        }
        private void dispatcherTimer_Tick(object sender, EventArgs e)
        {
            HinttextBox.Text = UpT.TextContent;
        }
        public void InitStriList()
        {
            stringFirList.Clear();
            for (int i = 0; i < Deviceinfo.StringNumberValue; i++)
            {
                stringFirList.Add(new StringFirmwareInfor { NodeAddress = "String " + (i + 1).ToString(), FirmwareVersion = "NotUpdate", OnlineStatus = false, BurningProgress = 0, FirmwareType = "NotUpdate" });
            }
            dataGridFirmwareVersion.ItemsSource = null;
            dataGridFirmwareVersion.ItemsSource = stringFirList;


        }

        public void InitBpStrList()
        {
            bpFirList.Clear();
            for (int i = 0; i < Deviceinfo.BPNumberValue; i++)
            {
                bpFirList.Add(new StringFirmwareInfor { NodeAddress = "Bp " + (i + 1).ToString(), FirmwareVersion = "NotUpdate", OnlineStatus = false, BurningProgress = 0, FirmwareType = "NotUpdate" });
            }
            dataGridFirmwareVersion.ItemsSource = null;
            dataGridFirmwareVersion.ItemsSource = bpFirList;
        }
        private void SendMessageTest()
        {
            int ssCount = 10;
            int sCount = 0;
            uint pulNumberofWritten = 0;
            Deviceinfo di = new Deviceinfo();
            AdvCan.canmsg_t[] mMsg = new AdvCan.canmsg_t[100];
            uint mMsgc = 0;
            mMsg[0].flags = 4;
            uint SendSSCount = 0;
            while (true)
            {
                Thread.Sleep(5);
                do
                {
                    if(mCan.gSendMsgBufHead == mCan.gSendMsgBufTail)
                        break;
                    mMsgc = mCan.gSendMsgBufHead - mCan.gSendMsgBufTail;

                    if (mMsgc < 100)
                    {
                        SendSSCount = mMsgc;
                    }
                    else
                    {
                        SendSSCount = 100;
                    }
                    for (uint i = 0; i < SendSSCount; i++)
                    {
                        mMsg[i] = mCan.gSendMsgBuf[mCan.gSendMsgBufTail];
                        mMsg[i].data = mCan.gSendMsgBuf[mCan.gSendMsgBufTail].data;
                        mCan.gSendMsgBufTail += 1;
                        if (mCan.gSendMsgBufTail >= 10000)
                        {
                            mCan.gSendMsgBufTail = 0;
                        }
                    }
                    while (ssCount > 0)
                    {
                        if (MainWindow.Device.acCanWrite(mMsg, SendSSCount, ref pulNumberofWritten) != 0)//
                        {
                            ssCount--;                                                                     /*send error*/
                            Thread.Sleep(50);
                        }
                        else
                        {
                            Deviceinfo.devicecanstatus = true;
                            break;
                        }
                        if (ssCount == 0)
                        {
                            Deviceinfo.devicecanstatus = false;
                            break;
                        }
                    }
                    sCount++;
                }
                while (sCount < 1);
            }
        }
        private void ReadMessages()
        {
            while (ReadMsgThreadFlag)
            {

               
                AdvCan.canmsg_t[] mMsg = new AdvCan.canmsg_t[100];
                Deviceinfo di = new Deviceinfo();
                int sCount = 0;
                uint pulNumberofRead = 0;
                int nRet = 0;
                //       do
                //         {
                nRet = Device.acCanRead(mMsg, 1, ref pulNumberofRead);
                //if (pulNumberofRead == 0)
                //    return;
                //if (Device.acClearRxFifo() == 0)
                //    ;
                //else
                //{
                //    Console.WriteLine("shit can!!");
                //}
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
                        if (PageSelected == 0)
                        {
                            if (di.DeviceStruct.MessageID == 0x78 || di.DeviceStruct.MessageID == 0x79 || di.DeviceStruct.MessageID == 0x80)
                            {
                                mCan.gRecMsgBuf[mCan.gRecMsgBufHead].id = mMsg[i].id;
                                mCan.gRecMsgBuf[mCan.gRecMsgBufHead].length = mMsg[i].length;
                                mCan.gRecMsgBuf[mCan.gRecMsgBufHead].data = mMsg[i].data;
                                mCan.gRecMsgBuf[mCan.gRecMsgBufHead].flags = mMsg[i].flags;
                                mCan.gRecMsgBuf[mCan.gRecMsgBufHead].cob = mMsg[i].cob;
                                mCan.gRecMsgBufHead += 1;
                            }
                        }
                        else
                        {
                            mCan.gRecMsgBuf[mCan.gRecMsgBufHead].id = mMsg[i].id;
                            mCan.gRecMsgBuf[mCan.gRecMsgBufHead].length = mMsg[i].length;
                            mCan.gRecMsgBuf[mCan.gRecMsgBufHead].data = mMsg[i].data;
                            mCan.gRecMsgBuf[mCan.gRecMsgBufHead].flags = mMsg[i].flags;
                            mCan.gRecMsgBuf[mCan.gRecMsgBufHead].cob = mMsg[i].cob;
                            mCan.gRecMsgBufHead += 1;
                        }

                    }
                }
                if (mCan.gRecMsgBufHead >= 1000)
                {
                    mCan.gRecMsgBufHead = 0;
                }
            }
              //  sCount++;
         //   }
         //   while (sCount < 1);
        }
      


        private void AdvantechCANStart()
        {
 
            /*Open usbcan port*/

            string CanPortName;
            uint BaudRateValue = 0;
            UInt32 ReadTimeOutValue = 0;
            UInt32 WriteTimeOutValue = 0;
            int nRet = 0;
            uint dwMaskCode = 0;
            uint dwAccCode = 0;

            AdvCan.CanStatusPar_t CanStatus = new AdvCan.CanStatusPar_t();
            CanPortName = Deviceinfo.CanPort;                                       //Get the CAN port name,CAN1 or CAN2

             
                 
              
                #region
                /**********************************************************************************************
               *  NOTE: acCanOpen Usage
               * 
               *	  Description:
               *		 Open can port by name, and indicate the max send and receive Frame number each time.
               * 
               *    acCanOpen arguments:
               *		  PortName			         - port name
               *		  synchronization	         - TRUE, synchronization ; FALSE, asynchronous
               *		  MsgNumberOfReadBuffer	   - The max frames number to read each time
               *		  MsgNumberOfWriteBuffer	- The max frames number to write each time
               * 
               *    When open port, user must pass the value of 'MsgNumberOfReadBuffer' and 'MsgNumberOfWriteBuffer' 
               *    auguments to indicate the max sent and received packages number of each time.
               *    In this example, we send 100 CAN frames by default
               *    User can change the value of 'nMsgCount' to send different frames each time in this examples.
               **********************************************************************************************/
                nRet = Device.acCanOpen(Deviceinfo.CanPort, false, Deviceinfo.MaxMsgCount, Deviceinfo.MaxMsgCount);                               //Open CAN port
                if (nRet < 0)
                {
                    HinTextShow("Failed to open the CAN port, please check the CAN port name!");
                    return;
                }

                nRet = Device.acEnterResetMode();                     //Enter reset mode          
                if (nRet < 0)
                {
                    MessageBox.Show("Failed to stop opertion!");
                    Device.acCanClose();
                    return;
                }

                BaudRateValue = (uint)Convert.ToInt32(Deviceinfo.CanBaudrate);        //Get baud rate
                nRet = Device.acSetBaud(BaudRateValue);                               //Set Baud Rate
                if (nRet < 0)
                {
                    MessageBox.Show("Failed to set baud!");
                    Device.acCanClose();
                    return;
                }

 
            nRet = Device.acSetAcceptanceFilterMode(AdvCan.PELICAN_SINGLE_FILTER);  //Set acceptance filter mode
            if (nRet < 0)
            {
                HinTextShow("Failed to set acceptance filter mode!");
                Device.acCanClose();
                return;
            }


            nRet = Device.acSetAcceptanceFilterMask(0xFFFFFFFF);
          //         nRet = Device.acSetAcceptanceFilterMask(0xFFFFF807);                        //Set acceptance mask
            if (nRet < 0)
            {
                HinTextShow("Failed to set acceptance mask!");
                Device.acCanClose();
                return;
            }

            nRet = Device.acSetAcceptanceFilterCode(0x00000000);
           //         nRet = Device.acSetAcceptanceFilterCode(0x000003C8);                          //Set acceptance code
            if (nRet < 0)
            {
                HinTextShow("Failed to set acceptance code!");
                Device.acCanClose();
                return;
            }

            try
            {
                WriteTimeOutValue = Convert.ToUInt32(Deviceinfo.CanWriteTimeOut);     //Get write timeout
            }
            catch (Exception)
            {
                MessageBox.Show("Invalid Write TimeOut value!");
                Device.acCanClose();
                return;
            }
                try
                {
                    ReadTimeOutValue = Convert.ToUInt32(Deviceinfo.CanReadTimeOut);        //Get read timeout
                }
                catch (Exception)
                {
                    MessageBox.Show("Invalid Read TimeOut value!");
                    Device.acCanClose();
                    return;
                }
                nRet = Device.acSetTimeOut(ReadTimeOutValue, WriteTimeOutValue);      //Set timeout
                if (nRet < 0)
                {
                    MessageBox.Show("Failed to set Timeout!");
                    Device.acCanClose();
                    return;
                }
                nRet = Device.acClearRxFifo();                                        //Clear receive fifo of driver
                if (nRet < 0)
                {
                    Device.acCanClose();
                    return;
                }
                
                nRet = Device.acEnterWorkMode();                                     //Enter work mode
                if (nRet < 0)
                {
                    MessageBox.Show("Failed to restart operation!");
                    Device.acCanClose();
                    return;
                }                              
                HinTextShow("CAN Start successfully!");
            #endregion             
        }

        private void ReadStringVersion()
        {
            string ReceiveStatus = " ";
            uint nReadCount = Deviceinfo.MaxMsgCount;           
            AdvCan.canmsg_t[] msgRead = new AdvCan.canmsg_t[1];            
            Deviceinfo Dc = new Deviceinfo();
            bool QueryVersionCountFlag = true;
           
            while (true)
            {
                Thread.Sleep(5);
                if (QueryThreadExitFlag == true)
                    break;
                if (QueryOnlineStringFlag == true)
                {
                    TimeSpan GworkTime = DateTime.Now - CanTimer.TimeStart;
                    double Wholeseconds = GworkTime.TotalSeconds;
                    if (Wholeseconds >= 1 && Wholeseconds <= 2 && QueryVersionCountFlag == true)  /**/
                    {
                        QueryVersionCountFlag = false;
                        if (myOnStringQ.Count == 0)
                        {
                            for (int i = 0; i < Deviceinfo.StringNumberValue; i++)
                            {
                                QueryOnLineDevice(i + 1, 0x00, (byte)UpgradeDataField.StringTypeMsg);
                            }

                        }
                        else
                        {
                            HinTextShow("There are " + myOnStringQ.Count.ToString() + " string controller online");
                            if (Deviceinfo.TotalOnline == Deviceinfo.StringNumberValue)
                                ;
                            else
                                HinTextShow("Not all the String controller are online,Plz check string address and can communication problem!!!");
                            QueryOnlineStringFlag = false;
                        }

                    }
                    else if (Wholeseconds > 4)
                    {
                        QueryOnlineStringFlag = false;
                        if (Deviceinfo.TotalOnline == 0)
                        {
                            HinTextShow("No deviece online,check the can communication");
                        }
                        else
                        {
                            HinTextShow("There are " + Deviceinfo.TotalOnline.ToString() + " string controller online");

                        }
                        if (Deviceinfo.TotalOnline == Deviceinfo.StringNumberValue)
                            ;
                        else
                            HinTextShow("Not all the String controller are online,Plz check string address and can communication problem!!!");
                    }
                }

                while (mCan.gRecMsgBufHead != mCan.gRecMsgBufTail)
                {
                    
                    msgRead[0] = mCan.gRecMsgBuf[mCan.gRecMsgBufTail];
                    Dc.DeviceStruct.RawID = msgRead[0].id;
                    if (Dc.DeviceStruct.MessageID == (uint)UpgradeMessageID.QueryAckMsg && Dc.DeviceStruct.StringID == 0x00)
                    {
                        MessageBox.Show(this, " 检测到当前某个String Control 的地址是 0", "String Address Error", MessageBoxButton.OK);
                        mCan.gRecMsgBufTail += 1;
                        if (mCan.gRecMsgBufTail >= AdcantechCAnFIFO.REC_MSG_BUF_MAX)
                        {
                            mCan.gRecMsgBufTail = 0;
                        }
                        /**/
                        break;
                    }
                    if (Dc.DeviceStruct.BpID != 0x00)
                    {
                        if (Dc.DeviceStruct.MessageID == (uint)UpgradeMessageID.QueryAckMsg)
                        {
                            switch (msgRead[0].data[1])
                            {
                                case 1:

                                    HinTextShow("bp" + Dc.DeviceStruct.BpID.ToString() + "当前停留在boot区域,");
                                    HinTextShow("当前版本号是：" + msgRead[0].data[2].ToString());

                                    bpFirList.First(d => d.NodeAddress == "Bp " + Dc.DeviceStruct.BpID.ToString()).FirmwareVersion = "bp_B_" + msgRead[0].data[2].ToString();
                                    bpFirList.First(d => d.NodeAddress == "Bp " + Dc.DeviceStruct.BpID.ToString()).FirmwareType = "Boot";
                                    break;

                                case 2:

                                    HinTextShow("bp" + Dc.DeviceStruct.BpID.ToString() + "当前停留在AP区域,");
                                    HinTextShow("当前版本号是：" + msgRead[0].data[2].ToString());

                                    bpFirList.First(d => d.NodeAddress == "Bp " + Dc.DeviceStruct.BpID.ToString()).FirmwareVersion = "bp_A_" + msgRead[0].data[2].ToString();
                                    bpFirList.First(d => d.NodeAddress == "Bp " + Dc.DeviceStruct.BpID.ToString()).FirmwareType = "Ap";

                                    break;
                            }
                            bpFirList.First(d => d.NodeAddress == "Bp " + Dc.DeviceStruct.BpID.ToString()).OnlineStatus = true;

                        }
                        dataGridFirmwareVersion.Dispatcher.Invoke(() =>
                        {
                            dataGridFirmwareVersion.ItemsSource = null;
                            dataGridFirmwareVersion.ItemsSource = bpFirList;
                        });

                    }
                    else
                    {
                       
                        if (Dc.DeviceStruct.MessageID == (uint)UpgradeMessageID.QueryAckMsg)
                        {
                            lock (locker)
                            {
                                if (myOnStringQ.Count == 0)
                                {
                                    myOnStringQ.Enqueue(Dc.DeviceStruct.StringID);
                                    Deviceinfo.TotalOnline++;
                                }
                                else
                                {
                                    uint SameDataCount = 0;
                                    foreach (uint obj in myOnStringQ)
                                    {
                                        if ((uint)Dc.DeviceStruct.StringID == obj)
                                        {
                                            SameDataCount++;
                                            break;
                                        }

                                    }
                                    if (SameDataCount == 0)
                                    {
                                        Deviceinfo.TotalOnline++;
                                        myOnStringQ.Enqueue(Dc.DeviceStruct.StringID);
                                    }
                                }
                            }

                            switch (msgRead[0].data[1])
                            {
                                case 1:

                                    stringFirList.First(d => d.NodeAddress == "String " + Dc.DeviceStruct.StringID.ToString()).FirmwareVersion = "string_B_" + msgRead[0].data[2].ToString();
                                    stringFirList.First(d => d.NodeAddress == "String " + Dc.DeviceStruct.StringID.ToString()).FirmwareType = "Boot";

                                     HinTextShow("string" + Dc.DeviceStruct.StringID.ToString() + "当前停留在boot区域,");
                                     HinTextShow("当前版本号是：" + msgRead[0].data[2].ToString());


                                    break;
                                case 2:
                                    stringFirList.First(d => d.NodeAddress == "String " + Dc.DeviceStruct.StringID.ToString()).FirmwareVersion = "string_A_" + msgRead[0].data[2].ToString();
                                    stringFirList.First(d => d.NodeAddress == "String " + Dc.DeviceStruct.StringID.ToString()).FirmwareType = "Ap";

                                    HinTextShow("string" + Dc.DeviceStruct.StringID.ToString() + "当前停留在AP区域,");
                                    HinTextShow("当前版本号是：" + msgRead[0].data[2].ToString());
                

                                    break;
                                default:
                                    break;

                                     
                            }
                           stringFirList.First(d => d.NodeAddress == "String " + Dc.DeviceStruct.StringID.ToString()).OnlineStatus = true;



                            dataGridFirmwareVersion.Dispatcher.Invoke(() =>
                            {
                                dataGridFirmwareVersion.ItemsSource = null;
                                dataGridFirmwareVersion.ItemsSource = stringFirList;
                            });


                        }
                    }
                    mCan.gRecMsgBufTail += 1;
                    if (mCan.gRecMsgBufTail >= AdcantechCAnFIFO.REC_MSG_BUF_MAX)
                    {
                        mCan.gRecMsgBufTail = 0;
                    }
                }
            }
        }

        void QueryOnLineDevice(int string_ID, int bp_id, byte device_type)
        {
            Deviceinfo sendDeviceInfo = new Deviceinfo();
 
            sendDeviceInfo.DeviceStruct.DeviceTypeID = (uint)DeviceType.ArrayController;
            sendDeviceInfo.DeviceStruct.MessageID = (uint)UpgradeMessageID.RestartMsg;
            sendDeviceInfo.DeviceStruct.StringID = (uint)string_ID;                               /*暂时 用 广播替代*/
            sendDeviceInfo.DeviceStruct.BpID = (uint)bp_id;
            sendDeviceInfo.DeviceStruct.MessagegroupID = (uint)MessageGroupId.Data;

           
            AdvCan.canmsg_t[] msgWrite = new AdvCan.canmsg_t[1];
          
            msgWrite[0].flags = AdvCan.MSG_EXT;
            msgWrite[0].cob = 0;
            msgWrite[0].id = sendDeviceInfo.DeviceStruct.RawID;
            msgWrite[0].length = 1;
            msgWrite[0].data = new byte[8];
            msgWrite[0].data[0] = device_type;
            FillSendBuffer(msgWrite);

           
            sendDeviceInfo.DeviceStruct.MessageID = (uint)UpgradeMessageID.QueryAckMsg;
            AdvCan.canmsg_t[] msgWrite79 = new AdvCan.canmsg_t[1];
            msgWrite79[0].flags = AdvCan.MSG_EXT;
            msgWrite79[0].cob = 0;
            msgWrite79[0].id = sendDeviceInfo.DeviceStruct.RawID;
            msgWrite79[0].length = 1;
            msgWrite79[0].data = new byte[8];
            msgWrite79[0].data[0] = 0x00;
            FillSendBuffer(msgWrite79);

        }

        void FillSendBuffer(AdvCan.canmsg_t[] frame)
        {
            lock (locker)
            {
                if (mCan.gSendMsgBufHead > AdcantechCAnFIFO.SEND_MSG_BUF_MAX - 1)
                    mCan.gSendMsgBufHead = 0;
                mCan.gSendMsgBuf[mCan.gSendMsgBufHead].id = frame[0].id;
                mCan.gSendMsgBuf[mCan.gSendMsgBufHead].length = frame[0].length;
                mCan.gRecMsgBuf[mCan.gSendMsgBufHead].data = new byte[8];
                 mCan.gSendMsgBuf[mCan.gSendMsgBufHead].data = frame[0].data;
                mCan.gSendMsgBuf[mCan.gSendMsgBufHead].flags = frame[0].flags;
                mCan.gSendMsgBuf[mCan.gSendMsgBufHead].cob = frame[0].cob;
                mCan.gSendMsgBufHead += 1;
                if (mCan.gSendMsgBufHead > AdcantechCAnFIFO.SEND_MSG_BUF_MAX - 1)
                    mCan.gSendMsgBufHead = 0;
                if (Deviceinfo.TotalOnline <= 1)
                    ;
                else
                    ;
                //Thread.Sleep(50);
            }
        }
        public void HinTextShow(string msg)
        {

            UpT.TextContent += msg + Environment.NewLine ;

            //if (Application.Current.Dispatcher.CheckAccess())
            //{
            //    this.DataContext = (object)null;
            //    this.DataContext = (object)UpT;
            //}
            //else
            //{
            //    Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() => {
            //        this.DataContext = (object)null;
            //        this.DataContext = (object)UpT;
            //    }));
            //}
          
        }

 

        void Show_StringInfoList()
        {
       

            InitStriList();
        }
         
        private void ResetOnlineStatus()
        {
            foreach(StringFirmwareInfor SFIn in stringFirList)
            {
                
            }
        }
        private void String_Online_Click(object sender, RoutedEventArgs e)
        {
          
            Deviceinfo.TotalOnline = 0;
            myOnStringQ.Clear();
            Show_StringInfoList();
            QueryOnLineDevice(StringNumberSelected.SelectedIndex, 0x00, (byte)UpgradeDataField.StringTypeMsg);
            QueryOnlineStringFlag = true;

            CanTimer.TimeStart = DateTime.Now;
            if (QueryThreadExitFlag == true) //烧写线程未在运行
            {

                Deviceinfo.FuncCanReceiveThreadFlag = false;
                //开启query线程
                QueryThreadExitFlag = false;
                GReadVersion = new Thread(ReadStringVersion);
                GReadVersion.Start();
                CanTimer.TimeStart = DateTime.Now;

            }
        }

        private void ClearText_Click(object sender, RoutedEventArgs e)
        {
            UpT.TextContent = null;
             HinttextBox.Clear();
        }
        void Show_BpInfoList()
        {
           // dataGridFirmwareVersion.Items.Clear();
            InitBpStrList();
        }
        private void BpSelected_Checked(object sender, RoutedEventArgs e)
        {
            if (BpSelected.IsChecked == true)
            {
                StrInfor.IsEnabled = false;
                BpInfor.IsEnabled = true;
                Show_BpInfoList();
            }
            else
            {
                Show_StringInfoList();
                StrInfor.IsEnabled = true;
                BpInfor.IsEnabled = false;
            }
        }

        private void QueryBp_Online_Click(object sender, RoutedEventArgs e)
        {
            Show_BpInfoList();
            QueryOnLineDevice(StringNumberSelected.SelectedIndex, 0x00, (byte)UpgradeDataField.BpTypeMsg);
            QueryOnlineStringFlag = true;

            if (QueryThreadExitFlag == true)
            {
               Deviceinfo.FuncCanReceiveThreadFlag = false;               
               //开启query线程
               QueryThreadExitFlag = false;
               GReadVersion = new Thread(ReadStringVersion);
               GReadVersion.Start();
               CanTimer.TimeStart = DateTime.Now;                 
            }

 
        }

        private void BpToBooter_Click(object sender, RoutedEventArgs e)
        {
            Show_BpInfoList();
            JumpToSection((byte)UpgradeDataField.BpTypeMsg, (byte)StringNumberSelected.SelectedIndex, (byte)FirmwareSection.Booter);
        }

        private void BpToAP_Click(object sender, RoutedEventArgs e)
        {
            Show_BpInfoList();
            JumpToSection((byte)UpgradeDataField.BpTypeMsg, (byte)StringNumberSelected.SelectedIndex, (byte)FirmwareSection.Ap);

        }

        private void BpSelected_Unchecked(object sender, RoutedEventArgs e)
        {
            if (BpSelected.IsChecked == true)
            {
                StrInfor.IsEnabled = false;
                BpInfor.IsEnabled = true;
                Show_BpInfoList();
            }
            else
            {
                Show_StringInfoList();
                StrInfor.IsEnabled = true;
            }
        }

        void DisableButton(bool newstate)
        {
            

            StrInfor.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() => {
                StrInfor.IsEnabled = newstate;
            }));
        }
        private void StringNumberSelected_Loaded(object sender, RoutedEventArgs e)
        {
            List<string> LS = new List<string>();
            LS.Add("stringall");
            for (int i = 0; i < new Global().stringnumber; i++)
            {
                LS.Add("string" + (i + 1).ToString());
            }
            StringNumberSelected.ItemsSource = LS;
            StringNumberSelected.SelectedIndex = 0;
        }
        void JumpToSection(byte devicetype, byte deviceID, byte section)
        {
            Deviceinfo.TotalOnline = 0;
 
            AdvCan.canmsg_t[] burndatainfo = new AdvCan.canmsg_t[1];
            AdvCan.canmsg_t[] burndatainfo1 = new AdvCan.canmsg_t[1];
            AdvCan.canmsg_t[] burndatainfo2= new AdvCan.canmsg_t[1];
            Deviceinfo sendDeviceInfo = new Deviceinfo();
            burndatainfo[0].data = new byte[8];
            burndatainfo1[0].data = new byte[8];
            burndatainfo2[0].data = new byte[8];

            sendDeviceInfo.DeviceStruct.DeviceTypeID = (uint)DeviceType.ArrayController;
            sendDeviceInfo.DeviceStruct.MessageID = (uint)UpgradeMessageID.RestartMsg;
            sendDeviceInfo.DeviceStruct.StringID = (uint)deviceID;                               /*暂时 用 广播替代*/
            sendDeviceInfo.DeviceStruct.BpID = 0x00;
            sendDeviceInfo.DeviceStruct.MessagegroupID = (uint)MessageGroupId.FirmwareUpdate;

            burndatainfo[0].id = sendDeviceInfo.DeviceStruct.RawID;
            burndatainfo[0].data[0] = devicetype; /*默认 为 string controller*/
            burndatainfo[0].length = 1;
            burndatainfo[0].flags = AdvCan.MSG_EXT;
            burndatainfo[0].cob = 0;
            FillSendBuffer(burndatainfo);

            sendDeviceInfo.DeviceStruct.MessageID = (uint)UpgradeMessageID.QueryAckMsg;
            burndatainfo1[0].id = sendDeviceInfo.DeviceStruct.RawID;
            burndatainfo1[0].data[0] = 0x01;
            burndatainfo1[0].data[1] = section;
            burndatainfo1[0].length = 2;
            burndatainfo1[0].flags = AdvCan.MSG_EXT;
            burndatainfo1[0].cob = 0;
            FillSendBuffer(burndatainfo1);

            sendDeviceInfo.DeviceStruct.MessageID = (uint)UpgradeMessageID.RestartMsg;
            burndatainfo2[0].cob = 0;
            burndatainfo2[0].id = sendDeviceInfo.DeviceStruct.RawID;
            burndatainfo2[0].data[0] = 0x01;
            burndatainfo2[0].length = 1;
            burndatainfo2[0].flags = AdvCan.MSG_EXT;

            FillSendBuffer(burndatainfo2);

        }
        private void StringToBooter_Click(object sender, RoutedEventArgs e)
        {
            Show_StringInfoList();
            JumpToSection((byte)UpgradeDataField.StringTypeMsg, (byte)(StringNumberSelected.SelectedIndex ), (byte)FirmwareSection.Booter);
        }

        private void StringToAP_Click(object sender, RoutedEventArgs e)
        {
            Show_StringInfoList();
            JumpToSection((byte)UpgradeDataField.StringTypeMsg, (byte)(StringNumberSelected.SelectedIndex), (byte)FirmwareSection.Ap);
        }

        private void BroadCastBurning_Click(object sender, RoutedEventArgs e)
        {
            QueryThreadExitFlag = true;
          
            //        timerReadmsg.Stop();
            for (int i = 0; i < Deviceinfo.StringNumberValue; i++)
            {

                stringFirList.First(d => d.NodeAddress == "String " + (i + 1).ToString()).BurningProgress = 0;
                dataGridFirmwareVersion.ItemsSource = null;
                dataGridFirmwareVersion.ItemsSource = stringFirList;
            }
            JumpToSection((byte)UpgradeDataField.StringTypeMsg, (byte)StringNumberSelected.SelectedIndex, (byte)FirmwareSection.Booter);

            AdvCan.canmsg_t[] burndatainfo = new AdvCan.canmsg_t[1];
            AdvCan.canmsg_t[] burndatainfo1 = new AdvCan.canmsg_t[1];
            AdvCan.canmsg_t[] burndatainfo2 = new AdvCan.canmsg_t[1];
            Burner Burner = new Burner();
            Deviceinfo sendDeviceInfo = new Deviceinfo();
            burndatainfo[0].data = new byte[8];
            burndatainfo1[0].data = new byte[8];
            burndatainfo2[0].data = new byte[8];

            sendDeviceInfo.DeviceStruct.DeviceTypeID = (uint)DeviceType.ArrayController;
            sendDeviceInfo.DeviceStruct.MessageID = (uint)UpgradeMessageID.RestartMsg;
            //      sendDeviceInfo.DeviceStruct.StringID = (uint)0x00;                               /*暂时 用 广播替代*/
            sendDeviceInfo.DeviceStruct.StringID = (uint)StringNumberSelected.SelectedIndex;
            sendDeviceInfo.DeviceStruct.BpID = 0x00;
            sendDeviceInfo.DeviceStruct.MessagegroupID = (uint)MessageGroupId.Data;

            sendDeviceInfo.DeviceStruct.MessageID = (uint)UpgradeMessageID.RestartMsg;
            burndatainfo[0].id = sendDeviceInfo.DeviceStruct.RawID;
            burndatainfo[0].data[0] = 0x03;
            burndatainfo[0].length = 1;
            burndatainfo[0].flags = AdvCan.MSG_EXT;
            burndatainfo[0].cob = 0;

            FillSendBuffer(burndatainfo);
            sendDeviceInfo.DeviceStruct.MessageID = (uint)UpgradeMessageID.QueryAckMsg;
            burndatainfo1[0].id = sendDeviceInfo.DeviceStruct.RawID;
            burndatainfo1[0].data[0] = 0x00;
            burndatainfo1[0].length = 1;
            burndatainfo1[0].flags = AdvCan.MSG_EXT;
            burndatainfo1[0].cob = 0;
            FillSendBuffer(burndatainfo1);

            sendDeviceInfo.DeviceStruct.MessageID = (uint)UpgradeMessageID.RestartMsg;
            burndatainfo2[0].id = sendDeviceInfo.DeviceStruct.RawID;
            burndatainfo2[0].data[0] = 0x03;
            burndatainfo2[0].length = 1;
            burndatainfo2[0].flags = AdvCan.MSG_EXT;
            burndatainfo2[0].cob = 0;
            FillSendBuffer(burndatainfo2);


            if (File.Exists(this.textBoxFilePath.Text))
            {
                Burner.FirmwareFileSize = (int)new FileInfo(textBoxFilePath.Text).Length;
                br = new BinaryReader(new FileStream(textBoxFilePath.Text, FileMode.Open));

                Burner.upgradingnotsuccess = true;
                Deviceinfo.FuncCanReceiveThreadFlag = false;

                broadcastburningstatus = Burner.upgradingnotsuccess;
                QueryThreadExitFlag = true;
                //  BurnStartTImeLabel.Text = DateTime.Now.ToLongTimeString();
                Burner.Status = (int)burnerstatus.SendBurnerAddress;
                DisableButton(false);
                int BurnedStringID = StringNumberSelected.SelectedIndex;
                Burn = new Thread(() => BroadcastBurningFirmwareProcess(BurnedStringID, Burner.Status, DateTime.Now));
                       Burn.Start();
            }
            else
            {
                HinttextBox.Text += "Load the bin file first!";
                MessageBox.Show(this, "无法打开固件文件，是否选择了固件文件？", "警告", MessageBoxButton.OK, MessageBoxImage.Warning);
            }

            BurnStartTime.Text = "Starttime:" + DateTime.Now.ToString("h:mm:ss tt");
        }

        void BroadcastBurningFirmwareProcess(int stringID, int BurnerStatus, DateTime TT)
        {
            HinTextShow("Enter BurningFirmwareProcess Thread!!!!!");
            //double BurnePage = 0;
            int sumcrc = 0;
            Burner Burner = new Burner();
            Burner.Status = BurnerStatus;
            byte[] DataBuffer = new byte[Burner.burnbyte];
            TimeCount CanTimerTemp = new TimeCount();
            CanTimerTemp.TimeStart = TT;
            int AckDeviceCount = 0;
            while (Burner.upgradingnotsuccess)
            {
                 
                int ThreadStringID = stringID;
                Deviceinfo sendDeviceInfo = new Deviceinfo();

                sendDeviceInfo.DeviceStruct.DeviceTypeID = (uint)DeviceType.ArrayController;
                sendDeviceInfo.DeviceStruct.MessageID = (uint)UpgradeMessageID.BurnMsg;
                sendDeviceInfo.DeviceStruct.StringID = (uint)stringID;                               /*暂时 用 广播替代*/
                sendDeviceInfo.DeviceStruct.BpID = 0x00;
                sendDeviceInfo.DeviceStruct.MessagegroupID = (uint)MessageGroupId.Data;

                //    CAN_OBJ frameinfo = new CAN_OBJ();

                AdvCan.canmsg_t[] frameinfo = new AdvCan.canmsg_t[1];

                Deviceinfo Dc = new Deviceinfo();

                TimeSpan GworkTime = DateTime.Now - CanTimerTemp.TimeStart;
                double Wholeseconds = GworkTime.TotalSeconds;
                switch (Burner.Status)
                {

                    case (int)burnerstatus.WaitSendBurnerAddress:
                        {
                            if (Wholeseconds > 5)
                            {
                                Burner.Status = (int)burnerstatus.SendBurnerAddress;
                                HinTextShow("string" + stringID.ToString() + "烧写延时，将会重新发送烧写指令");
                            }
                        }
                        break;
                    case (int)burnerstatus.WaitSendBurnerDatas:
                        {
                            if (Wholeseconds > 5)
                            {
                                Burner.Status = (int)burnerstatus.SendBurnerAddress;
                                HinTextShow("string" + stringID.ToString() + "烧写延时，将会重新发送烧写地址");
                            }
                        }
                        break;
                    case (int)burnerstatus.WaitDeviceReadytoBurn:
                        {
                            if (Wholeseconds > 20)
                            {
                                Burner.Status = (int)burnerstatus.SendBurnerAddress;
                                HinTextShow("string" + stringID.ToString() + "发送烧写数据未得到回应，将会重新发送烧写数据");
                            }
                        }
                        break;
                }


                while (mCan.gRecMsgBufHead != mCan.gRecMsgBufTail)
                {
                  //  Thread.Sleep(80);
                    frameinfo[0] = mCan.gRecMsgBuf[mCan.gRecMsgBufTail];
                    Dc.DeviceStruct.RawID = frameinfo[0].id;

                    //if (Dc.DeviceStruct.StringID != ThreadStringID && Dc.DeviceStruct.StringID != 0x00 && frameinfo.data[1] != 2)
                    //{
                    //    if (Dc.DeviceStruct.MessageID == 0x79 && frameinfo.DataLen == 3)  //如果是版本号发上来
                    //    {

                    //    }
                    //    else
                    //    {
                    //        break;
                    //    }
                    //}

                    mCan.gRecMsgBufTail += 1;

                    if (mCan.gRecMsgBufTail >= 1000)
                    {
                        mCan.gRecMsgBufTail = 0;
                    }

                    switch (Burner.Status)
                    {
                        case (int)burnerstatus.NormalWork:
                            {
                                if (Dc.DeviceStruct.MessageID == (uint)UpgradeMessageID.QueryAckMsg)
                                {
                                    switch (frameinfo[0].data[1])
                                    {
                                        case 1:

                                            HinTextShow("string" + Dc.DeviceStruct.StringID.ToString() + "当前停留在boot区域,");
                                            HinTextShow("string" + stringID.ToString() + "当前版本号是：" + frameinfo[0].data[2].ToString());
                                            stringFirList.First(d => d.NodeAddress == "String " + Dc.DeviceStruct.StringID.ToString()).FirmwareVersion = "string_B_" + frameinfo[0].data[2].ToString();
                                            stringFirList.First(d => d.NodeAddress == "String " + Dc.DeviceStruct.StringID.ToString()).FirmwareType = "Boot";
                                            break;
                                        case 2:
                                            HinTextShow("string" + Dc.DeviceStruct.StringID.ToString() + "当前停留在AP区域,");
                                            HinTextShow("string" + stringID.ToString() + "当前版本号是：" + frameinfo[0].data[2].ToString());
                                            stringFirList.First(d => d.NodeAddress == "String " + Dc.DeviceStruct.StringID.ToString()).FirmwareVersion = "string_A_" + frameinfo[0].data[2].ToString();
                                            stringFirList.First(d => d.NodeAddress == "String " + Dc.DeviceStruct.StringID.ToString()).FirmwareType = "Ap";
                                            break;
                                    }
                                    stringFirList.First(d => d.NodeAddress == "String " + Dc.DeviceStruct.StringID.ToString()).OnlineStatus = true;

                                    dataGridFirmwareVersion.Dispatcher.Invoke(() =>
                                    {
                                        dataGridFirmwareVersion.ItemsSource = null;
                                        dataGridFirmwareVersion.ItemsSource = stringFirList;
                                    });
                                }
                            }
                            break;

                        case (int)burnerstatus.WaitSendBurnerAddress:
                            {
                                if (Dc.DeviceStruct.MessageID == (uint)UpgradeMessageID.BurnMsg)
                                {
                                    if (frameinfo[0].data[1] == (byte)UpgradeDataField.AckMsg2)
                                    {
                                        AckDeviceCount++;

                                        HinTextShow("string" + Dc.DeviceStruct.StringID.ToString() + "区域" + Burner.succeedburnedSectionAmount.ToString() + "烧写成功");
                                        if (AckDeviceCount == myOnStringQ.Count)
                                        {
                                           
                                            Burner.Status = (byte)burnerstatus.SendBurnerAddress;
                                            Burner.succeedburnedSectionAmount++;
                                            AckDeviceCount = 0;
                                            foreach (uint StringID in myOnStringQ)
                                            //  dataGridView1.Rows[(int)StringID - 1].Cells[4].Value = (100 / Deviceinfo.BurnePage) * Burner.succeedburnedSectionAmount * 110 / 100;
                                            {
                                                stringFirList.First(d => d.NodeAddress == "String " + StringID.ToString()).BurningProgress = (100 / Deviceinfo.BurnePage) * Burner.succeedburnedSectionAmount * 110 / 100;
                                              
                                   

                                                dataGridFirmwareVersion.Dispatcher.Invoke(() =>
                                                {
                                                    dataGridFirmwareVersion.ItemsSource = null;
                                                    dataGridFirmwareVersion.ItemsSource = stringFirList;
                                                });

                                            }
                                            TotalProgressbar.Dispatcher.Invoke(() =>
                                            {
                                                TotalProgressbar.Value = (100 / Deviceinfo.BurnePage) * Burner.succeedburnedSectionAmount * 110 / 100;
                                            });

                                            
                                        }


                                    }
                                    else if (frameinfo[0].data[1] == (byte)UpgradeDataField.WrongAckMsg)
                                    {
                                        HinTextShow("string" + Dc.DeviceStruct.StringID.ToString() + "Burned error,重新烧写区域 " + Burner.succeedburnedSectionAmount.ToString());
                                        Burner.Status = (byte)burnerstatus.SendBurnerAddress;
                                        AckDeviceCount = 0;

                                    }
                                   

                                }
                            }
                            break;
                        case (int)burnerstatus.WaitSendBurnerDatas:
                            {
                                if (Dc.DeviceStruct.MessageID == (uint)UpgradeMessageID.BurnMsg)
                                {
                                    AckDeviceCount++;
                                    HinTextShow("收到" + "string" + Dc.DeviceStruct.StringID.ToString() + "的回应");
                                    if (AckDeviceCount == myOnStringQ.Count)
                                    {
                                        Burner.Status = (int)burnerstatus.SendBunerDatas;
                                        AckDeviceCount = 0;
                                    }
                                }
                            }
                            break;
                        case (int)burnerstatus.WaitDeviceReadytoBurn:
                            {
                                if (Dc.DeviceStruct.MessageID == (uint)UpgradeMessageID.BurnMsg)
                                {
                                    if (frameinfo[0].data[1] == (byte)UpgradeDataField.AckMsg2)
                                    {
                                        AckDeviceCount++;
                                        HinTextShow("string" + Dc.DeviceStruct.StringID.ToString() + " 接收区域" + Burner.succeedburnedSectionAmount.ToString() + "的烧写数据完毕！");
                                        if (AckDeviceCount == myOnStringQ.Count)
                                        {
                                            Burner.Status = (int)burnerstatus.DeviceReadytoBurn;
                                            AckDeviceCount = 0;
                                        }
                                    }
                                    else if (frameinfo[0].data[1] == (byte)UpgradeDataField.WrongAckMsg)
                                    {
                                        HinTextShow("string" + Dc.DeviceStruct.StringID.ToString() + "未完成区域" + Burner.succeedburnedSectionAmount.ToString() + "的数据接收");
                                        HinTextShow("burn将重新发送" + "string" + Dc.DeviceStruct.StringID.ToString() + "区域" + Burner.succeedburnedSectionAmount.ToString() + "的烧写数据");
                                        Burner.Status = (int)burnerstatus.SendBurnerAddress; /*重新开始烧写此区域*/
                                        AckDeviceCount = 0;
                                    }
                                }
                            }
                            break;
                        default:
                            break;
                    }
                }


                if (Burner.upgradingnotsuccess)
                {

                    switch (Burner.Status)
                    {

                        case (int)burnerstatus.SendBurnerAddress:
                            double BurnePage = Math.Ceiling((double)Burner.FirmwareFileSize / Burner.burnbyte);
                            if (Burner.succeedburnedSectionAmount == BurnePage)
                            {
                       

                                BurnStopTime.Dispatcher.Invoke(() =>
                                {
                                    BurnStopTime.Text = "Stoptime:" + DateTime.Now.ToString("h:mm:ss tt");
                                });

                                HinTextShow("string" + Dc.DeviceStruct.StringID.ToString() + "烧写成功！");
                                Burner.Status = (byte)burnerstatus.NormalWork;
                                Burner.upgradingnotsuccess = false;
                                broadcastburningstatus = Burner.upgradingnotsuccess;
                                 


                                foreach (uint StringID in myOnStringQ)
                                {
                                    stringFirList.First(d => d.NodeAddress == "String " + StringID.ToString()).BurningProgress =  100;

                                    JumpToSection((byte)UpgradeDataField.StringTypeMsg, (byte)StringID, (byte)FirmwareSection.Ap);
                                }
                                TotalProgressbar.Dispatcher.Invoke(() =>
                                {
                                    TotalProgressbar.Value =  100;
                                });

                                
                                            BurnStopTime.Dispatcher.Invoke(() =>
                                            {
                                                BurnStopTime.Text = "Stoptime:" + DateTime.Now.ToString("h:mm:ss tt");
                                            });
                                //    burnerSuccessCount++;

                                //if (burnerSuccessCount == myOnStringQ.Count)
                                //{
                                DisableButton(true);
                                QueryOnlineStringFlag = true;
                                QueryThreadExitFlag = false;
                                GReadVersion = new Thread(ReadStringVersion);
                                GReadVersion.Start();
                                CanTimer.TimeStart = DateTime.Now;

                            //    burnerSuccessCount = 0;
                                br.Close();
                                //  BurnEndTimeLabel.Text = DateTime.Now.ToLongTimeString();
                                //}


                                break;
                            }
                            //            HinttextBox.Invoke(new MethodInvoker(delegate { HinttextBox.Text += Environment.NewLine + "开始烧写所有在线string" + "区域" + Burner.succeedburnedSectionAmount.ToString(); }));
                            HinTextShow("开始烧写所有在线string" + "区域" + Burner.succeedburnedSectionAmount.ToString());

                            //  CAN_OBJ frame = new CAN_OBJ();
                            AdvCan.canmsg_t[] frame = new AdvCan.canmsg_t[1];

                            ulong burnedStartAddress = Burner.StartAddress + (ulong)(Burner.succeedburnedSectionAmount * Burner.burnbyte);

                            frame[0].data = new byte[8];

                            frame[0].id = sendDeviceInfo.DeviceStruct.RawID;
                            frame[0].length = 7;
                            frame[0].data[0] = (byte)UpgradeDataField.Querymsg1;
                            frame[0].data[1] = (byte)burnedStartAddress;
                            frame[0].data[2] = (byte)(burnedStartAddress >> 8);
                            frame[0].data[3] = (byte)(burnedStartAddress >> 16);
                            frame[0].data[4] = (byte)(burnedStartAddress >> 24);
                            frame[0].data[5] = 0x00;
                            frame[0].data[6] = 0x08;
                            frame[0].flags = 4 ;
                            frame[0].cob = 0;

                            FillSendBuffer(frame);

                            CanTimerTemp.TimeStart = DateTime.Now;
                            Burner.Status = (int)burnerstatus.WaitSendBurnerDatas;
                            break;

                        case (int)burnerstatus.SendBunerDatas:

                            HinTextShow("开始传输烧写数据给所有在线" + "string");
                             for (int i = 0; i < Burner.burnbyte; i++)
                                DataBuffer[i] = 0xff;

                            br.BaseStream.Seek(Burner.succeedburnedSectionAmount * Burner.burnbyte, SeekOrigin.Begin);
                            UInt32 read_data_num = (UInt32)br.Read(DataBuffer, 0, (int)Burner.burnbyte);
                            for (int i = 0; i < Burner.burnbyte; i = i + 8)
                            {
                                //CAN_OBJ burndatainfo = new CAN_OBJ();
                                AdvCan.canmsg_t[] burndatainfo = new AdvCan.canmsg_t[1];

                                burndatainfo[0].id = sendDeviceInfo.DeviceStruct.RawID;
                                burndatainfo[0].data = new byte[8];
                                burndatainfo[0].flags = AdvCan.MSG_EXT;
                                burndatainfo[0].length = 8;
                                for (int k = i; k < 8 + i; k++)
                                {
                                    burndatainfo[0].data[k - i] = DataBuffer[k];
                                    sumcrc += burndatainfo[0].data[k - i];
                                }
                                FillSendBuffer(burndatainfo);
                            }


                            int checksum = 0;
                            for (int i = 0; i < Burner.burnbyte; i++)
                            {
                                checksum += DataBuffer[i];
                            }


                            {
                                AdvCan.canmsg_t[] crccheck = new AdvCan.canmsg_t[1];                               
                                crccheck[0].data = new byte[8];
                                crccheck[0].id = sendDeviceInfo.DeviceStruct.RawID;
                             
                                crccheck[0].flags = AdvCan.MSG_EXT;
                                crccheck[0].length = 3;
                                crccheck[0].data[0] = (byte)UpgradeDataField.AckMsg1;
                                crccheck[0].data[1] = (byte)UpgradeDataField.CrcCheck;
                                crccheck[0].data[2] = (byte)checksum;
                                FillSendBuffer(crccheck);
                                
                            }
                            Burner.Status = (int)burnerstatus.WaitDeviceReadytoBurn;                
                            HinTextShow("burn 发送完成所有在线" + "string" + "区域" + Burner.succeedburnedSectionAmount.ToString() + "的烧写数据");
                            HinTextShow("等待所有在线" + "string" + "接收");
                            CanTimerTemp.TimeStart = DateTime.Now;

                            break;

                        case (int)burnerstatus.DeviceReadytoBurn:
                            {
                               // CAN_OBJ burndatainfo = new CAN_OBJ();
                                AdvCan.canmsg_t[] burndatainfo = new AdvCan.canmsg_t[1];
                                burndatainfo[0].id = sendDeviceInfo.DeviceStruct.RawID;
                                burndatainfo[0].flags =  AdvCan.MSG_EXT;
                                burndatainfo[0].length = 2;
                                burndatainfo[0].data = new byte[8];
                                burndatainfo[0].cob = 0;
                                burndatainfo[0].data[0] = (byte)UpgradeDataField.AckMsg1;
                                burndatainfo[0].data[1] = (byte)UpgradeDataField.QueryMsg;
                                FillSendBuffer(burndatainfo);
                                Burner.Status = (byte)burnerstatus.WaitSendBurnerAddress;
                            }
                            CanTimerTemp.TimeStart = DateTime.Now;
                            break;
                        default:
                            break;
                    }
                }
            }
        }
        private void OpenFile_Click(object sender, RoutedEventArgs e)
        {

            Burner Burner = new Burner();

            // dataGridView1[4, 0].Value = 20;

            OpenFileDialog fd = new OpenFileDialog();
            fd.Filter = "二进制文件(*.bin)|*.bin";
          

            if (fd.ShowDialog() == true)
            {
                this.textBoxFilePath.Text = fd.FileName;

                string FirmwarePathName = fd.FileName;

                string FirmwareName = System.IO.Path.GetFileName(FirmwarePathName);

                if (FirmwareName.Contains("string_A") || FirmwareName.Contains("String_A"))
                {

                    HinTextShow("File" + FirmwareName + " loaded successfully");
                    Regex re = new Regex(@"\d+");
                    Match m = re.Match(FirmwareName);
                    HinTextShow("Current Loaded Firmware version is: " + m);

                }
                else
                {
                    BurnerFilePath.Text = " ";
                    textBoxFilePath.Text = " ";
                    MessageBox.Show("Wrong binary file loaded, check plz!");
                    return;
                }

                BurnerFilePath.Text = textBoxFilePath.Text;


                Burner.FirmwareFileSize = (int)new FileInfo(textBoxFilePath.Text).Length;
                

                 
                Deviceinfo.BurnePage = (int)Math.Ceiling((double)Burner.FirmwareFileSize / Burner.burnbyte);

                HinTextShow("总共有" + Burner.FirmwareFileSize.ToString() + "byte" + "加载进来。");
                HinTextShow("分" + Deviceinfo.BurnePage.ToString() + "区烧写！");
 
            }
        }

        private void HinttextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            HinttextBox.SelectionStart = HinttextBox.Text.Length;
            HinttextBox.ScrollToEnd();
        }

 

        private void SCNumberselect_Loaded(object sender, RoutedEventArgs e)
        {
            List<string> LS = new List<string>();        
            for (int i = 0; i < new Global().stringnumber; i++)
            {
                LS.Add("string" + (i + 1).ToString());
            }
            SCNumberselect.ItemsSource = LS;
            SCNumberselect.SelectedIndex = 0;

        }

        private void BpNumberselect_Loaded(object sender, RoutedEventArgs e)
        {
            List<string> LS = new List<string>();     
            for (int i = 0; i < new Global().bpnumber; i++)
            {
                LS.Add("bp" + (i + 1).ToString());
            }
            BpNumberselect.ItemsSource = LS;
            BpNumberselect.SelectedIndex = 0;
        }

 
        private void FuncCanRecThreadMethod()
        {
            AdvCan.canmsg_t[] msgRead = new AdvCan.canmsg_t[1];
            Deviceinfo Dc = new Deviceinfo();
            Deviceinfo.FuncCanReceiveThreadFlag = true;
            while (Deviceinfo.FuncCanReceiveThreadFlag)
            {
                while (mCan.gRecMsgBufHead != mCan.gRecMsgBufTail)
                {

                    msgRead[0] = mCan.gRecMsgBuf[mCan.gRecMsgBufTail];
                    Dc.DeviceStruct.RawID = msgRead[0].id;

                     AnalyseMessage(msgRead);



                    mCan.gRecMsgBufTail += 1;

                    if (mCan.gRecMsgBufTail >= AdcantechCAnFIFO.REC_MSG_BUF_MAX)
                    {
                        mCan.gRecMsgBufTail = 0;
                    }
                }


            }
        }
        private void AnalyseMessage(AdvCan.canmsg_t[] msgRead)
        {
            AdvCan.canmsg_t[] msgReadTemp = new AdvCan.canmsg_t[0];
            AdvCan.canmsg_t[] msgReadVol = new AdvCan.canmsg_t[0];
            AdvCan.canmsg_t[] msgReadBalstate = new AdvCan.canmsg_t[0];

            uint DType = (msgRead[0].id >> 18) & 0x0F;
            byte MsgID = (byte)(msgRead[0].id & 0xFF);
            int BPID = (int)(msgRead[0].id >> 8) & 0x3F;
            int StringID = (int)(msgRead[0].id >> 14) & 0x0F;

            if (StringID != stringnumber_selected    )
            {
                return;
            }
 
            if (MsgID >= (byte)StringMessageID.SelectedBP1_4Vol && MsgID <= (byte)StringMessageID.SelectedBP13_16Vol)
                msgReadVol = msgRead;

            else if (MsgID >= (byte)StringMessageID.SelectedBP1_4Temp && MsgID <= (byte)StringMessageID.SelectedBP13_16Temp)
                msgReadTemp = msgRead;


            if (DType != (uint)DeviceType.StringController) return;
            switch (MsgID)
            {
                case (byte)StringMessageID.DCBusVol:
                    ListStringInfor.First(d => d.stringID == StringID).dcbusvoltage = (msgRead[0].data[0] * 256 + msgRead[0].data[1]).ToString() + "  " + "V";
                    break;

                case (byte)StringMessageID.TargetVol:
                    ListStringInfor.First(d => d.stringID == StringID).targetvalue = ((float)(msgRead[0].data[0] * 256 + msgRead[0].data[1]) / 1000).ToString() + "  " + "V";
                    break;
                case (byte)StringMessageID.StringCurrentAHSOC:
                    ListStringInfor.First(d => d.stringID == StringID).stringcurrent = (((Int16)(msgRead[0].data[0] * 256 + msgRead[0].data[1])) / 100.00).ToString() + "  " + "A";
                    ListStringInfor.First(d => d.stringID == StringID).pcontactorsattus = msgRead[0].data[6].ToString();
                    ListStringInfor.First(d => d.stringID == StringID).ncontactorstatus = msgRead[0].data[7].ToString();
                    break;

                case (byte)StringMessageID.HLAVol:
                    ListStringInfor.First(d => d.stringID == StringID).highvoltage = ((float)(msgRead[0].data[0] * 256 + msgRead[0].data[1]) / 1000).ToString() + "  " + "V";
                    ListStringInfor.First(d => d.stringID == StringID).lowvoltage = ((float)(msgRead[0].data[2] * 256 + msgRead[0].data[3]) / 1000).ToString() + "  " + "V";
                    ListStringInfor.First(d => d.stringID == StringID).avevoltage = ((float)(msgRead[0].data[4] * 256 + msgRead[0].data[5]) / 1000).ToString() + "  " + "V";
                    ListStringInfor.First(d => d.stringID == StringID).stringvoltage = (msgRead[0].data[6] * 256 + msgRead[0].data[7]).ToString() + "  " + "V";


                    break;
                case (byte)StringMessageID.HLATemp:
                    ListStringInfor.First(d => d.stringID == StringID).hightemp = ((float)(Int16)(msgRead[0].data[0] * 256 + msgRead[0].data[1]) / 10).ToString() + "  " + "℃";
                    ListStringInfor.First(d => d.stringID == StringID).lowtemp = ((float)(Int16)(msgRead[0].data[2] * 256 + msgRead[0].data[3]) / 10).ToString() + "  " + "℃";
                    ListStringInfor.First(d => d.stringID == StringID).avetemp = ((float)(Int16)(msgRead[0].data[4] * 256 + msgRead[0].data[5]) / 10).ToString() + "  " + "℃";

                    break;
                case (byte)StringMessageID.StringKWKWH:
                    ListStringInfor.First(d => d.stringID == StringID).kw = ((float)(Int16)(msgRead[0].data[0] * 256 + msgRead[0].data[1]) / 1).ToString() + "  " + "kw";
                    ListStringInfor.First(d => d.stringID == StringID).kwh = ((float)(Int16)(msgRead[0].data[2] * 256 + msgRead[0].data[3]) / 1).ToString() + "  " + "kwh";
                    if (msgRead[0].data[4] == 1)
                        ListStringInfor.First(d => d.stringID == StringID).contactorclosedpermissionstatus = "true";
                    else
                        ListStringInfor.First(d => d.stringID == StringID).contactorclosedpermissionstatus = "false";
                    break;
                case (byte)StringMessageID.SelectedBP1_4Vol:
                case (byte)StringMessageID.SelectedBP5_8Vol:
                case (byte)StringMessageID.SelectedBP9_12Vol:
                case (byte)StringMessageID.SelectedBP13_16Vol:
                    {
                        int index = (int)(MsgID - StringMessageID.SelectedBP1_4Vol);
                        if (BPID == 0)
                            return;

                        for (int i = 0; i < 4; i++)
                        {
                            string cellvol = ((float)(msgReadVol[0].data[2 * i] * 256 + msgReadVol[0].data[2 * i + 1]) / 1000).ToString();
                            ListVolTemp[BPID - 1].First(d => d.CellNumber == (1 + i + index * 4).ToString()).Vol = cellvol;
                        }
                        {
                            CellinfodataGridView.Dispatcher.Invoke(() =>
                            {
                                CellinfodataGridView.ItemsSource = null;
                                CellinfodataGridView.ItemsSource = ListVolTemp[BPID - 1];
                            });
                        }
                    }
                    break;
                case (byte)StringMessageID.SelectedBP1_4Temp:
                case (byte)StringMessageID.SelectedBP5_8Temp:
                case (byte)StringMessageID.SelectedBP9_12Temp:
                case (byte)StringMessageID.SelectedBP13_16Temp:
                    //                uiContext.Post(SlectedBPTempCal, null);
                    {
                        int index = (int)(MsgID - StringMessageID.SelectedBP1_4Temp);
                        if (BPID == 0) return;
                        for (int i = 0; i < 4; i++)
                        {
                            string celltemp;
                            celltemp = ((float)(Int16)(msgReadTemp[0].data[2 * i] * 256 + msgReadTemp[0].data[2 * i + 1]) / 10).ToString();
                            ListVolTemp[BPID - 1].First(d => d.CellNumber == (1 + i + index * 4).ToString()).Temp = celltemp;
                        }

                        {
                            CellinfodataGridView.Dispatcher.Invoke(() =>
                            {
                                CellinfodataGridView.ItemsSource = null;
                                CellinfodataGridView.ItemsSource = ListVolTemp[BPID - 1];
                            });
                        }
                    }
                    break;
                case (byte)StringMessageID.SelectedBPBalancingState:
                    msgReadBalstate = msgRead;

    //                uiContext.Post(Cellbalancingstate, null);
                    break;
                case (byte)ArrayMessageID.UpgradeQueryVersion:
                    if (BPID == 0) // that's string firmware version
                    {

                    }
                    else         // that's bp firmware version
                    {

                    }
                    break;
            }
            //            comboBoxStringNumber_SelectedIndexChanged_1(null, null);
            this.Dispatcher.Invoke(() =>
            {
                this.DataContext = null;
                this.DataContext = ListStringInfor[(int)stringnumber_selected - 1];
            });
            if (BPID != 0)
            {


            }

    }
        private void SendQueryMsgTimer_Tick(object sender, EventArgs e)
        {
            //AWEJudge();
            //JudgeDatagriview();
            if (Deviceinfo.devicecanstatus == false)
            {

 //               if (Deviceinfo.canSendErrorTimeout % 300 == 0)
//                    MessageBox.Show(this, " USBCAN is not connected to the device,plz check the wire", "发送失败提示", MessageBoxButtons.OK, MessageBoxIcon.Question);
 //               Deviceinfo.canSendErrorTimeout++;
            }
            else
            {
//                Deviceinfo.canSendErrorTimeout = 0;
            }

            if (EnableListencheckBox.IsChecked == false)
            {
                AdvCan.canmsg_t[] burndatainfo1 = new AdvCan.canmsg_t[1];
                AdvCan.canmsg_t[] burndatainfo2 = new AdvCan.canmsg_t[1];
                AdvCan.canmsg_t[] burndatainfo3 = new AdvCan.canmsg_t[1];
                AdvCan.canmsg_t[] burndatainfo4 = new AdvCan.canmsg_t[1];
                AdvCan.canmsg_t[] burndatainfo5 = new AdvCan.canmsg_t[1];
                AdvCan.canmsg_t[] burndatainfo6 = new AdvCan.canmsg_t[1];
                AdvCan.canmsg_t[] burndatainfo7 = new AdvCan.canmsg_t[1];
                AdvCan.canmsg_t[] burndatainfo8 = new AdvCan.canmsg_t[1];
                AdvCan.canmsg_t[] burndatainfo9 = new AdvCan.canmsg_t[1];
                AdvCan.canmsg_t[] burndatainfo10 = new AdvCan.canmsg_t[1];

                burndatainfo1[0].data = new byte[8];
                burndatainfo2[0].data = new byte[8];
                burndatainfo3[0].data = new byte[8];
                burndatainfo4[0].data = new byte[8];
                burndatainfo5[0].data = new byte[8];
                burndatainfo6[0].data = new byte[8];
                burndatainfo7[0].data = new byte[8];
                burndatainfo8[0].data = new byte[8];
                burndatainfo9[0].data = new byte[8];
                burndatainfo10[0].data = new byte[8];


                Deviceinfo sendDeviceInfo = new Deviceinfo();
                UInt32 stringnumber = Convert.ToUInt32(SCNumberselect.SelectedIndex + 1);
                UInt32 bpnumber = Convert.ToUInt32(BpNumberselect.SelectedIndex + 1);



                sendDeviceInfo.DeviceStruct.DeviceTypeID = (uint)DeviceType.ArrayController;
                sendDeviceInfo.DeviceStruct.StringID = (uint)stringnumber;                               /*暂时 用 广播替代*/
                sendDeviceInfo.DeviceStruct.BpID = bpnumber;
                sendDeviceInfo.DeviceStruct.MessagegroupID = (uint)MessageGroupId.Data;

                sendDeviceInfo.DeviceStruct.MessageID = (uint)StringMessageID.SelectedBP1_4Vol;
                burndatainfo1[0].id = sendDeviceInfo.DeviceStruct.RawID;
                burndatainfo1[0].length = 0;
                burndatainfo1[0].flags = AdvCan.MSG_EXT;
                FillSendBuffer(burndatainfo1);

                sendDeviceInfo.DeviceStruct.MessageID = (uint)StringMessageID.SelectedBP1_4Temp;
                burndatainfo2[0].id = sendDeviceInfo.DeviceStruct.RawID;
                burndatainfo2[0].length = 1;
                burndatainfo2[0].flags = AdvCan.MSG_EXT;
                FillSendBuffer(burndatainfo2);

                sendDeviceInfo.DeviceStruct.MessageID = (uint)ArrayMessageID.EnableBalancing;
                burndatainfo3[0].id = sendDeviceInfo.DeviceStruct.RawID;
                burndatainfo3[0].data[0] = Convert.ToByte(Deviceinfo.BalancingEnable);
                burndatainfo3[0].length = 1;
                burndatainfo3[0].flags = AdvCan.MSG_EXT;
                FillSendBuffer(burndatainfo3);

                sendDeviceInfo.DeviceStruct.MessageID = (uint)StringMessageID.DCBusVol;
                burndatainfo4[0].id = sendDeviceInfo.DeviceStruct.RawID;
                burndatainfo4[0].flags = AdvCan.MSG_EXT;
                FillSendBuffer(burndatainfo4);

                sendDeviceInfo.DeviceStruct.MessageID = (uint)StringMessageID.HLATemp;
                burndatainfo5[0].id = sendDeviceInfo.DeviceStruct.RawID;
                burndatainfo5[0].flags = AdvCan.MSG_EXT;
                FillSendBuffer(burndatainfo5);

                sendDeviceInfo.DeviceStruct.MessageID = (uint)StringMessageID.HLAVol;
                burndatainfo6[0].id = sendDeviceInfo.DeviceStruct.RawID;
                burndatainfo6[0].flags = AdvCan.MSG_EXT;
                FillSendBuffer(burndatainfo6);

                sendDeviceInfo.DeviceStruct.MessageID = (uint)StringMessageID.StringCurrentAHSOC;
                burndatainfo7[0].id = sendDeviceInfo.DeviceStruct.RawID;
                burndatainfo7[0].flags = AdvCan.MSG_EXT;
                FillSendBuffer(burndatainfo7);


                sendDeviceInfo.DeviceStruct.MessageID = (uint)StringMessageID.TargetVol;

                burndatainfo8[0].id = sendDeviceInfo.DeviceStruct.RawID;
                burndatainfo8[0].flags = AdvCan.MSG_EXT;
                burndatainfo8[0].length = 2;
                burndatainfo8[0].data[1] = (byte)Deviceinfo.TargetValue;
                burndatainfo8[0].data[0] = (byte)(Deviceinfo.TargetValue >> 8);

                FillSendBuffer(burndatainfo8);


                sendDeviceInfo.DeviceStruct.MessageID = (uint)StringMessageID.SelectedBPBalancingState;
                burndatainfo9[0].id = sendDeviceInfo.DeviceStruct.RawID;
                burndatainfo9[0].flags = AdvCan.MSG_EXT;
                FillSendBuffer(burndatainfo9);

                sendDeviceInfo.DeviceStruct.MessageID = (uint)StringMessageID.StringKWKWH;
                burndatainfo10[0].id = sendDeviceInfo.DeviceStruct.RawID;
                burndatainfo10[0].flags = AdvCan.MSG_EXT;
                FillSendBuffer(burndatainfo10);

            }
        }


        private void CanPortControlButton_Click(object sender, RoutedEventArgs e)
        {
            if ((string)CanPortControlButton.Content == "Start")
            {
                CanPortControlButton.Content = "Stop";
                QueryThreadExitFlag = true;

                Thread CanReceiveThread = new Thread(FuncCanRecThreadMethod);
                CanReceiveThread.Priority = ThreadPriority.Normal;
                CanReceiveThread.Start();

                SendQueryMsgTimer.Start();
            }
            else  //停止发送/监听
            {
                Deviceinfo.FuncCanReceiveThreadFlag = false;
                CanPortControlButton.Content = "Start";
                SendQueryMsgTimer.Stop();
            }
        }

        private void EnableListencheckBox_Checked(object sender, RoutedEventArgs e)
        {
            if (EnableListencheckBox.IsChecked == false)
            {
                Deviceinfo.EnableListenMode = false;
            }
            else
            {
                Deviceinfo.EnableListenMode = true;
            }
        }

        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            if (EnableBalancingcheckBox.IsChecked == false)
            {
                Deviceinfo.BalancingEnable = false;
            }
            else
            {
                Deviceinfo.BalancingEnable = true;
            }
        }

        private void BpNumberselect_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            bpnumber_selected = (uint)BpNumberselect.SelectedIndex + 1;
            ListVolTemp[bpnumber_selected - 1].Clear();
            for (int j = 0; j < Deviceinfo.CellNumberValue; j++)
                ListVolTemp[bpnumber_selected - 1].Add(new VolTemp { CellNumber = (j + 1).ToString(), Vol = "NotUpdate", Temp = "NotUpdate", BalState = false });
            CellinfodataGridView.ItemsSource = null;
            CellinfodataGridView.ItemsSource = ListVolTemp[bpnumber_selected - 1];

            

            //for (int i = 0; i < Deviceinfo.StringNumberValue; i++)
            //{
            //    ListStringInfor.Add(new stringInformation
            //    {
            //        stringID = i + 1,
            //        stringvoltage = "NotUpdate",
            //        dcbusvoltage = "NotUpdate",
            //        stringcurrent = "NotUpdate",
            //        targetvalue = "NotUpdate",
            //        kwh = "NotUpdate",
            //        kw = "NotUpdate",
            //        contactorclosedpermissionstatus = "NotUpdate",
            //        pcontactorsattus = "NotUpdate",
            //        ncontactorstatus = "NotUpdate",
            //        highvoltage = "NotUpdate",
            //        avevoltage = "NotUpdate",
            //        lowvoltage = "NotUpdate",
            //        hightemp = "NotUpdate",
            //        avetemp = "NotUpdate",
            //        lowtemp = "NotUpdate",
            //    });
            //}
            //this.DataContext = null;
            //this.DataContext = ListStringInfor;
        }

        private void SCNumberselect_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
             
            stringnumber_selected = (uint)SCNumberselect.SelectedIndex + 1;
            //for (int j = 0; j < Deviceinfo.CellNumberValue; j++)
            //    ListVolTemp[bpnumber_selected - 1].Add(new VolTemp { CellNumber = (j + 1).ToString(), Vol = "NotUpdate", Temp = "NotUpdate", BalState = false });
            //CellinfodataGridView.ItemsSource = null;
            //CellinfodataGridView.ItemsSource = ListVolTemp[bpnumber_selected - 1];
            ListStringInfor.First(d => d.stringID == stringnumber_selected).stringvoltage = "NotUpdate";
            ListStringInfor.First(d => d.stringID == stringnumber_selected).dcbusvoltage = "NotUpdate";
            ListStringInfor.First(d => d.stringID == stringnumber_selected).stringcurrent = "NotUpdate";
            ListStringInfor.First(d => d.stringID == stringnumber_selected).targetvalue = "NotUpdate";
            ListStringInfor.First(d => d.stringID == stringnumber_selected).kwh = "NotUpdate";
            ListStringInfor.First(d => d.stringID == stringnumber_selected).kw = "NotUpdate";
            ListStringInfor.First(d => d.stringID == stringnumber_selected).contactorclosedpermissionstatus = "NotUpdate";
            ListStringInfor.First(d => d.stringID == stringnumber_selected).pcontactorsattus = "NotUpdate";
            ListStringInfor.First(d => d.stringID == stringnumber_selected).ncontactorstatus = "NotUpdate";
            ListStringInfor.First(d => d.stringID == stringnumber_selected).highvoltage = "NotUpdate";
            ListStringInfor.First(d => d.stringID == stringnumber_selected).avevoltage = "NotUpdate";
            ListStringInfor.First(d => d.stringID == stringnumber_selected).lowvoltage = "NotUpdate";
            ListStringInfor.First(d => d.stringID == stringnumber_selected).hightemp = "NotUpdate";
            ListStringInfor.First(d => d.stringID == stringnumber_selected).avetemp = "NotUpdate";
            ListStringInfor.First(d => d.stringID == stringnumber_selected).lowtemp = "NotUpdate";

            
            this.DataContext = null;
            this.DataContext = ListStringInfor[(int)stringnumber_selected - 1];
        }

        private void tabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            PageSelected = tabControl.SelectedIndex;
            if (tabControl.SelectedIndex == 0)  //upgrade page
            {
                Deviceinfo.FuncCanReceiveThreadFlag = false;                    
            }
            else                                //function page
            {
                QueryThreadExitFlag = true;
                Deviceinfo.FuncCanReceiveThreadFlag = true;
            }

        }
    }
}
