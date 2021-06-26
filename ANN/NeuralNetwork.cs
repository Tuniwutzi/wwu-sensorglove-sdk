using System;

namespace SensorhandSDK.ANN
{
    public class NeuralNetwork
    {
        private int nrOfInputs            = 0;
        private int nrOfOutputs           = 0;
        private int nrOfHidLyrs           = 0;
        private int nrOfNeuronsPerHidLyr  = 0;
        private Layer[] layers;
        private Random rnd = new Random();

        private void SetNrOfInpputs(int nrOfInpputs)
        {
            if (nrOfInpputs < 1)
            {
                Console.WriteLine("ERROR negative number of neuron inputs");
                Console.Read();
            }

            this.nrOfInputs = nrOfInpputs;
        }

        private void SetNrOfOutputs(int nrOfOutputs)
        {
            if (nrOfOutputs < 1)
            {
                Console.WriteLine("ERROR negative number of neuron outputs");
                Console.Read();
            }

            this.nrOfOutputs = nrOfOutputs;
        }

        private void SetNrOfHidLyrs(int nrOfHidLyrs)
        {
            if (nrOfHidLyrs < 1)
            {
                Console.WriteLine("ERROR negative number of hidden layer");
                Console.Read();
            }

            this.nrOfHidLyrs = nrOfHidLyrs;
        }

        private void SetNrOfNeuronsPerHidLyrs(int nrOfNeuronsPerHidLyr)
        {
            if (nrOfNeuronsPerHidLyr < 1)
            {
                Console.WriteLine("ERROR negative number of neurons per hidden layers");
                Console.Read();
            }

            this.nrOfNeuronsPerHidLyr = nrOfNeuronsPerHidLyr;
        }

        // gets all weights of all neurons of all layers
        public float[] getWeights()
        {
            float[] weights = new float[getNrOfWeights()];

            int k = 0;
            for(int i=0; i < layers.Length; i++)
            {
                float[] layerWeights = layers[i].getWeightsOfNeurons();
                
                for (int j = 0; j < layerWeights.Length; j++)
                {
                    weights[k + j] = layerWeights[j];
                }

                k += layerWeights.Length;
            }


            return weights;
        }

        // gets number of all weights of all neurons of all layer
        public int getNrOfWeights()
        {

            int sum = 0;
            for (int i = 0; i < layers.Length; i++)
            {
                sum += layers[i].getNrOfWeights();
            }
            return sum;
        }

        // returns a random set of weights
        public float[] getRandomWeights()
        {
            float[] weights = new float[getNrOfWeights()]; 

            for (int i = 0; i < weights.Length; i++)
            {

                weights[i] = Convert.ToSingle(rnd.NextDouble());

            }

            return weights;
        }

        // sets all weights of all neurons of all layers
        public void setWeights(float[] weights)
        {
            int k = 0;
            
            for (int i = 0; i < layers.Length; i++)
            {
                //float[] weightsForLayer = new float[layers[i].getNrOfWeights()];

                //for (int j = 0; j < weightsForLayer.Length; j++)
                //{
                //    weightsForLayer[j] = weights[k + j];
                //}
                layers[i].setWeightsOfNeurons(weights, k);

                k += layers[i].getNrOfWeights();

            }
        }

        // create the variable layers
        private void createLayers()
        {
            // +1 = Outputlayer
            layers = new Layer[nrOfHidLyrs + 1];

        }
        
        // creates all layers and adds them to variable layers
        private void addLayers()
        {
            if (nrOfHidLyrs > 0)
            {
                // creates first layer. Receives the original input
                // public Layer(int nrOfNeurons, int nrOfInputsPerNeuron)
                layers[0] = new Layer(nrOfNeuronsPerHidLyr, nrOfInputs);


                // creates hidden layers
                for (int i = 1; i < nrOfHidLyrs; i++)
                {
                    layers[i] = new Layer(nrOfNeuronsPerHidLyr, nrOfNeuronsPerHidLyr);
                }

                // create output layer. Receives input from all members of the last hidden layer
                layers[layers.Length - 1] = new Layer(nrOfOutputs, nrOfNeuronsPerHidLyr);
            }
            else
                layers[0] = new Layer(nrOfOutputs, nrOfInputs);
        }


        // transforms the input data into the output via the Neural Network
        public float[] update(float[] inputs)
        {


            float[] outputs = null;

            // for each layer
            for(int i=0; i < layers.Length; i++)
            {
                if(i > 0)
                {
                    inputs = outputs;
                }

            
                // nrOfNeurons == nrOfOutputs
                outputs = new float[layers[i].getNrOfNeurons()];

                Neuron[] neurons = layers[i].getNeurons();
                nrOfInputs = neurons[0].getNrOfInputs();

                // for each neuron of the layer
                for (int j=0; j < neurons.Length; j++)
                {
                    float netinput = 0;

                    float[] weights = neurons[j].getWeights();
                    // for each weight
                    for (int k = 0; k < nrOfInputs; k++)
                        netinput += weights[k] * inputs[k];

                    // add bias
                    netinput -= weights[weights.Length - 1];

                    outputs[j] = netinput;

                }

            }

            //const float DegreeConversionFactor = 180.0f/(float)Math.PI;
            //for (int i = 0; i < outputs.Length; i++)
            //    outputs[i] = (Math.Abs(outputs[i]) % 360) - 180.0f;

            return outputs;
        }
                
        // Ctor 
        public NeuralNetwork(int nrOfInputs, int nrOfOutputs, int nrOfHidLyrs, int nrOfNeuronsPerHidLyr) 
        {
            SetNrOfInpputs(nrOfInputs);
            SetNrOfOutputs(nrOfOutputs);
            SetNrOfHidLyrs(nrOfHidLyrs);
            SetNrOfNeuronsPerHidLyrs(nrOfNeuronsPerHidLyr);

            createLayers();
            addLayers();

        }

        public int GetNrOfOutputs()
        {
            return this.nrOfOutputs;
        }

        // Ctor existing settings
        public NeuralNetwork(Settings settings)
        {
            SetNrOfInpputs(settings.getNrOfInputs());
            SetNrOfOutputs(settings.getNrOfOutputs());
            SetNrOfHidLyrs(settings.getNrOfHidLyrs());
            SetNrOfNeuronsPerHidLyrs(settings.getNrOfNeuronsPerHidLyr());

            createLayers();
            addLayers();

            if (settings.getFinalWeights() != null)
                setWeights(settings.getFinalWeights());


        }


    }
}
