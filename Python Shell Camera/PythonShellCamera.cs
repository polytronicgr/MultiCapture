﻿using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using SharedDeviceItems;
using SharedDeviceItems.Exceptions;
using SharedDeviceItems.Interface;
using SharedDeviceItems.Helpers;

namespace Python_Shell_Camera
{
    public class PythonShellCamera : ICamera
    {
        private const int ILimit = 2000;

        private string fileLocation = null;
        private string name;

        private Process pyThread;
        private StreamWriter pyInput;
        private StreamReader pyOutput;

        private string currentDir = Directory.GetCurrentDirectory() + Path.DirectorySeparatorChar;

        public PythonShellCamera(string name = "pyShellCam", string saveLocation = "/scanImage/")
        {
            Console.WriteLine("--- Python Shell Capture ---");
            fileLocation = saveLocation;
            this.name = name;

            try
            {
                ProcessStartInfo start = new ProcessStartInfo()
                {
                    //FileName = "/usr/bin/pyhton2.7",
                    //Arguments = "camera.py",
                    FileName = "/bin/bash",
                    Arguments = "-c python camera.py",
                    UseShellExecute = false,
                    RedirectStandardInput = true,
                    RedirectStandardOutput = true
                };

                pyThread = new Process { StartInfo = start };
                pyThread.Start();
                pyOutput = pyThread.StandardOutput;
                pyInput = pyThread.StandardInput;
            }
            catch (Exception e)
            {
                Console.WriteLine("Exception");
                throw new CameraInitialisationException(e.Message);
            }

            //Console.WriteLine(pyOutput.ReadLine());
            pyInput.Write("cap\n");
            pyInput.Write("hij\n");

            Console.WriteLine("Setting Name...");
            pyInput.Write("name");
            string output = pyOutput.ReadToEnd();
            //Console.WriteLine(output);
            if (output == "Name: ")
                pyInput.WriteLine(name);

            output = pyOutput.ReadLine();
            if (output != "command: ")
                throw new CameraInitialisationException("Failed to set name");

            Console.WriteLine("Python Shell Camera init done!!");
        }

        public void SetDirectory(string location)
        {
            fileLocation = location;
        }

        public void SetCameraName(string name)
        {
            pyInput.Write("name\n");
            string output = pyOutput.ReadToEnd();
            if (output != "name: ")
                throw new Exception("Name not set correctly, expected \"name: \" but text was: " + output);
            pyInput.Write(name + "\n");

            output = pyOutput.ReadLine();
            if (output != "command: ")
                throw new Exception("Name not set correctly, text was: " + output);
        }

        public string CaptureImage(string identifier)
        {
            pyInput.Write("cap\n");
            pyInput.Write(identifier + "\n");

            string loc = currentDir + name + identifier + ".jpg";
            int i = 0;
            Console.Write("Checking for image file");

            do
            {
                Thread.Sleep(10);
                ++i;
            } while (!File.Exists(loc) || i > ILimit);
            if (i > ILimit)
            {
                throw new CaptureFailedException("Image could not be found after taking the image");
            }
            Console.WriteLine(" ...Done");

            Console.Write("Checking if file is still writing");
            i = 0;
            int identicleCount = 0;
            bool b = true;
            long lastSize = -1;

            do
            {
                try
                {
                    if (File.GetAttributes(loc).HasFlag(FileAttributes.ReadOnly)) b = true;

                    //figure out if more data is being written to the file
                    FileInfo info = new FileInfo(loc);
                    Console.WriteLine(i + " image size: " + info.Length);
                    if (info.Length <= 0) b = true;
                    else
                    {
                        if (info.Length > lastSize)
                        {
                            lastSize = info.Length;
                            identicleCount = 0;
                        }
                        else if (info.Length == lastSize)
                        {
                            ++identicleCount;
                            if (identicleCount > 2) b = false;
                        }
                    }

                    if (b) ++i;
                }
                catch (Exception e)
                {
                    Console.WriteLine("Attempt " + i + " Failed: " + e.Message);
                    b = true;
                    ++i;
                }
            } while (b || i > ILimit / 2);
            Console.WriteLine(i > ILimit ? " ...Checking aborted" : " ...Done");

            return loc;
        }

        public void SetResolution(int x, int y)
        {
            pyInput.Write("res\n");
            pyInput.Write(x + "\n");
            pyInput.Write(y + "\n");

            string output = pyOutput.ReadLine();
            if (output != "command: ")
                throw new Exception("Resolution not set correctly, expected \"command: \" but text was: " + output);
        }

        public byte[] CaptureImageByte(string identifier)
        {
            string imageLocation = CaptureImage(identifier);
            byte[] data = ByteHelpers.FileToBytes(imageLocation);
            if (File.Exists(imageLocation)) File.Delete(imageLocation);
            return data;
        }

        public void setFlip(bool verticleFlip, bool horizontalFlip)
        {
            throw new NotImplementedException();
        }

        public void setRotation(Rotation rotation)
        {
            throw new NotImplementedException();
        }
    }
}
