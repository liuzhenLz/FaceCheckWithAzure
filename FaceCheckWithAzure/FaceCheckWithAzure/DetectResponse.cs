﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FaceCheckWithAzure
{
    public class FaceRectangle
    {
        public int top { get; set; }
        public int left { get; set; }
        public int width { get; set; }
        public int height { get; set; }
    }

    public class DetectResponse
    {
        public string faceId { get; set; }
        public FaceRectangle faceRectangle { get; set; }
    }
}
