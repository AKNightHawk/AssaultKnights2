// Copyright (C) NeoAxis Group Ltd. This is part of NeoAxis 3D Engine SDK.
using System;
using System.Diagnostics;

namespace ProjectEntities
{
    ////////////////////////////////////////////////////////////////////////////////////////////////

    public class BigDamageInfluenceType : InfluenceType
    {
        [FieldSerialize]
        private float coefficient;

        public float Coefficient
        {
            get { return coefficient; }
            set { coefficient = value; }
        }
    }

    public class BigDamageInfluence : Influence
    {
        private BigDamageInfluenceType _type = null; public new BigDamageInfluenceType Type { get { return _type; } }
    }

    ////////////////////////////////////////////////////////////////////////////////////////////////

    public class FastAttackInfluenceType : InfluenceType
    {
        [FieldSerialize]
        private float coefficient;

        public float Coefficient
        {
            get { return coefficient; }
            set { coefficient = value; }
        }
    }

    public class FastAttackInfluence : Influence
    {
        private FastAttackInfluenceType _type = null; public new FastAttackInfluenceType Type { get { return _type; } }
    }

    ////////////////////////////////////////////////////////////////////////////////////////////////

    public class FastMoveInfluenceType : InfluenceType
    {
        [FieldSerialize]
        private float coefficient;

        public float Coefficient
        {
            get { return coefficient; }
            set { coefficient = value; }
        }
    }

    public class FastMoveInfluence : Influence
    {
        private FastMoveInfluenceType _type = null; public new FastMoveInfluenceType Type { get { return _type; } }
    }

    //Incin -- Influence type
    public class FreezeMoveInfluenceType : InfluenceType
    {
        [FieldSerialize]
        private float timefrozen;

        public float TimeFrozen
        {
            get { return timefrozen; }
            set { timefrozen = value; }
        }

        [FieldSerialize]
        private float coefficient;

        public float Coefficient
        {
            get { return coefficient; }
            set { coefficient = value; }
        }
    }

    public class FreezeMoveInfluence : Influence
    {
        private FreezeMoveInfluenceType _type = null; public new FreezeMoveInfluenceType Type { get { return _type; } }
    }

    ////////////////////////////////////////////////////////////////////////////////////////////////

    public class FireInfluenceType : InfluenceType
    {
        [FieldSerialize]
        private float damagePerSecond;

        public float DamagePerSecond
        {
            get { return damagePerSecond; }
            set { damagePerSecond = value; }
        }
    }

    public class FireInfluence : Influence
    {
        private FireInfluenceType _type = null; public new FireInfluenceType Type { get { return _type; } }

        /// <summary>Overridden from <see cref="Engine.EntitySystem.Entity.OnPostCreate(Boolean)"/>.</summary>
        protected override void OnPostCreate(bool loaded)
        {
            base.OnPostCreate(loaded);
            SubscribeToTickEvent();
        }

        /// <summary>Overridden from <see cref="Engine.EntitySystem.Entity.OnTick()"/>.</summary>
        protected override void OnTick()
        {
            base.OnTick();

            Trace.Assert(Parent is Dynamic);
            Dynamic obj = (Dynamic)Parent;
            if (!obj.IsSetForDeletion)
                obj.DoDamage(obj, obj.Position, null, Type.DamagePerSecond * TickDelta, true);
        }
    }

    public class DamageInfluenceType : InfluenceType
    {
        [FieldSerialize]
        private float damageAmount;

        public float DamageAmount
        {
            get { return damageAmount; }
            set { damageAmount = value; }
        }
    }

    public class DamageInfluence : Influence // Incin
    {
        private DamageInfluenceType _type = null; public new DamageInfluenceType Type { get { return _type; } }

        protected override void OnPostCreate(bool loaded)
        {
            base.OnPostCreate(loaded);
            SubscribeToTickEvent();
        }

        /// <summary>Overridden from <see cref="Engine.EntitySystem.Entity.OnTick()"/>.</summary>
        protected override void OnTick()
        {
            base.OnTick();

            Trace.Assert(Parent is Dynamic);
            Dynamic obj = (Dynamic)Parent;
            if (!obj.IsSetForDeletion)
                obj.DoDamage(obj, obj.Position, null, Type.DamageAmount, true);
        }
    }

    ////////////////////////////////////////////////////////////////////////////////////////////////

    public class SmokeInfluenceType : InfluenceType
    {
    }

    public class SmokeInfluence : Influence
    {
        private SmokeInfluenceType _type = null; public new SmokeInfluenceType Type { get { return _type; } }
    }

    ////////////////////////////////////////////////////////////////////////////////////////////////
}