using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO.Ports;
using System.Linq;
using System.Text;

namespace SerialPortReceiver
{
    internal class Serial
    {
        private readonly SerialPort _serialPort = new SerialPort();
        public IEnumerable<string> PortNames => SerialPort.GetPortNames();

        /// <summary>
        /// COMポートオープン
        /// </summary>
        /// <param name="port"></param>
        /// <returns></returns>
        public bool Open(string port)
        {
            _serialPort.PortName = port;
            try
            {
                _serialPort.ReadTimeout = 10000;
                _serialPort.WriteTimeout = 10000;
                _serialPort.Open();
                _serialPort.DiscardInBuffer();
                _serialPort.DiscardOutBuffer();
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                return false;
            }
            return true;
        }

        /// <summary>
        /// COMポートクローズ
        /// </summary>
        /// <returns></returns>
        public bool Close()
        {
            try
            {
                _serialPort.Close();
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                return false;
            }
            return true;
        }

        /// <summary>
        /// 受信イベント登録
        /// </summary>
        /// <param name="OnReceived"></param>
        public void SetReceiveEvent(SerialDataReceivedEventHandler OnReceived)
        {
            _serialPort.DataReceived += OnReceived;
        }

        #region Byte
        /// <summary>
        /// Byte送信(ASCII)
        /// </summary>
        /// <param name="src"></param>
        public void Send(IEnumerable<byte> src)
        {
            if (!_serialPort.IsOpen) return;
            try
            {
                // STX, ETX付与
                var data = new byte[] { 0x02 }
                    .Concat(src)
                    .Concat(new byte[] { 0x03 });
                _serialPort.Write(data.ToArray(), 0, data.Count());
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }

        /// <summary>
        /// Byte受信(ASCII)
        /// </summary>
        /// <returns></returns>
        public List<byte> Receive()
        {
            var data = new List<byte>();
            if (!_serialPort.IsOpen) return data;

            var isStx = false;
            // 受信データ取得
            while (true)
            {
                try
                {
                    var d = (byte)_serialPort.ReadByte();
                    // STXが来るまでは、バッファを読み飛ばす
                    if (d == 0x02)
                        isStx = true;
                    else if (isStx)
                        // ETXが来たら終了
                        if (d == 0x03)
                            break;
                        else
                            data.Add(d);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.Message);
                    return new List<byte>();
                }
            }

            // ASCII → byte変換
            var ret = new List<byte>();
            if (data.Count >= 1)
                for (var i = 0; i < data.Count(); i += 2)
                {
                    var tmp = new byte[] { data[i], data[i + 1] };
                    var tmp2 = Encoding.ASCII.GetString(tmp);
                    var tmp3 = byte.Parse(tmp2, NumberStyles.HexNumber);
                    ret.Add(tmp3);
                }
            return ret;
        }
        #endregion


        #region String
        /// <summary>
        /// 文字列送信
        /// </summary>
        /// <param name="data"></param>
        public void SendString(string data)
        {
            if (!_serialPort.IsOpen) return;
            try
            {
                _serialPort.WriteLine(data);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }

        /// <summary>
        /// 文字列受信
        /// </summary>
        /// <returns></returns>
        public string ReceiveString()
        {
            var ret = string.Empty;
            if (!_serialPort.IsOpen) return ret;
            try
            {
                ret = _serialPort.ReadLine();
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
            return ret;
        }
        #endregion


    }
}
