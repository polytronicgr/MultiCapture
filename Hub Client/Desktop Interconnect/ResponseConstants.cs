﻿
using System.Text;

namespace Hub.DesktopInterconnect
{

    public class ResponseConstants
    {
        public const float ApiVersion = 1.1f;

        public const string SuccessString = "Success";
        public const string FailString = "Fail";

        public static readonly byte[] SuccessResponse = Encoding.ASCII.GetBytes(SuccessString);
        public static readonly byte[] ApiResponse = Encoding.ASCII.GetBytes(ApiVersion.ToString("F"));
    }


    /// <summary>
    /// Scanner command response codes
    /// </summary>
    public enum ScannerCommands
    {
        Unknown = 0,
        ApiCompatability = 1,
        ApiVersion = 2,

        //Global Commands
        SetName = 100,
        LogFile = 120,
        LogDiff = 121,
        CurrentProjects = 130,
        CameraPairs = 140,
        Capacity = 150,

        //Camera Commands
        CaptureImageSet = 200,

        //Project Management Commands
        RemoveProject = 300,
        getAllImageSets = 310,
        ImageSetMetaData = 320,
        ImageSetImageData = 321,
        ProjectDetails = 330,
        ProjectNameChange = 350,
        CurrentProject = 331,
    }
}
