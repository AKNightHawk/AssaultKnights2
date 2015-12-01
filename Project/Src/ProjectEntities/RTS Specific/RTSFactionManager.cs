// Copyright (C) NeoAxis Group Ltd. This is part of NeoAxis 3D Engine SDK.
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing.Design;
using Engine;
using Engine.MapSystem;
using Engine.Utils;

namespace ProjectEntities
{
    /// <summary>
    /// Defines the <see cref="RTSFactionManager"/> entity type.
    /// </summary>
    public class RTSFactionManagerType : MapGeneralObjectType
    {
        public RTSFactionManagerType()
        {
            UniqueEntityInstance = true;
            AllowEmptyName = true;
        }
    }

    public class RTSFactionManager : MapGeneralObject
    {
        private static RTSFactionManager instance;

        [FieldSerialize]
        private List<FactionItem> factions = new List<FactionItem>();

        ///////////////////////////////////////////

        public class FactionItem
        {
            [FieldSerialize]
            private FactionType factionType;

            [FieldSerialize]
            private float money;

            //

            public FactionType FactionType
            {
                get { return factionType; }
                set { factionType = value; }
            }

            [DefaultValue(0.0f)]
            public float Money
            {
                get { return money; }
                set { money = value; }
            }

            public override string ToString()
            {
                if (FactionType == null)
                    return "(not initialized)";
                return FactionType.FullName;
            }
        }

        ///////////////////////////////////////////

        private RTSFactionManagerType _type = null; public new RTSFactionManagerType Type { get { return _type; } }

        public static RTSFactionManager Instance
        {
            get { return instance; }
        }

        public RTSFactionManager()
        {
            if (instance != null)
                Log.Fatal("RTSFactionManager: instance != null");
            instance = this;
        }

        /// <summary>
        /// Don't modify
        /// </summary>
        [TypeConverter(typeof(CollectionTypeConverter))]
        [Editor("ProjectEntities.Editor.RTSFactionManager_FactionsCollectionEditor, ProjectEntities.Editor", typeof(UITypeEditor))]
        public List<FactionItem> Factions
        {
            get { return factions; }
        }

        /// <summary>Overridden from <see cref="Engine.EntitySystem.Entity.OnPostCreate(Boolean)"/>.</summary>
        protected override void OnPostCreate(bool loaded)
        {
            if (instance == this)//for undo support
                instance = this;

            base.OnPostCreate(loaded);
        }

        /// <summary>Overridden from <see cref="Engine.EntitySystem.Entity.OnDestroy()"/>.</summary>
        protected override void OnDestroy()
        {
            base.OnDestroy();

            if (instance == this)//for undo support
                instance = null;
        }

        public FactionItem GetFactionItemByType(FactionType type)
        {
            foreach (FactionItem item in factions)
                if (item.FactionType == type)
                    return item;
            return null;
        }
    }
}