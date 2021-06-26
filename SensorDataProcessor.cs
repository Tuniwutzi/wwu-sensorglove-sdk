/// <author>
/// Jan Buenker
/// <author>

using SensorhandSDK.ANN;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace SensorhandSDK
{
    public class SensorDataProcessor
    {
        private SensorDataSource dataSource;
        private NeuralNetwork neuralNetwork;

        private HandPose result;
        private object resultLock = new object();

        public HandPose CurrentPose
        {
            get
            {
                lock (this.resultLock)
                {
                    return this.result;
                }
            }
        }

        public SensorDataProcessor(SensorDataSource dataSource, NeuralNetwork nn)
        {
            this.dataSource = dataSource;
            this.neuralNetwork = nn;
            this.result = null;

            this.dataSource.OnSensorDataUpdate += glove_OnSensorDataUpdate;
        }

        void glove_OnSensorDataUpdate(object sender, SensorDataEventArgs data)
        {
            HandPose pose = new HandPose();

            var angles = this.neuralNetwork.update(data.Sensors.Select(b => (float)b).ToArray());
            pose.SetRawAngles(angles);

            lock (this.resultLock)
            {
                this.result = pose;
            }
        }
    }
}
