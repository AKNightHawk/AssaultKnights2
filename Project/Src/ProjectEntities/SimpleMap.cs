// Copyright (C) NeoAxis Group Ltd. This is part of NeoAxis 3D Engine SDK.
using Engine.EntitySystem;
using Engine.MapSystem;

namespace ProjectEntities
{
    /// <summary>
    /// Defines the <see cref="SimpleMap"/> entity type.
    /// Used for preview types in the Resource Editor.
    /// </summary>
    [AllowToCreateTypeBasedOnThisClass(false)]
    public class SimpleMapType : MapType
    {
    }

    public class SimpleMap : Map
    {
        private SimpleMapType _type = null; public new SimpleMapType Type { get { return _type; } }
    }
}