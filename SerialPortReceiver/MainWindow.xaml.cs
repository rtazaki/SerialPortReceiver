using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Windows;

namespace SerialPortReceiver
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly Serial _serial = new Serial();
        public MainWindow()
        {
            InitializeComponent();
            // COMポート名の取得
            ComPort.ItemsSource = _serial.PortNames;
            ComPort.SelectedIndex = 0;
            // ヘッダ
            var header = "00 01 02 03 04 05 06 07 08 09 0A 0B 0C 0D 0E 0F \n";
            ReceiveHeader.Content = header;
            SendHeader.Content = header;
            // 送信データ
            var sendData = Enumerable.Range(0, 256).Select(x => (byte)x);
            SendData.Text = FormatData(sendData);
            // 受信イベントの登録
            _serial.SetReceiveEvent(OnReceived);
        }

        /// <summary>
        /// 送信ボタン
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Send_Click(object sender, RoutedEventArgs e)
        {
            ComSend.IsEnabled = false;
            Send();
        }

        /// <summary>
        /// 送信処理
        /// </summary>
        private void Send()
        {
            if (ByteR.IsChecked == true)
            {
                var data = SendData.Text.Split()
                    .SelectMany(x => Encoding.ASCII.GetBytes(x));
                _serial.Send(data);
            }

            if (StringR.IsChecked == true)
            {
                var data = SendData.Text.Replace("\n", "_");
                _serial.SendString(data);
            }
        }

        /// <summary>
        /// 受信イベント
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnReceived(object sender, SerialDataReceivedEventArgs e)
        {
            ReceiveData.Dispatcher.Invoke(
                new Action(() =>
                {
                    var data = string.Empty;
                    if (ByteR.IsChecked == true)
                    {
                        data = FormatData(_serial.Receive());
                    }
                    if (StringR.IsChecked == true)
                    {
                        data = _serial.ReceiveString().Replace("_", "\n");
                    }
                    ReceiveData.Text = data;
                    // ワンショットの場合、データ送信ボタンのみ活性化
                    if (OneShot.IsChecked == true)
                        ComSend.IsEnabled = true;
                    // 受信契機で送信処理を実施する。
                    else
                        Send();
                }));
        }

        /// <summary>
        /// 16Byteごとに改行
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        private string FormatData(IEnumerable<byte> data)
        {
            var ret = string.Empty;
            foreach (var (v, i) in data.Select((v, i) => (v, i)))
            {
                ret += $"{v:X2} ";
                if ((i + 1) % 16 == 0) ret += "\n";
            }
            return ret;
        }

        /// <summary>
        /// COMポートOPEN
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Open_Click(object sender, RoutedEventArgs e)
        {
            if (ComPort.Text == "") return;
            if (_serial.Open(ComPort.Text))
            {
                ComPort.IsEnabled = false;
                ComOpen.IsEnabled = false;
                ComClose.IsEnabled = true;
            }
        }

        /// <summary>
        /// COMポートClose
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Close_Click(object sender, RoutedEventArgs e)
        {
            if (_serial.Close())
            {
                ComPort.IsEnabled = true;
                ComOpen.IsEnabled = true;
                ComClose.IsEnabled = false;
            }
        }

        /// <summary>
        /// ウィンドウ終了
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Window_Closed(object sender, EventArgs e)
        {
            _serial.Close();
        }

        /// <summary>
        /// ワンショット有効
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OneShot_Checked(object sender, RoutedEventArgs e)
        {
            Title = "送信 ⇒ 受信";
            ComSend.IsEnabled = true;
        }

        /// <summary>
        /// ワンショット無効
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OneShot_Unchecked(object sender, RoutedEventArgs e)
        {
            Title = "受信 ⇒ 送信";
            ComSend.IsEnabled = false;
        }
    }
}
