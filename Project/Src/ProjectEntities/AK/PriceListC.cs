// Copyright (C) 2006-2009 NeoAxis Group Ltd.
using System.Collections.Generic;
using System.ComponentModel;
using Engine.EntitySystem;

namespace ProjectEntities
{
    public class PriceListCType : EntityType
    {
        [FieldSerialize]
        private List<pricelist> pricelists = new List<pricelist>();

        public class pricelist
        {
            [FieldSerialize]
            private UnitType Pricedunit;

            [Description("Unit that will be Priced")]
            public UnitType PricedUnit
            {
                get { return Pricedunit; }
                set { Pricedunit = value; }
            }

            [FieldSerialize]
            private int price;

            [Description("Price of the Selected Unit")]
            public int Price
            {
                get { return price; }
                set { price = value; }
            }

            [FieldSerialize]
            private string name;

            [Description("Name of the Selected Unit")]
            public string Name
            {
                get { return name; }
                set { name = value; }
            }
        }

        public List<pricelist> PriceLists
        {
            get { return pricelists; }
        }
    }

    public class PriceListC : Entity
    {
        private PriceListCType _type = null; public new PriceListCType Type { get { return _type; } }
    }
}