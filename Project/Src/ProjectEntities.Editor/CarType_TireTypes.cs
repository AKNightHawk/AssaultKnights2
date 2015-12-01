// Copyright (C) NeoAxis Group Ltd. This is part of NeoAxis 3D Engine SDK.
using System.Collections.Generic;

namespace ProjectEntities.Editor
{
    public class CarType_TireTypesCollectionEditor : ProjectEntitiesGeneralListCollectionEditor
    {
        public CarType_TireTypesCollectionEditor()
            : base(typeof(List<CarType.TireTypeItem>))
        { }
    }
}