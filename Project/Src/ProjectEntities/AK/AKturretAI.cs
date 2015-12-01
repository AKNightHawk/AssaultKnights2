// Copyright (C) 2006-2007 NeoAxis Group
using System;
using System.Collections.Generic;
using Engine;
using Engine.EntitySystem;
using Engine.MapSystem;
using Engine.MathEx;
using Engine.PhysicsSystem;
using Engine.Renderer;
using ProjectCommon;

namespace ProjectEntities
{
    /// <summary>
    /// Defines the <see cref="TankGameUnitAI"/> entity type.
    /// </summary>
    public class AKturretAIType : AIType
    {
    }

    public class AKturretAI : AI
    {
        //general task
        private GeneralTaskTypes generalTaskType;

        private float generalTaskUpdateTimer;

        //attack tasks
        private List<AttackTask> attackTasks = new List<AttackTask>();

        private float attackTasksUpdateTimer;

        private List<Weapon> unitWeapons = new List<Weapon>();

        ///////////////////////////////////////////

        public enum GeneralTaskTypes
        {
            None,
            Battle,
        }

        ///////////////////////////////////////////

        public class AttackTask
        {
            private Weapon weapon;
            private Vec3 targetPosition;
            private Dynamic targetEntity;
            private float taskTime;

            public AttackTask(Weapon weapon, Vec3 target)
            {
                this.taskTime = 0f;
                this.weapon = weapon;
                this.targetPosition = target;
                this.targetEntity = null;
            }

            public AttackTask(Weapon weapon, Dynamic target)
            {
                this.taskTime = 0f;
                this.weapon = weapon;
                this.targetPosition = new Vec3(float.NaN, float.NaN, float.NaN);
                this.targetEntity = target;
            }

            public float TaskTime
            {
                set { taskTime = value; }
                get { return taskTime; }
            }

            public Weapon Weapon
            {
                get { return weapon; }
            }

            public Vec3 TargetPosition
            {
                get { return targetPosition; }
            }

            public Dynamic TargetEntity
            {
                get { return targetEntity; }
            }
        }

        ///////////////////////////////////////////

        private AKturretAIType _type = null; public new AKturretAIType Type { get { return _type; } }

        public AKturretAI()
        {
            generalTaskUpdateTimer = World.Instance.Random.NextFloat() * 2;
            attackTasksUpdateTimer = World.Instance.Random.NextFloat() * 1;
        }

        private void AddTimer()
        {
            SubscribeToTickEvent();
        }

        /// <summary>Overridden from <see cref="Engine.EntitySystem.Entity.OnPostCreate(Boolean)"/>.</summary>
        protected override void OnPostCreate(bool loaded)
        {
            base.OnPostCreate(loaded);
            AddTimer();

            FindUnitWeapons();
        }

        protected override void OnTick()
        {
            base.OnTick();

            //tick general task
            generalTaskUpdateTimer -= TickDelta;
            if (generalTaskUpdateTimer <= 0)
            {
                UpdateGeneralTask();
                generalTaskUpdateTimer += 1;
            }

            //tick attack tasks
            attackTasksUpdateTimer -= TickDelta;
            if (attackTasksUpdateTimer <= 0)
            {
                TickAttackTasks();
                attackTasksUpdateTimer += .5f;
            }

            UpdateAttackTasksControlKeys();
        }

        protected override void OnControlledObjectRender(Camera camera)
        {
            base.OnControlledObjectRender(camera);

            if (camera != RendererWorld.Instance.DefaultCamera)
                return;

            //debug geometry
            if (EngineDebugSettings.DrawGameSpecificDebugGeometry)
            {
                //view radius
                if (ControlledObject.ViewRadius != 0)
                {
                    camera.DebugGeometry.Color = new ColorValue(0, 1, 0, .5f);
                    Vec3 lastPos = Vec3.Zero;
                    for (float angle = 0; angle <= MathFunctions.PI * 2 + .001f;
                        angle += MathFunctions.PI / 16)
                    {
                        Vec3 pos = ControlledObject.Position +
                            new Vec3(MathFunctions.Cos(angle), MathFunctions.Sin(angle), 0) *
                            ControlledObject.ViewRadius;

                        if (angle != 0)
                            camera.DebugGeometry.AddLine(lastPos, pos);

                        lastPos = pos;
                    }
                }

                //weapons
                foreach (Weapon weapon in unitWeapons)
                {
                    float radius = 0;

                    if (weapon.Type.WeaponNormalMode.IsInitialized)
                        radius = Math.Max(radius, weapon.Type.WeaponNormalMode.UseDistanceRange.Maximum);
                    if (weapon.Type.WeaponAlternativeMode.IsInitialized)
                        radius = Math.Max(radius, weapon.Type.WeaponAlternativeMode.UseDistanceRange.Maximum);

                    camera.DebugGeometry.Color = new ColorValue(1, 0, 0, .5f);
                    Vec3 lastPos = Vec3.Zero;
                    for (float angle = 0; angle <= MathFunctions.PI * 2 + .001f;
                        angle += MathFunctions.PI / 16)
                    {
                        Vec3 pos = weapon.Position +
                            new Vec3(MathFunctions.Cos(angle), MathFunctions.Sin(angle), 0) * radius;

                        if (angle != 0)
                            camera.DebugGeometry.AddLine(lastPos, pos);

                        lastPos = pos;
                    }
                }

                //attack tasks
                foreach (AttackTask attackTask in attackTasks)
                {
                    Vec3 targetPos = (attackTask.TargetEntity != null) ?
                        attackTask.TargetEntity.Position : attackTask.TargetPosition;

                    camera.DebugGeometry.Color = IsWeaponDirectedToTarget(attackTask) ?
                        new ColorValue(1, 1, 0) : new ColorValue(1, 0, 0);
                    camera.DebugGeometry.AddArrow(attackTask.Weapon.Position, targetPos, 1);
                    camera.DebugGeometry.AddSphere(new Sphere(targetPos, 3), 10);
                }
            }
        }

        private void DoGeneralTask(GeneralTaskTypes type, MapCurve way)
        {
            generalTaskType = type;
        }

        private void UpdateGeneralTask()
        {
            switch (generalTaskType)
            {
                case GeneralTaskTypes.Battle:
                    {
                        Dynamic enemy = FindEnemy(ControlledObject.ViewRadius);
                        if (enemy != null)
                        {
                            //Tank specific
                            AKturret Akunit = ControlledObject as AKturret;
                            if (Akunit != null)
                            {
                                Range range = Akunit.Type.OptimalAttackDistanceRange;
                                float distance = (enemy.Position - ControlledObject.Position).Length();

                                if (attackTasks.Count != 0)
                                {
                                    //to check up a line of fire
                                    foreach (AttackTask attackTask in attackTasks)
                                    {
                                        if (IsWeaponDirectedToTarget(attackTask))
                                        {
                                            break;
                                        }
                                    }
                                }
                            }
                        }
                        else
                        {
                            DoGeneralTask(GeneralTaskTypes.None, null);
                        }
                    }
                    break;
            }

            //find enemies
            {
                if (generalTaskType != GeneralTaskTypes.Battle)
                {
                    Dynamic enemy = FindEnemy(ControlledObject.ViewRadius);
                    if (enemy != null)
                        DoGeneralTask(GeneralTaskTypes.Battle, null);
                }
            }
        }

        private void TickAttackTasks()
        {
            foreach (Weapon weapon in unitWeapons)
            {
                float radius = 0;
                if (weapon.Type.WeaponNormalMode.IsInitialized)
                    radius = Math.Max(radius, weapon.Type.WeaponNormalMode.UseDistanceRange.Maximum);
                if (weapon.Type.WeaponAlternativeMode.IsInitialized)
                    radius = Math.Max(radius, weapon.Type.WeaponAlternativeMode.UseDistanceRange.Maximum);

                Dynamic enemy = FindEnemy(radius);

                AttackTask task = attackTasks.Find(delegate(AttackTask t)
                {
                    t.TaskTime += TickDelta;
                    return t.Weapon == weapon;
                });

                if (task != null)
                {
                    if (task.TargetEntity != enemy)
                    {
                        if (enemy != null)
                            DoAttackTask(weapon, enemy);
                        else
                            ResetAttackTask(task);
                    }
                }
                else
                {
                    if (enemy != null)
                        DoAttackTask(weapon, enemy);
                }
            }
        }

        private Vec3 CalculateTargetPosition(AttackTask attackTask)
        {
            Dynamic target = attackTask.TargetEntity;

            Vec3 targetPos = target != null ? target.Position : attackTask.TargetPosition;

            //to consider speed of the target
            if (target != null)
            {
                Gun gun = attackTask.Weapon as Gun;
                if (gun != null)
                {
                    BulletType bulletType = gun.Type.NormalMode.BulletType;
                    if (bulletType.Velocity != 0)
                    {
                        float flyTime = (targetPos - ControlledObject.Position).Length() /
                            bulletType.Velocity;

                        if (target.PhysicsModel != null && target.PhysicsModel.Bodies.Length != 0)
                        {
                            Body targetBody = target.PhysicsModel.Bodies[0];
                            targetPos += targetBody.LinearVelocity * flyTime;
                        }
                    }
                }
            }

            return targetPos;
        }

        private void UpdateAttackTasksControlKeys()
        {
            ControlKeyRelease(GameControlKeys.Fire1);
            ControlKeyRelease(GameControlKeys.Fire2);

            foreach (AttackTask attackTask in attackTasks)
            {
                //Tank specific
                AKunit Akunit = ControlledObject as AKunit;
                if (Akunit != null)
                {
                    if (attackTask.Weapon.Ready)
                    {
                        MissileLauncher msl = attackTask.Weapon as MissileLauncher;

                        if (msl != null)
                        {
                            MissileType mtype = msl.NormalMode.typeMode.BulletType as MissileType;
                            if (mtype == null || attackTask.TaskTime < mtype.LockingTime)
                            {
                                continue;
                            }
                            else
                            {
                                Akunit.CurrentMissileTarget = attackTask.TargetEntity as Unit;
                            }
                        }

                        Akunit.MainGun = (Gun)attackTask.Weapon;

                        Vec3 targetPos = CalculateTargetPosition(attackTask);

                        //turn turret
                        Akunit.SetNeedTurnToPosition(targetPos);

                        //fire
                        if (IsWeaponDirectedToTarget(attackTask))
                            ControlKeyPress(GameControlKeys.Fire1, 1);
                    }
                }
            }
        }

        private bool IsWeaponDirectedToTarget(AttackTask attackTask)
        {
            Vec3 targetPos = CalculateTargetPosition(attackTask);

            Weapon weapon = attackTask.Weapon;

            //to check up a weapon angle
            {
                Vec3 needDirection = (targetPos - weapon.Position).GetNormalize();
                Vec3 weaponDirection = weapon.Rotation.GetForward();

                Radian angle = Math.Abs(MathFunctions.ACos(Vec3.Dot(needDirection, weaponDirection)));
                Radian minimalDifferenceAngle = new Degree(2).InRadians();

                if (angle > minimalDifferenceAngle)
                    return false;
            }

            //to check up a line of fire
            {
                Ray ray = new Ray(weapon.Position, targetPos - weapon.Position);

                RayCastResult[] piercingResult = PhysicsWorld.Instance.RayCastPiercing(
                    ray, (int)ContactGroup.CastOnlyContact);

                foreach (RayCastResult result in piercingResult)
                {
                    Dynamic dynamic = MapSystemWorld.GetMapObjectByBody(result.Shape.Body) as Dynamic;
                    if (dynamic != null)
                    {
                        Unit parentUnit = dynamic.GetParentUnit();
                        if (parentUnit != null)
                        {
                            if (parentUnit == attackTask.TargetEntity)
                                continue;
                            if (parentUnit == ControlledObject)
                                continue;
                        }
                    }

                    return false;
                }
            }

            return true;
        }

        private float GetAttackObjectPriority(Unit obj)
        {
            if (ControlledObject == obj)
                return 0;
            if (obj.Intellect == null)
                return 0;
            if (obj.Intellect.Faction == null)
                return 0;
            if (Faction == obj.Intellect.Faction)
                return 0;

            Vec3 distance = obj.Position - ControlledObject.Position;
            float len = distance.Length();
            return 1.0f / len + 1.0f;
        }

        private Dynamic FindEnemy(float radius)
        {
            if (Faction == null)
                return null;

            Unit controlledObject = ControlledObject;

            Unit enemy = null;
            float enemyPriority = 0;

            Map.Instance.GetObjects(new Sphere(controlledObject.Position, radius),
                MapObjectSceneGraphGroups.UnitGroupMask, delegate(MapObject mapObject)
                {
                    Unit obj = (Unit)mapObject;

                    //check by distance
                    Vec3 diff = obj.Position - controlledObject.Position;
                    float objDistance = diff.Length();
                    if (objDistance > radius)
                        return;

                    float priority = GetAttackObjectPriority(obj);
                    if (priority != 0 && priority > enemyPriority)
                    {
                        enemy = obj;
                        enemyPriority = priority;
                    }
                });

            return enemy;
        }

        /// <summary>Overridden from <see cref="Engine.EntitySystem.Entity.OnDeleteSubscribedToDeletionEvent(Entity)"/></summary>
        protected override void OnDeleteSubscribedToDeletionEvent(Entity entity)
        {
            base.OnDeleteSubscribedToDeletionEvent(entity);

            //reset related task
        again:
            foreach (AttackTask task in attackTasks)
            {
                if (task.Weapon == entity || task.TargetEntity == entity)
                {
                    ResetAttackTask(task);
                    goto again;
                }
            }

            //remove deleted weapon
            Weapon weapon = entity as Weapon;
            if (weapon != null)
                unitWeapons.Remove(weapon);
        }

        private void DoAttackTask(Weapon weapon, Vec3 target)
        {
            AttackTask task = attackTasks.Find(delegate(AttackTask t)
            {
                return t.Weapon == weapon;
            });

            if (task != null && task.TargetPosition == target)
                return;

            ResetAttackTask(task);

            task = new AttackTask(weapon, target);
            attackTasks.Add(task);
        }

        private void AddRelationship(Entity ent)
        {
            SubscribeToDeletionEvent(ent);
        }

        private void RemoveRelationship(Entity ent)
        {
            UnsubscribeToDeletionEvent(ent);
        }

        private void DoAttackTask(Weapon weapon, Dynamic target)
        {
            AttackTask task = attackTasks.Find(delegate(AttackTask t)
            {
                return t.Weapon == weapon;
            });

            if (task != null && task.TargetEntity == target)
                return;

            if (task != null)
                ResetAttackTask(task);

            task = new AttackTask(weapon, target);
            AddRelationship(target);
            attackTasks.Add(task);
        }

        private void ResetAttackTask(AttackTask task)
        {
            if (task.TargetEntity != null)
                SubscribeToDeletionEvent(task.TargetEntity);
            attackTasks.Remove(task);
        }

        private void ResetAllAttackTasks()
        {
            while (attackTasks.Count != 0)
                ResetAttackTask(attackTasks[attackTasks.Count - 1]);
        }

        private void FindUnitWeapons()
        {
            foreach (MapObjectAttachedObject attachedObject in ControlledObject.AttachedObjects)
            {
                MapObjectAttachedMapObject attachedMapObject =
                    attachedObject as MapObjectAttachedMapObject;
                if (attachedMapObject != null)
                {
                    Weapon weapon = attachedMapObject.MapObject as Weapon;
                    if (weapon != null)
                        unitWeapons.Add(weapon);
                }
            }

            foreach (Weapon weapon in unitWeapons)
                AddRelationship(weapon);
        }

        public override bool IsActive()
        {
            return generalTaskType != GeneralTaskTypes.None;
        }

        protected override void OnControlledObjectChange(Unit oldObject)
        {
            base.OnControlledObjectChange(oldObject);

            if (oldObject != null)
                oldObject.Damage -= ControlledObject_Damage;
            if (ControlledObject != null)
                ControlledObject.Damage += ControlledObject_Damage;
        }

        private void ControlledObject_Damage(Dynamic entity, MapObject prejudicial, Vec3 pos, float damage)
        {
            if (generalTaskType != GeneralTaskTypes.Battle && prejudicial != null)
            {
                Unit sourceUnit = null;

                Bullet bullet = prejudicial as Bullet;
                if (bullet != null)
                    sourceUnit = bullet.SourceUnit;
                Explosion explosion = prejudicial as Explosion;
                if (explosion != null)
                    sourceUnit = explosion.SourceUnit;

                if (sourceUnit != null)
                {
                    Intellect unitIntellect = sourceUnit.Intellect as Intellect;
                    if (unitIntellect != null && unitIntellect.Faction != Faction)
                    {
                        //do battle task
                        DoGeneralTask(GeneralTaskTypes.Battle, null);
                    }
                }
            }
        }

        public void OnNotifyFromAllyOnEnemy(Vec3 enemyPos)
        {
            if (generalTaskType != GeneralTaskTypes.Battle)
            {
                //do battle task
                DoGeneralTask(GeneralTaskTypes.Battle, null);
            }
        }
    }
}