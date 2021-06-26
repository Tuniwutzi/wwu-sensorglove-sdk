using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SensorhandSDK
{
    public class SensorDataEventArgs : EventArgs
    {
        public uint Id;
        public byte[] Sensors;

        public const uint NoId = 0;
        private static uint id = 1;
        public SensorDataEventArgs(byte[] data)
        {
            if (data.Length != SensorDataSource.SensorCount)
                throw new Exception("Invalid length of sensor data");

            this.Id = id++;
            this.Sensors = data;
        }
    }
    public class SensorErrorEventArgs : EventArgs
    {
        public Exception Error { get; private set; }
        public SensorErrorEventArgs(Exception ex)
        {
            this.Error = ex;
        }
    }

    public abstract class SensorDataSource : IDisposable
    {
        public const int SensorCount = 14;
        public const int BytesPerSensor = sizeof(byte);
        public const int PacketStart = 0xff;


        public abstract event EventHandler<SensorDataEventArgs> OnSensorDataUpdate;
        public abstract event EventHandler OnDisconnected; //SensorDataSource gilt als getrennt, wenn keine Daten mehr gelesen werden koennen (Arduino nicht mehr angeschlossen, Datei zuende, ...)
        public abstract event EventHandler<SensorErrorEventArgs> OnError;

        public virtual bool Connected { get; protected set; }


        public SensorDataSource()
        {
            this.Connected = false;
        }


        public abstract void Connect(string port);
        public abstract void Connect();

        public abstract void SetCalibrating(bool calibrating);
        public abstract bool IsCalibrating();
        public abstract void ResetCalibration();

        public abstract void Dispose();
    }
}
