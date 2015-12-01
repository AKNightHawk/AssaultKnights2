// Copyright (C) NeoAxis Group Ltd. This is part of NeoAxis 3D Engine SDK.
using System.Collections.Generic;

namespace ProjectEntities.Editor
{
    public class DecalCreatorType_MaterialsCollectionEditor : ProjectEntitiesGeneralListCollectionEditor
    {
        public DecalCreatorType_MaterialsCollectionEditor()
            : base(typeof(List<DecalCreatorType.MaterialItem>))
        { }
    }
}