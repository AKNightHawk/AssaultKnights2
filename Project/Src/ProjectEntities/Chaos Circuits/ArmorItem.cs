using System;
using System.ComponentModel;
using System.Drawing.Design;
using Engine.EntitySystem;
using Engine.MapSystem;
using Engine.MathEx;
using Engine.Renderer;
using Engine.Utils;
using GameCommon;


namespace GameEntities
{
    /// <summary>
    /// Defines the <see cref="ArmorItem"/> entity type.
    /// </summary>
    public class ArmorItemType : ItemType //, IDisposable
    {
        [FieldSerialize]
        string defaultParticleName;

        [FieldSerialize]
        Vec3 particleposition = Vec3.Zero;

        [FieldSerialize]
        float armor = 50f;

        [FieldSerialize]
        bool israndom;

        [DefaultValue(50.0f)]
        [Description("Armor Value of the item, do increments 50, 100, 150, 200, 300, 400, 500, 750, 1000, 2000, 2500, or negative values there of.")]
        public float Armor
        {
            get { return armor; }
            set { armor = value; }
        }

        [Description("Armor Value of the item is ignored, and random values between 0 and 200 are used on each respawn")]
        public bool RandomArmorValue
        {
            get { return israndom; }
            set { israndom = value; }
        }

        [Editor(typeof(EditorParticleUITypeEditor), typeof(UITypeEditor))]
        public string DefaultParticleName
        {
            get { return defaultParticleName; }
            set { defaultParticleName = value; }
        }

        [Editor(typeof(EditorParticleUITypeEditor), typeof(UITypeEditor))]
        public Vec3 ParticlePosition
        {
            get { return particleposition; }
            set { particleposition = value; }
        }

        //enum armorType
        //{
        //    Armor_50 = (int)50,
        //    Armor_100 = (int)100,
        //    Armor_150 = (int)150,
        //    Armor_200 = (int)200,
        //    Armor_250 = (int)250,
        //    Armor_300 = (int)300,
        //    Armor_400 = (int)400,
        //    Armor_500 = (int)500,
        //    Armor_750 = (int)750,
        //    Armor_1000 = (int)1000,
        //    Armor_2000 = (int)2000,
        //    Armor_Max = (int)2500,
        //    Armor_Maxm = (int)-2500,
        //    Armor_2000m = (int)-2000,
        //    Armor_1000m = (int)-1000,
        //    Armor_750m = (int)-750,
        //    Armor_500m = (int)-500,
        //    Armor_400m = (int)-400,
        //    Armor_300m = (int)-300,
        //    Armor_250m = (int)-250,
        //    Armor_200m = (int)-200,
        //    Armor_150m = (int)-150,
        //    Armor_100m = (int)-100,
        //    Armor_50m = (int)-50,
        //  }

    }

    /// <summary>
    /// Represents a item of the healths. When the player take this item his
    /// <see cref="Dynamic.Life"/> increase.
    /// </summary>
    [Serializable()]
    public class ArmorItem : Item
    {
 
        MapObjectAttachedParticle defaultAttachedParticle;
        ArmorItemType _type = null; public new ArmorItemType Type { get { return _type; } }
        private bool taken = false;

        protected override bool OnLoad(TextBlock block)
        {
            taken = false;
            if (!base.OnLoad(block))
                return false;

            if (this.Type.RandomArmorValue == true)
            {
                int randomarmor;

                randomarmor = World.Instance.Random.Next(0, 200) + 1;
                if (EngineConsole.Instance != null)
                {
                    string v = "Random Armoritem value: " + randomarmor.ToString();
                    EngineConsole.Instance.Print(v);
                }

                this.Type.Armor = randomarmor;
            }
            return true;
        }

        protected override void OnCreate()
        {
            taken = false;
            base.OnCreate();

            if (this.Type.RandomArmorValue == true)
            {
                int randomarmor;

                randomarmor = World.Instance.Random.Next(0, 200) + 1;

                //this.Type.Armor = cases;
                if (EngineConsole.Instance != null)
                {
                    string v = "Random Armoritem value: " + randomarmor.ToString();
                    EngineConsole.Instance.Print(v);
                }

                this.Type.Armor = randomarmor;
                
            }
        }

        protected override void OnPostCreate(bool loaded)
        {
            base.OnPostCreate(loaded);
            AddTimer();
            bool existsAttachedObjects = false;

            //show attached objects for this influence
            foreach (MapObjectAttachedObject attachedObject in this.AttachedObjects)
            {
                if (attachedObject.Alias == Type.Name)
                {
                    attachedObject.Visible = true;
                    existsAttachedObjects = true;
                }
            }

            if (!existsAttachedObjects)
            {
                //create default particle system
                if (!string.IsNullOrEmpty(Type.DefaultParticleName))
                {
                    defaultAttachedParticle = new MapObjectAttachedParticle();
                    defaultAttachedParticle.ParticleName = Type.DefaultParticleName;
                    this.Attach(defaultAttachedParticle);
                    defaultAttachedParticle.PositionOffset = this.Type.ParticlePosition;
                }
            }
        }

          /// <summary>Overridden from <see cref="Engine.EntitySystem.Entity.OnDestroy()"/>.</summary>
        protected override void OnDestroy()
        {
            //Dynamic parent = (Dynamic)Parent;

            //hide attached objects for this influence
            foreach (MapObjectAttachedObject attachedObject in this.AttachedObjects)
            {
                if (attachedObject.Alias == Type.Name)
                    attachedObject.Visible = false;
            }

            //destroy default particle system
            if (defaultAttachedParticle != null)
            {
                this.Detach(defaultAttachedParticle);
                defaultAttachedParticle = null;
            }

            base.OnDestroy();
        }

        /// <summary>Overridden from <see cref="Engine.EntitySystem.Entity"/>.</summary>
        protected override void OnTick()
        {
            base.OnTick();

            //remainingTime -= TickDelta;
            //remainingTime <= 0  ||
            if (taken == true)
            {
                //deletion of object can do only on server
                SetShouldDelete();
                return;
            }
        }

        protected override void OnRender(Camera camera)
        {
            base.OnRender(camera);
        }

        protected override bool OnTake(Unit unit)
        {
            bool take = base.OnTake(unit);
            float armorMax = unit.Type.ArmorMax;
            //float armornow = unit.Armor;
            taken = true;

            OnRender(RendererWorld.Instance.DefaultCamera);

            if (EngineConsole.Instance != null && Type.RandomArmorValue == true)
            {
                string v = "Random Armoritem value: " + Armor.ToString();
                EngineConsole.Instance.Print(v);
            }

            if (unit.Life > 0f && Type.Armor <= armorMax)
            {
                float armor = unit.Armor + Type.Armor;

                if (armor > armorMax)
                    armor = armorMax;
                else if (armor < 0f)
                    armor = 0;

                unit.Armor = armor;

                take = true;
            }

            return take;
        }
    }
}