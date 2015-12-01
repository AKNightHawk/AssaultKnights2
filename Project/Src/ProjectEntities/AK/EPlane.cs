using System.Collections.ObjectModel;
using Engine;
using Engine.MapSystem;
using Engine.MathEx;
using Engine.PhysicsSystem;
using Engine.Renderer;

namespace GameEntities
{
    public class EPlaneType : UnitType
    { }

    public class EPlane : Unit
    {
        [FieldSerialize]
        private MapCurve TaskWay;

        [FieldSerialize]
        private MapCurvePoint CurrentWayPoint;

        [FieldSerialize]
        private Vec3 TaskPosition;

        [FieldSerialize]
        private float velocity;

        public float Velocity
        {
            get { return velocity; }
            set { velocity = value; }
        }

        private Body chassisBody;
        private float ZADiffAngle;

        private EPlaneType _type = null; public new EPlaneType Type { get { return _type; } }

        [FieldSerialize]
        private float flyHeight = 10;

        [FieldSerialize]
        private MapCurve way;

        [FieldSerialize]
        private bool firstWay = true;

        public MapCurve Way
        {
            get { return way; }
            set { way = value; }
        }

        public float FlyHeight
        {
            get { return flyHeight; }
            set { flyHeight = value; }
        }

        protected override void OnPostCreate(bool loaded)
        {
            base.OnPostCreate(loaded);
            AddTimer();
        }

        protected override void OnTick()
        {
            base.OnTick();

            if (firstWay == true)
            {
                ActivateWayMovement();
            }

            UpdateGeneralTask();
            CalculateThings();

            chassisBody = PhysicsModel.GetBody("main");
            if (chassisBody == null)
            {
                Log.Error("Emrah Helli: \"main\" chassisBody dose not exists.");
                return;
            }

            float diff = Position.Z - flyHeight;
            float force = -diff - chassisBody.LinearVelocity.Z;
            MathFunctions.Clamp(ref force, -10, 10);

            chassisBody.AddForce(ForceType.GlobalAtLocalPos, TickDelta,
                new Vec3(0, 0, force * 3) * chassisBody.Mass, new Vec3(0, 0, 1));

            chassisBody.AngularDamping = 1;

            this.flyHeight = TaskPosition.Z;

            float ZA = ZADiffAngle * 4;

            if (ZA < 20 && ZA > -20)
            {
                chassisBody.AngularVelocity = new Vec3
                    (chassisBody.AngularVelocity.X, chassisBody.AngularVelocity.Y, ZA);
            }
            else if (ZA > 20)
            {
                chassisBody.AngularVelocity = new Vec3
                    (chassisBody.AngularVelocity.X, chassisBody.AngularVelocity.Y, -2);
            }
            else if (ZA < -20)
            {
                chassisBody.AngularVelocity = new Vec3
                    (chassisBody.AngularVelocity.X, chassisBody.AngularVelocity.Y, 2);
            }

            chassisBody.AddForce(ForceType.LocalAtLocalPos, TickDelta, new Vec3(Velocity * chassisBody.Mass, 0, 0), new Vec3(-1, 0, 0));
            chassisBody.LinearDamping = 1;
            //check outside Map position
            Bounds checkBounds = Map.Instance.InitialCollisionBounds;
            checkBounds.Expand(new Vec3(300, 300, 10000));
            if (!checkBounds.IsContainsPoint(Position))
                SetDeleted();
        }

        //you can delete this part Bro its for debug things
        protected override void OnRender(Camera camera)
        {
            base.OnRender(camera);

            //debug geometry
            if (EngineDebugSettings.DrawGameSpecificDebugGeometry)
            {
                //way
                if (CurrentWayPoint != null)
                {
                    ReadOnlyCollection<MapCurvePoint> points = TaskWay.Points;

                    camera.DebugGeometry.Color = new ColorValue(0, 1, 0, .5f);
                    int index = points.IndexOf(CurrentWayPoint);
                    for (; index < points.Count - 1; index++)
                    {
                        camera.DebugGeometry.AddArrow(
                            points[index].Position, points[index + 1].Position, 1);
                    }
                }

                //view radius
                if (this.ViewRadius != 0)
                {
                    camera.DebugGeometry.Color = new ColorValue(0, 1, 0, .5f);
                    Vec3 lastPos = Vec3.Zero;
                    for (float angle = 0; angle <= MathFunctions.PI * 2 + .001f;
                        angle += MathFunctions.PI / 16)
                    {
                        Vec3 pos = this.Position + new Vec3(MathFunctions.Cos(angle),
                            MathFunctions.Sin(angle), 0) * this.ViewRadius;

                        if (angle != 0)
                            camera.DebugGeometry.AddLine(lastPos, pos);

                        lastPos = pos;
                    }
                }

                camera.DebugGeometry.Color = new ColorValue(0, 1, 0);
                camera.DebugGeometry.AddArrow(this.Position, TaskPosition, 1);
            }
        }

        // start AI
        public void ActivateWayMovement()
        {
            if (way != null)
                DoGeneralTask(way);
        }

        private void DoGeneralTask(MapCurve way)
        {
            TaskWay = way;
            CurrentWayPoint = null;

            if (TaskWay != null && CurrentWayPoint == null)
            {
                CurrentWayPoint = TaskWay;
            }
            if (CurrentWayPoint != null)
            {
                firstWay = false;
            }
        }

        private void UpdateGeneralTask()
        {
            const float wayPointCheckDistance = 10;

            float wayPointDistance = (this.Position -
                CurrentWayPoint.Position).LengthFast();

            if (wayPointDistance < wayPointCheckDistance)
            {
                //next way point or start again from the 1 point

                int index = TaskWay.Points.IndexOf(CurrentWayPoint);
                index++;

                if (index < TaskWay.Points.Count)
                {
                    //next way point
                    CurrentWayPoint = TaskWay.Points[index];
                }
                else
                {
                    CurrentWayPoint = TaskWay.Points[0];
                }
            }

            if (CurrentWayPoint != null)
                DoMoveTask(CurrentWayPoint.Position);
        }

        private void DoMoveTask(Vec3 pos)
        {
            TaskPosition = pos;
        }

        private void CalculateThings()
        {
            Vec3 unitPos = this.Position;
            Vec3 unitDir = this.Rotation.GetForward();
            Vec3 needDir;

            needDir = TaskPosition - unitPos;
            Radian unitAngle = MathFunctions.ATan(unitDir.Y, unitDir.X);
            Radian needAngle = MathFunctions.ATan(needDir.Y, needDir.X);
            Radian diffAngle = needAngle - unitAngle;

            if (diffAngle < -MathFunctions.PI)
                diffAngle = -MathFunctions.PI * 2;
            if (diffAngle > MathFunctions.PI)
                diffAngle = MathFunctions.PI * 2;
            ZADiffAngle = diffAngle;
        }

        // End AI
        private float GetRealSpeed()
        {
            return (chassisBody.LinearVelocity * chassisBody.Rotation.GetInverse()).X;
        }
    }
}