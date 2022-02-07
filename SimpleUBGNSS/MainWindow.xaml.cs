using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO.Ports;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
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

namespace SimpleUBGNSS
{
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        private CancellationTokenSource cts;
        private bool isRunning;
        private UbxNavPosLLH ubxNavPosLLH;

        public event PropertyChangedEventHandler PropertyChanged;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void NotifyPropertyChanged([CallerMemberName] string propertyName = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        [ReadOnly(true), Bindable(BindableSupport.Yes)]
        public UbxNavPosLLH UbxNavPosLLH
        {
            get => ubxNavPosLLH;
            private set
            {
                ubxNavPosLLH = value;
                Dispatcher.Invoke(() => NotifyPropertyChanged());
            }
        }

        public uint IToW { get; private set; }

        public MainWindow() => InitializeComponent();

        private async void BtnStart_Click(object sender, RoutedEventArgs e)
        {
            if (isRunning)
            {
                isRunning = false;
                cts.Cancel();
                cts.Dispose();
            }
            else
            {
                cts = new();
                isRunning = true;
                await Task.Run(() => ReadCOM("COM8", cts.Token));
            }
        }


        [SkipLocalsInit(), MethodImpl(MethodImplOptions.AggressiveOptimization)]
        private unsafe Task ReadCOM(string portName, CancellationToken cancellationToken)
        {
            using SerialPort port = new(portName, 115200)
            {
                Encoding = Encoding.Latin1,
            };

            port.Open();
            port.DiscardInBuffer();
            port.ReadTo("µb");

            byte[] buffer = GC.AllocateUninitializedArray<byte>(64, pinned: true);
            byte* p = (byte*)Unsafe.AsPointer(ref buffer[0]);

            MessageHeader* messageHeader = (MessageHeader*)p;
            UbxNavPosLLH* posLLH = (UbxNavPosLLH*)(p + 4);

            while (!cancellationToken.IsCancellationRequested)
            {
                ReadPort(buffer, 0, MessageHeader.Size, port);
                ReadPort(buffer, MessageHeader.Size,  messageHeader->LengthToReadWithFlatcher, port);
                if (!Fletcher(buffer, messageHeader->LengthToTest))
                {
                    port.ReadTo("µb");
                    continue;
                }

                switch (messageHeader->MessageType)
                {
                    case UbxType.UbxNavPosLLH:
                        UbxNavPosLLH = *posLLH;
                        break;
                }
                ReadPort(buffer, 0, 2, port);
            }

            port.Close();
            return Task.CompletedTask;
        }

        [SkipLocalsInit(), MethodImpl(MethodImplOptions.AggressiveOptimization | MethodImplOptions.AggressiveInlining)]
        private static bool Fletcher(byte[] buffer, int count)
        {
            //Debug.Assert(buffer is not null);
            //Debug.Assert(offset >= 0);
            //Debug.Assert(count >= 1);
            //Debug.Assert((2 + count + offset) < buffer.Length);
            
            byte sum1 = 0, sum2 = 0;
            int offset = 0;
            do
            {
                sum2 += sum1 += buffer[offset];
            }
            while (++offset != count);
            return buffer[offset++] == sum1 && buffer[offset] == sum2;
        }

        [SkipLocalsInit(), MethodImpl(MethodImplOptions.AggressiveOptimization | MethodImplOptions.AggressiveInlining)]
        private static void ReadPort(byte[] buffer, int offset, int count, SerialPort port)
        {
            int readCount;
            do
            {
                offset += readCount = port.Read(buffer, offset, count);
            }
            while ((count -= readCount) != 0);
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            if (isRunning)
            {
                isRunning = false;
                cts.Cancel();
                cts.Dispose();
            }
        }
    }
}
