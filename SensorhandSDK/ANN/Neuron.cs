using System;

namespace SensorhandSDK.ANN
{
    class Neuron
    {
        private int nrOfInputs = 1;
        private float[] weights; // number of weights = nrOfInputs + 1
        private Random rnd = new Random();

        // sets nrOfInputs
        private void setNrOfInputs(int nrOfInputs)
        {
            if(nrOfInputs < 1)
            {
                Console.WriteLine("ERROR negative number of neuron inputs");
                Console.Read();
            }

            this.nrOfInputs = nrOfInputs;
                                 

        }

        // creates variable weights 
        private void createWeights()
        {
            // +1 for bias (actually the threshold)
            weights = new float[nrOfInputs + 1];
        }

        // sets weights
        public void setWeights(float[] weights)
        {
            if (weights.Length !=  this.weights.Length)
            {
                Console.WriteLine("ERROR nr of weights wrong");
                Console.Read();
            }

            //for(int i=0; i< weights.Length; i++)
            //{
            //    this.weights[i] = weights[i];
            //}

            this.weights = weights;
        }

        // gets weights
        public float[] getWeights()
        {
            return weights;
        }

        // gets nrOfInputs
        public int getNrOfInputs()
        {
            return nrOfInputs;
        }

        // Ctor
        public Neuron(int nrOfInputs)
        {
            setNrOfInputs(nrOfInputs);
            createWeights();
        }

    }
}
