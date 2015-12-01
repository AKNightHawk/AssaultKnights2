// Copyright (C) NeoAxis Group Ltd. This is part of NeoAxis 3D Engine SDK.

namespace ProjectEntities
{
    /// <summary>
    /// Defines the <see cref="AI"/> entity type.
    /// </summary>
    public abstract class AIType : IntellectType
    {
    }

    public abstract class AI : Intellect
    {
        private AIType _type = null; public new AIType Type { get { return _type; } }
    }
}