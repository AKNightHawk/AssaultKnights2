// iNCIN modded this
using System;
using System.ComponentModel;
using System.Drawing.Design;
using Engine;
using Engine.EntitySystem;
using Engine.MapSystem;
using Engine.MathEx;
using Engine.PhysicsSystem;
using Engine.Renderer;
using Engine.SoundSystem;
using Engine.Utils;

//using Engine.EntitySystem;
//using EngineApp;

namespace ProjectEntities
{
    /// <summary>
    /// Defines the <see cref="BoxTeleporter"/> entity type.
    /// </summary>
    public class BoxTeleporterType : DynamicType
    {
        [FieldSerialize]
        private float teleportTransitTime = .3f;

        [FieldSerialize]
        private string sendParticleName = "";

        [FieldSerialize]
        private string receiveParticleName = "";

        //!!!!!!
        [FieldSerialize]
        private string soundTeleportation = "";

        [DefaultValue(.3f)]
        public float TeleportTransitTime
        {
            get { return teleportTransitTime; }
            set { teleportTransitTime = value; }
        }

        private Vec3 size = new Vec3(1, 1, 1);

        //maxs of box
        [Description("Set sizes of teleporter box in Map Editor: Use BoxLength, BoxWidth, and BoxHeight")]
        public Vec3 BoxSize
        {
            get { return size; }
        }

        [Editor(typeof(EditorParticleUITypeEditor), typeof(UITypeEditor))]
        public string SendParticleName
        {
            get { return sendParticleName; }
            set { sendParticleName = value; }
        }

        [Editor(typeof(EditorParticleUITypeEditor), typeof(UITypeEditor))]
        public string ReceiveParticleName
        {
            get { return receiveParticleName; }
            set { receiveParticleName = value; }
        }

        [DefaultValue("")]
        [Editor(typeof(EditorSoundUITypeEditor), typeof(UITypeEditor))]
        [SupportRelativePath]
        public string SoundTeleportation
        {
            get { return soundTeleportation; }
            set { soundTeleportation = value; }
        }

        protected override void OnPreloadResources()
        {
            base.OnPreloadResources();

            PreloadSound(SoundTeleportation, 0);
            PreloadSound(SoundTeleportation, SoundMode.Mode3D);
        }
    }

    /// <summary>
    /// Defines the teleporter for transfering objects.
    /// </summary>
    public class BoxTeleporter : Dynamic
    {
        private static Box box;
        private static Bounds bounds;

        [FieldSerialize]
        private bool active = true;

        [FieldSerialize]
        private bool traversal = false;

        [FieldSerialize]
        private float length = 1f;

        [FieldSerialize]
        private float width = 1f;

        [FieldSerialize]
        private float height = 1f;

        [Description("Length of Box Teleporter.")]
        [DefaultValue("1")]
        public float BoxLength
        {
            get { return length; }
            set
            {
                if (length <= 0)
                    length = .1f;
                else
                    length = value;
            }
        }

        [Description("Width of Box Teleporter.")]
        [DefaultValue("1")]
        public float BoxWidth
        {
            get { return width; }
            set
            {
                if (width <= 0)
                    width = .1f;
                else
                    width = value;
            }
        }

        [Description("Height of Box Teleporter.")]
        [DefaultValue("1")]
        public float BoxHeight
        {
            get { return height; }
            set
            {
                if (height <= 0)
                    height = .1f;
                else
                    height = value;
            }
        }

        [FieldSerialize]
        private BoxTeleporter destination;

        [FieldSerialize(FieldSerializeSerializationTypes.World)]
        private float teleportTransitProgress;

        [FieldSerialize(FieldSerializeSerializationTypes.World)]
        private MapObject objectWhichActivatesTransition;

        private Set<MapObject> processedObjectsInActiveArea = new Set<MapObject>();

        private BoxTeleporterType _type = null; public new BoxTeleporterType Type { get { return _type; } }

        /// <summary>
        /// Gets or sets a value indicating whether the teleporter is currently active.
        /// </summary>
        [Description("A value indicating whether the teleporter is currently active.")]
        [DefaultValue(true)]
        public bool Active
        {
            get { return active; }
            set
            {
                if (active == value)
                    return;
                active = value;
                if (IsPostCreated)
                    UpdateAttachedObjects();
            }
        }

        /// <summary>
        /// Allow a player to go through or not.
        /// </summary>
        [Description("A value indicating whether the Player can travel through teleporter.")]
        [DefaultValue(false)]
        public bool AllowPlayerTraversal
        {
            get { return traversal; }
            set { traversal = value; }
        }

        /// <summary>
        /// Gets or sets the destination teleporter.
        /// </summary>
        [Description("The destination teleporter.")]
        public BoxTeleporter Destination
        {
            get { return destination; }
            set
            {
                if (value == this)
                    throw new Exception("To refer to itself is impossible.");

                if (destination != null)
                    UnsubscribeToDeletionEvent(destination);
                destination = value;
                if (destination != null)
                    SubscribeToDeletionEvent(destination);
            }
        }

        /// <summary>Overridden from <see cref="Engine.EntitySystem.Entity.OnPostCreate(Boolean)"/>.</summary>
        protected override void OnPostCreate(bool loaded)
        {
            base.OnPostCreate(loaded);

            if (EntitySystemWorld.Instance.IsServer())
            {
                if (Active)
                {
                    Vec3 min = Position + new Vec3(0, 0, 0); //min
                    Vec3 max = Position + new Vec3(Type.BoxSize.X * BoxLength * Scale.X, Type.BoxSize.Y * BoxWidth * Scale.Y, Type.BoxSize.Z * BoxHeight * Scale.Z);  //max
                    Mat3 rotation = new Mat3(1, 0, 0, 0, 1, 0, 0, 0, 1);

                    //camera.DebugGeometry.Color = new ColorValue(0, 1, 0, .5f);
                    bounds = new Bounds(min, max);
                    box = new Box(bounds);
                    box.Axis = rotation * Rotation.ToMat3();

                    box.Center = Position + new Vec3(0, 0, ((max.Z - min.Z) / 2) + .1f);
                    box.Expand(.1f);
                    //camera.DebugGeometry.AddBox(box);
                }
            }

            SubscribeToTickEvent();
            UpdateAttachedObjects();
        }

        /// <summary>Overridden from <see cref="Engine.EntitySystem.Entity.OnDeleteSubscribedToDeletionEvent(Entity)"/></summary>
        protected override void OnDeleteSubscribedToDeletionEvent(Entity entity)
        {
            base.OnDeleteSubscribedToDeletionEvent(entity);

            if (entity == destination)
                destination = null;
        }

        protected override void OnTick()
        {
            base.OnTick();

            if (teleportTransitProgress != 0)
            {
                //teleportation progress
                teleportTransitProgress += TickDelta;
                if (teleportTransitProgress >= Type.TeleportTransitTime)
                    DoTransfer();
            }
            else
            {
                bool enable = false;
                if (Active)
                {
                    if (destination != null)
                        enable = true;
                }

                if (enable)
                {
                    Set<MapObject> objects = GetObjectsInActiveArea();

                   //remove objects from list of processed objects
                reply:
                    foreach (MapObject obj in processedObjectsInActiveArea)
                    {
                        if (!objects.Contains(obj))
                        {
                            processedObjectsInActiveArea.Remove(obj);
                            goto reply;
                        }
                    }

                    //find object for activation teleportation
                    foreach (MapObject obj in objects)
                    {
                        if (!processedObjectsInActiveArea.Contains(obj))
                        {
                            //object comes to active zone in this tick.
                            BeginTeleportation(obj);
                            break;
                        }
                    }
                }
                else
                {
                    processedObjectsInActiveArea.Clear();
                }
            }
        }

        protected virtual bool IsAllowToTeleport(MapObject obj)
        {
            //allow to teleport only for objects with physics model and only with dynamic bodies.
            if (obj.PhysicsModel != null)
            {
                bool allDynamic = true;
                foreach (Body body in obj.PhysicsModel.Bodies)
                {
                    if (body.Static)
                    {
                        allDynamic = false;
                        break;
                    }
                }
                if (allDynamic)
                    return true;
            }

            return false;
        }

        private bool CheckPositionInActiveArea(Bounds bounds, Vec3 pos)
        {
            if (bounds.IsContainsPoint(pos))
            {
                return true;
            }
            return false;
        }

        private Set<MapObject> GetObjectsInActiveArea()
        {
            Set<MapObject> result = new Set<MapObject>();
            Bounds areabounds = new Bounds(Position - box.Extents, Position + box.Extents);
            Body[] bodies = PhysicsWorld.Instance.VolumeCast(areabounds, (int)ContactGroup.CastOnlyDynamic);

            foreach (Body body in bodies)
            {
                if (!body.Static)
                {
                    MapObject obj = MapSystemWorld.GetMapObjectByBody(body);
                    if (obj != null && obj != this && IsAllowToTeleport(obj) &&
                       CheckPositionInActiveArea(areabounds, obj.Position))
                    {
                        result.AddWithCheckAlreadyContained(obj);
                    }
                }
            }

            return result;
        }

        public bool BeginTeleportation(MapObject objectWhichActivatesTransition)
        {
            if (teleportTransitProgress != 0)
                return false;

            if (!Active)
                return false;

            if (!AllowPlayerTraversal)
            {
                Unit unit = objectWhichActivatesTransition as Unit;
                if (unit != null)
                    return false;
            }

            teleportTransitProgress = .0001f;
            this.objectWhichActivatesTransition = objectWhichActivatesTransition;

            if (!string.IsNullOrEmpty(Type.SendParticleName))
                Map.Instance.CreateAutoDeleteParticleSystem(Type.SendParticleName, Position);

            //play teleportation sound
            Dynamic dynamic = objectWhichActivatesTransition as Dynamic;
            if (dynamic != null)
                dynamic.SoundPlay3D(Type.SoundTeleportation, .5f, false);
            else
                SoundPlay3D(Type.SoundTeleportation, .5f, false);

            return true;
        }

        private void DoTransfer()
        {
            Set<MapObject> objects = GetObjectsInActiveArea();
            if (objectWhichActivatesTransition != null && !objectWhichActivatesTransition.IsSetForDeletion)
                objects.AddWithCheckAlreadyContained(objectWhichActivatesTransition);

            foreach (MapObject obj in objects)
            {
                if (destination != null)
                    destination.ReceiveObject(obj, this);
            }

            teleportTransitProgress = 0;
            objectWhichActivatesTransition = null;
        }

        [Browsable(false)]
        public float TeleportTransitProgress
        {
            get { return teleportTransitProgress; }
        }

        private void UpdateAttachedObjects()
        {
            foreach (MapObjectAttachedObject attachedObject in AttachedObjects)
            {
                if (attachedObject.Alias == "active")
                    attachedObject.Visible = Active;
            }
        }

        protected override void OnRender(Camera camera)
        {
            base.OnRender(camera);

            if ((EngineDebugSettings.DrawGameSpecificDebugGeometry ||
               EngineApp.Instance.ApplicationType == EngineApp.ApplicationTypes.ResourceEditor ||
                   EngineApp.Instance.ApplicationType == EngineApp.ApplicationTypes.MapEditor ||
                   EngineApp.Instance.ApplicationType == EngineApp.ApplicationTypes.Simulation) &&
               camera.Purpose == Camera.Purposes.MainCamera)
            {
                if (Active)
                {
                    Vec3 min = Position + new Vec3(0, 0, 0); //min
                    Vec3 max = Position + new Vec3(Type.BoxSize.X * BoxLength * Scale.X, Type.BoxSize.Y * BoxWidth * Scale.Y, Type.BoxSize.Z * BoxHeight * Scale.Z);  //max
                    Mat3 rotation = new Mat3(1, 0, 0, 0, 1, 0, 0, 0, 1);

                    camera.DebugGeometry.Color = new ColorValue(0, 1, 0, .5f);
                    bounds = new Bounds(min, max);
                    box = new Box(bounds);
                    box.Axis = rotation * Rotation.ToMat3();

                    box.Center = Position + new Vec3(0, 0, ((max.Z - min.Z) / 2) + .1f);
                    box.Expand(.1f);
                    camera.DebugGeometry.AddBox(box);
                }
            }
        }

        public void ReceiveObject(MapObject obj, BoxTeleporter source)
        {
            if (!string.IsNullOrEmpty(Type.ReceiveParticleName))
                Map.Instance.CreateAutoDeleteParticleSystem(Type.ReceiveParticleName, Position);

            if (source == null)
            {
                float offset = obj.Position.Z - obj.PhysicsModel.GetGlobalBounds().Minimum.Z;
                obj.Position = Position + new Vec3(0, 0, offset);
                obj.Rotation = Rotation;
                obj.SetOldTransform(obj.Position, obj.Rotation, obj.Scale);
            }
            else
            {
                //iNCIN this is not correct -- needs to be fixed
                Quat destRotation = Rotation * Mat3.FromRotateByZ(new Degree(180).InRadians()).ToQuat();

                foreach (Body body in obj.PhysicsModel.Bodies)
                {
                    body.Rotation = body.Rotation * source.Rotation.GetInverse() * destRotation;
                    Vec3 localPosOffset = (body.Position - source.Position) * source.Rotation.GetInverse();
                    body.Position = Position + localPosOffset * destRotation;
                    body.OldPosition = body.Position;
                    body.OldRotation = body.Rotation;

                    body.LinearVelocity = body.LinearVelocity * source.Rotation.GetInverse() * destRotation;
                    body.AngularVelocity = body.AngularVelocity * source.Rotation.GetInverse() * destRotation;
                }

                obj.UpdatePositionAndRotationByPhysics(true);
                obj.SetOldTransform(obj.Position, obj.Rotation, obj.Scale);

                Unit unit = obj as Unit;
                if (unit != null)
                {
                    PlayerIntellect playerIntellect = unit.Intellect as PlayerIntellect;
                    if (playerIntellect != null)
                    {
                        Vec3 vec = playerIntellect.LookDirection.GetVector();
                        Vec3 v = vec * source.Rotation.GetInverse() * destRotation;
                        playerIntellect.LookDirection = SphereDir.FromVector(v);
                    }
                }
            }

            //add object to the list of processed objects. object can't activate teleportation.
            processedObjectsInActiveArea.AddWithCheckAlreadyContained(obj);
        }

        protected bool OnGetEditorSelectionByRay(Ray ray, out Vec3 pos, ref float priority)
        {
            float scale1, scale2;
            bool ret = GetBox().RayIntersection(ray, out scale1, out scale2);
            if (ret)
                pos = ray.GetPointOnRay(Math.Min(scale1, scale2));
            else
                pos = Vec3.Zero;
            return ret;
        }

        protected void OnEditorSelectionDebugRender(Camera camera, bool bigBorder, bool simpleGeometry)
        {
            Box box = GetBox();
            box.Expand(bigBorder ? .2f : .1f);
            camera.DebugGeometry.AddBox(box);
        }
    }
}