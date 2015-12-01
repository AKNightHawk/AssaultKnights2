using System;
using Engine.EntitySystem;
using Engine.MapSystem;
using Engine.MathEx;
using ProjectCommon;
using ProjectEntities.Community;

namespace ProjectEntities
{
    public partial class Character : Unit
    {
        private Ladder currentLadder;

        private Ladder FindLadder(bool alreadyAttachedToSomeLadder, bool wantMove, SphereDir lookDirection)
        {
            float fromPositionToFloorDistance = (Type.Height - Type.WalkUpHeight) * .5f + Type.WalkUpHeight;

            Ladder result = null;

            Bounds bounds = MapBounds;
            bounds.Expand(1);

            Map.Instance.GetObjects(bounds, delegate(MapObject mapObject)
            {
                if (result != null)
                    return;

                Ladder ladder = mapObject as Ladder;
                if (ladder == null)
                    return;
                //if (ladder.IsInvalidOrientation())
                //   return;

                Line line = ladder.GetClimbingLine();

                Vec3 projected = MathUtils.ProjectPointToLine(line.Start, line.End, Position);

                //check by distance
                float distanceToLine = (projected - Position).Length();
                if (distanceToLine > .5f)
                    return;

                //check by up and down limits
                if (alreadyAttachedToSomeLadder)
                {
                    if (projected.Z > line.End.Z + fromPositionToFloorDistance)
                        return;
                }
                else
                {
                    if (projected.Z > line.End.Z + fromPositionToFloorDistance * .5f)
                        return;
                }
                if (projected.Z < line.Start.Z)
                    return;

                //check by direction
                bool isHorizontallyOrientedToLadder;
                {
                    Vec2 ladderDirection2 = ladder.Rotation.GetForward().ToVec2();
                    Vec2 direction2 = Rotation.GetForward().ToVec2();
                    Radian angle = MathUtils.GetVectorsAngle(ladderDirection2, direction2);
                    isHorizontallyOrientedToLadder = angle.InDegrees() < 70;
                }

                if (Math.Abs(new Radian(lookDirection.Vertical).InDegrees()) < 45 &&
                    !isHorizontallyOrientedToLadder)
                {
                    if (alreadyAttachedToSomeLadder)
                    {
                        if (wantMove)
                            return;
                    }
                    else
                        return;
                }

                //got ladder
                result = ladder;
            });

            return result;
        }

        protected override void OnDeleteSubscribedToDeletionEvent(Entity entity)
        {
            base.OnDeleteSubscribedToDeletionEvent(entity);

            if (currentLadder == entity)
                SetCurrentLadder(null);
        }

        private void SetCurrentLadder(Ladder ladder)
        {
            if (currentLadder == ladder)
                return;

            if (currentLadder != null)
                SubscribeToDeletionEvent(currentLadder);
            currentLadder = ladder;
            if (currentLadder != null)
                UnsubscribeToDeletionEvent(currentLadder);

            if (mainBody != null)
            {
                mainBody.EnableGravity = currentLadder == null;
            }
        }

        private void TickLadder()
        {
            //!!!!!â òèï?
            const float ladderClimbingSpeedWalk = 1.5f;
            const float ladderClimbingSpeedRun = 3;

            SphereDir lookDirection = SphereDir.Zero;
            {
                PlayerIntellect playerIntellect = Intellect as PlayerIntellect;
                if (playerIntellect != null)
                    lookDirection = playerIntellect.LookDirection;
            }

            bool wantMove =
                Intellect.IsControlKeyPressed(GameControlKeys.Forward) ||
                Intellect.IsControlKeyPressed(GameControlKeys.Backward) ||
                Intellect.IsControlKeyPressed(GameControlKeys.Left) ||
                Intellect.IsControlKeyPressed(GameControlKeys.Right);

            Ladder ladder = FindLadder(currentLadder != null, wantMove, lookDirection);

            if (ladder != currentLadder)
            {
                SetCurrentLadder(ladder);
            }

            if (currentLadder != null)
            {
                Line line = currentLadder.GetClimbingLine();

                Vec3 projected = MathUtils.ProjectPointToLine(line.Start, line.End, Position);

                Vec3 newPosition = projected;

                float climbingSpeed = IsNeedRun() ? ladderClimbingSpeedRun : ladderClimbingSpeedWalk;

                Vec3 moveVector = Vec3.Zero;

                float lookingSide = new Radian(lookDirection.Vertical).InDegrees() > -20 ? 1 : -1;
                moveVector.Z += Intellect.GetControlKeyStrength(GameControlKeys.Forward) * lookingSide;
                moveVector.Z -= Intellect.GetControlKeyStrength(GameControlKeys.Backward) * lookingSide;

                newPosition += moveVector * (TickDelta * climbingSpeed);

                Position = newPosition;

                if (mainBody != null)
                {
                    mainBody.LinearVelocity = Vec3.Zero;
                    mainBody.AngularVelocity = Vec3.Zero;
                }
            }
        }

        private void JumpFromLadder()
        {
            Position += (currentLadder.Rotation * new Vec3(-0.4f, 0, 0));
            Vec3 vel = (currentLadder.Rotation * new Vec3(-4f, 0, 0));

            vel.Z = Type.JumpSpeed;

            mainBody.LinearVelocity = vel;
            jumpInactiveTime = .2f;
            shouldJumpTime = 0;

            UpdateMainBodyDamping();

            OnJump();

            if (EntitySystemWorld.Instance.IsServer())
                Server_SendJumpEventToAllClients();
        }
    }
}