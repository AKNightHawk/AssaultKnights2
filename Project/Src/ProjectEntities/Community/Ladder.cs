// Copyright (C) NeoAxis Group Ltd. This is part of NeoAxis 3D Engine SDK.
using Engine;
using Engine.EntitySystem;
using Engine.MapSystem;
using Engine.MathEx;
using Engine.Renderer;

namespace ProjectEntities.Community
{
    public class LadderType : MapObjectType
    {
    }

    public class Ladder : MapObject
    {
        //

        private LadderType _type = null; public new LadderType Type { get { return _type; } }

        protected override void OnCalculateMapBounds(ref Bounds bounds)
        {
            base.OnCalculateMapBounds(ref bounds);
            bounds = GetBox().ToBounds();
        }

        protected override void OnSetTransform(ref Vec3 pos, ref Quat rot, ref Vec3 scl)
        {
            scl.X = .1f;
            base.OnSetTransform(ref pos, ref rot, ref scl);
        }

        public bool IsInvalidOrientation()
        {
            if (Rotation.GetUp().Z < .999f)
                return true;
            return false;
        }

        public Line GetClimbingLine()
        {
            const float centerIndent = .45f;

            Vec3 point0 = Position + Rotation * new Vec3(-centerIndent, 0, -Scale.Z / 2);
            Vec3 point1 = Position + Rotation * new Vec3(-centerIndent, 0, Scale.Z / 2);
            return new Line(point0, point1);
        }

        protected override void OnRender(Camera camera)
        {
            base.OnRender(camera);

            if (EntitySystemWorld.Instance.WorldSimulationType == WorldSimulationTypes.Editor ||
                EngineDebugSettings.DrawGameSpecificDebugGeometry)
            {
                camera.DebugGeometry.Color = new ColorValue(0, 0, 1);
                camera.DebugGeometry.AddBox(GetBox());
            }

            if (EntitySystemWorld.Instance.WorldSimulationType == WorldSimulationTypes.Editor ||
                EngineDebugSettings.DrawGameSpecificDebugGeometry)
            {
                Vec3 direction = -Rotation.GetForward();

                Vec3 point0 = Position + Rotation * new Vec3(0, -Scale.Y / 2, -Scale.Z / 2);
                Vec3 point1 = Position + Rotation * new Vec3(0, Scale.Y / 2, -Scale.Z / 2);
                Vec3 point2 = Position + Rotation * new Vec3(0, -Scale.Y / 2, Scale.Z / 2);
                Vec3 point3 = Position + Rotation * new Vec3(0, Scale.Y / 2, Scale.Z / 2);

                camera.DebugGeometry.Color = new ColorValue(0, 1, 0);
                camera.DebugGeometry.AddArrow(point0, point0 + direction);
                camera.DebugGeometry.AddArrow(point1, point1 + direction);
                camera.DebugGeometry.AddArrow(point2, point2 + direction);
                camera.DebugGeometry.AddArrow(point3, point3 + direction);
            }

            if (EngineDebugSettings.DrawGameSpecificDebugGeometry)
            {
                Line line = GetClimbingLine();
                camera.DebugGeometry.Color = new ColorValue(0, 1, 0);
                camera.DebugGeometry.AddLine(line);
            }
        }

        public override void Editor_RenderSelectionBorder(Camera camera, bool simpleGeometry, DynamicMeshManager manager,
            DynamicMeshManager.MaterialData material)
        {
            DynamicMeshManager.Block block = manager.GetBlockFromCacheOrCreate("Ladder.Editor_RenderSelectionBorder: Box");
            block.AddBox(false, new Box(Vec3.Zero, Vec3.One, Mat3.Identity), null);

            Box box = GetBox();
            manager.AddBlockToScene(block, box.Center, box.Axis.ToQuat(), box.Extents, false, material);
        }
    }
}