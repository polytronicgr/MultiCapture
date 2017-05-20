﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharedDeviceItems;

namespace Hub.Helpers.Interface
{
    public interface ICameraManager
    {
        string SavePath { get; set; }

        void CaptureImageSet();
        void CaptureImageSet(CameraRequest wanted);

#if DEBUG
        /// <summary>
        /// use when debugging - clears every socket buffer of data
        /// </summary>
        void ClearSockets();
#endif
    }
}
