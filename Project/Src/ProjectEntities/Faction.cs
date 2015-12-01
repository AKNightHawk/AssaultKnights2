// Copyright (C) NeoAxis Group Ltd. This is part of NeoAxis 3D Engine SDK.
using Engine.EntitySystem;

namespace ProjectEntities
{
    /// <summary>
    /// Defines the <see cref="Faction"/> entity type.
    /// </summary>
    public class FactionType : EntityType
    {
    }

    /// <summary>
    /// Concept of the command. Opponents with an artificial intelligences attack
    /// units of another's fraction.
    /// </summary>
    public class Faction : Entity
    {
        private FactionType _type = null; public new FactionType Type { get { return _type; } }
    }
}