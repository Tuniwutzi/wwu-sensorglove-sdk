/// <author>
/// Jan Buenker
/// <author>

using SensorhandSDK.ANN;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SensorhandSDK
{
    public class SensorGlove : IDisposable
    {
        public string DataSourcePath { get; private set; }
        public string NNWeightsPathName { get; private set; }

        public HandPose LatestPose
        {
            get
            {
                return this.processor.CurrentPose;
            }
        }
        public bool Calibrating
        {
            get
            {
                return this.source.IsCalibrating();
            }
            set
            {
                this.source.SetCalibrating(value);
            }
        }

        private SensorDataSource source;
        private NeuralNetwork neuralNetwork;
        private SensorDataProcessor processor;

        public SensorGlove(string nnWeightsPathName, string dataSourcePath, SensorDataSource dataSource)
        {
            this.DataSourcePath = dataSourcePath;
            this.NNWeightsPathName = nnWeightsPathName;

            SensorhandSDK.ANN.Settings set;
            System.Xml.Serialization.XmlSerializer reader = new System.Xml.Serialization.XmlSerializer(typeof(SensorhandSDK.ANN.Settings));
            using (System.IO.StreamReader file = new System.IO.StreamReader(this.NNWeightsPathName))
            {
                set = (SensorhandSDK.ANN.Settings)reader.Deserialize(file);
            }

            this.source = dataSource;
            this.neuralNetwork = new NeuralNetwork(set);
            this.processor = new SensorDataProcessor(this.source, this.neuralNetwork);
        }
        public SensorGlove(string nnWeightsPathName, string comPort = null)
            : this(nnWeightsPathName, comPort, new SerialSensorSource())
        {
        }

        public void Start()
        {
            if (this.DataSourcePath == null)
                this.source.Connect();
            else
                this.source.Connect(this.DataSourcePath);
        }
        public void Stop()
        {
            this.source.Dispose();
            this.source = new SerialSensorSource();
        }

        public void ResetCalibration()
        {
            this.source.ResetCalibration();
        }

        public void Dispose()
        {
            this.source.Dispose();
        }
    }
}
