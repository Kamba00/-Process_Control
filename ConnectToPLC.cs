using System;
using System.IO;
using System.Collections.Generic;
using UnityEngine;
using Sharp7;
using Newtonsoft.Json;
using System.Threading;
using System.Text.RegularExpressions;
using System.Text;
namespace Sharp7
{
    public enum SignalType { Bool, Int, Real }

    public class ConnectToPLC : MonoBehaviour
    {
        [SerializeField] string ipAddress;
        [SerializeField] int bufferLength;
        [SerializeField] int db_num = 1;
        [SerializeField] short startByte = 0;
        [SerializeField] public static Dictionary<string, IIoTSignal> apcsSignals = new Dictionary<string, IIoTSignal>();
        [SerializeField] string signals_data_path = "Assets/SignalData.json";
        S7Client plc;
        byte[] byte_buffer;

        struct Byte_Bit
        {
            public short byte_num;
            public short bit_num;
            public SignalType datatype;
            public short s7Area;
        }

        public delegate void ForwardValueHandler(string label, string value, SignalType signalType);
        public static event ForwardValueHandler ForwardValue;

        Timer S7PollTimer;

        void Start()
        {
            readJsonFile();
            SetupPLC();
        }

        void readJsonFile()
        {
            var result = JsonConvert.DeserializeObject<Dictionary<string, IIoTSignal>>(File.ReadAllText(signals_data_path));
            foreach (IIoTSignal item in result.Values)
            {
                apcsSignals.Add(item.label, item);

            }
        }

        /**
        * Establish Connection with PLC
        */
        void SetupPLC()
        {
            plc = new S7Client();
            plc.ConnectTo(ipAddress, 0, 1);
            Debug.Log($"PLC CONNECTION STATUS: {plc.Connected}");
            byte_buffer = new byte[bufferLength];
            S7PollTimer = new Timer(new TimerCallback(PollPLC), null, 2000, 50);
        }


        void PollPLC(object state)
        {
            byte_buffer = new byte[bufferLength];
            plc.ReadArea(S7Consts.S7AreaDB, db_num, startByte, bufferLength, S7Consts.S7WLByte, byte_buffer);
            foreach (var _signal in apcsSignals.Values)
            {
                // var _readVal = plc.ReadTag( _signal.address, _signal.type , true);
                var _readVal = _ExtractValue(_signal.address, byte_buffer);

                if (_readVal != _signal.value)
                {
                    OnForwardValue(_signal.label, _readVal, _signal.type);
                    _signal.value = _readVal;
                }
            }
        }

        public void PLCWrite(string _signal, string value)
        {
            WriteTag(apcsSignals[_signal].address, value);
        }

        private string _ExtractValue(string _address, byte[] _byteArray)
        {

            string _value;
            Byte_Bit bytbit = decodeTag(_address);
            switch (bytbit.datatype)
            {
                case SignalType.Bool:
                    _value = S7.GetBitAt(_byteArray, bytbit.byte_num, bytbit.bit_num).ToString();
                    break;

                case SignalType.Int:
                    _value = S7.GetIntAt(_byteArray, bytbit.byte_num).ToString();
                    break;

                case SignalType.Real:
                    _value = S7.GetRealAt(_byteArray, bytbit.byte_num).ToString();
                    break;

                default:
                    _value = "";
                    break;
            }

            return _value;
        }

        private void WriteTag(string address, string value)
        {
            Byte_Bit bytbit = decodeTag(address);
            switch (bytbit.datatype)
            {
                case SignalType.Bool:
                    S7.SetBitAt(ref byte_buffer, bytbit.byte_num, bytbit.bit_num, bool.Parse(value));
                    break;

                case SignalType.Int:
                    S7.SetIntAt(byte_buffer, bytbit.byte_num, (short)float.Parse(value));
                    break;

                case SignalType.Real:
                    S7.SetRealAt(byte_buffer, bytbit.byte_num, float.Parse(value));
                    break;

                default:
                    break;
            }

            // Write the byte buffer to the PLC
            plc.WriteArea(
                S7Consts.S7AreaDB,
                db_num,
                startByte,
                bufferLength,
                S7Consts.S7WLByte,
                byte_buffer
            );
        }


        private Byte_Bit decodeTag(string _address)
        {
            Byte_Bit _bbnum = new Byte_Bit();

            if (RegeXTags.DBBoolRegX.IsMatch(_address))
            {
                _bbnum.byte_num = Int16.Parse(_address.Split('.')[1].Substring(3));
                _bbnum.bit_num = Int16.Parse(_address.Split('.')[2]);
                _bbnum.s7Area = S7Consts.S7AreaDB;
                _bbnum.datatype = SignalType.Bool;

            }
            else if (RegeXTags.DBIntRegX.IsMatch(_address))
            {
                _bbnum.byte_num = short.Parse(_address.Split('.')[1].Substring(3));
                _bbnum.s7Area = S7Consts.S7AreaDB;
                _bbnum.datatype = SignalType.Int;
            }
            else if (RegeXTags.DBRealRegX.IsMatch(_address))
            {
                _bbnum.byte_num = short.Parse(_address.Split('.')[1].Substring(3));
                _bbnum.s7Area = S7Consts.S7AreaDB;
                _bbnum.datatype = SignalType.Real;
            }
            return _bbnum;
        }

        /**
         * Invokes the ForwardValue Event Handler
         */
        protected virtual void OnForwardValue(string _signal_label, string _value, SignalType _type)
        {
            if (ForwardValue != null)
            {
                ForwardValue(_signal_label, _value, _type);
            }
        }
    }

    public class IIoTSignal
    {
        public string label { get; set; }
        public SignalType type { get; set; }
        public string value { get; set; }
        public string address { get; set; }
        public string topic { get; set; }
        public string nodeid { get; set; }

    }

    public class RegeXTags
    {
        private static string memBoolPat = "^[M,m]{1}[0-9]{1,3}\\.[0-7]{1}$"; public static Regex memBoolRegX = new Regex(memBoolPat);
        private static string inpBoolPat = "^[I,i]{1}[0-9]{1,3}\\.[0-7]{1}$"; public static Regex inpBoolRegX = new Regex(inpBoolPat);
        private static string outBoolPat = "^[Q,q]{1}[0-9]{1,3}\\.[0-7]{1}$"; public static Regex outBoolRegX = new Regex(outBoolPat);
        private static string memIntPat = "^[M,m]{1}[W,w]{1}[0-9]{1,3}$"; public static Regex memIntRegX = new Regex(memIntPat);
        private static string inpIntPat = "^[I,i]{1}[W,w]{1}[0-9]{1,3}$"; public static Regex intIntRegX = new Regex(inpIntPat);
        private static string outIntPat = "^[Q,q]{1}[W,w]{1}[0-9]{1,3}$"; public static Regex outIntRegX = new Regex(outIntPat);
        private static string memRealPat = "^[M,m]{1}[D,d]{1}[0-9]{1,3}$"; public static Regex memRealRegX = new Regex(memRealPat);
        private static string DBBoolPat = "^[D,d]{1}[B,b]{1}[0-9]{1,2}\\.[D,d]{1}[B,b]{1}[X,x]{1}[0-9]{1,3}\\.[0-7]{1}$"; public static Regex DBBoolRegX = new Regex(DBBoolPat);
        private static string DBIntPat = "^[D,d]{1}[B,b]{1}[0-9]{1,2}\\.[D,d]{1}[B,b]{1}[W,w]{1}[0-9]{1,3}$"; public static Regex DBIntRegX = new Regex(DBIntPat);
        private static string DBRealPat = "^[D,d]{1}[B,b]{1}[0-9]{1,2}\\.[D,d]{1}[B,b]{1}[D,d]{1}[0-9]{1,3}$"; public static Regex DBRealRegX = new Regex(DBRealPat);
    }
}