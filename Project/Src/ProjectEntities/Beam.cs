//// Copyright (C) 2006-2007 NeoAxis Group
//using System;
//using System.Collections.Generic;
//using System.Text;
//using System.Runtime.InteropServices;
//using System.Drawing.Design;
//using System.ComponentModel;
//using Engine;
//using Engine.EntitySystem;
//using Engine.Renderer;
//using Engine.MapSystem;
//using Engine.MathEx;
//using Engine.PhysicsSystem;
//using Engine.Utils;

//namespace GameGeneral
//{
//    public class BeamType : MapObjectType
//    {
//        public const float torad = (1.0f / 360.0f) * 2.0f * (float)Math.PI;

//        [TypeConverter(typeof(ExpandableObjectConverter))]
//        public class AnimatedFloat
//        {
//            [TypeConverter(typeof(ExpandableObjectConverter))]
//            public abstract class BaseFunction
//            {
//                [FieldSerialize]
//                bool islooped;
//                public bool IsLooped
//                {
//                    get { return islooped; }
//                    set { islooped = value; }
//                }

//                protected float localval;
//                public float Val
//                {
//                    get { return localval; }
//                }

//                public abstract void Tick(float deltatime);

//                static protected void CheckLoop(ref float valtockeck, ref float speed, float min, float max)
//                {
//                    if (valtockeck > max)
//                    {
//                        speed *= (-1);
//                        valtockeck += (max - valtockeck);
//                    }
//                    else
//                        if (valtockeck < min)
//                        {
//                            speed *= (-1);
//                            valtockeck += (min - valtockeck);
//                        }
//                }

//                static protected void CheckClamp(ref  float valtockeck, float min, float max)
//                {
//                    if (valtockeck > max)
//                    {
//                        valtockeck = max;
//                    }
//                    else
//                        if (valtockeck < min)
//                        {
//                            valtockeck = min;
//                        }
//                }

//                static protected void CheckReset(ref  float valtockeck, float min, float max)
//                {
//                    if (valtockeck > max)
//                    {
//                        valtockeck = min;
//                    }
//                    else
//                        if (valtockeck < min)
//                        {
//                            valtockeck = max;
//                        }
//                }

//                public override string ToString()
//                {
//                    return String.Format("Evolution Fonction");
//                }
//            }

//            [TypeConverter(typeof(ExpandableObjectConverter))]
//            public class CirularFunction : BaseFunction
//            {
//                [FieldSerialize]
//                float anglemin = 0;
//                public float AngleMin
//                {
//                    get { return anglemin; }
//                    set
//                    {
//                        if (value <= anglemax)
//                            anglemin = value;

//                    }
//                }

//                [FieldSerialize]
//                float anglemax = 0;
//                public float AngleMax
//                {
//                    get { return anglemax; }
//                    set
//                    {
//                        if (value >= AngleMin)
//                            anglemax = value;
//                    }
//                }

//                [FieldSerialize]
//                float originalanglespeed = 0;
//                [Description("Speed of Variation of the angle, in degree per second")]
//                public float AngleSpeedVariation
//                {
//                    get { return originalanglespeed; }
//                    set
//                    {
//                        originalanglespeed = value;
//                        localanglespeed = value;
//                    }
//                }

//                // phasis is used for cosine et sinus variation
//                [FieldSerialize]
//                float phasis = 0;
//                public float Phasis
//                {
//                    get { return phasis; }
//                    set { phasis = value; }
//                }

//                [FieldSerialize]
//                float radius = 0.0f;
//                public float Radius
//                {
//                    get { return radius; }
//                    set { radius = value; }
//                }

//                [FieldSerialize]
//                float center = 0.0f;
//                public float Center
//                {
//                    get { return center; }
//                    set { center = value; }
//                }

//                [FieldSerialize]
//                float anglestartvalue = 0;
//                public float AngleStartValue
//                {
//                    get { return anglestartvalue; }
//                    set { anglestartvalue = value; }
//                }

//                float localangle;
//                float localanglespeed;

//                public CirularFunction(float argstartangle, float argphasis, float argradius, float argcenter,
//                                       float arganglemin, float arganglemax, float arganglespeed, bool looped)
//                {
//                    phasis = argphasis; radius = argradius; center = argcenter;
//                    anglemin = Math.Min(arganglemin, arganglemax);
//                    anglemax = Math.Max(anglemin, arganglemax);
//                    anglestartvalue = Math.Max(argstartangle, anglemin);
//                    anglestartvalue = Math.Min(anglestartvalue, anglemax);
//                    originalanglespeed = arganglespeed;
//                    localanglespeed = arganglespeed;
//                    localangle = anglestartvalue;
//                    this.IsLooped = looped;

//                }

//                public override void Tick(float deltatime)
//                {
//                    if (IsLooped)
//                    {
//                        localangle += localanglespeed * deltatime;
//                        CheckLoop(ref  localangle, ref localanglespeed, anglemin, anglemax);
//                        localval = center + radius * (float)Math.Cos((localangle + phasis) * torad);
//                    }
//                    else
//                    {
//                        localangle += localanglespeed * deltatime;
//                        CheckReset(ref  localangle, anglemin, anglemax);
//                        localval = center + radius * (float)Math.Cos((localangle + phasis) * torad);
//                    }
//                }
//            }

//            [TypeConverter(typeof(ExpandableObjectConverter))]
//            public class LinearFunction : BaseFunction
//            {
//                [FieldSerialize]
//                float min;
//                public float Min
//                {
//                    get { return min; }
//                    set
//                    {
//                        if (value <= max)
//                            min = value;

//                    }
//                }

//                [FieldSerialize]
//                float max;
//                public float Max
//                {
//                    get { return max; }
//                    set
//                    {
//                        if (value >= min)
//                            max = value;
//                    }
//                }

//                [FieldSerialize]
//                float originalspeed;
//                public float Speed
//                {
//                    get { return originalspeed; }
//                    set
//                    {
//                        originalspeed = value;
//                        localspeed = value;
//                    }

//                }

//                [FieldSerialize]
//                float startval = 0.0f;
//                public float StartValue
//                {
//                    get { return startval; }
//                    set
//                    {
//                        startval = value;
//                        startval = Math.Max(startval, min);
//                        startval = Math.Min(startval, max);
//                        localval = startval;
//                    }
//                }

//                float localspeed;

//                /*
//                public LinearFunction(float argmin, float argmax, float argstartval,float speed, bool looped)
//                {
//                    min = Math.Min(argmin, argmax);
//                    max = Math.Max(argmin, argmax);
//                    startval = Math.Max(argstartval, min);
//                    startval = Math.Min(startval, max);
//                    originalspeed = speed;
//                    localspeed = originalspeed;
//                    localval = startval;
//                    IsLooped = looped;
//                }
//                */
//                public override void Tick(float deltatime)
//                {
//                    if (IsLooped)
//                    {
//                        localval += localspeed * deltatime;
//                        CheckLoop(ref  localval, ref localspeed, min, max);
//                    }
//                    else
//                    {
//                        localval += localspeed * deltatime;
//                        CheckReset(ref  localval, min, max);
//                    }
//                }

//                public void Init()
//                {
//                    localval = StartValue;
//                    localspeed = originalspeed;
//                }
//            }

//            [Browsable(false)]
//            float totalval;
//            public float Val
//            {
//                get
//                {
//                    totalval = /*circularcomponent.Val +*/linearcomponent.Val;
//                    return totalval;
//                }
//            }

//            /*
//            [FieldSerialize]
//            CirularFunction circularcomponent = new CirularFunction(0,0,0,0,0,0,0,true);
//            public CirularFunction CircularComponent
//            {
//                get { return circularcomponent; }
//                set { circularcomponent = value; }
//            }
//             */

//            [FieldSerialize]
//            LinearFunction linearcomponent = new LinearFunction(); //0,0,0,0,true);
//            public LinearFunction LinearComponent
//            {
//                get { return linearcomponent; }
//                set { linearcomponent = value; }
//            }

//            public void Tick(float deltatime)
//            {
//                //circularcomponent.Tick(deltatime);
//                linearcomponent.Tick(deltatime);
//            }

//            public override string ToString()
//            {
//                return string.Format("Variable Parameters");
//            }

//            public void Init()
//            {
//                linearcomponent.Init();
//                totalval = linearcomponent.Val;
//            }
//        }

//        [TypeConverter(typeof(ExpandableObjectConverter))]
//        public class CylindricCoord
//        {
//            [FieldSerialize]
//            AnimatedFloat r;
//            [Description("Radius, distance from the axis")]
//            public AnimatedFloat R
//            {
//                get { return r; }
//                set { r = value; }
//            }

//            [FieldSerialize]
//            AnimatedFloat teta;
//            [Description("Angular position, around X axis")]
//            public AnimatedFloat Teta
//            {
//                get { return teta; }
//                set { teta = value; }
//            }

//            [FieldSerialize]
//            AnimatedFloat z;
//            [Description("Position along the axis of the cylindric space, ie the X axis in carthesian position. 0 is the position of the object, 1 is the extremity (length)")]
//            public AnimatedFloat Z
//            {
//                get { return z; }
//                set { z = value; }
//            }

//            public Vec3 ToXYZ()
//            {
//                return new Vec3(z.Val,
//                                r.Val * (float)Math.Cos(teta.Val * torad),
//                                r.Val * (float)Math.Sin(teta.Val * torad));
//            }

//            public CylindricCoord()
//            {
//                r = new AnimatedFloat();
//                Teta = new AnimatedFloat();
//                z = new AnimatedFloat();
//            }
//            public void Tick(float deltatime)
//            {
//                r.Tick(deltatime);
//                z.Tick(deltatime);
//                teta.Tick(deltatime);
//            }

//            public override string ToString()
//            {
//                return string.Format("Cylindric Coordinate");
//            }

//            public void Init()
//            {
//                r.Init();
//                teta.Init();
//                z.Init();
//            }
//        }

//        [FieldSerialize]
//        bool useatachedcontrolpoint = false;
//        [DefaultValue(false)]
//        public bool UseAttachedObjectAsControlPoint
//        {
//            get { return useatachedcontrolpoint; }
//            set { useatachedcontrolpoint = value; }
//        }

//        [FieldSerialize]
//        float duration = 0;
//        [Category("Settings : Time and Spawning")]
//        [DefaultValue(0)]
//        public float Duration
//        {
//            get { return duration; }
//            set { duration = value; }
//        }

//        [FieldSerialize]
//        int numrepeat = 0;
//        [Category("Settings : Time and Spawning")]
//        [DefaultValue(0)]
//        public int CycleRepetitions
//        {
//            get { return numrepeat; }
//            set { numrepeat = value; }
//        }

//        [FieldSerialize]
//        float updatetime = 0.1f;
//        [Category("Settings : Time and Spawning")]
//        [DefaultValue(0.1f)]
//        public float UpDateTime
//        {
//            get { return updatetime; }
//            set { updatetime = value; }
//        }

//        [FieldSerialize]
//        MapObjectType mainpointobjtype;
//        [Category("Settings : Time and Spawning")]
//        public MapObjectType MainPointObjectType
//        {
//            get { return mainpointobjtype; }
//            set { mainpointobjtype = value; }
//        }

//        [FieldSerialize]
//        float jointobjectprobability;
//        [Category("Settings : Time and Spawning")]
//        [DefaultValue(0.1f)]
//        public float JointObjectSpawnProbability
//        {
//            get { return jointobjectprobability; }
//            set { jointobjectprobability = value; }
//        }

//        [FieldSerialize]
//        float pitchspawningdeviation;
//        [Category("Settings : Time and Spawning")]
//        public float PitchSpawningDeviation
//        {
//            get { return pitchspawningdeviation; }
//            set { pitchspawningdeviation = value; }
//        }

//        [FieldSerialize]
//        float yawspawningdeviation;
//        [Category("Settings : Time and Spawning")]
//        public float YawSpawningDeviation
//        {
//            get { return yawspawningdeviation; }
//            set { yawspawningdeviation = value; }
//        }

//        [FieldSerialize]
//        float rollspawningdeviation;
//        [Category("Settings : Time and Spawning")]
//        public float RollSpawningDeviation
//        {
//            get { return rollspawningdeviation; }
//            set { rollspawningdeviation = value; }
//        }

//        # region Settings : Shape
//        [FieldSerialize]
//        int numberofpoints = 10;
//        [Category("Settings : Shape")]
//        [DefaultValue(10)]
//        public int NumberOfPoints
//        {
//            get { return numberofpoints; }
//            set { numberofpoints = value; }
//        }

//        [FieldSerialize]
//        float length = 1.0f;
//        [Category("Settings : Shape")]
//        [DefaultValue(1.0f)]
//        public float Length
//        {
//            get { return length; }
//            set { length = value; }
//        }

//        [FieldSerialize]
//        bool raycast;
//        [Category("Settings : Shape")]
//        public bool UseRayCast
//        {
//            get { return raycast; }
//            set { raycast = value; }
//        }

//        [FieldSerialize]
//        AnimatedFloat startwidth = new AnimatedFloat();
//        [Category("Settings : Shape")]
//        public AnimatedFloat WidthStart
//        {
//            get { return startwidth; }
//            set { startwidth = value; }
//        }

//        [FieldSerialize]
//        AnimatedFloat endwidth = new AnimatedFloat();
//        [Category("Settings : Shape")]
//        public AnimatedFloat WidthEnd
//        {
//            get { return endwidth; }
//            set { endwidth = value; }
//        }

//        [FieldSerialize]
//        Vec3 noisealongcurve = Vec3.Zero;
//        [Category("Settings : Shape")]
//        public Vec3 NoiseAlongCurve
//        {
//            get { return noisealongcurve; }
//            set { noisealongcurve = value; }
//        }

//        [FieldSerialize]
//        bool usefastbeam = false;
//        [Category("Settings : Shape")]
//        public bool UseFastBeam
//        {
//            get { return usefastbeam; }
//            set { usefastbeam = value; }
//        }

//        [FieldSerialize]
//        Vec3 controlpointnoisefactor = Vec3.Zero;
//        [Category("Settings : Shape")]
//        public Vec3 ControlPointNoiseFactor
//        {
//            get { return controlpointnoisefactor; }
//            set { controlpointnoisefactor = value; }
//        }

//        [FieldSerialize]
//        CylindricCoord Bcontrolpoint = new CylindricCoord();
//        [Category("Settings : Shape")]
//        public CylindricCoord BControlPoint
//        {
//            get { return Bcontrolpoint; }
//            set { Bcontrolpoint = value; }
//        }

//        [FieldSerialize]
//        CylindricCoord Ccontrolpoint = new CylindricCoord();
//        [Category("Settings : Shape")]
//        public CylindricCoord CControlPoint
//        {
//            get { return Ccontrolpoint; }
//            set { Ccontrolpoint = value; }
//        }
//        #endregion

//        [FieldSerialize]
//        bool permainpointcamface = false;
//        [Description("Set to True to enable a per main point segment facing. CPU consuming.")]
//        [Category("Settings : Rendering")]
//        [DefaultValue(false)]
//        public bool PerMainPointCameraFace
//        {
//            get { return permainpointcamface; }
//            set { permainpointcamface = value; }
//        }

//        [FieldSerialize]
//        bool doubleshape = false;
//        [Category("Settings : Rendering")]
//        [DefaultValue(false)]
//        public bool DoubleShape
//        {
//            get { return doubleshape; }
//            set { doubleshape = value; }
//        }

//        [FieldSerialize]
//        bool disablemipmapfiltering = false;
//        [Category("Settings : Rendering")]
//        public bool DisableMipMapFiltering
//        {
//            get { return disablemipmapfiltering; }
//            set { disablemipmapfiltering = value; }
//        }

//        [FieldSerialize]
//        float textureperunit = 1.0f;
//        [Category("Settings : Rendering")]
//        public float TexturePerWorldUnit
//        {
//            get { return textureperunit; }
//            set { textureperunit = value; }
//        }

//        [FieldSerialize]
//        string materialName = "";
//        [Category("Settings : Rendering")]
//        [Editor(typeof(EditorMaterialUITypeEditor), typeof(UITypeEditor))]
//        public string MaterialName
//        {
//            get { return materialName; }
//            set { materialName = value; }
//        }

//        #region Settings : color
//        [FieldSerialize]
//        ColorValue startcolorvalue = new ColorValue();
//        [Category("Settings : Color")]
//        public ColorValue StartColor
//        {
//            get { return startcolorvalue; }
//            set { startcolorvalue = value; }
//        }

//        [FieldSerialize]
//        ColorValue middlecolorvalue = new ColorValue();
//        [Category("Settings : Color")]
//        public ColorValue MiddleColor
//        {
//            get { return middlecolorvalue; }
//            set { middlecolorvalue = value; }
//        }

//        [FieldSerialize]
//        float middlecolorpos = 0.5f;
//        [Category("Settings : Color")]
//        public float MiddleColorPosition
//        {
//            get { return middlecolorpos; }
//            set { middlecolorpos = value; }
//        }

//        [FieldSerialize]
//        ColorValue endcolorvalue = new ColorValue();
//        [Category("Settings : Color")]
//        public ColorValue EndColor
//        {
//            get { return endcolorvalue; }
//            set { endcolorvalue = value; }
//        }

//        [FieldSerialize]
//        float fadeinend = 0.0f;
//        [Category("Settings : Color")]
//        [EditorLimitsRange(0.0f, 1.0f)]
//        public float FadeInEnd
//        {
//            get { return fadeinend; }
//            set
//            {
//                fadeinend = value;
//                if (fadeinend > fadeoutstart)
//                    fadeinend = fadeoutstart;
//            }
//        }

//        [FieldSerialize]
//        float fadeoutstart = 1.0f;
//        [Category("Settings : Color")]
//        [EditorLimitsRange(0.0f, 1.0f)]
//        public float FadeOutStart
//        {
//            get { return fadeoutstart; }
//            set
//            {
//                fadeoutstart = value;
//                if (fadeoutstart < fadeinend)
//                    fadeoutstart = fadeinend;
//            }
//        }
//        #endregion
//    }

//    /// <summary>
//    /// Example of dynamic geometry.
//    /// </summary>
//    public class Beam : MapObject
//    {
//        Mesh mesh;

//        MapObjectAttachedMesh attachedMesh;
//        bool useoverridenparam = false;
//        float raycastdistance;

//        [FieldSerialize]
//        float length = 1.0f;
//        [DefaultValue(1.0f)]
//        public float Length
//        {
//            get
//            {
//                if (Type.UseRayCast)
//                    return raycastdistance;
//                if (useoverridenparam)
//                    return length;
//                else
//                    return this.Type.Length;
//            }
//            set
//            {
//                if (useoverridenparam)
//                {
//                    length = value;
//                    raycastdistance = length;
//                }
//                else return;
//            }
//        }

//        // local variable...
//        BeamType.AnimatedFloat endwidth = new BeamType.AnimatedFloat();
//        BeamType.AnimatedFloat startwidth = new BeamType.AnimatedFloat();
//        BeamType.CylindricCoord bcontrolpoint = new BeamType.CylindricCoord();
//        BeamType.CylindricCoord ccontrolpoint = new BeamType.CylindricCoord();

//        float currentduration = 1.0f;
//        int numofcycle;
//        public int NumberOfPoints { get { return this.Type.NumberOfPoints; } }

//        public bool DoubleShape
//        {
//            get
//            {
//                if (editormode)
//                    return false;
//                else
//                    return this.Type.DoubleShape;
//            }

//        }

//        float updateTimeRemaining;
//        bool editormode = false;
//        bool needUpdateVertices;
//        bool needUpdateIndices;
//        List<Vec3> MainPoint = new List<Vec3>();
//        List<Vec3> MainPointDir = new List<Vec3>();
//        ///////////////////////////////////////////

//        Bounds bounds = new Bounds(new Vec3(-10.0f, -5.0f, -5.0f), new Vec3(10.0f, 5.0f, 5.0f));

//        [StructLayout(LayoutKind.Sequential)]
//        struct Vertex
//        {
//            public Vec3 position;
//            public Vec3 normal;
//            public Vec2 texCoord;
//            public uint color;
//        }
//        ///////////////////////////////////////////

//        BeamType _type = null; public new BeamType Type { get { return _type; } }

//        protected override void OnCreate()
//        {
//            base.OnCreate();
//        }

//        protected override void OnPostCreate(bool loaded)
//        {
//            base.OnPostCreate(loaded);
//            CreateMesh();
//            AttachMesh();
//            AddTimer();
//            RenderSystem.Instance.RenderSystemEvent += RenderSystem_RenderSystemEvent;

//        }

//        void ControlPointInit()
//        {
//            ccontrolpoint = this.Type.CControlPoint;
//            bcontrolpoint = this.Type.BControlPoint;
//            ccontrolpoint.Init();
//            bcontrolpoint.Init();
//        }

//        void ControlPointTick(float delta)
//        {
//            ccontrolpoint.Tick(delta);
//            bcontrolpoint.Tick(delta);
//        }

//        protected override void OnPreCreate()
//        {
//            base.OnPreCreate();
//            raycastdistance = this.Type.Length;
//            ControlPointInit();

//            endwidth = this.Type.WidthEnd;
//            startwidth = this.Type.WidthStart;
//            length = this.Type.Length;
//            currentduration = Type.Duration;
//            numofcycle = Type.CycleRepetitions;
//            if (EntitySystemWorld.Instance.WorldSimulationType == WorldSimulationTypes.Editor)
//            { editormode = true; }
//        }

//        protected override void OnDestroy()
//        {
//            RenderSystem.Instance.RenderSystemEvent -= RenderSystem_RenderSystemEvent;
//            DetachMesh();
//            DestroyMesh();
//            base.OnDestroy();
//        }

//        void RenderSystem_RenderSystemEvent(string name)
//        {
//            if (name == "DeviceRestored")
//            {
//                needUpdateVertices = true;
//                needUpdateIndices = true;
//            }
//        }

//        void CreateMesh()
//        {
//            string meshName = MeshManager.Instance.GetUniqueName("DynamicMesh");
//            mesh = MeshManager.Instance.CreateManual(meshName);

//            mesh.SetBoundsAndRadius(bounds, bounds.Radius(Vec3.Zero));

//            SubMesh subMesh = mesh.CreateSubMesh();
//            subMesh.UseSharedVertices = false;

//            int maxVertices = (NumberOfPoints + 1) * 2;
//            int maxIndices = (NumberOfPoints) * 6;// *3;

//            if (DoubleShape)
//            {
//                maxVertices = maxVertices * 2;
//                maxIndices = maxIndices * 2;
//            }

//            //init vertexData
//            VertexDeclaration declaration = subMesh.VertexData.VertexDeclaration;
//            declaration.AddElement(0, 0, VertexElementType.Float3, VertexElementSemantic.Position);
//            declaration.AddElement(0, 12, VertexElementType.Float3, VertexElementSemantic.Normal);
//            declaration.AddElement(0, 24, VertexElementType.Float2, VertexElementSemantic.TextureCoordinates);
//            declaration.AddElement(0, 32, VertexElementType.Color, VertexElementSemantic.Diffuse, 0);

//            VertexBufferBinding bufferBinding = subMesh.VertexData.VertexBufferBinding;
//            HardwareVertexBuffer vertexBuffer = HardwareBufferManager.Instance.CreateVertexBuffer(
//               32 + sizeof(uint), maxVertices, HardwareBuffer.Usage.DynamicWriteOnly);
//            bufferBinding.SetBinding(0, vertexBuffer, true);

//            //init indexData
//            HardwareIndexBuffer indexBuffer = HardwareBufferManager.Instance.CreateIndexBuffer(
//               HardwareIndexBuffer.IndexType._16Bit, maxIndices, HardwareBuffer.Usage.DynamicWriteOnly);
//            subMesh.IndexData.SetIndexBuffer(indexBuffer, true);

//            //set material
//            subMesh.MaterialName = Type.MaterialName;
//            if (Type.DisableMipMapFiltering)
//            {
//                // todo : currently, it only supports highlevel material
//                HighLevelMaterial material = HighLevelMaterialManager.Instance.GetMaterialByName(Type.MaterialName);
//                Material basemat = material.BaseMaterial;
//                foreach (Technique tech in basemat.Techniques)
//                    foreach (Pass pass in tech.Passes)
//                        foreach (TextureUnitState techunitstat in pass.TextureUnitStates)
//                        {
//                            techunitstat.SetTextureFiltering(FilterOptions.None, FilterOptions.None, FilterOptions.None);
//                        }
//            }

//            needUpdateVertices = true;
//            needUpdateIndices = true;
//        }

//        void DestroyMesh()
//        {
//            if (mesh != null)
//            {
//                mesh.Dispose();
//                mesh = null;
//            }
//        }

//        void AttachMesh()
//        {
//            attachedMesh = new MapObjectAttachedMesh();
//            attachedMesh.CastShadows = false;
//            attachedMesh.SetMeshObject(mesh.Name);
//            Attach(attachedMesh);
//        }

//        void DetachMesh()
//        {
//            if (attachedMesh != null)
//            {
//                Detach(attachedMesh);
//                attachedMesh = null;
//            }
//        }

//        float compwidth(float param)
//        {
//            float localparam = param / Length;
//            return (endwidth.Val - startwidth.Val) * localparam + startwidth.Val;
//        }

//        uint ComputeColorValue(float param)
//        {
//            ColorValue color;
//            ColorValue A, B;
//            float localparam = param / Length;
//            // interpolation
//            if (param > Type.MiddleColorPosition)
//            {
//                B = (Type.MiddleColor * 1.0f - Type.EndColor * Type.MiddleColorPosition) / (1.0f - Type.MiddleColorPosition);
//                A = (Type.EndColor - B) / 1.0f;
//                color = A * localparam + B;
//            }
//            else
//            {
//                B = Type.StartColor;
//                A = (Type.MiddleColor - B) / (Type.MiddleColorPosition);
//                color = A * localparam + B;
//            }

//            // time related computation
//            float timefact = 1.0f - currentduration / Type.Duration;
//            float a = 1.0f;
//            // color = new ColorValue(1.0f, 1.0f, 1.0f, 1.0f);
//            if (timefact < Type.FadeInEnd)
//            {
//                a = timefact / Type.FadeInEnd;
//            }
//            else
//                if (timefact > Type.FadeOutStart)
//                {
//                    a = (1 - (timefact - Type.FadeOutStart) / (1.0f - Type.FadeOutStart));
//                }

//            color *= a;
//            color.Clamp(ColorValue.Zero, new ColorValue(1, 1, 1, 1));
//            return RenderSystem.Instance.ConvertColorValue(color);
//        }

//        void TrySpawnObjects()
//        {
//            if (!editormode)
//            {
//                if (MainPoint.Count > 0)
//                {
//                    if (Type.MainPointObjectType != null)
//                    {
//                        foreach (Vec3 curvepoint in MainPoint)
//                        {
//                            if (World.Instance.Random.NextFloat() <= Type.JointObjectSpawnProbability)
//                            {
//                                MapObject curveobj = (MapObject)Entities.Instance.Create(Type.MainPointObjectType, Map.Instance);
//                                // MapObjectAttachedMapObject curveattached = new MapObjectAttachedMapObject();
//                                //curveattached =

//                                curveobj.Position = curvepoint * this.Rotation + Position;
//                                curveobj.Rotation = this.Rotation * new Angles(World.Instance.Random.NextFloatCenter() * Type.RollSpawningDeviation,
//                                                                World.Instance.Random.NextFloatCenter() * Type.PitchSpawningDeviation,
//                                                                World.Instance.Random.NextFloatCenter() * Type.YawSpawningDeviation).ToQuat();
//                                curveobj.PostCreate();
//                            }
//                        }
//                    }
//                }
//            }
//        }

//        protected override void OnTick()
//        {
//            base.OnTick();
//            ControlPointTick(TickDelta);
//            endwidth.Tick(TickDelta);
//            startwidth.Tick(TickDelta);
//            TickDuration(TickDelta);
//            updateTimeRemaining -= TickDelta;
//            if (updateTimeRemaining < 0)
//            {
//                updateTimeRemaining += Type.UpDateTime;
//                needUpdateVertices = true; // Indices = true;
//                TrySpawnObjects();
//            }
//        }

//        void TickDuration(float tickdelta)
//        {
//            if (Type.Duration > 0)
//            {
//                currentduration -= tickdelta;
//                if (currentduration < 0)
//                {
//                    if (editormode)
//                    {
//                        currentduration += Type.Duration;
//                    }
//                    else
//                    {
//                        // checking if infini repetition enabled ?
//                        if (Type.CycleRepetitions > 0)
//                        {
//                            numofcycle--;
//                        }
//                        else
//                        {
//                            currentduration += Type.Duration;
//                            return;
//                        }

//                        // checking number of cycle remaining
//                        if (numofcycle > 0)
//                        {
//                            currentduration += Type.Duration;
//                        }
//                        else
//                        { this.SetShouldDelete(); }
//                    }
//                }
//            }
//        }

//        protected override void OnRender(Camera camera)
//        {
//            base.OnRender(camera);

//            if (attachedMesh != null)
//            {
//                bool visible = true; // camera.IsIntersectsFast(MapBounds);
//                attachedMesh.Visible = visible;

//                if (editormode)
//                {
//                    ControlPointTick(0.01f);
//                    endwidth.Tick(0.01f);
//                    startwidth.Tick(0.01f);
//                    needUpdateVertices = true;
//                    TickDuration(0.01f);
//                }

//                if (visible)
//                {
//                    //update mesh if needed
//                    if (needUpdateVertices)
//                    {
//                        UpdateMeshVertices_Bezier(camera, Type.UseFastBeam);
//                        needUpdateVertices = false;
//                    }

//                    if (needUpdateIndices)
//                    {
//                        UpdateMeshIndices();
//                        needUpdateIndices = false;
//                    }
//                }
//            }
//        }

//        void UpdateMeshVertices_Bezier(Camera camera, bool fastbeam)
//        {
//            SubMesh subMesh = mesh.SubMeshes[0];
//            //bounds = Bounds.Cleared;
//            MainPoint.Clear();
//            // init...
//            Quat rot = this.Rotation; rot.Inverse();
//            Vec3 v_A, v_B, v_C, v_D;
//            HardwareVertexBuffer vertexBuffer = subMesh.VertexData.VertexBufferBinding.GetBuffer(0);
//            Vec3 v_dir = new Vec3(1, 0, 0); // this.Rotation.ToMat3().Item0; // Type.EndPoint - Type.StartPoint;
//            Vec3 v_pos = Vec3.Zero;
//            Vec3 v_pos_prev = v_pos;
//            Vec3 v_pos_next = v_pos;
//            Vec3 tempy = GetLateralVector(this.Rotation, rot, v_pos, v_dir, camera);
//            Vec3 tempz = tempy.Cross(v_dir); // GetUpVector(rot, v_pos, v_dir, camera);
//            Vec3 localnoise = Vec3.Zero;
//            Vec3 v_dir2 = new Vec3(1, 0, 0);
//            int pointnum = NumberOfPoints;
//            bool doperpointfacing = Type.PerMainPointCameraFace;
//            float a = 1.0f - 1.0f / pointnum;
//            float b = 1.0f - a;
//            float localwidth = compwidth(0);

//            // performingraycast if needed
//            if (Type.UseRayCast)
//                PerformRayCast();
//            float f_basedistance = Length;
//            unsafe
//            {
//                Vertex* buffer = (Vertex*)vertexBuffer.Lock(HardwareBuffer.LockOptions.Normal).ToPointer();
//                subMesh.VertexData.VertexCount = pointnum * 2;

//                // extremity point
//                v_A = v_pos;
//                v_D = v_pos + v_dir * f_basedistance;

//                // controlpoint
//                Vec3 loccpoint = bcontrolpoint.ToXYZ(); loccpoint.X *= Length;
//                v_B = v_pos + (loccpoint + Type.ControlPointNoiseFactor * World.Instance.Random.NextFloatCenter());
//                loccpoint = ccontrolpoint.ToXYZ(); loccpoint.X *= Length;
//                v_C = v_pos + (loccpoint + Type.ControlPointNoiseFactor * World.Instance.Random.NextFloatCenter());

//                // first point.
//                *buffer = BuildBeamVertex(v_pos_prev + (tempy * localwidth / 2.0f), 0.0f, 0.0f, tempy, v_dir);
//                buffer++;
//                *buffer = BuildBeamVertex(v_pos_prev - (tempy * localwidth / 2.0f), 0.0f, 1.0f, tempy, v_dir);
//                buffer++;
//                MainPoint.Add(v_pos);
//                MainPointDir.Add(v_dir);// this value is not used, in fact...
//                float currentlen = 0.0f;
//                // go through all points...
//                for (int i = 1; i < pointnum + 1; i++)
//                {
//                    if (fastbeam)
//                    {
//                        v_pos_next = v_pos_prev + v_dir * f_basedistance / pointnum;
//                        if (i < pointnum)
//                        {
//                            localnoise = new Vec3(Type.NoiseAlongCurve.X * World.Instance.Random.NextFloatCenter(),
//                                    Type.NoiseAlongCurve.Y * World.Instance.Random.NextFloatCenter(),
//                                    Type.NoiseAlongCurve.Z * World.Instance.Random.NextFloatCenter());
//                            v_pos_next += localnoise;
//                        }
//                        v_dir = v_D - v_pos_next;
//                        v_dir.Normalize();
//                        currentlen += f_basedistance / pointnum;
//                    }
//                    else
//                    {
//                        // Get a point on the curve
//                        v_pos_next = v_A * (float)Math.Pow(a, 3.0f) + v_B * 3.0f * (float)Math.Pow(a, 2.0f) * b + v_C * 3.0f * a * (float)Math.Pow(b, 2.0f) + v_D * (float)Math.Pow(b, 3.0f);
//                        if (i < pointnum)
//                        {
//                            localnoise = new Vec3(Type.NoiseAlongCurve.X * World.Instance.Random.NextFloatCenter(),
//                                    Type.NoiseAlongCurve.Y * World.Instance.Random.NextFloatCenter(),
//                                    Type.NoiseAlongCurve.Z * World.Instance.Random.NextFloatCenter());
//                            v_pos_next += localnoise;
//                        }
//                        v_dir = v_pos_next - v_pos_prev;
//                        currentlen += v_dir.Length();
//                        v_dir.Normalize();
//                    }

//                    localwidth = compwidth(v_pos_next.X);
//                    MainPoint.Add(v_pos_next);
//                    MainPointDir.Add(v_dir);

//                    // Change the variable
//                    a -= 1.0f / pointnum;
//                    b = 1.0f - a;

//                    if (doperpointfacing)
//                    {
//                        tempy = GetLateralVector(this.Rotation, rot, v_pos_next, v_dir, camera);
//                        tempz = tempy.Cross(v_dir);
//                    }
//                    *buffer = BuildBeamVertex(v_pos_next + (tempy * localwidth / 2.0f), currentlen * Type.TexturePerWorldUnit, 0.0f, tempy, v_dir);
//                    buffer++;
//                    *buffer = BuildBeamVertex(v_pos_next - (tempy * localwidth / 2.0f), currentlen * Type.TexturePerWorldUnit, 1.0f, tempy, v_dir);
//                    buffer++;

//                    v_pos_prev = v_pos_next;

//                }

//                if (DoubleShape)
//                {
//                    v_dir = new Vec3(1, 0, 0); // this.Rotation.ToMat3().Item0; // Type.EndPoint - Type.StartPoint;
//                    f_basedistance = Length;
//                    v_pos = Vec3.Zero;
//                    v_pos_prev = v_pos;
//                    v_pos_next = v_pos;
//                    tempy = GetLateralVector(this.Rotation, rot, v_pos, v_dir, camera);
//                    tempz = tempy.Cross(v_dir); // GetUpVector(rot, v_pos, v_dir, camera);
//                    localwidth = compwidth(0);
//                    *buffer = BuildBeamVertex2(v_pos + (tempz * localwidth / 2.0f), 0.0f, 0.0f, tempy, v_dir);
//                    buffer++;
//                    *buffer = BuildBeamVertex2(v_pos - (tempz * localwidth / 2.0f), 0.0f, 1.0f, tempy, v_dir);
//                    buffer++;
//                    currentlen = 0.0f;
//                    Vec3 localdist;
//                    localdist = v_pos;
//                    // go through all points, second pass.
//                    for (int i = 0; i < pointnum + 1; i++)
//                    {
//                        // Get a point on the curve
//                        v_pos_prev = v_pos_next;
//                        v_pos_next = MainPoint[i];
//                        localdist = v_pos_next - v_pos_prev;
//                        currentlen += localdist.Length();
//                        v_dir = MainPointDir[i];
//                        if (doperpointfacing)
//                        {
//                            tempy = GetLateralVector(this.Rotation, rot, v_pos_next, v_dir, camera);
//                            tempz = tempy.Cross(v_dir);
//                        }
//                        localwidth = compwidth(v_pos_next.X);
//                        *buffer = BuildBeamVertex2(v_pos_next + (tempz * localwidth / 2.0f), i * 1.0f, 0.0f, tempy, v_dir);
//                        buffer++;
//                        *buffer = BuildBeamVertex2(v_pos_next - (tempz * localwidth / 2.0f), i * 1.0f, 1.0f, tempy, v_dir);
//                        buffer++;
//                    }
//                }

//                vertexBuffer.Unlock();
//            }//end of unsafe bloc.

//            // updating bounds. a margin of 1.5 time the mainpoint coord are taken to ensure visibility
//            bounds = Bounds.Zero;
//            bounds.Add(Vec3.Zero);
//            foreach (Vec3 mainpoint in MainPoint)
//                bounds.Add(mainpoint * 1.5f);

//            mesh.SetBoundsAndRadius(bounds, bounds.Radius(Vec3.Zero));

//            foreach (MapObjectAttachedObject obj in AttachedObjects)
//            {
//                if (obj.Alias.Contains("EndObject"))
//                { obj.PositionOffset = new Vec3(Length, 0, 0); continue; }

//                if (obj.Alias.Contains("StartObject"))
//                { obj.PositionOffset = new Vec3(0, 0, 0); continue; }

//                if ((!Type.UseAttachedObjectAsControlPoint) && (!fastbeam))
//                {
//                    if (obj.Alias.Contains("BControlPoint"))
//                    { obj.PositionOffset = v_B; continue; }

//                    if (obj.Alias.Contains("CControlPoint"))
//                    { obj.PositionOffset = v_C; continue; }
//                }

//            }
//        }

//        Vec3 GetLateralVector(Quat rot, Quat invertedrot, Vec3 point, Vec3 dir, Camera camera)
//        {
//            //taking camera position into account....
//            Vec3 campos = (camera.Position);// - Position);
//            Vec3 worldpoint = point * rot + Position;
//            Vec3 worldcamdirtopoint = campos - worldpoint;
//            Vec3 localcamdirtopoint = worldcamdirtopoint;
//            localcamdirtopoint = localcamdirtopoint * invertedrot;
//            localcamdirtopoint.Normalize();
//            Vec3 returnresult = localcamdirtopoint.Cross(dir);
//            returnresult.Normalize();
//            return returnresult;

//        }

//        Vec3 GetUpVector(Quat invertedrot, Vec3 point, Vec3 dir, Camera camera)
//        {
//            //taking camera position into account....
//            Vec3 campos = (camera.Position - Position);

//            if (invertedrot.X != 0 && invertedrot.Y != 0 && invertedrot.X != 0)
//            {
//                campos = campos * invertedrot;
//            }

//            Vec3 computed_dir = campos - point;
//            computed_dir.Normalize();
//            Vec3 upvec = computed_dir.Cross(dir);
//            return upvec.Cross(dir);

//        }

//        // normal version, for one shape
//        Vertex BuildBeamVertex(Vec3 Point, float u, float v, Vec3 lateral, Vec3 dir)
//        {
//            Vertex vertex = new Vertex();
//            vertex.position = Point;
//            vertex.normal = lateral.Cross(dir);
//            vertex.texCoord = new Vec2(u, v);
//            vertex.color = ComputeColorValue(Point.X);
//            return vertex;
//        }

//        // upvector for 2shape
//        Vertex BuildBeamVertex2(Vec3 Point, float u, float v, Vec3 lateral, Vec3 dir)
//        {
//            Vertex vertex = new Vertex();
//            vertex.position = Point;
//            vertex.normal = lateral;
//            vertex.texCoord = new Vec2(u, v);
//            vertex.color = ComputeColorValue(Point.X);
//            return vertex;
//        }

//        void UpdateMeshIndices()
//        {
//            if (mesh == null)
//                return;

//            SubMesh subMesh = mesh.SubMeshes[0];
//            HardwareIndexBuffer indexBuffer = subMesh.IndexData.IndexBuffer;

//            unsafe
//            {
//                ushort* buffer = (ushort*)indexBuffer.Lock(HardwareBuffer.LockOptions.Normal).ToPointer();

//                subMesh.IndexData.IndexCount = 0;
//                int curindex = 0;
//                for (int i = 0; i < (NumberOfPoints); i++)
//                {
//                    curindex = i * 2;
//                    *buffer = (ushort)(curindex); buffer++;
//                    *buffer = (ushort)(curindex + 1); buffer++;
//                    *buffer = (ushort)(curindex + 3); buffer++;
//                    *buffer = (ushort)(curindex + 3); buffer++;
//                    *buffer = (ushort)(curindex + 2); buffer++;
//                    *buffer = (ushort)(curindex); buffer++;
//                    subMesh.IndexData.IndexCount += 6;
//                }

//                if (DoubleShape)
//                {
//                    for (int i = 0; i < (NumberOfPoints); i++)
//                    {
//                        curindex = (NumberOfPoints + 1) * 2 + i * 2;
//                        *buffer = (ushort)(curindex); buffer++;
//                        *buffer = (ushort)(curindex + 1); buffer++;
//                        *buffer = (ushort)(curindex + 3); buffer++;
//                        *buffer = (ushort)(curindex + 3); buffer++;
//                        *buffer = (ushort)(curindex + 2); buffer++;
//                        *buffer = (ushort)(curindex); buffer++;
//                        subMesh.IndexData.IndexCount += 6;
//                    }

//                }
//                indexBuffer.Unlock();
//            }

//        }

//        void PerformRayCast()
//        {
//            if (!editormode)
//            {
//                Ray lookRay = new Ray(Position, Rotation.ToMat3().Item0);
//                Body body = null;
//                Vec3 lookFrom = lookRay.Origin;
//                Vec3 lookDir = Vec3.Normalize(lookRay.Direction);
//                if (useoverridenparam)
//                    raycastdistance = length;
//                else
//                    raycastdistance = this.Type.Length; ;

//                List<RayCastResult> piercingResult = PhysicsWorld.Instance.RayCastPiercing(
//                    new Ray(lookFrom, lookDir * raycastdistance), (int)ContactGroup.CastOnlyContact);

//                foreach (RayCastResult result in piercingResult)
//                {
//                    MapObject obj = MapSystemWorld.GetMapObjectByBody(result.Shape.Body);
//                    body = result.Shape.Body;
//                    raycastdistance = result.Distance;
//                    break;
//                }
//            }
//        }
//    }
//}