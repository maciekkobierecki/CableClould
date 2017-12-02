using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CableCloud
{
    class SendingManager
    {
        private static readonly object SyncObject = new object();
        private static List<OutSocket> outSockets;

        public static void init()
        {
            outSockets = new List<OutSocket>();
        }

        public static void addOutSocket(OutSocket outSocket)
        {
            outSockets.Add(outSocket);
        }

        public static void Send(byte[] dataToSend, String sourceNode, int sourcePort)
        {
            int destinationPort = ConnectionsTable.getDestinationPort(sourceNode, sourcePort);
            byte[] destinationPortBytes = BitConverter.GetBytes(destinationPort);
            swap(dataToSend, destinationPortBytes);
            byte[] dataWithLengthInformation = containDataLengthInformation(dataToSend);

            String destinationNodeName = ConnectionsTable.getDestinationNodeName(sourceNode, sourcePort);
            lock (SyncObject)
            {
                OutSocket outputSocket = getAssociatedSocket(destinationNodeName);
                if(outputSocket!=null)
                    outputSocket.Send(dataWithLengthInformation);
                Console.WriteLine("Sent to node " + outputSocket.getNodeName()+"to port "+destinationPort.ToString());
              //  Console.WriteLine("{0} | Received packet from node: {1}", DateTime.Now, outputSocket.getNodeName());
            }
        }

        private static void swap(byte[] data, byte[] port)
        {
            for(int i=0; i<4; i++)
            {
                data[i] = port[i];
            }
        }

        private static byte[] containDataLengthInformation(byte[] dataToSend)
        {
            int dataSize = dataToSend.Length;
            byte[] lengthInformation = BitConverter.GetBytes(dataSize);
            int completeDataLength = dataToSend.Length + lengthInformation.Length;
            Console.WriteLine(completeDataLength);
            byte[] preparedData = combineArrays(dataToSend, lengthInformation);
            return preparedData;
            
        }

        private static byte[] combineArrays(byte[] dataBytes, byte[] lengthBytes)
        {
            byte[] combined = new byte[dataBytes.Length + lengthBytes.Length];
            Buffer.BlockCopy(lengthBytes, 0, combined, 0, lengthBytes.Length);
            Buffer.BlockCopy(dataBytes, 0, combined, lengthBytes.Length, dataBytes.Length);
            return combined;        
        }

        private static OutSocket getAssociatedSocket(string destinationNodeName)
        {
            OutSocket socketToReturn = null;
            String nodeName = null;
            for (int i=0; i<outSockets.Count; i++)
            {
                OutSocket outputSocket = outSockets.ElementAt(i);
                nodeName = outputSocket.getNodeName();
                if (nodeName.Equals(destinationNodeName))
                    socketToReturn = outputSocket;
            }
            return socketToReturn;
        }
    }
}
