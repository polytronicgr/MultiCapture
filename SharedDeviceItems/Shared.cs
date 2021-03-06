﻿using System.IO;
using System.Text;

namespace SharedDeviceItems
{
    public enum CameraRequest
    {
        Unknown = -1,
        Alive = 0,
        SendFullResImage = 1,
        SendTestImage = 9,
        SetProporties = 8
    }

    public enum Rotation
    {
        Zero = 0,
        Ninty = 90,
        OneEighty = 180,
        TwoSeventy = 270
    };

    public static class Constants
    {
        #region data transfer settings
        public const string EndOfMessage = "xtx";
        public const string MessageSeparator = "<,,,>";
        public const string FailString = "FAIL";
        public const string SuccessString = "SUCCESS";
        public const string EndTransferMessage = "DONE";
        public const string TransferReady = "READY";

        public static readonly byte[] EndOfMessageBytes = Encoding.ASCII.GetBytes(EndOfMessage);
        public static readonly byte[] FailStringBytes = Encoding.ASCII.GetBytes(FailString);
        public static readonly byte[] SuccessStringBytes = Encoding.ASCII.GetBytes(SuccessString);
        public static readonly byte[] EndTransferBytes = Encoding.ASCII.GetBytes(EndTransferMessage);
        public static readonly byte[] ReadyTransferBytes = Encoding.ASCII.GetBytes(TransferReady);

        public const int HubBufferSize = 400000;
        public const int CameraBufferSize = 128;

        public const string ParamSeparator = "&&";
        public const string ParamKeyValueSeparator = "=";
        #endregion

        #region camera setting transfer codes
        public const string CameraSettingName = "name";
        public const string CameraCaptureImageName = "id";
        #endregion

        public static string DefaultHubSaveLocation()
        {
            return Path.DirectorySeparatorChar + "scanimage" + Path.DirectorySeparatorChar;
        }
    }
}
