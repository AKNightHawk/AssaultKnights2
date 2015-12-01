// Copyright (C) NeoAxis Group Ltd. This is part of NeoAxis 3D Engine SDK.
using System.Collections.Generic;

namespace ProjectEntities.Editor
{
    public class DynamicType_AutomaticInfluencesCollectionEditor : ProjectEntitiesGeneralListCollectionEditor
    {
        public DynamicType_AutomaticInfluencesCollectionEditor()
            : base(typeof(List<DynamicType.AutomaticInfluenceItem>))
        { }
    }
}