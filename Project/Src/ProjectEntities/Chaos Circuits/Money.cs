using System.ComponentModel;
using Engine.Utils;

namespace GameEntities
{
    /// <summary>
    /// Summary of New Money Type -- Not Implemented Fully. Money Pickups and Base for Money System
    /// </summary>
    public class MoneyType : ItemType
    {
        const float moneyValueDefault = 10.00f;
        [FieldSerialize]
        float moneyValue = moneyValueDefault;

        [DefaultValue(moneyValueDefault)]
        public float MoneyValue
        {
            get { return moneyValue; }
            set { moneyValue = value; }
        }
    }

    /// <summary>
    /// Summary of Money Alotment pickup Item
    /// </summary>
    public class Money : Item
    {
        private MoneyType _type = null; public new MoneyType Type { get { return this._type; } }

        protected override void OnPostCreate(bool loaded)
        {
            base.OnPostCreate(loaded);
        }

        protected override bool OnLoad(TextBlock block)
        {
            return base.OnLoad(block);
        }

        protected override void OnSave(TextBlock block)
        {
            base.OnSave(block);
        }

        protected override void OnTick()
        {
            base.OnTick();
        }

        protected override bool OnTake(Unit unit)
        {
            //need to add values for player to store money values so they can buy stuff
            bool taken;
            taken =  base.OnTake(unit);
            return taken;
        }
    }
}