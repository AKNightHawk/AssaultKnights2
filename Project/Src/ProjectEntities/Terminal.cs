// Copyright (C) NeoAxis Group Ltd. This is part of NeoAxis 3D Engine SDK.
using Engine.EntitySystem;
using Engine.MapSystem;

namespace ProjectEntities
{
    /// <summary>
    /// Defines the <see cref="Terminal"/> entity type.
    /// </summary>
    public class TerminalType : GameGuiObjectType
    {
    }

    public class Terminal : GameGuiObject
    {
        private TerminalType _type = null; public new TerminalType Type { get { return _type; } }

        protected override void OnPostCreate(bool loaded)
        {
            base.OnPostCreate(loaded);

            if (EntitySystemWorld.Instance.IsClientOnly())
            {
                MapObjectAttachedObject attachedObject = GetFirstAttachedObjectByAlias("clientNotSupported");
                if (attachedObject != null)
                    attachedObject.Visible = true;
            }
        }
    }
}