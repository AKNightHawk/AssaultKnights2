using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel;
using System.Reflection;
using System.Drawing.Design;
using Engine;
using Engine.EntitySystem;
using Engine.MapSystem;
using Engine.MathEx;
using Engine.PhysicsSystem;
using Engine.Renderer;
using Engine.Utils;

namespace GameEntities
{
    public class MissileLauncherType : GunType
    {

    }

    public class MissileLauncher : Gun
    {

        MissileLauncherType _type = null; public new MissileLauncherType Type { get { return _type; } }

        protected override void CreateBullet(Mode mode)
        {
            //only missiles for missilelauncher
            if (mode.typeMode.BulletType as MissileType == null) return;

            Missile obj = (Missile)Entities.Instance.Create(mode.typeMode.BulletType, Parent);
            obj.SourceUnit = GetParentUnitHavingIntellect();
            obj.Position = GetFirePosition(mode.typeMode);

            //Correcting position at a shot in very near object (when the point of a shot inside object).
            {
                Vec3 startPos = Position;
                if (AttachedMapObjectParent != null)
                    startPos = AttachedMapObjectParent.Position;

                Ray ray = new Ray(startPos, obj.Position - startPos);
                if (ray.Direction != Vec3.Zero)
                {
                    RayCastResult[] piercingResult = PhysicsWorld.Instance.RayCastPiercing(
                        ray, (int)ContactGroup.CastOnlyContact);

                    foreach (RayCastResult result in piercingResult)
                    {
                        MapObject mapObject = MapSystemWorld.GetMapObjectByBody(result.Shape.Body);

                        if (mapObject != null)
                        {
                            if (mapObject == this)
                                continue;
                            if (mapObject == this.AttachedMapObjectParent)
                                continue;
                        }

                        obj.Position = result.Position - ray.Direction * .01f;
                        break;
                    }
                }
            }

            Quat rot = GetFireRotation(mode.typeMode);
            Radian dispersionAngle = mode.typeMode.DispersionAngle;
            if (dispersionAngle != 0)
            {
                EngineRandom random = World.Instance.Random;

                float halfDir;

                halfDir = random.NextFloatCenter() * dispersionAngle * .5f;
                rot *= new Quat(new Vec3(0, 0, MathFunctions.Sin16(halfDir)),
                    MathFunctions.Cos16(halfDir));
                halfDir = random.NextFloatCenter() * dispersionAngle * .5f;
                rot *= new Quat(new Vec3(0, MathFunctions.Sin16(halfDir), 0),
                    MathFunctions.Cos16(halfDir));
                halfDir = random.NextFloatCenter() * dispersionAngle * .5f;
                rot *= new Quat(new Vec3(MathFunctions.Sin16(halfDir), 0, 0),
                    MathFunctions.Cos16(halfDir));
            }
            obj.Rotation = rot;

            obj.PostCreate();

            //set damage coefficient
            float coef = obj.DamageCoefficient;
            Unit unit = GetParentUnitHavingIntellect();
            if (unit != null && unit.BigDamageInfluence != null)
                coef *= unit.BigDamageInfluence.Type.Coefficient;
            obj.DamageCoefficient = coef;

            foreach (MapObjectAttachedObject attachedObject in AttachedObjects)
            {
                MapObjectAttachedMesh attachedMesh = attachedObject as MapObjectAttachedMesh;
                if (attachedMesh == null)
                    continue;

                if (attachedMesh.Alias.Equals("Missile"))
                {
                    if (!attachedMesh.Visible)
                        continue;
                    else
                    {
                        attachedMesh.Visible = false;
                        break;
                    }
                }
            }
        }

        public override bool TryFire(bool alternative)
        {

            return base.TryFire(alternative);
        }
    }
}