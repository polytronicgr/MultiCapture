﻿using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using Hub.Networking;
using SharedDeviceItems.Exceptions;
using SharedDeviceItems.Helpers;
using SharedDeviceItems.Networking;
using SharedDeviceItems.Networking.CameraHubConnection;
using Shell_Camera;
// ReSharper disable VirtualMemberCallInConstructor

#pragma warning disable 618

namespace CameraServer
{
    public class Listener
    {
        private static string lastRequest;
        protected bool stop { get; set; }
        protected ISocket listener;

        public Listener()
        {
            stop = false;
            IPEndPoint localEndPoint;

            {
                IPHostEntry ipHostInfo = Dns.GetHostEntry(Dns.GetHostName());
                IPAddress ipAddress = NetworkHelpers.GrabIpv4(ipHostInfo);
                localEndPoint = new IPEndPoint(ipAddress, int.Parse(CameraSettings.GetSetting("port", "11003")));

                Console.WriteLine("Camera Name\t= " + CameraSettings.GetSetting("name"));
                Console.WriteLine("IP address\t= " + ipAddress);
                Console.WriteLine("Port\t\t= " + CameraSettings.GetSetting("port"));
            }

            SetupSocket(localEndPoint);
        }

        protected virtual void SetupSocket(IPEndPoint localEndPoint)
        {
            // Bind the socket to the local endpoint and listen for incoming connections.
            if (listener == null) listener = new SocketWrapper(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            listener.Bind(localEndPoint);
            listener.Listen(0);
        }

        /// <summary>
        /// Listen and process incoming requests
        /// </summary>
        public void StartListening()
        {
            try
            {
                // Start listening for connections.
                while (!stop)
                {
                    lastRequest = null;
                    IResponder responder = NewResponder();
                    RequestProcess process = NewProcessor();

                    try
                    {
                        //Connect to the hub
                        Console.WriteLine("Waiting for a connection...");
                        responder.Connect(listener);
                        Console.WriteLine("Connected!!");

                        //respond to incoming requests
                        while(responder.Connected() && !stop)
                        {
                            byte[] request = responder.RecieveData();

                            lastRequest = Encoding.ASCII.GetString(request);
                            Console.WriteLine("Request Recieved: {0}", lastRequest);

                            byte[] response = process.ProcessRequest(request);

                            Console.WriteLine("Data size: " + response.Length);
                            responder.SendResponse(response);

                            Console.WriteLine("Waiting for next request...");
                        }
                    }
                    catch(TestException)
                    {
                        break;
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine("Exception thrown");
                        Console.WriteLine("\tmessage: " + e.Message);
#if DEBUG
                        Console.WriteLine("\tstack trace:" + e.StackTrace);
#endif
                        if (lastRequest == null) Console.WriteLine("\tlast request data is null");
                        else if (lastRequest.Length > 0) Console.WriteLine("\tlast request data: " + lastRequest);
                        else Console.WriteLine("\tlast request data: <Empty string>");
                    }
                }

            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }

#if DEBUG
            Console.WriteLine("SHUTDOWN");
            Console.Read();
#endif
        }

        /// <summary>
        /// Layer of abstarction for creating a request process so that tests can pass in a slightly more open version of the processer
        /// </summary>
        /// <returns></returns>
        protected virtual RequestProcess NewProcessor()
        {
            return new RequestProcess(new ShellCamera("0"));
        }

        /// <summary>
        /// Layer of abstarction for creating a responder so that tests can pass in a slightly more open version of the responder
        /// </summary>
        /// <returns></returns>
        protected virtual IResponder NewResponder()
        {
            return new SocketResponder();
        }
    }
}
