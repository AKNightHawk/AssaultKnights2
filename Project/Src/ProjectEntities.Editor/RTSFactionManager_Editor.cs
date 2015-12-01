// Copyright (C) NeoAxis Group Ltd. This is part of NeoAxis 3D Engine SDK.
using System.Collections.Generic;

namespace ProjectEntities.Editor
{
    public class RTSFactionManager_FactionsCollectionEditor : ProjectEntitiesGeneralListCollectionEditor
    {
        public RTSFactionManager_FactionsCollectionEditor()
            : base(typeof(List<RTSFactionManager.FactionItem>))
        { }
    }
}