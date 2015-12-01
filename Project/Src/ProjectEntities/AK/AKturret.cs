// Copyright (C) 2006-2009 NeoAxis Group Ltd.
using System;
using System.ComponentModel;
using Engine.MathEx;

namespace ProjectEntities
{
    /// <summary>
    /// Defines the <see cref="AKturret"/> entity type.
    /// </summary>
    public class AKturretType : AKunitType
    {
        [FieldSerialize]
        private Range optimalAttackDistanceRange;

        [DefaultValue(typeof(Range), "0 0")]
        public Range OptimalAttackDistanceRange
        {
            get { return optimalAttackDistanceRange; }
            set { optimalAttackDistanceRange = value; }
        }
    }

    /// <summary>
    /// Gives an opportunity of creation of the turrets.
    /// A turret can be rotated. Guns are attached on the tower and player can
    /// control the aiming and shooting of the turret.
    /// </summary>
    public class AKturret : AKunit
    {
        private AKturretType _type = null; public new AKturretType Type { get { return _type; } }

        /// <summary>Overridden from <see cref="Engine.EntitySystem.Entity.OnPostCreate(Boolean)"/>.</summary>
        protected override void OnPostCreate(bool loaded)
        {
            base.OnPostCreate(loaded);
            AddTimer();
        }

        private void AddTimer()
        {
            SubscribeToTickEvent();
        }

        /// <summary>Overridden from <see cref="Engine.EntitySystem.Entity.OnTick()"/>.</summary>
        protected override void OnTick()
        {
            base.OnTick();
        }
    }
}