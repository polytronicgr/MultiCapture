﻿using System;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using SharedDeviceItems;
using Hub.Helpers;
using Hub.Networking;

namespace Hub.Threaded
{
    public class CameraThread
    {
        public volatile bool Finish = false;
        public volatile CameraRequest Request = CameraRequest.Alive;
        public volatile string SavePath;

        public string ImageSetName { get; set; }

        private CameraSocket config;
        private INetwork connection;

        public CameraThread(CameraSocket socket)
        {
            config = socket;
            connection = new SynchronousNet(socket.DataSocket);
            SavePath = Constants.DefualtHubSaveLocation();
        }

        public CameraThread(CameraSocket socket, string saveLocation)
        {
            config = socket;
            connection = new SynchronousNet(socket.DataSocket);
            SavePath = saveLocation;
        }

        public void Start()
        {
            try
            {
                SetCameraProporties();

                while (!Finish)
                {
                    if (Request != CameraRequest.Alive)
                    {
                        ProcessRequest(Request);
                        Request = CameraRequest.Alive;
                    }

                    Thread.Sleep(5);
                }
            }
            catch (ThreadAbortException)
            {
                Console.WriteLine("Thread Aborting (Because of exception)");
            }
            finally
            {
                Shutdown();
            }
        }

        /// <summary>
        /// Close all sensitive parts of the thread that may cause problems later
        /// </summary>
        private void Shutdown()
        {
            try
            {
                config.DataSocket.Shutdown(SocketShutdown.Both);
                config.DataSocket.Close();
            }
            catch (Exception e)
            {
                Console.WriteLine("While Trying to close socket something went wrong... " + e.Message);
                Console.WriteLine(e);
            }
        }

        private void ProcessRequest(CameraRequest request)
        {
            //start asking the camera for a new image
            byte[] data = connection.MakeRequest(PropertyRequest(request));

            //extract image data
            string imageName;
            byte[] imageData;
            ByteManipulation.SeperateData(out imageName, data, out imageData);
            if (imageName == "" || imageData.Length <= 0)
            {
                Console.WriteLine("No Image data recieved!!");
                Console.WriteLine("Debug data:");
                Console.WriteLine("\tThread Camera: " + config.Config.Id);
                Console.WriteLine("\tImage set id: " + ImageSetName);
                Console.WriteLine("\tImage return string: " + Encoding.ASCII.GetString(data));
                return;
            }

            SaveData(imageData, SavePath + Path.DirectorySeparatorChar + imageName);

            Console.WriteLine("Camera " + config.Config.Id + " image saved");
        }

        private void SaveData(byte[] data, string location)
        {
            using (FileStream fileStream = new FileStream(location, FileMode.CreateNew))
            {
                foreach (byte img in data)
                {
                    fileStream.WriteByte(img);
                }
            }
        }

        private byte[] PropertyRequest(CameraRequest request)
        {
            CommandBuilder builder = new CommandBuilder().Request(request);

            if (request == CameraRequest.Alive || request == CameraRequest.SendTestImage)
                return builder.Build();

            builder.AddParam("id", ImageSetName);

            return builder.Build();
        }

        /// <summary>
        /// Sets the camera proporties
        /// </summary>
        private void SetCameraProporties()
        {
            CommandBuilder builder = new CommandBuilder().Request(CameraRequest.SetProporties);
            builder.AddParam("name", config.Config.CamFileIdentity);
            builder.AddParam("id", "0");

            connection.MakeRequest(builder.Build());
        }

        /// <summary>
        /// use when debugging - clears socket buffer of data
        /// </summary>
        public void ClearSockets()
        {
            byte[] ignore = new byte[300];
            int total = 0;
            while (config.DataSocket.Available > 0)
            {
                total += config.DataSocket.Receive(ignore);
            }
#if DEBUG
            Console.WriteLine("Total bytes flushed: " + total);
#endif
        }
    }
}
