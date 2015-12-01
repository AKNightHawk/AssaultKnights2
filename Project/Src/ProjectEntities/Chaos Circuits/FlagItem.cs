using System.ComponentModel;
using Engine.EntitySystem;
using Engine.MathEx;

namespace GameEntities
{
    /// <summary>
    /// Summary of New_NeoAxis_Entity_Type1Type
    /// </summary>
    public class FlagItemType : ItemType
    {

        [FieldSerialize]
        FactionType factionTeam;

        

        //[FieldSerialize]
        private bool taken;

        //[FieldSerialize]
        private bool dropped;

        //[FieldSerialize]
        private bool atrestingposition;



        public FactionType InitialFaction
        {
            get { return factionTeam; }
            set { factionTeam = value; }
        }

        [Browsable(false)]
        public bool Taken
        {
            get { return taken; }
            set { taken = value; DroppedItem = false; AtRest = false; }
        }

        [Browsable(false)]
        public bool DroppedItem
        {
            get { return dropped; }
            set { dropped = value; AtRest = false; Taken = false; }
        }

        [Browsable(false)]
        public bool AtRest
        {
            get { return atrestingposition; }
            set { atrestingposition = value; Taken = false; DroppedItem = false; }
        }
    }

    /// <summary>
    /// Summary of FlagItem Item
    /// </summary>
    public class FlagItem : Item
    {

        static Unit playerpickup = null;

        private FlagItemType _type = null; public new FlagItemType Type{ get { return this._type; }}
 
        /// <summary>
        ///     Overrides the corresponding method from the Item base type.
        /// </summary>
        protected override void OnSetTransform(ref Vec3 pos, ref Quat rot, ref Vec3 scl)
        {
            //TODO: Add your implementation before or after the following base method call
            base.OnSetTransform(ref pos, ref  rot, ref  scl);
        }

        /// <summary>
        ///     Overrides the corresponding method from the Item base type.
        /// </summary>
        protected override void Client_OnTick()
        {
            //TODO: Add your implementation before or after the following base method call
            base.Client_OnTick();
        }

        /// <summary>
        ///     Overrides the corresponding method from the Item base type.
        /// </summary>
        protected override void OnPostCreate(bool loaded)
        {
            //TODO: Add your implementation before or after the following base method call
            base.OnPostCreate(loaded);
        }

        /// <summary>
        ///     Overrides the corresponding method from the Item base type.
        /// </summary>
        protected override bool OnTake(Unit unit)
        {
            bool take;
            
            //player faction will be set by selecting a team in gui
            // flags have faction set in item class
            
            if (playerpickup != null) //player pickup should be null prior to picking up which would mean code error or bug
                return false;

            if (this.Type.InitialFaction != null || unit.InitialFaction != null) //not a CTF game don't allow pickup 
            {
                if (this.Type.InitialFaction == unit.InitialFaction)
                {
                    //can't Take own flag .. for now
                    return false;
                }
                else if (this.Type.InitialFaction != unit.InitialFaction)
                {
                    //TODO: Add your implementation before or after the following base method call
                    take = base.OnTake(unit);
                    this.Type.Taken = true;
                    
                    //add model of object on the unit at Alias position

                    playerpickup = unit; //statically save the player holding this item
                    return take = true;
                }
                else
                    return false;
            }
            else
                return false;
        }

        public bool DropFlagItem(Unit unit)
        {
            bool droppeditem;


            this.Type.DroppedItem = true;
            
            //remove flag from player 
            //remove unit form playerpickup
            droppeditem = true;

            playerpickup = null;

            return droppeditem;
        }

        /*
        public override bool Take(Unit unit)
        {
            bool ret = OnTake(unit);
            if (ret)
            {
                unit.SoundPlay3D(Type.SoundTake, .5f, true);

                if (EntitySystemWorld.Instance.IsServer())
                    Server_SendSoundPlayTakeToAllClients();

                //Die();
            }
            return ret;
        }
        */

        /// <summary>
        ///     Overrides the corresponding method from the Item base type.
        /// </summary>
        protected override void Server_OnClientConnectedBeforePostCreate(RemoteEntityWorld remoteEntityWorld)
        {
            //TODO: Add your implementation before or after the following base method call
            base.Server_OnClientConnectedBeforePostCreate(remoteEntityWorld);
        }

        /// <summary>
        ///     Overrides the corresponding method from the Item base type.
        /// </summary>
        protected override void OnCreate()
        {
            //TODO: Add your implementation before or after the following base method call
            base.OnCreate();

        }

        /// <summary>
        ///     Overrides the corresponding method from the Engine.MapSystem.MapObject base type.
        /// </summary>
        protected override void OnTick()
        {
            //TODO: Add your implementation before or after the following base method call
            //check flag states here taken dropped atrest positions, plus scoring.
            base.OnTick();

        }

        /// <summary>
        ///     Overrides the corresponding method from the Item base type.
        /// </summary>
        protected override void OnPreCreate()
        {
            //TODO: Add your implementation before or after the following base method call
            base.OnPreCreate();
        }

        /// <summary>
        ///     Overrides the corresponding method from the Dynamic base type.
        /// </summary>
        protected override bool OnShouldDelete()
        {
            bool boolValue;

            //TODO: Add your implementation before or after the following base method call
            return boolValue = base.OnShouldDelete();

            //return false;// flags never get removed from games
        }

    }
}