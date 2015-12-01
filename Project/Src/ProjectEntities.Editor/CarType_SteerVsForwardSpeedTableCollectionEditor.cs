// Copyright (C) NeoAxis Group Ltd. This is part of NeoAxis 3D Engine SDK.
using System.Collections.Generic;

namespace ProjectEntities.Editor
{
    public class CarType_SteerVsForwardSpeedTableCollectionEditor : ProjectEntitiesGeneralListCollectionEditor
    {
        public CarType_SteerVsForwardSpeedTableCollectionEditor()
            : base(typeof(List<CarType.SteerVsForwardSpeedTableItem>))
        { }
    }
}