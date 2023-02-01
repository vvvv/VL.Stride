// Copyright (c) .NET Foundation and Contributors (https://dotnetfoundation.org/ & https://stride3d.net) and Silicon Studio Corp. (https://www.siliconstudio.co.jp)
// Distributed under the MIT license. See the LICENSE.md file in the project root for more information.
using Stride.Core.Mathematics;
using System.Runtime.InteropServices;

namespace BezierSegment
{
    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    public struct BezierTubeSegmentStruct 
    {
        public Vector3 Pos0;    //12
        public Vector3 Pos1;    //12
        public Vector3 Pos2;    //12
        public Vector3 Pos3;    //12    //48

        public Vector3 Up0;     //12
        public Vector3 Up1;     //12
        public Vector3 Up2;     //12
        public Vector3 Up3;     //12    //48

        public Vector2 WH0;     //8
        public Vector2 WH1;     //8
        public Vector2 WH2;     //8
        public Vector2 WH3;     //8     //32

        public Color4 Col0;     //16
        public Color4 Col1;     //16
        public Color4 Col2;     //16
        public Color4 Col3;     //16    //64

        public int   FrontCap;  //4
        public int   EndCap;    //4     //8
                                        //200


        public BezierTubeSegmentStruct(
            Vector3 Pos0, Vector3 Pos1, Vector3 Pos2, Vector3 Pos3,
            Vector3 Up0, Vector3 Up1, Vector3 Up2, Vector3 Up3,
            Vector2 WH0, Vector2 WH1, Vector2 WH2, Vector2 WH3,
            Color4 Col0, Color4 Col1, Color4 Col2, Color4 Col3,
            bool FrontCap,  bool EndCap
        )
        {
            this.Pos0 = Pos0;
            this.Pos1 = Pos1;
            this.Pos2 = Pos2;
            this.Pos3 = Pos3;

            this.Up0 = Up0;
            this.Up1 = Up1;
            this.Up2 = Up2;
            this.Up3 = Up3;

            this.WH0 = WH0;
            this.WH1 = WH1;
            this.WH2 = WH2;
            this.WH3 = WH3;

            this.Col0 = Col0;
            this.Col1 = Col1;
            this.Col2 = Col2;
            this.Col3 = Col3;

            this.FrontCap   = FrontCap ? 1 : 0;
            this.EndCap     = EndCap ? 1 : 0; ;
        }
    }

}