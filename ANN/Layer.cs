using System;

namespace SensorhandSDK.ANN
{

    class Layer
    {

        private int nrOfNeurons = 0;
        private int nrOfInputsPerNeuron  = 0;
        private Neuron[] neurons;

        // sets nrOfNeurons
        private void setNrOfNeurons(int nrOfNeurons)
        {
            if(nrOfNeurons < 1)
            {
                Console.WriteLine("ERROR wrong input setnrOfNeurons");
                Console.Read();
            }

            this.nrOfNeurons = nrOfNeurons;
            
        }

        // sets nrOfInputsPerNeuron
        private void setNrOfInputsPerNeuron(int nrOfInputsPerNeuron)
        {
            if (nrOfInputsPerNeuron < 1)
            {
                Console.WriteLine("ERROR wrong input setNeurons");
                Console.Read();
            }

            this.nrOfInputsPerNeuron = nrOfInputsPerNeuron;
        }

        // creates variable neurons
        private void createNeurons()
        {
            neurons = new Neuron[nrOfNeurons];
            

            for (int i = 0; i < neurons.Length; i++)
            {
                neurons[i] = new Neuron(nrOfInputsPerNeuron);
            }
        }

        // gets nrOfNeurons
        public int getNrOfNeurons()
        {
            return nrOfNeurons;
        }

        public Neuron[] getNeurons()
        {
            return neurons;
        }

        // gets nrOfInputsPerNeuron
        public int getNrOfInputsPerNeuron()
        {
            return nrOfInputsPerNeuron;
        }

        // gets number of weights of all neurons
        public int getNrOfWeights()
        {
            return nrOfNeurons * (nrOfInputsPerNeuron + 1);
        }
        
        // gets all weights of all neurons
        public float[] getWeightsOfNeurons()
        {
            float[] weights = new float[getNrOfWeights()];
            
            // for every neuron in this layer
            for (int i=0;i< neurons.Length; i++)
            {
                float[] neuronWeights = neurons[i].getWeights();

                // for every weight of the neuron
                for (int j=0; j< neuronWeights.Length;j++)
                {
                    weights[(i * neuronWeights.Length) + j] = neuronWeights[j];
                }
            }

            return weights;
        }
        
        // sets all weights of all neurons
        public void setWeightsOfNeurons(float[] weights, int offset)
        {
            int k = 0;

            for (int i=0; i < nrOfNeurons; i++)
            {
                float[] weightsForNeuron = new float[nrOfInputsPerNeuron + 1];

                for(int j=0; j < weightsForNeuron.Length; j++ )
                {
                    weightsForNeuron[j] = weights[offset + k + j];
                }

                neurons[i].setWeights(weightsForNeuron);
                k += weightsForNeuron.Length;
            }
        }

        // Ctor sets/creates Variables
        public Layer(int nrOfNeurons, int nrOfInputsPerNeuron)
        {
            setNrOfNeurons(nrOfNeurons);
            setNrOfInputsPerNeuron(nrOfInputsPerNeuron);
            createNeurons();

        }

    }
}
