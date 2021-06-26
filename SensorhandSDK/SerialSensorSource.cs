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

                //Trennzeichen wurfe bereits gelesen und verworfen - schreibe den Inhalt des Pakets ab Stelle 1 in den Buffer
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
            //Buffer für zwei Pakete
            var bytes = new byte[packetSize * 2];
            int read = 0;
            while (true)
            {
                //Offset im Buffer, bei dem mit dem Parsen angefangen wird
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
                            //Es wurde mehr als ein Paket empfangen, aber noch kein zweites komplett
                            //=> Verarbeite das erste Paket, verwirf alle weiteren Daten um beim nächsten Mal ein aktuelles Paket zu bekommen

                            //this.serialPort.DiscardInBuffer();
                            //synchronized = false;


                            //Korrektur: Verwirf NICHT alle weiteren Daten
                            read = read - packetSize;
                            for (int i = 0; i < read; i++)
                            {
                                bytes[i] = bytes[i + packetSize];
                            }
                        }
                        else if (read == bytes.Length)
                        {
                            //Es wurden 2 Pakete vollstaendig empfangen, es koennte noch mehr im Buffer liegen
                            //=> Verarbeite das zweite Paket und verwirf den restlichen Buffer, um im naechsten Schritt wieder aktuell zu sein

                            //Console.WriteLine("DISCARD");
                            parseOffset += packetSize;
                            read = 0;
                            this.serialPort.DiscardInBuffer();
                            synchronized = false;
                        }
                        else// if (read == packetSize)
                            read = 0;
                        //else if (read < packetSize) ist nicht moeglich
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

        //Lockt nicht
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

        // Sucht automatisch den Port
        public override void Connect()
        {
            if (this.Connected)
                throw new Exception("SensorGlove is already connected");

            //this.Connect("COM3");
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
