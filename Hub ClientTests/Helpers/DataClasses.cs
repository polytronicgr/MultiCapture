﻿using System;
using System.Net;
using System.Net.Sockets;
using NUnit.Framework;
using Hub.Networking;
using SharedDeviceItems.Helpers;
using SharedDeviceItems.Networking;

#pragma warning disable 618

namespace Hub.Helpers.Tests
{
    [TestFixture]
    public class DataClassTests
    {
        [Test]
        public void GetterSetterTest()
        {
            CameraSocket testCameraSocket = new CameraSocket();

            #region socket

            Assert.IsNull(testCameraSocket.DataSocket);

            SocketWrapper testSocketWrapper = new SocketWrapper(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            testCameraSocket.DataSocket = testSocketWrapper;

            Assert.IsTrue(testCameraSocket.DataSocket != null);
            Assert.IsTrue(testCameraSocket.DataSocket.Equals(testSocketWrapper));

            ISocket testSocketget = testCameraSocket.DataSocket;
            Assert.IsTrue(testSocketWrapper.Equals(testSocketget));

            #endregion

            #region config

            CameraConfiguration camConfigTest = new CameraConfiguration
            {
                Id = 233,
                CamFileIdentity = "superTest string"
            };

            Assert.IsNull(testCameraSocket.Config);

            testCameraSocket.Config = camConfigTest;

            Assert.IsTrue(testCameraSocket.Config.Id == 233);
            Assert.IsTrue(testCameraSocket.Config.CamFileIdentity == "superTest string");

            #endregion
        }

        [Test]
        public void SetupTestException()
        {
            CameraSocket testSocket = new CameraSocket
            {
                DataSocket = new SocketWrapper(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp),
                Config = new CameraConfiguration()
            };

            bool threw = false;

            try
            {
                testSocket.Setup();
            }
            catch (Exception e)
            {
                Assert.IsTrue(e is InvalidOperationException);
                Assert.IsTrue(e.Message.Equals("Configuration address not configured"));
                threw = true;
            }

            Assert.IsTrue(threw);
            threw = false;

            IPAddress address = NetworkHelpers.GrabIpv4();
            testSocket.Config.Address = address.Address;
            try
            {
                testSocket.Setup();
            }
            catch (Exception e)
            {
                Assert.IsTrue(e is InvalidOperationException);
                Assert.IsTrue(e.Message.Equals("Configuration port not configured"));
                threw = true;
            }
            Assert.IsTrue(threw);

            testSocket.Config.Port = 700;
            try
            {
                Assert.IsFalse(testSocket.Setup());
            }
            catch (Exception)
            {
                Assert.Fail("Shouldn't throw an exception");
            }

        }
    }
}