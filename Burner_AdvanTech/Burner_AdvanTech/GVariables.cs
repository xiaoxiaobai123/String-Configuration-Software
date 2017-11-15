using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
 
using System.IO;
using Burner_AdvanTech;

namespace BurnerWinform
{
    class GVariables
    {

    }

    public class stringInformation
    {
        public int stringID { get; set; }
        public string stringvoltage { get; set; }
        public string dcbusvoltage { get; set; }
        public string stringcurrent { get; set; }
        public string targetvalue { get; set; }
        public string kwh { get; set; }
        public string kw { get; set; }
        public string pcontactorsattus { get; set; }
        public string ncontactorstatus { get; set; }

        public string contactorclosedpermissionstatus { get; set; }
        public string highvoltage { get; set; }
        public string avevoltage { get; set; }
        public string lowvoltage { get; set; }
        public string hightemp { get; set; }
        public string avetemp { get; set; }
        public string lowtemp { get; set; }

    }

    public class Nodeinfo
     {
        public string NodeAddress{ set; get; }
        public string FirmwareType { set; get; }
        public string FirmwareVersion { set; get; }
        public bool OnlineStatus { set; get; }       

    }
    public enum FirmwareSection
    {
        Ap = 0x02,
        Booter = 0x01,
    }
    public enum MessageGroupId  
    {
        Alarm = 0x00,
        Warning = 0x01,
        Error = 0x02,
        Command = 0x03,
        Data = 0x04,
        Configuration = 0x05,
        FirmwareUpdate = 0x06,
    }

    public enum DeviceType
    {
        SystemController = 0x01,
        ArrayController = 0x02,
        RelayController = 0x03,
        StringController = 0x04,
        BatteryPackController = 0x05,
    }
    public enum ArrayMessageID : byte
    {
        EnableBalancing = 0x57,
        UpgradeDatas = 0x78,
        UpgradeQueryVersion = 0x79,
        UpgradeSelectDevice = 0x80,
    }
    public enum StringMessageID : byte
    {
        DCBusVol = 0x09,
        SocKwhAh = 0x0F,
        ResendAWE = 0x11,
        TargetVol = 0x20,
        StringCurrentAHSOC = 0x51,
        HLAVol = 0x52,
        HLATemp = 0x53,
        StringKWKWH = 0x54,
        BalancingChargerOnTime = 0x55,
        SelectedBP1_4Vol = 0xA2,
        SelectedBP5_8Vol = 0xA3,
        SelectedBP9_12Vol = 0xA4,
        SelectedBP13_16Vol = 0xA5,
        SelectedBP1_4Temp = 0xA6,
        SelectedBP5_8Temp = 0xA7,
        SelectedBP9_12Temp = 0xA8,
        SelectedBP13_16Temp = 0xA9,
        SelectedBPBalancingState = 0xAA
    }
    class Deviceinfo
    {
        public static int BurnePage;
        public static  int TotalOnline;
        public static int canSendErrorTimeout
        {
            set;get;
        }
        public static bool devicecanstatus
        {
            set; get;
        } = true;
        public static bool FuncCanReceiveThreadFlag
        {
            set; get;
        } = true;
        public static bool EnableListenMode
        {
            set; get;
        } = true;
        public static bool BalancingEnable
        {
            set; get;
        } = true;
        public static int TargetValue
        {
            set;get;
        }  
        public static int CellNumberValue
        {
            set; get;
        } = new Global().cellnumber;
        public static int BPNumberValue
        {
            set; get;
        } = new Global().bpnumber;
        public static int StringNumberValue
        {
            set; get;
        } = new Global().stringnumber;

        public static string CanPort
        {
            set; get;
        } = new Global().CanPort;

        public static uint MaxMsgCount
        {
            set; get;
        }= Convert.ToUInt32(new Global().CanMaxMsgCount);

        public static string CanBaudrate
        {
            set; get;
        } = new Global().CanBaudrate;

        public static string CanWriteTimeOut
        {
            set; get;
        } = new Global().CanWriteTimeOut;

        public static string CanReadTimeOut
        {
            set; get;
        } = new Global().CanReadTimeOut;

        int DeviceID { set; get; }
         int FirmwareType { set; get; }
         int FirmwareVersion { set; get; }

        public struct MyStruct
        {
            internal uint RawID;

            const int sz0 = 8, loc0 = 0, mask0 = ((1 << sz0) - 1) << loc0;
            const int sz1 = 6, loc1 = loc0 + sz0, mask1 = ((1 << sz1) - 1) << loc1;
            const int sz2 = 4, loc2 = loc1 + sz1, mask2 = ((1 << sz2) - 1) << loc2;
            const int sz3 = 4, loc3 = loc2 + sz2, mask3 = ((1 << sz3) - 1) << loc3;
            const int sz4 = 4, loc4 = loc3 + sz3, mask4 = ((1 << sz4) - 1) << loc4;

            public uint MessageID
            {
                get { return (uint)(RawID & mask0) >> loc0; }
                set { RawID = (uint)(RawID & ~mask0 | (value << loc0) & mask0); }
            }

            public uint BpID
            {
                get { return (uint)(RawID & mask1) >> loc1; }
                set { RawID = (uint)(RawID & ~mask1 | (value << loc1) & mask1); }
            }

            public uint StringID
            {
                get { return (uint)(RawID & mask2) >> loc2; }
                set { RawID = (uint)(RawID & ~mask2 | (value << loc2) & mask2); }
            }

            public uint DeviceTypeID
            {
                get { return (uint)((RawID & mask3) >> loc3); }
                set { RawID = (uint)(RawID & ~mask3 | (value << loc3) & mask3); }
            }

            public uint MessagegroupID
            {
                get { return (uint)((RawID & mask4) >> loc4); }
                set { RawID = (uint)(RawID & ~mask4 | (value << loc4) & mask4); }
            }
        }

        public  MyStruct DeviceStruct;
    }

    public enum UpgradeMessageID 
    {
        RestartMsg = 0x80,
        QueryAckMsg = 0x79,
        BurnMsg = 0x78,
    }

    public enum UpgradeDataField:byte
    {
        AckMsg1 = 0x79,
        QueryMsg = 0x05,
        AckMsg2 = 0x06,
        WrongAckMsg = 0xFF,
        ResartMsg = 0x01,
        VersionQueryMsg = 0x00,
        BpTypeMsg = 0x02,
        StringTypeMsg = 0x03,
        Querymsg1 = 0xF1,
        CrcCheck = 0x04,
    }

    public enum burnerstatus
    {
        WaitSendBurnerAddress = 0x00,
        SendBurnerAddress = 0x01,
        WaitSendBurnerDatas = 0x02,
        SendBunerDatas = 0x03,
        WaitDeviceReadytoBurn = 0x4,
        DeviceReadytoBurn = 0x5,
        
        NormalWork = 0xFF,
    }

    public class TimeCount
    {
        public DateTime TimeStart { set; get; }
        
        public TimeCount()
        {
            TimeStart = new DateTime();
        }
    }

    public class binfileoperation
    {
        public BinaryReader br { set; get; }

        public binfileoperation()
        {
          //  br = new BinaryReader();
        }

    }
    class GCanDrvier
    {
        public static bool CanDriverStatusChanged
        {
            set; get;
        } = false;

        public static bool CanDetected
        {
            set; get;
        } = false;
    }

    class Burner
    {
        public  ulong StartAddress { set; get; } = new Global().APStartAddress;
        public static ulong EndAddress { set; get; }
        public  int Status { set; get; }   

        public  int OldStatus { set; get; }   

        public static int burnbyte = 2048; /*一次烧写2048byte*/

        public static int upgradeFirmwareByteLeft { get; set; }

        public  int succeedburnedSectionAmount { get; set; } = 0;

        public  bool upgradingnotsuccess { get; set; } = true;

        public  static int FirmwareFileSize { get; set; } 

        
    }

    class TextHint : INotifyPropertyChanged
    {
        public  static string hint;
        public  string Hint
        {
            get { return hint;}
            set
            {
                hint= value;
                InvokePropertyChanged(new PropertyChangedEventArgs("Hint"));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public void  InvokePropertyChanged(PropertyChangedEventArgs e)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
                handler(this, e);                              
        }
    }


    public class VolTemp
    {
        public string CellNumber { set; get; }
        public string Vol { set; get; }
        public string Temp { set; get; }

        public string BalState { set; get; }
    }



}
