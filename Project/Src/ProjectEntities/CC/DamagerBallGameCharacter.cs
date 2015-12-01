// Copyright (C) 2006-2012 NeoAxis Group Ltd.
using System;
using System.ComponentModel;
using Engine.EntitySystem;
using Engine.MathEx;
using Engine.Renderer;

namespace ProjectEntities
{
    /// <summary>
    /// Defines the <see cref="GameCharacter"/> entity type.
    /// </summary>
    public class DamagerBallGCType : DamagerBallType
    {
        [FieldSerialize]
        [DefaultValue(typeof(Range), "10 0")]
        private Range optimalAttackDistanceRange;

        ///////////////////////////////////////////

        [DefaultValue(typeof(Range), "10 0")]
        public Range OptimalAttackDistanceRange
        {
            get { return optimalAttackDistanceRange; }
            set { optimalAttackDistanceRange = value; }
        }
    }

    public class DamagerBallGC : DamagerBall
    {
        private DamagerBallGCType _type = null; public new DamagerBallGCType Type { get { return _type; } }

        /// <summary>Overridden from <see cref="Engine.EntitySystem.Entity.OnPostCreate(Boolean)"/>.</summary>
        protected override void OnPostCreate(bool loaded)
        {
            base.OnPostCreate(loaded);
            SubscribeToTickEvent();
        }

        protected override void OnRenderFrame()
        {
            UpdateAnimationTree();

            base.OnRenderFrame();
        }

        private void UpdateAnimationTree()
        {
            if (EntitySystemWorld.Instance.Simulation && !EntitySystemWorld.Instance.SystemPauseOfSimulation)
            {
                AnimationTree tree = GetFirstAnimationTree();
                if (tree != null)
                {
                    bool move = false;
                    Degree moveAngle = 0;
                    float moveSpeed = 0;

                    if (GroundRelativeVelocity.ToVec2().LengthSqr() > .1f)
                    {
                        move = true;

                        Vec2 localVec = (Rotation.GetInverse() * GroundRelativeVelocity).ToVec2();
                        Radian angle = MathFunctions.ATan(localVec.Y, localVec.X);
                        moveAngle = angle.InDegrees();
                        moveSpeed = GroundRelativeVelocity.ToVec2().Length();
                    }

                    tree.SetParameterValue("move", move ? 1 : 0);
                    //tree.SetParameterValue("run", move && IsNeedRun() ? 1 : 0);
                    tree.SetParameterValue("moveAngle", moveAngle);
                    tree.SetParameterValue("moveSpeed", moveSpeed);
                    //tree.SetParameterValue("fly", !IsOnGround() ? 1 : 0);
                }
            }
        }

        //protected override void OnJump()
        //{
        //    base.OnJump();

        //    //play jump animation
        //    AnimationTree tree = GetFirstAnimationTree();
        //    if (tree != null)
        //        tree.ActivateTrigger("jump");
        //}
    }
}