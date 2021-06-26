using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SensorhandSDK.ANN
{
    public class Settings
    {
        public int nrOfInputs;
        public int nrOfOutputs;
        public int nrOfHidLyrs;
        public int nrOfNeuronsPerHidLyr;
        public float[] finalWeights;

        // setter
        public void setNrOfInputs(int nrOfInputs)
        {
            this.nrOfInputs = nrOfInputs;
        }

        public void setNrOfOutputs(int nrOfOutputs)
        {
            this.nrOfOutputs = nrOfOutputs;
        }

        public void setNrOfHidLyrs(int nrOfHidLyrs)
        {
            this.nrOfHidLyrs = nrOfHidLyrs;
        }

        public void setNrOfNeuronsPerHidLyr(int nrOfNeuronsPerHidLyr)
        {
            this.nrOfNeuronsPerHidLyr = nrOfNeuronsPerHidLyr;
        }

        public void setFinalWeights(float[] finalWeights)
        {
            this.finalWeights = finalWeights;
        }

        // getter
        public int getNrOfInputs()
        {
            return nrOfInputs;
        }

        public int getNrOfOutputs()
        {
            return nrOfOutputs;
        }

        public int getNrOfHidLyrs()
        {
            return nrOfHidLyrs;
        }

        public int getNrOfNeuronsPerHidLyr()
        {
            return nrOfNeuronsPerHidLyr;
        }

        public float[] getFinalWeights()
        {
            return finalWeights;
        }

        // Ctor
        public Settings(int nrOfInputs, int nrOfOutputs, int nrOfHidLyrs, int nrOfNeuronsPerHidLyr, float[] finalWeights)
        {
            setNrOfInputs(nrOfInputs);

            setNrOfOutputs(nrOfOutputs);

            setNrOfHidLyrs(nrOfHidLyrs);

            setNrOfNeuronsPerHidLyr(nrOfNeuronsPerHidLyr);

            setFinalWeights(finalWeights);

        }

        // Ctor for export
        public Settings()
        {

        }

        public static bool operator ==(Settings a, Settings b)
        {
            if ((object)a == null)
                return (object)b == null;
            else if ((object)b == null)
                return false;


            if (a.finalWeights != null && b.finalWeights != null && a.finalWeights.Length == b.finalWeights.Length)
            {
                for (int i = 0; i < a.finalWeights.Length; i++)
                {
                    if (a.finalWeights[i] != b.finalWeights[i])
                        return false;
                }
            }
            else if (a.finalWeights != null || b.finalWeights != null)
                return false;


            return a.nrOfHidLyrs == b.nrOfHidLyrs && a.nrOfInputs == b.nrOfInputs && a.nrOfNeuronsPerHidLyr == b.nrOfNeuronsPerHidLyr && a.nrOfOutputs == b.nrOfOutputs;
        }
        public static bool operator !=(Settings a, Settings b)
        {
            return !(a == b);
        }
    }
}
