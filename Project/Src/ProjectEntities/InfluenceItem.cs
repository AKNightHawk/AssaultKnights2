// Copyright (C) NeoAxis Group Ltd. This is part of NeoAxis 3D Engine SDK.
using System;
using System.ComponentModel;
using Engine;

namespace ProjectEntities
{
    /// <summary>
    /// Defines the <see cref="InfluenceItem"/> entity type.
    /// </summary>
    public class InfluenceItemType : ItemType
    {
        [FieldSerialize]
        private InfluenceType influenceType;

        [FieldSerialize]
        private float influenceTime;

        public InfluenceType InfluenceType
        {
            get { return influenceType; }
            set { influenceType = value; }
        }

        [DefaultValue(0.0f)]
        public float InfluenceTime
        {
            get { return influenceTime; }
            set { influenceTime = value; }
        }
    }

    public class InfluenceItem : Item
    {
        private InfluenceItemType _type = null; public new InfluenceItemType Type { get { return _type; } }

        protected override bool OnTake(Unit unit)
        {
            base.OnTake(unit);

            if (Type.InfluenceType == null)
            {
                Log.Warning("InfluenceItem.OnTake: Type.InfluenceType == null");
                return false;
            }

            unit.AddInfluence(Type.InfluenceType, Type.InfluenceTime, true);
            return true;
        }
    }
}