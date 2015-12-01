// Copyright (C) NeoAxis Group Ltd. This is part of NeoAxis 3D Engine SDK.
using System;
using System.ComponentModel;
using Engine.EntitySystem;
using Engine.MapSystem;

namespace ProjectEntities
{
    /// <summary>
    /// Defines the <see cref="ItemCreator"/> entity type.
    /// </summary>
    public class ItemCreatorType : MapObjectType
    {
    }

    public class ItemCreator : MapObject
    {
        [FieldSerialize]
        private ItemType itemType;

        [FieldSerialize]
        private float createRemainingTime;

        [FieldSerialize]
        private float remainingTime;

        [FieldSerialize]
        private Item item;

        //

        private ItemCreatorType _type = null; public new ItemCreatorType Type { get { return _type; } }

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

            if (item == null && remainingTime == 0)
                remainingTime = createRemainingTime;

            if (remainingTime != 0)
            {
                remainingTime -= TickDelta;
                if (remainingTime <= 0)
                {
                    remainingTime = 0;

                    Item i = (Item)Entities.Instance.Create(itemType, Parent);
                    i.Position = Position;
                    i.PostCreate();
                    Item = i;
                }
            }
        }

        /// <summary>Overridden from <see cref="Engine.EntitySystem.Entity.OnDeleteSubscribedToDeletionEvent(Entity)"/></summary>
        protected override void OnDeleteSubscribedToDeletionEvent(Entity entity)
        {
            base.OnDeleteSubscribedToDeletionEvent(entity);
            if (item == entity)
                item = null;
        }

        public ItemType ItemType
        {
            get { return itemType; }
            set { itemType = value; }
        }

        public float CreateRemainingTime
        {
            get { return createRemainingTime; }
            set { createRemainingTime = value; }
        }

        [Browsable(false)]
        public Item Item
        {
            get { return item; }
            set
            {
                if (item != null)
                    UnsubscribeToDeletionEvent(item);
                item = value;
                if (item != null)
                    SubscribeToDeletionEvent(item);
            }
        }
    }
}