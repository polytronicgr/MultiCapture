﻿using System;
using Hub.Helpers;
using Hub.Helpers.Interface;
using Hub.Threaded;
using Hub.Util;
using SharedDeviceItems;

namespace Hub
{
    public class HubMain
    {
        public static int Main(String[] args)
        {
            HubMain program = new HubMain();
            program.Start();
            return 0;
        }

        public HubMain()
        {
            Start();
        }

        /// <summary>
        /// Main runtime loop used to collect and send data off too the computer
        /// </summary>
        private void Start()
        {
            Logger.Logger logs = new Logger.Logger();
            logs.RemoveOldLogs(DateTime.Today.AddMonths(-1));
            ICameraManager manager = new TaskManager(SaveContainer.Load());

            string command = "";
            while ((command = Console.ReadLine()) != "e")
            {
                if (command == "t" || command == "test") manager.CaptureImageSet(CameraRequest.SendTestImage);
                else if (command == "s")
                {
                    Console.WriteLine(manager.SavePath);
                    Console.Write("new save path: ");
                    manager.SavePath = Console.ReadLine();
                }
                else if (command == "clear") manager.ClearSockets();
                else manager.CaptureImageSet();
            }
            ProjectMapper.Instance.Save();
            Console.WriteLine("Quitting");
        }
    }
}