/// <author>
/// Jan Buenker
/// <author>

using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading;

namespace SensorhandSDK
{
    public class SerialSensorSource : SensorDataSource
    {
        public override event EventHandler<SensorDataEventArgs> OnSensorDataUpdate;
        public override event EventHandler OnDisconnected;
        public override event EventHandler<SensorErrorEventArgs> OnError;

        public override bool Connected
        {
            get
            {
                return this.serialPort != null;
            }
            protected set
            {
            }
        }

        private SerialPort serialPort;
        private Thread serialReader;
        private object serialLock;

        private bool isCalibrating = true;

        public SerialSensorSource()
        {
            this.serialPort = null;
            this.serialReader = null;
            this.serialLock = new object();
        }


        private int doRead(byte[] buffer, int packetSize, int offset, bool synchronized)
        {
            var read = offset;
            if (!synchronized)
            {
                while (this.serialPort.ReadByte() != SensorDataSource.PacketStart) ;

                // Separator was already read and discarded, so write the contents from index 1 onwards to the buffer
                read++;
            }

            while (read < packetSize)
                read += this.serialPort.Read(buffer, read, buffer.Length - read);

            return read;
        }
        //TODO: Exceptions
        private void read()
        {
            bool synchronized = false;

            var packetSize = ((SensorDataSource.SensorCount * SensorDataSource.BytesPerSensor) + 1);
            // Buffer for two packets
            var bytes = new byte[packetSize * 2];
            int read = 0;
            while (true)
            {
                var parseOffset = 1;
                lock (this.serialLock)
                {
                    if (this.Connected)
                    {
                        try
                        {
                            read = this.doRead(bytes, packetSize, read, synchronized);
                            synchronized = true;
                        }
                        catch (Exception ex)
                        {
                            this.doDispose();

                            if (this.OnError != null)
                                this.OnError(this, new SensorErrorEventArgs(ex));
                            if (this.OnDisconnected != null)
                                this.OnDisconnected(this, EventArgs.Empty);

                            return;
                        }

                        if (read > packetSize && read < bytes.Length)
                        {
                            // We received more than one packet, but not a full second packet

                            // Process the first packet, discard further data to get an up-to-date packet next time
                            //this.serialPort.DiscardInBuffer();
                            //synchronized = false;

                            // Correction: do NOT discard all further data
                            read = read - packetSize;
                            for (int i = 0; i < read; i++)
                            {
                                bytes[i] = bytes[i + packetSize];
                            }
                        }
                        else if (read == bytes.Length)
                        {
                            // 2 full packets were received, more could be in the buffer
                            
                            // Process the second packet and discard the remaining buffer, to get an up-to-date packet next time
                            parseOffset += packetSize;
                            read = 0;
                            this.serialPort.DiscardInBuffer();
                            synchronized = false;
                        }
                        else// if (read == packetSize)
                            read = 0;
                        //else if (read < packetSize) is impossible
                    }
                    else
                    {
                        if (this.OnDisconnected != null)
                            this.OnDisconnected(this, EventArgs.Empty);
                        break;
                    }
                }

                var package = new byte[SensorDataSource.SensorCount];
                for (var i = 0; i < SensorDataSource.SensorCount; i++)
                    package[i] = bytes[i + parseOffset];

                if (this.OnSensorDataUpdate != null)
                    this.OnSensorDataUpdate(this, new SensorDataEventArgs(package));
            }
        }

        // Does not lock
        private void doDispose()
        {
            if (this.serialPort != null)
            {
                this.serialPort.Dispose();
                this.serialLock = null;
            }

            this.serialReader = null;
        }

        public override void SetCalibrating(bool calibrating)
        {
            if (this.Connected)
            {
                this.isCalibrating = calibrating;
                lock (this.serialLock)
                {
                    byte[] buffer = new byte[] { 0x00, 0x01 };
                    this.serialPort.Write(buffer, calibrating ? 1 : 0, 1);
                }
            }
            else
                throw new InvalidOperationException();
        }
        public override bool IsCalibrating()
        {
            if (this.Connected)
                return this.isCalibrating;
            else
                throw new InvalidOperationException();
        }
        public override void ResetCalibration()
        {
            if (this.Connected)
            {
                lock (this.serialLock)
                {
                    byte[] buffer = new byte[] { 0x02 };
                    this.serialPort.Write(buffer, 0, 1);
                }
            }
            else
                throw new InvalidOperationException();
        }

        public override void Connect(string port)
        {
            lock (this.serialLock)
            {
                if (this.Connected)
                    throw new Exception("SensorGlove is already connected");

                try
                {
                    this.serialPort = new SerialPort(port, 9600);
                    this.serialPort.Open();

                    this.serialReader = new Thread(read);
                    this.serialReader.Start();
                }
                catch (Exception)
                {
                    if (this.serialPort != null)
                    {
                        this.serialPort.Dispose();
                        this.serialPort = null;

                        this.serialReader = null;
                    }
                    throw;
                }
            }
        }

        // Automatically detects the serial port
        public override void Connect()
        {
            if (this.Connected)
                throw new Exception("SensorGlove is already connected");

            var portOptions = SerialPort.GetPortNames();
            foreach (var port in portOptions)
            {
                try
                {
                    this.Connect(port);
                    return;
                }
                catch (Exception)
                {
                }
            }

            throw new Exception("Could not find connected Port");
        }

        public override void Dispose()
        {
            lock (this.serialLock)
            {
                this.doDispose();
            }
        }
    }
}
