using System.ComponentModel;

// Copyright (C) NeoAxis Group Ltd. This is part of NeoAxis 3D Engine SDK.
using System.Drawing.Design;
using Engine.MathEx;
using Engine.Renderer;

namespace ProjectCommon
{
    [CompositorName("NightVision")]
    public class NightVisionCompositorParameters : CompositorParameters
    {
        private float red = 1;
        private float green = 1;
        private float blue = 1;
        private float alpha = 1; //Opacity
        //Material Material = "\\Base\\FullScreenEffects\\NightVision\\Random3D.dds";

        [DefaultValue(1.0f)]
        [Editor(typeof(SingleValueEditor), typeof(UITypeEditor))]
        [EditorLimitsRange(0, 5)]
        public float Red
        {
            get { return red; }
            set { red = value; }
        }

        [DefaultValue(1.0f)]
        [Editor(typeof(SingleValueEditor), typeof(UITypeEditor))]
        [EditorLimitsRange(0, 5)]
        public float Green
        {
            get { return green; }
            set { green = value; }
        }

        [DefaultValue(1.0f)]
        [Editor(typeof(SingleValueEditor), typeof(UITypeEditor))]
        [EditorLimitsRange(0, 5)]
        public float Blue
        {
            get { return blue; }
            set { blue = value; }
        }

        [DefaultValue(0.5f)]
        [Editor(typeof(SingleValueEditor), typeof(UITypeEditor))]
        [EditorLimitsRange(0, 1)]
        public float Alpha
        {
            get { return alpha; }
            set { alpha = value; }
        }
    }

    /// <summary>
    /// Represents work with the ColorCorrection post effect.
    /// </summary>
    [CompositorName("NightVision")]
    public class NightVisionCompositorInstance : CompositorInstance
    {
        private float red = 1;
        private float green = 1;
        private float blue = 1;
        private float alpha = 1;

        //

        [EditorLimitsRange(0, 5)]
        public float Red
        {
            get { return red; }
            set { red = value; }
        }

        [EditorLimitsRange(0, 5)]
        public float Green
        {
            get { return green; }
            set { green = value; }
        }

        [EditorLimitsRange(0, 5)]
        public float Blue
        {
            get { return blue; }
            set { blue = value; }
        }

        [DefaultValue(1.0f)]
        [Editor(typeof(SingleValueEditor), typeof(UITypeEditor))]
        [EditorLimitsRange(0, 5)]
        public float Alpha
        {
            get { return alpha; }
            set { alpha = value; }
        }

        protected override void OnCreateTexture(string definitionName, ref Vec2I size, ref PixelFormat format)
        {
            base.OnCreateTexture(definitionName, ref size, ref format);

            //if (definitionName == "scene" || definitionName == "temp")
            //    size = Owner.DimensionsInPixels.Size / 2;
        }

        protected override void OnMaterialRender(uint passId, Material material, ref bool skipPass)
        {
            base.OnMaterialRender(passId, material, ref skipPass);

            if (passId == 333) //Incin framerate?
            {
                Vec4 multiplier = new Vec4(Red, Green, Blue, alpha);

                GpuProgramParameters parameters = material.Techniques[0].Passes[0].FragmentProgramParameters;
                parameters.SetNamedConstant("multiplier", multiplier);
            }
        }

        protected override void OnUpdateParameters(CompositorParameters parameters)
        {
            base.OnUpdateParameters(parameters);

            NightVisionCompositorParameters p = (NightVisionCompositorParameters)parameters;
            Red = p.Red;
            Green = p.Green;
            Blue = p.Blue;
            Alpha = p.Alpha;
        }
    }
}