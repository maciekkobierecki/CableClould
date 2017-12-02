﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;
using tsst_client;


namespace CableCloud
{
    public class InSocket
    {
        public static int PORT_NUMBER_SIZE = 4;
        private Socket inputSocket = null;
        private Packet messageOut = null;
        private Packet messageIn = null;
        private TcpListener listener;
        private string nodeName;
        private int port;
        private Boolean listening;


        public InSocket()
        {
            //Console.Title = "Cable Cloud";            
        }

        public InSocket(int _port, String associatedNodeName)
        {
            //GetNewInSocket(_port);
            nodeName = associatedNodeName;
            port = _port;
        }

        private void GetNewInSocket(int p)
        {
            inputSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            //IPEndPoint remoteEP = new IPEndPoint(IPAddress.Any, p);
            //inputSocket.Bind(remoteEP);
        }
        public void ListenForConnection()
        {
            if(!listening)
            {
                listener = new TcpListener(IPAddress.Any, port);
                listener.Start();
                listening = true;
            }
            inputSocket = listener.AcceptSocket();
        }

        public void ListenForIncomingData()
        {
            byte[] bytes = new byte[4];
            int i = 1;
            while (true)
            {
                    int inputSize = ReceiveInputSize();
                    byte[] receivedData = receiveData(inputSize);
                    int sourcePort = GetSourcePort(receivedData);
               // int destPort = getDestPort(receivedData);
                Console.WriteLine("Recieved from node " + nodeName+ "From port "+sourcePort.ToString());
              //  Console.WriteLine("{0} | Received packet from node: {1}", DateTime.Now, nodeName);
                    SendingManager.Send(receivedData, nodeName, sourcePort);
            }
        }
        private int getDestPort(byte[] receivedData)
        {
            byte[] sourcePort = new byte[4];
            for (int i = 0; i < PORT_NUMBER_SIZE; i++)
                sourcePort[i] = receivedData[i];
            int convertedSourcePort = BitConverter.ToInt32(sourcePort, 0);
            return convertedSourcePort;
        }
        private int ReceiveInputSize()
        {
                byte[] objectSize = new byte[4];
                inputSocket.Receive(objectSize, 0, 4, SocketFlags.None);
                int messageSize = BitConverter.ToInt32(objectSize, 0);
                Console.WriteLine(messageSize);
                return messageSize;    
        }

        private byte[] receiveData(int inputSize)
        {
            byte[] bytes = new byte[inputSize];
            inputSocket.Receive(bytes, 0, inputSize, SocketFlags.None);
            return bytes;
        }

        private int GetSourcePort(byte[] receivedData)
        {
            byte[] sourcePort = new byte[4];
            for (int i = 0; i < PORT_NUMBER_SIZE; i++)
                sourcePort[i]= receivedData[i+4];
            int convertedSourcePort = BitConverter.ToInt32(sourcePort,0);
            return convertedSourcePort;
        }



        private Packet GetDeserializedMessage(byte[] b)
        {
            Packet m = null;
            MemoryStream memStream = new MemoryStream();
            BinaryFormatter binForm = new BinaryFormatter();
            memStream.Write(b, 0, b.Length);
            memStream.Seek(0, SeekOrigin.Begin);
            m = (Packet)binForm.Deserialize(memStream);
            return m;
        }

        private byte[] GetSerializedMessage(Packet mes)    //Serializacja bajtowa
        {
            BinaryFormatter bf = new BinaryFormatter();
            MemoryStream ms = new MemoryStream();
            bf.Serialize(ms, mes);
            return ms.ToArray();
        }

    }
}