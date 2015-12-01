// Copyright Assault Knights
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using Engine;
using Engine.EntitySystem;
using Engine.MapSystem;
using Engine.MathEx;
using Engine.PhysicsSystem;
using Engine.Renderer;
using ProjectCommon;

namespace ProjectEntities
{
    public class MechUnitAIType : AIType
    {
        [FieldSerialize]
        private float notifyAlliesRadius = 50;

        [DefaultValue(50.0f)]
        public float NotifyAlliesRadius
        {
            get { return notifyAlliesRadius; }
            set { notifyAlliesRadius = value; }
        }

        private static float heat = 300;

        [FieldSerialize]
        private float heatlevel = heat;

        [DefaultValue(300.0f)]
        public float Ai_HeatLevelWarning
        {
            get { return heatlevel; }
            set { heatlevel = value; }
        }
    }

    public class MechUnitAI : AI
    {
        //initial data
        private Region activationRegion;

        //general task
        private GeneralTaskTypes generalTaskType;

        private MapCurve generalTaskWay;
        private MapCurvePoint generalTaskCurrentWayPoint;
        private float generalTaskUpdateTimer;

        //
        private PathController pathController = new PathController();

        //move task
        private bool moveTaskEnabled;

        private Vec3 moveTaskPosition;

        //attack tasks
        private List<AttackTask> attackTasks = new List<AttackTask>();

        private float attackTasksUpdateTimer;

        private List<Weapon> unitWeapons = new List<Weapon>();

        public enum NavigationSystemType //Incin
        {
            None,
            RecastNavigation, //Type
            MapCurves, //Type
            MapCameras, //Type
            Total, //Total Supported Navigation types
        }

        ///////////////////////////////////////////

        public enum GeneralTaskTypes
        {
            None,               //mapload?
            EnemyFlag,          //Incin -- protect or carry? //still targets enemies where movetask is to capture flag
            TeamFlag,           //Incin - protect or carry? //runs from target enemies where MoveTask is back to Home base
            Capped,             //Incin -- Team score or enemy die // targets enemies
            PrimaryTargets,     //Incin -- enemy spawn locations or other objectives/goals(objects in maps) /// special commands
            SecondaryTargets,   //incin -- player spawn locations, secondary targets(objects in maps) //special commands
            Patrol,             //Incin -- Protect a location(object or flags) //special commands
            WayMove,            //Previous Code -- by MapCurve (taken from TankGameExtendedProperties.Way)
            Battle,             //Attack -- 4 methods, use custom use waypoints, use map curves, use recast..recast not implemented
            Leave,              //Incin -- leave scene of battle.. or move away from target
            Suicide,            //Incin -- Die //force commands -- death damage effects?
        }

        private static bool skillrandom = true;

        [FieldSerialize]
        private bool randomizeskill = skillrandom;

        [DefaultValue(true)]
        public bool RandomizeSkill
        {
            get { return randomizeskill; }
            set { randomizeskill = value; }
        }

        ///iNCIN -- Random skill levelling for AI
        ///should identify good skill ranges per level
        public enum MechSkillLevel
        {
            NEWB = 1, //basic skill level and speed
            BETTER = 2,
            NORMAL = 3,
            HARD = 4,
            HOLYCRAP = 5
        }

        private static MechSkillLevel lvl = MechSkillLevel.NORMAL;

        ///iNCIN -- Defaulet skill levelNormal
        [FieldSerialize]
        [DefaultValue(MechSkillLevel.NORMAL)]
        private MechSkillLevel skilllevel = lvl;

        //iNCIN
        [DefaultValue(MechSkillLevel.NORMAL)]
        public MechSkillLevel SkillLevel
        {
            get { return skilllevel; }
            set { skilllevel = value; }
        }

        ///////////////////////////////////////////

        public class AttackTask
        {
            private Weapon weapon;
            private Vec3 targetPosition;    //targetEntity position
            private Dynamic targetEntity;   //identifies current target
            private float taskTime;         //time a task takes

            private MechUnitAI owner;

            public MechUnitAI Owner
            {
                get { return owner; }
            }

            public AttackTask(MechUnitAI owner, Weapon weapon, Vec3 target)
            {
                this.owner = owner;
                this.taskTime = 0f;
                this.weapon = weapon;
                this.targetPosition = target;
                this.targetEntity = null;
            }

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
                if (target != null)
                {
                    this.targetPosition = target.Position;
                }
                else
                {
                    this.targetPosition = new Vec3(float.NaN, float.NaN, float.NaN);
                }

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

        private MechUnitAIType _type = null; public new MechUnitAIType Type { get { return _type; } }

        private MechSkillLevel GetSkillLevel()
        {
            return this.SkillLevel;
        }

        public MechUnitAI()
        {
            MechSkillLevel lvl = GetSkillLevel();

            switch (lvl)
            {
                case MechSkillLevel.NEWB:
                    {
                        generalTaskUpdateTimer = 3;
                        attackTasksUpdateTimer = 7;
                        break;
                    }
                case MechSkillLevel.BETTER:
                    {
                        generalTaskUpdateTimer = 3;
                        attackTasksUpdateTimer = 5;
                        break;
                    }
                case MechSkillLevel.NORMAL:
                    {
                        generalTaskUpdateTimer = World.Instance.Random.NextFloat() * 3;
                        attackTasksUpdateTimer = 3;
                        break;
                    }
                case MechSkillLevel.HARD:
                    {
                        generalTaskUpdateTimer = World.Instance.Random.NextFloat() * 3;
                        attackTasksUpdateTimer = World.Instance.Random.NextFloat() * 5;
                        break;
                    }
                case MechSkillLevel.HOLYCRAP:
                    {
                        generalTaskUpdateTimer = World.Instance.Random.NextFloat() * 2;
                        attackTasksUpdateTimer = World.Instance.Random.NextFloat() * 5;
                        break;
                    }
            }

            Unit_Ai_Skill_Label();
        }

        //return label for Mech skill level, us
        private String Unit_Ai_Skill_Label()
        {
            string SkillLevel = "<<" + this.Name.ToString() + ">><<" + (string)GetSkillLevel().ToString() + ">>";

            if (EngineConsole.Instance != null)
                EngineConsole.Instance.Print("Skill level: " + SkillLevel);

            return SkillLevel;
        }

        /// <summary>Overridden from <see cref="Engine.EntitySystem.Entity.OnPostCreate(Boolean)"/>.</summary>
        protected override void OnPostCreate(bool loaded)
        {
            base.OnPostCreate(loaded);
            SubscribeToTickEvent();//AddTimer();

            //get activationRegion
            //TankGameExtendedProperties extendedProperties =
            //    ControlledObject.ExtendedProperties as TankGameExtendedProperties;
            //get activationRegion
            EntityComponent_ForTankDemo component = (EntityComponent_ForTankDemo)
                ControlledObject.Component_GetFirstWithType(typeof(EntityComponent_ForTankDemo));
            //if (extendedProperties != null)
            //    activationRegion = extendedProperties.ActivationRegion;
            if (component != null)
                activationRegion = component.ActivationRegion;

            //listen activationRegion
            if (activationRegion != null)
                activationRegion.ObjectIn += ActivationRegion_ObjectIn;

            if (this.RandomizeSkill == true)
                this.SkillLevel = (MechSkillLevel)(World.Instance.Random.NextFloat() * 5);

            //Unit_Ai_Skill_Label();

            FindUnitWeapons();
        }

        protected override void OnDestroy()
        {
            //stop listen activationRegion
            if (activationRegion != null)
            {
                activationRegion.ObjectIn -= ActivationRegion_ObjectIn;
                activationRegion = null;
            }
            base.OnDestroy();
        }

        /*
        private bool IsAllowUpdateControlledObject()
        {
            //bad for system with disabled renderer, because here game logic depends animation.
            AnimationTree tree = this.Owner.ControlledObject.GetFirstAnimationTree();
            if (tree != null && tree.GetActiveTriggers().Count != 0)
                return false;
            return true;
        }

        private bool CheckDirectVisibilityByRayCast(Vec3 from, Vec3 targetPosition, MapObject targetObject)
        {
            MechUnitAI controlledObj = this.Owner.ControlledObject;

            Ray ray = new Ray(from, targetPosition - from);

            RayCastResult[] piercingResult = PhysicsWorld.Instance.RayCastPiercing(
                ray, (int)ContactGroup.CastOnlyContact);
            foreach (RayCastResult result in piercingResult)
            {
                MapObject obj = MapSystemWorld.GetMapObjectByBody(result.Shape.Body);
                if (obj != null)
                {
                    //skip target object
                    if (targetObject != null && obj == targetObject)
                        continue;
                    //skip controlled object
                    if (obj == controlledObj)
                        continue;
                }

                //found body which breaks visibility
                return false;
            }

            return true;
        }
        */

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

            /*
            MechUnitAI controlledObj = Owner.ControlledObject;

            if (IsAllowUpdateControlledObject() && controlledObj.IsOnGround())
            {
                float targetDistance = (GetTargetPosition() - controlledObj.Position).Length();

                //movement
                {
                    Vec3 rayCastFrom;
                    if (Owner.weapons.Length != 0)
                        rayCastFrom = Owner.weapons[0].Position;
                    else
                        rayCastFrom = controlledObj.Position;

                    Range optimalAttackDistanceRange = controlledObj.Type.OptimalAttackDistanceRange;
                    if (targetDistance < optimalAttackDistanceRange.Maximum &&
                        CheckDirectVisibilityByRayCast(rayCastFrom, GetTargetPosition(), TaskEntity))
                    {
                        //destination target is visible
                        controlledObj.SetTurnToPosition(GetTargetPosition());

                        if (targetDistance < optimalAttackDistanceRange.Minimum)
                        {
                            //move backward
                            Vec2 direction = GetTargetPosition().ToVec2() - controlledObj.Position.ToVec2();
                            if (direction != Vec2.Zero)
                                direction = -direction.GetNormalize();
                            controlledObj.SetForceMoveVector(direction);
                        }
                        else
                        {
                            //sees the target and stay on optimal attack distance
                            controlledObj.SetForceMoveVector(Vec2.Zero);
                        }
                    }
                    else
                    {
                        //need move

                        //update path controller
                        Owner.pathController.Update(Entity.TickDelta, controlledObj.Position,
                            GetTargetPosition(), false);

                        //update character
                        Vec3 nextPointPosition;
                        if (Owner.pathController.GetNextPointPosition(out nextPointPosition))
                        {
                            Vec2 vector = nextPointPosition.ToVec2() - controlledObj.Position.ToVec2();
                            if (vector != Vec2.Zero)
                                vector.Normalize();

                            controlledObj.SetTurnToPosition(nextPointPosition);
                            controlledObj.SetForceMoveVector(vector);
                        }
                        else
                        {
                            controlledObj.SetForceMoveVector(Vec2.Zero);
                        }
                    }
                }

                //shot
                foreach (Weapon weapon in Owner.weapons)
                {
                    if (weapon.Ready)
                    {
                        Range normalRange = weapon.Type.WeaponNormalMode.UseDistanceRange;
                        bool normalInRange = targetDistance >= normalRange.Minimum &&
                            targetDistance <= normalRange.Maximum;

                        Range alternativeRange = weapon.Type.WeaponAlternativeMode.UseDistanceRange;
                        bool alternativeInRange = targetDistance >= alternativeRange.Minimum &&
                            targetDistance <= alternativeRange.Maximum;

                        if ((normalInRange || alternativeInRange) &&
                            CheckDirectVisibilityByRayCast(weapon.Position, GetTargetPosition(), TaskEntity))
                        {
                            //update weapon fire orientation
                            {
                                Vec3 pos = GetTargetPosition();
                                Gun gun = weapon as Gun;
                                if (gun != null)
                                    gun.GetAdvanceAttackTargetPosition(false, TaskEntity, false, out pos);
                                weapon.SetForceFireRotationLookTo(pos);
                            }

                            controlledObj.SetTurnToPosition(GetTargetPosition());
                            if (normalInRange)
                                weapon.TryFire(false);
                            if (alternativeInRange)
                                weapon.TryFire(true);
                        }
                    }
                }
            }
            else
            {
                controlledObj.SetForceMoveVector(Vec2.Zero);
            }
            */

            UpdateMoveTaskControlKeys();
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
                //way
                if (generalTaskCurrentWayPoint != null)
                {
                    ReadOnlyCollection<MapCurvePoint> points = generalTaskWay.Points;

                    camera.DebugGeometry.Color = new ColorValue(0, 1, 0, .5f);
                    int index = points.IndexOf(generalTaskCurrentWayPoint);
                    for (; index < points.Count - 1; index++)
                    {
                        camera.DebugGeometry.AddArrow(
                            points[index].Position, points[index + 1].Position, 1);
                    }
                }

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
                    if (weapon != null) //Incin weapon shot off?
                        continue;

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

                //move task
                if (moveTaskEnabled)
                {
                    camera.DebugGeometry.Color = new ColorValue(0, 1, 0);
                    camera.DebugGeometry.AddArrow(ControlledObject.Position, moveTaskPosition, 1);
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

        private void ActivationRegion_ObjectIn(Entity entity, MapObject obj)
        {
            if (activationRegion == null)
                return;

            bool isPlayer = false;

            Unit unit = obj as Unit;
            if (unit != null && unit.Intellect as PlayerIntellect != null)
                isPlayer = true;

            if (isPlayer)
                ActivateRegion();
        }

        private void ActivateRegion()
        {
            //stop listen activationRegion
            if (activationRegion != null)
            {
                activationRegion.ObjectIn -= ActivationRegion_ObjectIn;
                activationRegion = null;
            }

            ActivateWayMovement();
        }

        public void ActivateWayMovement()
        {
            //MapCurve way = null;
            //{
            //    TankGameExtendedProperties extendedProperties =
            //        ControlledObject.ExtendedProperties as TankGameExtendedProperties;
            //    if (extendedProperties != null)
            //        way = extendedProperties.Way;
            //}

            //if (way != null)
            //    DoGeneralTask(GeneralTaskTypes.WayMove, way);

            MapCurve way = null;
            {
                EntityComponent_ForTankDemo component = (EntityComponent_ForTankDemo)
                    ControlledObject.Component_GetFirstWithType(typeof(EntityComponent_ForTankDemo));
                if (component != null)
                    way = component.Way;
            }

            if (way != null)
                DoGeneralTask(GeneralTaskTypes.WayMove, way);
        }

        private void DoGeneralTask(GeneralTaskTypes type, MapCurve way)
        {
            generalTaskType = type;
            generalTaskWay = way;
            generalTaskCurrentWayPoint = null;

            switch (generalTaskType)
            {
                case GeneralTaskTypes.Battle:
                case GeneralTaskTypes.EnemyFlag:
                case GeneralTaskTypes.Leave:
                case GeneralTaskTypes.None:
                case GeneralTaskTypes.Patrol:
                case GeneralTaskTypes.PrimaryTargets:
                case GeneralTaskTypes.SecondaryTargets:
                case GeneralTaskTypes.TeamFlag:
                case GeneralTaskTypes.WayMove:
                    {
                        if (generalTaskWay != null)
                            generalTaskCurrentWayPoint = generalTaskWay;
                        break;
                    }
            }
            if (generalTaskType == GeneralTaskTypes.None)
                ResetMoveTask();
        }

        private void UpdateGeneralTask()
        {
            switch (generalTaskType)
            {
                case GeneralTaskTypes.WayMove:
                    {
                        if (generalTaskCurrentWayPoint == null)
                            return;
                        const float wayPointCheckDistance = 100;

                        float wayPointDistance = (ControlledObject.Position -
                            generalTaskCurrentWayPoint.Position).Length();

                        if (wayPointDistance < wayPointCheckDistance)
                        {
                            //next way point or stop

                            int index = generalTaskWay.Points.IndexOf(generalTaskCurrentWayPoint);
                            index++;

                            if (index < generalTaskWay.Points.Count)
                            {
                                //next way point
                                generalTaskCurrentWayPoint = generalTaskWay.Points[index];
                            }
                            else
                            {
                                //task completed
                                DoGeneralTask(GeneralTaskTypes.None, null);
                            }
                        }

                        if (generalTaskCurrentWayPoint != null)
                            DoMoveTask(generalTaskCurrentWayPoint.Position);
                    }
                    break;

                case GeneralTaskTypes.Battle:
                    {
                        Dynamic enemy = FindEnemy(ControlledObject.ViewRadius);
                        if (enemy != null)
                        {
                            //notify allies
                            NotifyAlliesOnEnemy(enemy.Position);

                            //Tank specific
                            Mech tank = ControlledObject as Mech;
                            if (tank != null)
                            {
                                Range range = tank.Type.OptimalAttackDistanceRange;
                                float distance = (enemy.Position - ControlledObject.Position).Length();

                                bool needMove = true;

                                if (distance > range.Maximum / 2f)
                                    needMove = true;
                                {
                                    if (!needMove && attackTasks.Count != 0)
                                    {
                                        //to check up a line of fire
                                        bool existsDirectedWeapons = false;
                                        foreach (AttackTask attackTask in attackTasks)
                                        {
                                            if (IsWeaponDirectedToTarget(attackTask))
                                            {
                                                existsDirectedWeapons = true;
                                                break;
                                            }
                                        }
                                        if (!existsDirectedWeapons)
                                            needMove = true;
                                    }
                                    if (generalTaskCurrentWayPoint != null)
                                        DoMoveTask(generalTaskCurrentWayPoint.Position);
                                    else if (needMove)
                                        DoMoveTask(enemy.Position);
                                    else
                                        ResetMoveTask();
                                }
                            }
                        }
                        else
                        {
                            if (moveTaskEnabled)
                            {
                                const float needDistance = 10;
                                float distance = (moveTaskPosition - ControlledObject.Position).Length();
                                if (distance < needDistance)
                                    ResetMoveTask();
                            }

                            if (!moveTaskEnabled)
                                DoGeneralTask(GeneralTaskTypes.None, null);
                        }
                    }
                    break;

                case GeneralTaskTypes.Leave:
                    {
                        Dynamic enemy = FindEnemy(ControlledObject.ViewRadius);
                        if (enemy != null)
                        {
                            //notify allies
                            //NotifyAlliesOnEnemy(enemy.Position);

                            //Tank specific
                            Mech tank = ControlledObject as Mech;
                            if (tank != null)
                            {
                                Range range = tank.Type.OptimalAttackDistanceRange;
                                float distance = (enemy.Position - ControlledObject.Position).Length();
                                distance -= tank.Type.OptimalAttackDistanceRange.Minimum;

                                if (tank.Health < 200 || tank.MechShutDown == true || tank.GetHeatLevel() >= tank.Type.AKunitHeatMax - Type.Ai_HeatLevelWarning)
                                {
                                    bool needMove = false;

                                    if (distance <= range.Minimum)
                                        needMove = true;

                                    //if (attackTasks.Count >= 0)
                                    //{
                                    //    //to check up a line of fire
                                    //    bool existsDirectedWeapons = false;
                                    //    foreach (AttackTask attackTask in attackTasks)
                                    //    {
                                    //        if (IsWeaponDirectedToTarget(attackTask))
                                    //        {
                                    //            existsDirectedWeapons = true;
                                    //            break;
                                    //        }
                                    //    }
                                    //    if (!existsDirectedWeapons)
                                    //        needMove = true;
                                    //}

                                    if (needMove)
                                    {
                                        Vec3 pos = moveTaskPosition + ControlledObject.Position;
                                        DoMoveTask(pos);
                                    }
                                    else
                                        ResetMoveTask();
                                }
                            }
                        }
                        else
                        {
                            if (moveTaskEnabled)
                            {
                                const float needDistance = 20;
                                float distance = (moveTaskPosition + ControlledObject.Position).Length();
                                if (distance < needDistance)
                                    ResetMoveTask();
                            }

                            if (!moveTaskEnabled)
                                DoGeneralTask(GeneralTaskTypes.Leave, null);
                        }
                    }
                    break;

                case GeneralTaskTypes.Patrol:
                    {
                        //follow a map curve here
                        Dynamic enemy = FindEnemy(ControlledObject.ViewRadius);
                        if (enemy != null)
                            DoGeneralTask(GeneralTaskTypes.Battle, null);
                        else
                            DoGeneralTask(GeneralTaskTypes.Patrol, null);

                        break;
                    }
                case GeneralTaskTypes.PrimaryTargets:
                    {
                        DoGeneralTask(GeneralTaskTypes.Battle, null);
                        break;
                    }
                case GeneralTaskTypes.SecondaryTargets:
                    {
                        DoGeneralTask(GeneralTaskTypes.Battle, null);
                        break;
                    }
            }

            //find enemies
            {
                if (generalTaskType != GeneralTaskTypes.Battle)
                {
                    Dynamic enemy = FindEnemy(ControlledObject.ViewRadius);
                    if (enemy != null)
                        DoGeneralTask(GeneralTaskTypes.Battle, null);
                    else
                        DoGeneralTask(GeneralTaskTypes.WayMove, null);
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
                //if (weapon.Type.WeaponAlternativeMode.IsInitialized)
                //	radius = Math.Max(radius, weapon.Type.WeaponAlternativeMode.UseDistanceRange.Maximum);

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
            const float globalBetweenFireTime = 15f; // can be moved to a .type for setting per IA
            float _nextFireAllowedTime; //inside class
            var currentTime = EngineApp.Instance.Time;

            ControlKeyRelease(GameControlKeys.Fire1);
            ControlKeyRelease(GameControlKeys.Fire2);

            ControlKeyRelease(GameControlKeys.NextWeapon);
            ControlKeyRelease(GameControlKeys.PreviousWeapon);

            //ControlKeyRelease(GameControlKeys.Mode);

            foreach (AttackTask attackTask in attackTasks)
            {
                //Tank specific
                Mech tank = ControlledObject as Mech;
                if (tank != null)
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
                                tank.CurrentMissileTarget = attackTask.TargetEntity as Unit;
                            }
                        }

                        tank.MainGun = (Gun)attackTask.Weapon;

                        Vec3 targetPos = CalculateTargetPosition(attackTask);

                        //turn turret
                        tank.SetNeedTurnToPosition(targetPos);

                        int test = (int)(World.Instance.Random.NextFloat() * 3f);
                        if (test == 1)
                        {
                            ControlKeyPress(GameControlKeys.NextWeapon, 1);
                        }
                        else if (test == 2)
                        {
                            ControlKeyPress(GameControlKeys.PreviousWeapon, 1);
                        }
                        else
                        {
                            test = (int)(World.Instance.Random.NextFloat() * 3f);
                            if (test == 1)
                            {
                                ControlKeyPress(GameControlKeys.Weapon1, 1);
                                ControlKeyRelease(GameControlKeys.Weapon1);
                            }
                            else if (test == 2)
                            {
                                ControlKeyPress(GameControlKeys.Weapon2, 1);
                                ControlKeyRelease(GameControlKeys.Weapon2);
                            }
                            else if (test == 3)
                            {
                                ControlKeyPress(GameControlKeys.Weapon3, 1);
                                ControlKeyRelease(GameControlKeys.Weapon3);
                            }
                            else
                            {
                                ;
                            }
                        }
                        {
                            {
                                if (tank.AKunitHeat < 500)
                                {
                                    ControlKeyPress(GameControlKeys.Fire1, 1);
                                    _nextFireAllowedTime = currentTime + globalBetweenFireTime;
                                    //// Incin we need some kind of timer here to make the
                                    //Mechs wait 5 seconds before fireing. Or even 2 would be good. Something that will make them fire slowly.
                                }
                            }
                        }
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

        private void DoMoveTask(Vec3 pos)
        {
            moveTaskEnabled = true;
            moveTaskPosition = pos;
        }

        private void ResetMoveTask()
        {
            moveTaskEnabled = false;
        }

        ////        private void UpdateMoveTaskControlKeys()
        ////        {
        ////            //!!!!!!slowly?

        ////            bool forward = false;
        ////            bool backward = false;
        ////            bool left = false;
        ////            bool right = false;

        ////            if (moveTaskEnabled)
        ////            {
        ////                //Vehicle specific

        ////                //int avoidtimer = 1000;
        ////                Vec3 tempvoid = new Vec3(0,0,0);
        ////                Vec3 unitPos = ControlledObject.Position;
        ////                Vec3 unitDir = ControlledObject.Rotation.GetForward();
        ////                Vec3 needDir = moveTaskPosition - unitPos;
        ////                Vec3 Objectinway = new Vec3(0,0,0);

        ////                MapObject obj = null;
        ////                float dist = 1000f;
        ////                //tempvoid = getFrontObjectSize();
        ////                GetFrontObjectDistance(out obj, out dist);
        ////                if (obj != null)
        ////                {
        ////                    Objectinway = obj.GetInterpolatedPosition();
        ////                    Box box = obj.GetBox();
        ////                    //if(box.Expand(30)
        ////                }

        ////                //if(tempvoid != Vec3.Zero)
        ////                //    needDir += tempvoid;

        ////                Radian unitAngle = MathFunctions.ATan(unitDir.Y, unitDir.X);
        ////                Radian needAngle = MathFunctions.ATan(needDir.Y, needDir.X);

        ////                Radian diffAngle = needAngle - unitAngle;
        ////                while (diffAngle < -MathFunctions.PI)
        ////                    diffAngle += MathFunctions.PI * 2;
        ////                while (diffAngle > MathFunctions.PI)
        ////                    diffAngle -= MathFunctions.PI * 2;

        ////                //!!!!!!! 10.0f
        ////                if ((Math.Abs(diffAngle) > new Degree(10.0f).InRadians()) || (dist < 20f && obj != null))
        ////                {
        ////                    if (dist < 20f && obj != null && diffAngle > 0)
        ////                        left = true;
        ////                    else if (dist < 20f && obj != null && diffAngle < 0)
        ////                        right = true;
        ////                    else if (diffAngle > 0)
        ////                        left = true;
        ////                    else
        ////                        right = true;
        ////                }

        ////                if ((diffAngle > -MathFunctions.PI / 2 && diffAngle < MathFunctions.PI / 2) || (dist < 20f && obj != null))
        ////                {
        ////                    if (needDir.LengthFast() < 20.0f || dist < 5f) //we are too close!!
        ////                    {
        ////                        backward = true;
        ////                    }
        ////                    else forward = true;
        ////                }
        ////            }
        /////*
        ////                if (dist == 0)
        ////                {
        ////                    if (Math.Abs(diffAngle) >= new Degree(10f).InRadians())
        ////                        left = true;
        ////                    else
        ////                        right = true;
        ////                }
        ////                else if ( diffAngle >= new Degree(.1f).InRadians())
        ////                {
        ////                    if (diffAngle > 0)
        ////                        right = true;
        ////                    if ((dist >= 1f && dist <= 20f) && diffAngle > 0)
        ////                        right = true;

        ////                }
        ////                else if (diffAngle <= new Degree(-0.1f).InRadians())
        ////                {
        ////                    if (diffAngle < 0)
        ////                        left = true;
        ////                    if ((dist >= 1f && dist <= 20f) && diffAngle < 0)
        ////                        left = true;
        ////                }

        ////                if (diffAngle > -MathFunctions.PI / 2 || diffAngle < MathFunctions.PI / 2)
        ////                {
        ////                    if (needDir.LengthFast() < 30.0f || dist < 10f) //we are too close!!
        ////                    {
        ////                        backward = true;
        ////                        //if (diffAngle > 0)
        ////                        //{
        ////                        //    right = true;
        ////                        //    left = false;
        ////                        //}
        ////                        //else
        ////                        //{
        ////                        //    left = true;
        ////                        //    right = false;
        ////                        //}
        ////                    }
        ////                    else
        ////                    {
        ////                        forward = true;
        ////                        //if (diffAngle < 0)
        ////                        //{
        ////                        //    right = true;
        ////                        //    left = false;
        ////                        //}
        ////                        //else
        ////                        //{
        ////                        //    left = true;
        ////                        //    right = false;
        ////                        //}
        ////                    }
        ////                }
        ////            }
        ////*/
        ////            if (forward)
        ////                ControlKeyPress(GameControlKeys.Forward, 1);
        ////            else
        ////                ControlKeyRelease(GameControlKeys.Forward);

        ////            if (backward)
        ////                ControlKeyPress(GameControlKeys.Backward, 1);
        ////            else
        ////                ControlKeyRelease(GameControlKeys.Backward);

        ////            if (left)
        ////                ControlKeyPress(GameControlKeys.Left, 1);
        ////            else
        ////                ControlKeyRelease(GameControlKeys.Left);

        ////            if (right)
        ////                ControlKeyPress(GameControlKeys.Right, 1);
        ////            else
        ////                ControlKeyRelease(GameControlKeys.Right);
        ////        }

        private void UpdateMoveTaskControlKeys()
        {
            bool forward = false;
            bool backward = false;
            bool left = false;
            bool right = false;

            Vec3 ObjectinwayMax = new Vec3(0, 0, 0);
            Vec3 ObjectinwayMin = new Vec3(0, 0, 0);
            MapObject obj = null;
            float dist = 100f;
            GetFrontObjectDistance(out obj, out dist);

            if (moveTaskEnabled)
            {
                //Vehicle specific
                if (obj != null)
                {
                    ObjectinwayMax = obj.MapBounds.Maximum;
                    ObjectinwayMin = obj.MapBounds.Minimum;
                    //Box box = obj.GetBox();
                    //Objectinway = box.Extents;
                    //if (EngineConsole.Instance != null)
                    //    EngineConsole.Instance.Print(ObjectinwayMax.ToString());
                }

                Vec3 unitPos = ControlledObject.Position;
                Vec3 unitDir = ControlledObject.Rotation.GetForward();
                Vec3 needDir;
                if (dist < 20)
                    needDir = moveTaskPosition - ObjectinwayMax;
                else
                    needDir = moveTaskPosition - unitPos;

                Radian unitAngle = MathFunctions.ATan(unitDir.Y, unitDir.X);
                Radian needAngle = MathFunctions.ATan(needDir.Y, needDir.X);

                Radian diffAngle = needAngle - unitAngle;
                while (diffAngle < -MathFunctions.PI)
                    diffAngle += MathFunctions.PI * 2;
                while (diffAngle > MathFunctions.PI)
                    diffAngle -= MathFunctions.PI * 2;

                //!!!!!!! 10.0f
                if (Math.Abs(diffAngle) > new Degree(10.0f).InRadians())
                {
                    if (diffAngle > 0)
                        left = true;
                    else
                        right = true;
                }
                Mech tank = ControlledObject as Mech;
                if (tank != null)
                {
                    Range range = tank.Type.OptimalAttackDistanceRange;

                    var objectifDist = (moveTaskPosition - unitPos).Length();
                    if (objectifDist > range.Maximum)
                        forward = true;
                    else if (objectifDist < range.Minimum)
                        backward = true;
                }

                if (forward)
                    ControlKeyPress(GameControlKeys.Forward, 1);
                else
                    ControlKeyRelease(GameControlKeys.Forward);

                if (backward)
                    ControlKeyPress(GameControlKeys.Backward, 1);
                else
                    ControlKeyRelease(GameControlKeys.Backward);

                if (left)
                    ControlKeyPress(GameControlKeys.Left, 1);
                else
                    ControlKeyRelease(GameControlKeys.Left);

                if (right)
                    ControlKeyPress(GameControlKeys.Right, 1);
                else
                    ControlKeyRelease(GameControlKeys.Right);
            }
        }

        private float GetLeftObjectDistance()
        {
            float Temptzise = 0f;
            //Here the magic happens
            Ray ray = new Ray(ControlledObject.Position + new Vec3(0, -1, 0), new Vec3(0, -1000, 0));
            RayCastResult[] piercingResult = PhysicsWorld.Instance.RayCastPiercing(
                    ray, (int)ContactGroup.CastOnlyContact);
            foreach (RayCastResult result in piercingResult)
            {
                HeightFieldShape heightFieldShape = result.Shape as HeightFieldShape;
                if (result.Shape.ShapeType == Shape.Type.HeightField)
                    continue;

                StaticMesh MO = MapSystemWorld.GetMapObjectByBody(result.Shape.Body) as StaticMesh;
                if (MO != null)
                {
                    string shapename = MO.Name;

                    if (shapename != null && shapename == "StaticMesh_Terrain_0")
                        continue;
                    return result.Distance;
                }
            }
            return Temptzise;
        }

        private float GetRightObjectDistance()
        {
            float Temptzise = 0f;
            //Here the magic happens
            Ray ray = new Ray(ControlledObject.Position + new Vec3(0, 1, 0), new Vec3(0, 1000, 0));
            RayCastResult[] piercingResult = PhysicsWorld.Instance.RayCastPiercing(
                    ray, (int)ContactGroup.CastOnlyContact);
            foreach (RayCastResult result in piercingResult)
            {
                HeightFieldShape heightFieldShape = result.Shape as HeightFieldShape;
                if (result.Shape.ShapeType == Shape.Type.HeightField)
                    continue;

                StaticMesh MO = MapSystemWorld.GetMapObjectByBody(result.Shape.Body) as StaticMesh;
                if (MO != null)
                {
                    string shapename = MO.Name;

                    if (shapename != null && shapename == "StaticMesh_Terrain_0")
                        continue;
                    return result.Distance;
                }
            }
            return Temptzise;
        }

        private float GetRearObjectDistance()
        {
            float Temptzise = 0f;
            //Here the magic happens
            Ray ray = new Ray(ControlledObject.Position + new Vec3(-1, 0, 0), new Vec3(-1000, 0, 0));
            RayCastResult[] piercingResult = PhysicsWorld.Instance.RayCastPiercing(
                    ray, (int)ContactGroup.CastOnlyContact);
            foreach (RayCastResult result in piercingResult)
            {
                HeightFieldShape heightFieldShape = result.Shape as HeightFieldShape;
                if (result.Shape.ShapeType == Shape.Type.HeightField)
                    continue;

                StaticMesh MO = MapSystemWorld.GetMapObjectByBody(result.Shape.Body) as StaticMesh;
                if (MO != null)
                {
                    string shapename = MO.Name;

                    if (shapename != null && shapename == "StaticMesh_Terrain_0")
                        continue;
                    return result.Distance;
                }
            }
            return Temptzise;
        }

        private void GetFrontObjectDistance(out MapObject obj, out float distance)
        {
            StaticMesh closestObject = null;
            float closestdistance = 300f;
            Ray ray = new Ray(ControlledObject.Position + new Vec3(1, 0, 0), new Vec3(300, 0, 0));
            RayCastResult[] piercingResult = PhysicsWorld.Instance.RayCastPiercing(
                    ray, (int)ContactGroup.CastOnlyContact);
            foreach (RayCastResult result in piercingResult)
            {
                HeightFieldShape heightFieldShape = result.Shape as HeightFieldShape;
                if (result.Shape.ShapeType == Shape.Type.HeightField)
                    continue;

                StaticMesh MO = MapSystemWorld.GetMapObjectByBody(result.Shape.Body) as StaticMesh;
                if (MO != null)
                {
                    string shapename = MO.Name;

                    if (shapename != null && shapename == "StaticMesh_Terrain_0")
                        continue;
                    //return result.Distance;
                    if (result.Distance <= closestdistance && MO != null)
                    {
                        closestObject = MO;
                        closestdistance = result.Distance;
                    }
                }
            }
            //return Temptzise;
            obj = closestObject;
            distance = closestdistance;
        }

        private Vec3 getFrontObjectSize()
        {
            Vec3 Temptzise = new Vec3(0, 0, 0);
            //Here the magic happens
            Ray ray = new Ray(ControlledObject.Position + new Vec3(1, 0, 0), new Vec3(1000, 0, 0));
            RayCastResult[] piercingResult = PhysicsWorld.Instance.RayCastPiercing(
                    ray, (int)ContactGroup.CastOnlyContact);
            foreach (RayCastResult result in piercingResult)
            {
                HeightFieldShape heightFieldShape = result.Shape as HeightFieldShape;
                if (result.Shape.ShapeType == Shape.Type.HeightField)
                    continue;

                StaticMesh MO = MapSystemWorld.GetMapObjectByBody(result.Shape.Body) as StaticMesh;
                if (MO != null)
                {
                    string shapename = MO.Name;

                    if (shapename != null && shapename.Contains("StaticMesh_Terrain"))
                        continue;

                    if (MO != null)
                    {
                        //Box box = MO.GetBox().Extents;

                        Vec3 position = MO.GetInterpolatedPosition();
                        Quat rotation = MO.GetInterpolatedRotation();
                        Vec3 scale = MO.GetInterpolatedScale();

                        //MO.GetInterpolatedTransform(out position, out rotation, out scale);
                        //if(size was not 0)
                        //Vec3 size = box;
                        //Temptzise = new Vec3(position);
                        //return Temptzise;
                        Vec3 interp = position * rotation * scale;
                        return new Vec3(interp.X, interp.Y, interp.Z);
                    }
                }
            }

            return Temptzise;
        }

        //protected override void OnRelatedEntityDelete(Entity entity)
        //{
        //    base.OnRelatedEntityDelete(entity);
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

        private Vec3 AvoidObjectTask()
        {
            Vec3 wall = getFrontObjectSize();
            return wall;
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

        private void AddRelationship(Entity ent)
        {
            SubscribeToDeletionEvent(ent);
        }

        private void RemoveRelationship(Entity ent)
        {
            UnsubscribeToDeletionEvent(ent);
        }

        private void ResetAttackTask(AttackTask task)
        {
            if (task.TargetEntity != null)
                RemoveRelationship(task.TargetEntity);
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

                        //move to enemy
                        DoMoveTask(sourceUnit.Position);

                        //notify allies
                        NotifyAlliesOnEnemy(sourceUnit.Position);
                    }
                }
            }
        }

        private void NotifyAlliesOnEnemy(Vec3 enemyPos)
        {
            if (Type.NotifyAlliesRadius <= 0)
                return;

            Map.Instance.GetObjects(new Sphere(ControlledObject.Position, Type.NotifyAlliesRadius),
                MapObjectSceneGraphGroups.UnitGroupMask, delegate(MapObject mapObject)
                {
                    Unit unit = (Unit)mapObject;

                    if (unit == ControlledObject)
                        return;

                    MechUnitAI unitAI = unit.Intellect as MechUnitAI;
                    if (unitAI != null && unitAI.Faction == Faction)
                    {
                        unitAI.OnNotifyFromAllyOnEnemy(enemyPos);
                    }
                });
        }

        public void OnNotifyFromAllyOnEnemy(Vec3 enemyPos)
        {
            if (generalTaskType != GeneralTaskTypes.Battle)
            {
                //do battle task
                DoGeneralTask(GeneralTaskTypes.Battle, null);

                DoMoveTask(enemyPos);
            }
        }
    }
}

////////////////////////////////////////////////////////////////
/////// Incin I added this to the end but I need some way to update it. Can ya do that? This is for the Recast Navigation system.
/// <summary>
/// This needs to be updated ontick. But I have no clue how to do that. See what you can do with it. Thanks bud. :D
/// Also I am not sure if I even got this code in the right place. I just copied and pasted it from GameCharacterAI.cs
/// </summary>

public class PathController
{
    private readonly float reachDestinationPointDistance = .5f;
    private readonly float reachDestinationPointZDifference = 1.5f;
    private readonly float maxAllowableDeviationFromPath = .5f;
    private readonly float updatePathWhenTargetPositionHasChangedMoreThanDistance = 2;
    private readonly float stepSize = 1; //
    private readonly Vec3 polygonPickExtents = new Vec3(2, 2, 2);
    private readonly int maxPolygonPath = 512; //quadrants / 8;
    private readonly int maxSmoothPath = 4096; //plane? quadrants / 4 ;
    private readonly int maxSteerPoints = 16; //360 radians X 360 x 360;

    private Vec3 foundPathForTargetPosition = new Vec3(float.NaN - 1, float.NaN - 1, float.NaN - 1);
    private Vec3[] path;
    private float pathFindWaitTime;
    private int currentIndex;

    //

    private RecastNavigationSystem GetNavigationSystem()
    {
        //use first instance on the map
        if (RecastNavigationSystem.Instances.Count != 0)
            return RecastNavigationSystem.Instances[0];
        return null;
    }

    public void DropPath()
    {
        foundPathForTargetPosition = new Vec3(float.NaN, float.NaN, float.NaN);
        path = null;
        currentIndex = 0;
    }

    public void Reset()
    {
        DropPath();
    }

    public void Update(float delta, Vec3 unitPosition, Vec3 targetPosition, bool dropPath)
    {
        if (dropPath)
            DropPath();

        //wait before last path find
        if (pathFindWaitTime > 0)
        {
            pathFindWaitTime -= delta;
            if (pathFindWaitTime < 0)
                pathFindWaitTime = 0;
        }

        //already on target position?
        if ((unitPosition.ToVec2() - targetPosition.ToVec2()).LengthSqr() <
            reachDestinationPointDistance * reachDestinationPointDistance &&
            Math.Abs(unitPosition.Z - targetPosition.Z) < reachDestinationPointZDifference)
        {
            DropPath();
            return;
        }

        //drop path when target position was updated
        if (path != null && (foundPathForTargetPosition - targetPosition).Length() >
            updatePathWhenTargetPositionHasChangedMoreThanDistance)
        {
            DropPath();
        }

        //drop path when unit goaway from path
        if (path != null && currentIndex > 0)
        {
            Vec3 previous = path[currentIndex - 1];
            Vec3 next = path[currentIndex];

            float min = Math.Min(previous.Z, next.Z);
            float max = Math.Max(previous.Z, next.Z);

            Vec2 projectedPoint = MathUtils.ProjectPointToLine(
                previous.ToVec2(), next.ToVec2(), unitPosition.ToVec2());
            float distance2D = (unitPosition.ToVec2() - projectedPoint).Length();

            if (distance2D > maxAllowableDeviationFromPath ||
                unitPosition.Z + reachDestinationPointZDifference < min ||
                unitPosition.Z - reachDestinationPointZDifference > max)
            {
                DropPath();
            }
        }

        //check if need update path
        if (path == null && pathFindWaitTime == 0)
        {
            bool found;

            RecastNavigationSystem system = GetNavigationSystem();
            if (system != null)
            {
                found = system.FindPath(unitPosition, targetPosition, stepSize, polygonPickExtents,
                    maxPolygonPath, maxSmoothPath, maxSteerPoints, out path);
            }
            else
            {
                found = true;
                path = new Vec3[] { targetPosition };
            }

            currentIndex = 0;

            if (found)
            {
                foundPathForTargetPosition = targetPosition;
                //can't find new path during specified time.
                pathFindWaitTime = .3f;
            }
            else
            {
                foundPathForTargetPosition = new Vec3(float.NaN, float.NaN, float.NaN);
                //can't find new path during specified time.
                pathFindWaitTime = 1.0f;
            }
        }

        //progress
        if (path != null)
        {
            Vec3 point;
            while (true)
            {
                point = path[currentIndex];

                if ((unitPosition.ToVec2() - point.ToVec2()).LengthSqr() <
                    reachDestinationPointDistance * reachDestinationPointDistance &&
                    Math.Abs(unitPosition.Z - point.Z) < reachDestinationPointZDifference)
                {
                    //reach point
                    currentIndex++;
                    if (currentIndex == path.Length)
                    {
                        //path is ended
                        DropPath();
                        break;
                    }
                }
                else
                    break;
            }
        }
    }

    public bool GetNextPointPosition(out Vec3 position)
    {
        if (path != null)
        {
            position = path[currentIndex];
            return true;
        }
        position = Vec3.Zero;
        return false;
    }

    public void DebugDrawPath(Camera camera)
    {
        if (path != null)
        {
            camera.DebugGeometry.Color = new ColorValue(0, 1, 0);
            for (int n = currentIndex; n < path.Length; n++)
                camera.DebugGeometry.AddSphere(new Sphere(path[n], .15f), 8);
        }
    }
}