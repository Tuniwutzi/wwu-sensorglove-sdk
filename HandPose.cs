/// <author>
/// Jan Buenker
/// <author>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace SensorhandSDK
{
    /// <summary>
    /// Angles of a finger in degrees
    /// </summary>
    [DataContract(Namespace="")]
    public class FingerAngles
    {
        [DataMember]
        public float Top { get; set; }
        [DataMember]
        public float Mid { get; set; }
        [DataMember]
        public float Base { get; set; }

        [DataMember]
        public float Side { get; set; }
    }

    /// <summary>
    /// Angles of the thumb in degrees
    /// <summary>
    [DataContract(Namespace = "")]
    public class ThumbAngles
    {
        [DataMember]
        public float Top { get; set; }
        [DataMember]
        public float Mid { get; set; }
        [DataMember]
        public float BaseRotation { get; set; }
        [DataMember]
        public float BaseSpread { get; set; }
        [DataMember]
        public float BaseTilt { get; set; }
    }

    /// <summary>
    /// Angles of the hand in degrees
    /// </summary>
    [DataContract(Namespace = "")]
    public class HandPose
    {
        public const int LittleFinger = 0;
        public const int RingFinger = 1;
        public const int MiddleFinger = 2;
        public const int Pointer = 3;

        [DataMember]
        public FingerAngles[] Fingers { get; set; }
        [DataMember]
        public ThumbAngles Thumb { get; set; }


        public uint Id;

        public const uint NoId = 0;
        private static uint nextId = 1;
        public HandPose()
        {
            this.Id = HandPose.nextId++;

            this.Fingers = new FingerAngles[4];
            for (int i = 0; i < this.Fingers.Length; i++)
            {
                this.Fingers[i] = new FingerAngles();
            }

            this.Thumb = new ThumbAngles();
        }

        public float[] GetRawAngles(bool skipCalculatableAngles = false)
        {
            var returnvalue = new float[skipCalculatableAngles ? 16 : 21];

            int k = 0;
            for (int j = 0; j < this.Fingers.Length; j++)
            {
                returnvalue[k++] = this.Fingers[j].Base;
                returnvalue[k++] = this.Fingers[j].Mid;
                returnvalue[k++] = this.Fingers[j].Side;
                if (!skipCalculatableAngles)
                    returnvalue[k++] = this.Fingers[j].Top;
            }

            // get Thumb 

            if (!skipCalculatableAngles)
                returnvalue[k++] = this.Thumb.BaseRotation;

            returnvalue[k++] = this.Thumb.BaseSpread;
            returnvalue[k++] = this.Thumb.BaseTilt;
            returnvalue[k++] = this.Thumb.Mid;
            returnvalue[k++] = this.Thumb.Top;

            return returnvalue;
        }
        public void SetRawAngles(float[] rawAngles, bool? skipCalculatableAngles = null)
        {
            if (rawAngles == null || (rawAngles.Length != 16 && rawAngles.Length != 21))
                throw new Exception("Invalid rawAngles");

            bool doSkipCalculatableAngles;
            if (skipCalculatableAngles.HasValue)
                doSkipCalculatableAngles = skipCalculatableAngles.Value;
            else
                doSkipCalculatableAngles = rawAngles.Length == 16;

            var anglesPerFinger = doSkipCalculatableAngles ? 3 : 4;

            for (int i = 0; i < this.Fingers.Length; i++)
            {
                this.Fingers[i].Base = rawAngles[(i * anglesPerFinger)];
                this.Fingers[i].Mid = rawAngles[(i * anglesPerFinger) + 1];
                this.Fingers[i].Side = rawAngles[(i * anglesPerFinger) + 2];

                if (!doSkipCalculatableAngles)
                    this.Fingers[i].Top = rawAngles[(i * anglesPerFinger) + 3];
                else
                    this.Fingers[i].Top = this.Fingers[i].Mid * 2.0f / 3.0f;
            }

            var thumbOffset = doSkipCalculatableAngles ? 0 : 1;
            this.Thumb.BaseSpread = rawAngles[(anglesPerFinger * 4) + thumbOffset];
            this.Thumb.BaseTilt = rawAngles[(anglesPerFinger * 4) + thumbOffset + 1];
            this.Thumb.Mid = rawAngles[(anglesPerFinger * 4) + thumbOffset + 2];
            this.Thumb.Top = rawAngles[(anglesPerFinger * 4) + thumbOffset + 3];

            if (!doSkipCalculatableAngles)
                this.Thumb.BaseRotation = rawAngles[(anglesPerFinger * 4)];
            else
            {
                const float AlphaMin = -80.001f;
                const float AlphaMax = 20.001f;
                const float AlphaRange = AlphaMax - AlphaMin;

                const float GammaMin = -0.001f;
                const float GammaMax = (45.0f * AlphaRange) / -AlphaMin + GammaMin;
                const float GammaRange = GammaMax - GammaMin;

                this.Thumb.BaseRotation = ((this.Thumb.BaseTilt - AlphaMin) / AlphaRange) * GammaRange + GammaMin;
            }
        }
    }
}
