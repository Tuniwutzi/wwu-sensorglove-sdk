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
            //for (int i = 0; i < 4; i++)
            //{
            //    pose.Fingers[i] = new FingerAngles();
            //    var finger = pose.Fingers[i];

            //    finger.Base = angles[(i * 4)];
            //    finger.Mid = angles[(i * 4) + 1];
            //    finger.Side = angles[(i * 4) + 2];
            //    finger.Top = angles[(i * 4) + 3];
            //}
            //pose.Thumb = new ThumbAngles();
            //var thumb = pose.Thumb;
            //thumb.BaseRotation = angles[(4 * 4)];
            //thumb.BaseSpread = angles[(4 * 4) + 1];
            //thumb.BaseTilt = angles[(4 * 4) + 2];
            //thumb.Mid = angles[(4 * 4) + 3];
            //thumb.Top = angles[(4 * 4) + 4];

            lock (this.resultLock)
            {
                this.result = pose;
            }
        }
    }
}
