using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing.Design;
using Engine;
using Engine.EntitySystem;
using Engine.FileSystem;
using Engine.MapSystem;
using Engine.MathEx;
using Engine.PhysicsSystem;
using Engine.Renderer;
using Engine.SoundSystem;
using Engine.Utils;
using ProjectCommon;

namespace ProjectEntities
{
    public class AKunitType : UnitType
    {
        [FieldSerialize]
        private bool useHeatVision = true;

        public bool UseHeatVision
        {
            get { return useHeatVision; }
            set { useHeatVision = value; }
        }

        [FieldSerialize]
        private string soundOn;

        [FieldSerialize]
        private string soundOff;

        [FieldSerialize]
        private string soundIdle;

        [Editor(typeof(EditorSoundUITypeEditor), typeof(UITypeEditor))]
        public string SoundOn
        {
            get { return soundOn; }
            set { soundOn = value; }
        }

        [Editor(typeof(EditorSoundUITypeEditor), typeof(UITypeEditor))]
        public string SoundOff
        {
            get { return soundOff; }
            set { soundOff = value; }
        }

        [Editor(typeof(EditorSoundUITypeEditor), typeof(UITypeEditor))]
        public string SoundIdle
        {
            get { return soundIdle; }
            set { soundIdle = value; }
        }

        public class VehicleEntrySoundCollection
        {
            [FieldSerialize]
            private string startupSequenceActivated;

            [Editor(typeof(EditorSoundUITypeEditor), typeof(UITypeEditor))]
            public string StartupSequenceActivated
            {
                get { return startupSequenceActivated; }
                set { startupSequenceActivated = value; }
            }

            [FieldSerialize]
            private string weaponSystemsActivated;

            [Editor(typeof(EditorSoundUITypeEditor), typeof(UITypeEditor))]
            public string WeaponSystemsActivated
            {
                get { return weaponSystemsActivated; }
                set { weaponSystemsActivated = value; }
            }

            [FieldSerialize]
            private string allSystemsNominal;

            [Editor(typeof(EditorSoundUITypeEditor), typeof(UITypeEditor))]
            public string AllSystemsNominal
            {
                get { return allSystemsNominal; }
                set { allSystemsNominal = value; }
            }
        }

        [FieldSerialize]
        private VehicleEntrySoundCollection vehicleEntrySounds = new VehicleEntrySoundCollection();

        [TypeConverter(typeof(ExpandableObjectConverter))]
        public VehicleEntrySoundCollection VehicleEntrySounds
        {
            get { return vehicleEntrySounds; }
            set { vehicleEntrySounds = value; }
        }

        [FieldSerialize]
        private int AkunitCoolantFlash = 1400;

        [Description("amount of heat witch is cooled on use of colantflash (F)Key")]
        [DefaultValue(1400)]
        public int AKunitCoolantFlash
        {
            get { return AkunitCoolantFlash; }
            set { AkunitCoolantFlash = value; }
        }

        [FieldSerialize]
        private int AkunitDefultHeatSink = 7;

        [Description("heat decrement on each Tick")]
        [DefaultValue(7)]
        public int AKunitDefultHeatSink
        {
            get { return AkunitDefultHeatSink; }
            set { AkunitDefultHeatSink = value; }
        }

        [FieldSerialize]
        private int AkunitHeatGenUpdateSpeed = 20;

        [Description("How fast gun heat transfers to Mech || higher value = slower transfer")]
        [DefaultValue(20)]
        public int AKunitHeatGenUpdateSpeed
        {
            get { return AkunitHeatGenUpdateSpeed; }
            set { AkunitHeatGenUpdateSpeed = value; }
        }

        [FieldSerialize]
        private int AkunitHeatMax = 1000;

        [Description("How high AKunit heat can go")]
        [DefaultValue(1000.0f)]
        public int AKunitHeatMax
        {
            get { return AkunitHeatMax; }
            set { AkunitHeatMax = value; }
        }

        [FieldSerialize]
        private int AkunitShutDownHeat = 800;

        [Description("How high AKunit heat can go till shutdown")]
        [DefaultValue(800.0f)]
        public int AKunitShutDownHeat
        {
            get { return AkunitShutDownHeat; }
            set { AkunitShutDownHeat = value; }
        }

        [FieldSerialize]
        private int AkunitExplodeHeat = 1600;

        [Description("How high AKunit heat can go till shutdown")]
        [DefaultValue(1600.0f)]
        public int AKunitExplodeHeat
        {
            get { return AkunitExplodeHeat; }
            set { AkunitExplodeHeat = value; }
        }

        [FieldSerialize]
        private string AkunitControlGui = "Assault Knights\\Huds\\AKunitHud.gui";

        //////////////////////////////////////////////////////

        [DefaultValue("Assault Knights\\Huds\\AKunitHud.gui")]
        public string AKunitControlGui
        {
            get { return AkunitControlGui; }
            set { AkunitControlGui = value; }
        }

        ///////////////////////////////////////////////////////////////

        [FieldSerialize]
        private Range torsoRotationAngleRange = new Range(-175, 175);

        [Description("In degrees.")]
        [DefaultValue(typeof(Range), "-175 175")]
        public Range TorsoRotationAngleRange
        {
            get { return torsoRotationAngleRange; }
            set { torsoRotationAngleRange = value; }
        }

        [FieldSerialize]
        private Range gunRotationAngleRange = new Range(-8, 40);

        [FieldSerialize]
        private Degree towerTurnSpeed = 60;

        [Description("In degrees.")]
        [DefaultValue(typeof(Range), "-8 40")]
        public Range GunRotationAngleRange
        {
            get { return gunRotationAngleRange; }
            set { gunRotationAngleRange = value; }
        }

        [Description("Degrees per second.")]
        [DefaultValue(typeof(Degree), "60")]
        public Degree TowerTurnSpeed
        {
            get { return towerTurnSpeed; }
            set { towerTurnSpeed = value; }
        }

        [FieldSerialize]
        private string soundTowerTurn;

        [Editor(typeof(EditorSoundUITypeEditor), typeof(UITypeEditor))]
        public string SoundTowerTurn
        {
            get { return soundTowerTurn; }
            set { soundTowerTurn = value; }
        }

        ///////////////////////////////////////////////////////////////

        [FieldSerialize]
        private float AkunitRadarDistanceMax = 500.0f;

        [Description("How far AKunit picks targets and Radar screen radius")]
        [DefaultValue(500.0f)]
        public float AKunitRadarDistanceMax
        {
            get { return AkunitRadarDistanceMax; }
            set { AkunitRadarDistanceMax = value; }
        }

        public class WeaponItem
        {
            [FieldSerialize]
            private string mapObjectAlias;

            [Description("Alias of the attached weapon")]
            public string MapObjectAlias
            {
                get { return mapObjectAlias; }
                set { mapObjectAlias = value; }
            }

            [FieldSerialize]
            private int magazineCapacity;

            [Description("How many times you can fire before recycle.")]
            public int MagazineCapacity
            {
                get { return magazineCapacity; }
                set { magazineCapacity = value; }
            }

            [FieldSerialize]
            private int ammo;

            [Description("Amount of ammunition carried for this weapon.")]
            public int Ammo
            {
                get { return ammo; }
                set { ammo = value; }
            }

            [FieldSerialize]
            private WeaponType weaponType;

            public WeaponType WeaponType
            {
                get { return weaponType; }
                set { weaponType = value; }
            }

            [FieldSerialize]
            private int fireGroup;

            [Description("Which group this weapon belongs to.")]
            public int FireGroup
            {
                get { return fireGroup; }
                set { fireGroup = value; }
            }

            [FieldSerialize]
            private List<AlternateWeaponItem> alternates = new List<AlternateWeaponItem>();

            public List<AlternateWeaponItem> Alternates
            {
                get { return alternates; }
                set { alternates = value; }
            }

            public override string ToString()
            {
                if (weaponType == null)
                    return "(not initialized)";
                return weaponType.Name;
            }
        }

        public class AlternateWeaponItem
        {
            [FieldSerialize]
            private int ammo;

            [FieldSerialize]
            private int magazineCapacity;

            [FieldSerialize]
            private WeaponType weaponType;

            [FieldSerialize]
            private int price;

            [Description("Amount of ammunition carried for this weapon.")]
            public int Ammo
            {
                get { return ammo; }
                set { ammo = value; }
            }

            [Description("How many times you can fire before recycle.")]
            public int MagazineCapacity
            {
                get { return magazineCapacity; }
                set { magazineCapacity = value; }
            }

            public WeaponType WeaponType
            {
                get { return weaponType; }
                set { weaponType = value; }
            }

            public int Price
            {
                get { return price; }
                set { price = value; }
            }

            public override string ToString()
            {
                if (weaponType == null)
                    return "Not Initialized";

                return string.Format("{0} ({1} AC)", weaponType.Name, price);
            }
        }

        [FieldSerialize]
        private string soundWeaponDestroyed;

        [Editor(typeof(EditorSoundUITypeEditor), typeof(UITypeEditor))]
        public string SoundWeaponDestroyed
        {
            get { return soundWeaponDestroyed; }
            set { soundWeaponDestroyed = value; }
        }

        [FieldSerialize]
        private List<BodyPart> bodyParts = new List<BodyPart>();

        public class BodyPart
        {
            [FieldSerialize]
            private string GUIdesplayName;

            [Description("name of the Part , to show in GUI under its bar")]
            public string GUIDesplayName
            {
                get { return GUIdesplayName; }
                set { GUIdesplayName = value; }
            }

            [FieldSerialize]
            private string physicsShape;

            [Description("name of the physics shape covering this bodypart like LLLBox.")]
            public string PhysicsShape
            {
                get { return physicsShape; }
                set { physicsShape = value; }
            }

            [FieldSerialize]
            private List<AKunitType.WeaponItem> weapons = new List<AKunitType.WeaponItem>();

            [FieldSerialize]
            private int hitPointsMax = 1000;

            [DefaultValue(1000)]
            public int HitPointsMax
            {
                get { return hitPointsMax; }
                set { hitPointsMax = value; }
            }

            //[Editor(typeof(WeaponsCollectionEditor), typeof(UITypeEditor))]
            public List<AKunitType.WeaponItem> Weapons
            {
                get { return weapons; }
            }

            /*
            [EditorBrowsable(EditorBrowsableState.Advanced)]
            public class WeaponsCollectionEditor : PropertyGridUtils
            {
                public WeaponsCollectionEditor()

                { }
            }
            */
        }

        public List<BodyPart> BodyParts
        {
            get { return bodyParts; }
        }

        protected override void OnPreloadResources()
        {
            base.OnPreloadResources();

            if (!string.IsNullOrEmpty(SoundWeaponDestroyed))
                SoundWorld.Instance.SoundCreate(SoundWeaponDestroyed, SoundMode.Mode3D);
        }

        [FieldSerialize]
        private string heatLevelCriticalSound;

        [Editor(typeof(EditorSoundUITypeEditor), typeof(UITypeEditor))]
        public string HeatLevelCriticalSound
        {
            get { return heatLevelCriticalSound; }
            set { heatLevelCriticalSound = value; }
        }

        [FieldSerialize]
        private string shutdownSound;

        [Editor(typeof(EditorSoundUITypeEditor), typeof(UITypeEditor))]
        public string ShutdownSound
        {
            get { return shutdownSound; }
            set { shutdownSound = value; }
        }
    }

    public class AKunit : Unit
    {
        private bool entrySoundsPlayed = false;
        private bool entrySounds2Played = false;
        private bool entrySounds3Played = false;

        //public bool EntrySoundsPlayed
        //{
        //    get { return entrySoundsPlayed; }
        //    set { entrySoundsPlayed = value; }
        //}

        [FieldSerialize]
        public bool MechShutDown;

        public int AKunitHeat;

        public List<BP> bp = new List<BP>();

        public List<BP> Bp
        {
            get { return bp; }
            set { bp = value; }
        }

        public class BP
        {
            private bool damaged;

            public bool Damaged
            {
                get { return damaged; }
                set { damaged = value; }
            }

            private float hitPoints;

            public float HitPoints
            {
                get { return hitPoints; }
                set { hitPoints = value; }
            }

            private string physicsShape;

            public string PhysicsShape
            {
                get { return physicsShape; }
                set { physicsShape = value; }
            }

            private string GUIdesplayName;

            public string GUIDesplayName
            {
                get { return GUIdesplayName; }
                set { GUIdesplayName = value; }
            }

            private List<AKunitType.WeaponItem> weapons = new List<AKunitType.WeaponItem>();

            private int hitpointsMax = 1000;

            public int HitpointsMax
            {
                get { return hitpointsMax; }
                set { hitpointsMax = value; }
            }

            //[Editor(typeof(WeaponsCollectionEditor), typeof(UITypeEditor))]
            public List<AKunitType.WeaponItem> Weapons
            {
                get { return weapons; }
                set { weapons = value; }
            }
        }

        public class WeaponItem
        {
            private int fireGroup;

            public int FireGroup
            {
                get { return fireGroup; }
                set { fireGroup = value; }
            }

            private Weapon weapon;

            public Weapon Weapon
            {
                get { return weapon; }
                set { weapon = value; }
            }

            private int ammoCapacity;

            public int AmmoCapacity
            {
                get { return ammoCapacity; }
                set { ammoCapacity = value; }
            }

            private int magazineCapacity;

            public int MagazineCapacity
            {
                get { return magazineCapacity; }
                set { magazineCapacity = value; }
            }

            private int currentAmmoCount;

            public int CurrentAmmoCount
            {
                get { return currentAmmoCount; }
                set { currentAmmoCount = value; }
            }

            private int currentMagazineCount;

            public int CurrentMagazineCount
            {
                get { return currentMagazineCount; }
                set { currentMagazineCount = value; }
            }

            private MapObjectAttachedMapObject mapObject;

            public MapObjectAttachedMapObject AttachedObject
            {
                get { return mapObject; }
                set { mapObject = value; }
            }
        }

        //cockpit location helper
        private MapObjectAttachedHelper cockpitLocation;

        [Browsable(false)]
        public MapObjectAttachedHelper CockpitLocation
        {
            get { return cockpitLocation; }
        }

        private MapObjectAttachedHelper camforward;

        [Browsable(false)]
        public MapObjectAttachedHelper CamForward
        {
            get { return camforward; }
        }

        private MapObjectAttachedHelper cambackward;

        [Browsable(false)]
        public MapObjectAttachedHelper CamBackward
        {
            get { return cambackward; }
        }

        private MapObjectAttachedHelper camleft;

        [Browsable(false)]
        public MapObjectAttachedHelper CamLeft
        {
            get { return camleft; }
        }

        private MapObjectAttachedHelper camright;

        [Browsable(false)]
        public MapObjectAttachedHelper CamRight
        {
            get { return camright; }
        }

        protected MapObjectAttachedMapObject mainGunAttachedObject;
        private Gun mainGun;
        private Vec3 mainGunOffsetPosition;

        [Browsable(false)]
        public Gun MainGun
        {
            get { return mainGun; }
            set
            {
                mainGun = value;

                foreach (AKunit.WeaponItem item in weapons)
                {
                    if (item.Weapon == (Weapon)value)
                    {
                        mainGunAttachedObject = item.AttachedObject;
                        mainGunOffsetPosition = mainGunAttachedObject.PositionOffset;
                    }
                }
            }
        }

        public Vec3 towerBodyLocalPosition;
        public Vec3 lookto;
        public Body towerBody;

        public Body TowerBody
        {
            get { return towerBody; }
        }

        public SphereDir towerLocalDirection;
        public SphereDir needTowerLocalDirection;
        private VirtualChannel towerTurnChannel;

        /////////////////Networking/////////////////

        private SphereDir server_sentTowerLocalDirection;
        public float tracksSpeed;
        private float server_sendTracksSpeed;
        private bool Server_colantflush;
        private bool Server_MechShutDown;

        ///////////////////////////////////////////

        public FireModes currentFireMode;

        [Browsable(false)]
        public FireModes CurrentFireMode
        {
            get { return currentFireMode; }
            //set { currentFireMode = value; }
        }

        //largest firegroup
        public int maxFireGroup;

        //FireModes
        public enum FireModes
        {
            Link,
            Group,
            Alpha,
            Count,
        }

        //current firegroup
        public int currentFireGroup;

        [Browsable(false)]
        public int CurrentFireGroup
        {
            get { return currentFireGroup; }
            //set { currentFireGroup = value; }
        }

        //the unit mech was aiming last tick...If different target this Tick start counting locking from 0
        private Unit lastTickTarget;

        private bool playedHeatLevelCriticalSound = false;
        private bool playedShutdownSound = false;

        [Browsable(false)]
        public Unit LastTickTarget
        {
            get { return lastTickTarget; }
            set { lastTickTarget = value; }
        }

        private List<Unit> visibleUnits;

        [Browsable(false)]
        public List<Unit> VisibleUnits
        {
            get { return visibleUnits; }
            set { visibleUnits = value; }
        }

        //the unit mech is aiming at
        private Unit currentTarget;

        private Unit reticuleTarget;

        [Browsable(false)]
        public Unit CurrentReticuleTarget
        {
            get { return reticuleTarget; }
            set { reticuleTarget = value; }
        }

        [Browsable(false)]
        public Unit CurrentMissileTarget
        {
            get { return currentTarget; }
            set { currentTarget = value; }
        }

        //Measured time how long we have been targeting...for missile lock
        private float missileLockCounter;

        [Browsable(false)]
        public float MissileLockCounter
        {
            get { return missileLockCounter; }
            set { missileLockCounter = value; }
        }

        public List<WeaponItem> weapons = new List<WeaponItem>();

        [Browsable(false)]
        public List<WeaponItem> Weapons
        {
            get { return weapons; }
            set { weapons = value; }
        }

        protected List<MapObjectAttachedLight> lights = new List<MapObjectAttachedLight>();
        private bool lightsOn = false;

        private AKunitType _type = null; public new AKunitType Type { get { return _type; } }

        protected override void OnPostCreate(bool loaded)
        {
            base.OnPostCreate(loaded);
            currentFireMode = FireModes.Link;

            towerBody = PhysicsModel.GetBody("CT");
            if (towerBody == null)
            {
                Log.Error("AKunit: \"CT\" body does not exist.");
                return;
            }

            foreach (AKunitType.BodyPart part in Type.BodyParts)
            {
                foreach (AKunitType.WeaponItem weapon in part.Weapons)
                {
                    WeaponItem item = new WeaponItem();

                    //how much weapon can hold ammo
                    item.AmmoCapacity = weapon.Ammo;
                    //How much ammo is left
                    item.CurrentAmmoCount = item.AmmoCapacity - weapon.MagazineCapacity;
                    //magazine capacity
                    item.MagazineCapacity = weapon.MagazineCapacity;
                    //how much ammo in the magazine
                    item.CurrentMagazineCount = weapon.MagazineCapacity;
                    //firegrouping for groupfire
                    item.FireGroup = weapon.FireGroup;
                    //weapons class
                    MapObjectAttachedMapObject attachedMapObject = GetFirstAttachedObjectByAlias(weapon.MapObjectAlias) as MapObjectAttachedMapObject;

                    Gun gun = attachedMapObject.MapObject as Gun;
                    if (gun != null)
                    {
                        item.AttachedObject = attachedMapObject;
                        gun.AddBullets(gun.Type.NormalMode.BulletType, weapon.Ammo);
                        item.Weapon = gun;
                    }

                    if (weapon.FireGroup > maxFireGroup) maxFireGroup = weapon.FireGroup;

                    this.weapons.Add(item);
                }

                BP IDp = new BP();

                IDp.HitPoints = part.HitPointsMax;
                IDp.PhysicsShape = part.PhysicsShape;
                IDp.GUIDesplayName = part.GUIDesplayName;
                IDp.HitpointsMax = part.HitPointsMax;
                //IDp.Weapons = part.Weapons;
                foreach (AKunitType.WeaponItem item in part.Weapons)
                {
                    AKunitType.WeaponItem wi = new AKunitType.WeaponItem();
                    wi.Ammo = item.Ammo;
                    wi.FireGroup = item.FireGroup;
                    wi.MagazineCapacity = item.MagazineCapacity;
                    wi.MapObjectAlias = item.MapObjectAlias;
                    wi.WeaponType = item.WeaponType;

                    foreach (AKunitType.AlternateWeaponItem altWeapon in item.Alternates)
                    {
                        AKunitType.AlternateWeaponItem awi = new AKunitType.AlternateWeaponItem();
                        awi.Ammo = altWeapon.Ammo;
                        awi.MagazineCapacity = altWeapon.MagazineCapacity;
                        awi.Price = altWeapon.Price;
                        awi.WeaponType = altWeapon.WeaponType;
                        wi.Alternates.Add(awi);
                    }

                    IDp.Weapons.Add(wi);
                }

                this.bp.Add(IDp);
            }

            foreach (MapObjectAttachedObject attachedObject in AttachedObjects)
            {
                MapObjectAttachedMapObject attachedMapObject = attachedObject as MapObjectAttachedMapObject;
                if (attachedMapObject == null)
                    continue;

                mainGun = attachedMapObject.MapObject as Gun;
                if (mainGun != null)
                {
                    mainGunAttachedObject = attachedMapObject;
                    mainGunOffsetPosition = attachedMapObject.PositionOffset;
                    break;
                }
            }
            // cockpit

            cockpitLocation = GetFirstAttachedObjectByAlias("cockpit") as MapObjectAttachedHelper;

            //Camera Views
            cambackward = GetFirstAttachedObjectByAlias("CamBack") as MapObjectAttachedHelper;

            //if(cambackward != null)
            //    cambackward.RotationOffset = new Angles(0, 0, 180f).ToQuat();

            camforward = GetFirstAttachedObjectByAlias("CamForward") as MapObjectAttachedHelper;

            camleft = GetFirstAttachedObjectByAlias("CamLeft") as MapObjectAttachedHelper;
            //if(camleft != null)
            //    camleft.RotationOffset = new Angles(0, 0, 270f).ToQuat();

            camright = GetFirstAttachedObjectByAlias("CamRight") as MapObjectAttachedHelper;
            //if(camright != null)
            //    camright.RotationOffset = new Angles(0, 0, 90f).ToQuat();

            //towerBodyLocalPosition
            if (towerBody != null)
                towerBodyLocalPosition = PhysicsModel.ModelDeclaration.GetBody(towerBody.Name).Position;
            if (loaded && EntitySystemWorld.Instance.WorldSimulationType != WorldSimulationTypes.Editor)
            {
                if (towerBody != null)
                    towerBody.Static = true;
            }

            if (!EntitySystemWorld.Instance.IsEditor())
            {
                foreach (MapObjectAttachedObject ao in AttachedObjects)
                {
                    MapObjectAttachedLight light = ao as MapObjectAttachedLight;

                    if (light != null)
                    {
                        light.Visible = false;
                        lights.Add(light);
                    }
                }
            }
        }

        //public MapObjectAttachedMapObject GetFirstAttachedObjectByAlias(string p)
        //{
        //    return Engine.MapSystem..MapObjectAttachedMapObject(p);
        //    //throw new NotImplementedException();
        //}

        protected override void OnSuspendPhysicsDuringMapLoading(bool suspend)
        {
            base.OnSuspendPhysicsDuringMapLoading(suspend);
            towerBody.Static = false;
        }

        //for use with the tech lab. should not be used for networked environment
        public void SetVariant(TextBlock varFile)
        {
            string clientData = string.Empty;
            for (int i = 0; i < bp.Count; i++)
            {
                TextBlock bodyPartBlock = varFile.FindChild(bp[i].GUIDesplayName);

                if (bodyPartBlock != null)
                {
                    BP bodyPart = bp[i];

                    for (int j = 0; j < bodyPart.Weapons.Count; j++)
                    {
                        TextBlock bodyPartWeaponBlock =
                            bodyPartBlock.FindChild(bodyPart.Weapons[j].MapObjectAlias);

                        if (bodyPartWeaponBlock != null)
                        {
                            AKunitType.WeaponItem wi = bodyPart.Weapons[j];

                            int altIndex = int.Parse(bodyPartWeaponBlock.Attributes[0].Value);
                            int fireGroup = int.Parse(bodyPartWeaponBlock.Attributes[1].Value);

                            if (fireGroup > maxFireGroup) maxFireGroup = fireGroup;

                            AKunitType.AlternateWeaponItem awi = wi.Alternates[altIndex];

                            MapObjectAttachedObject o = GetFirstAttachedObjectByAlias(wi.MapObjectAlias);
                            MapObjectAttachedMapObject mo = o as MapObjectAttachedMapObject;

                            Detach(mo);

                            Weapon weapon = Entities.Instance.Create(awi.WeaponType, Map.Instance) as Weapon;
                            weapon.PostCreate();
                            MapObjectAttachedMapObject amo = new MapObjectAttachedMapObject();
                            amo.MapObject = weapon;
                            amo.PositionOffset = mo.PositionOffset;
                            amo.Alias = mo.Alias;
                            amo.Body = mo.Body;
                            amo.BoneSlot = mo.BoneSlot;
                            amo.RotationOffset = mo.RotationOffset;
                            amo.ScaleOffset = mo.ScaleOffset;

                            Attach(amo);

                            //set the weapon info
                            Bp[i].Weapons[j].Ammo = awi.Ammo; ;
                            Bp[i].Weapons[j].MagazineCapacity = awi.MagazineCapacity;
                            Bp[i].Weapons[j].WeaponType = weapon.Type;
                            Bp[i].Weapons[j].FireGroup = fireGroup;

                            //set weapons
                            int index = i + j;
                            WeaponItem item = weapons[index];
                            //how much weapon can hold ammo
                            item.AmmoCapacity = awi.Ammo;
                            //How much ammo is left
                            item.CurrentAmmoCount = item.AmmoCapacity - awi.MagazineCapacity;
                            //magazine capacity
                            item.MagazineCapacity = awi.MagazineCapacity;
                            //how much ammo in the magazine
                            item.CurrentMagazineCount = awi.MagazineCapacity;
                            //weapons class
                            item.FireGroup = fireGroup;

                            Gun gun = amo.MapObject as Gun;
                            if (gun != null)
                            {
                                item.AttachedObject = amo;
                                gun.AddBullets(gun.Type.NormalMode.BulletType, awi.Ammo);
                                item.Weapon = gun;
                            }
                        }
                    }
                }
            }
        }

        /*public void Server_SetVariant(TextBlock varFile)
        {
            string clientData = string.Empty;
            for (int i = 0; i < bp.Count; i++)
            {
                TextBlock bodyPartBlock = varFile.FindChild(bp[i].GUIDesplayName);

                if (bodyPartBlock != null)
                {
                    BP bodyPart = bp[i];

                    for (int j = 0; j < bodyPart.Weapons.Count; j++)
                    {
                        TextBlock bodyPartWeaponBlock =
                            bodyPartBlock.FindChild(bodyPart.Weapons[j].MapObjectAlias);

                        if (bodyPartWeaponBlock != null)
                        {
                            AKunitType.WeaponItem wi = bodyPart.Weapons[j];

                            int altIndex = int.Parse(bodyPartWeaponBlock.Attributes[0].Value);

                            AKunitType.AlternateWeaponItem awi = wi.Alternates[altIndex];

                            MapObjectAttachedObject o = GetFirstAttachedObjectByAlias(wi.MapObjectAlias);
                            MapObjectAttachedMapObject mo = o as MapObjectAttachedMapObject;

                            Detach(mo);

                            Weapon weapon = Entities.Instance.Create(awi.WeaponType, Map.Instance) as Weapon;
                            weapon.PostCreate();
                            MapObjectAttachedMapObject amo = new MapObjectAttachedMapObject();
                            amo.MapObject = weapon;
                            amo.PositionOffset = mo.PositionOffset;
                            amo.Alias = mo.Alias;
                            amo.Body = mo.Body;
                            amo.BoneSlot = mo.BoneSlot;
                            amo.RotationOffset = mo.RotationOffset;
                            amo.ScaleOffset = mo.ScaleOffset;

                            Attach(amo);

                            //set the weapon info
                            Bp[i].Weapons[j].Ammo = awi.Ammo; ;
                            Bp[i].Weapons[j].MagazineCapacity = awi.MagazineCapacity;
                            Bp[i].Weapons[j].WeaponType = weapon.Type;

                            //set weapons
                            int index = i + j;
                            WeaponItem item = weapons[index];
                            //how much weapon can hold ammo
                            item.AmmoCapacity = awi.Ammo;
                            //How much ammo is left
                            item.CurrentAmmoCount = item.AmmoCapacity - awi.MagazineCapacity;
                            //magazine capacity
                            item.MagazineCapacity = awi.MagazineCapacity;
                            //how much ammo in the magazine
                            item.CurrentMagazineCount = awi.MagazineCapacity;
                            //weapons class

                            Gun gun = amo.MapObject as Gun;
                            if (gun != null)
                            {
                                item.AttachedObject = amo;
                                gun.AddBullets(gun.Type.NormalMode.BulletType, awi.Ammo);
                                item.Weapon = gun;
                            }

                            if (clientData != string.Empty)
                                clientData += ":";

                            clientData += string.Format("{0}:{1}:{2}:{3}", i, j,
                                altIndex, weapon.NetworkUIN);
                        }
                    }
                }
            }

            if (EntitySystemWorld.Instance.IsServer())
                Server_SendVariantToAllClients(EntitySystemWorld.Instance.RemoteEntityWorlds,
                    clientData);
        }*/

        public void Server_SetVariant(int[] varFile)
        {
            string clientData = string.Empty;

            for (int i = 0; i < varFile.Length; i += 4)
            {
                int bodyPartIndex = varFile[i];
                int weaponIndex = varFile[i + 1];
                int altWeaponIndex = varFile[i + 2];
                int fireGroup = varFile[i + 3];

                if (fireGroup > maxFireGroup) maxFireGroup = fireGroup;

                int index = 0;

                //count to where we should be in the weaponslist
                for (int a = 0; a < bodyPartIndex; a++)
                    index += Bp[a].Weapons.Count;

                index += (weaponIndex);

                AKunitType.WeaponItem wi = Bp[bodyPartIndex].Weapons[weaponIndex];
                AKunitType.AlternateWeaponItem awi = wi.Alternates[altWeaponIndex];

                WeaponItem item = weapons[index];

                MapObjectAttachedMapObject mo = item.AttachedObject;

                Weapon weapon = Entities.Instance.Create(awi.WeaponType, Map.Instance) as Weapon;
                weapon.PostCreate();

                MapObjectAttachedMapObject amo = new MapObjectAttachedMapObject();
                amo.MapObject = weapon;
                amo.PositionOffset = mo.PositionOffset;
                amo.Alias = mo.Alias;
                amo.Body = mo.Body;
                amo.BoneSlot = mo.BoneSlot;
                amo.RotationOffset = mo.RotationOffset;
                amo.ScaleOffset = mo.ScaleOffset;

                Detach(mo);
                Attach(amo);

                //set the weapon info
                Bp[bodyPartIndex].Weapons[weaponIndex].Ammo = awi.Ammo;
                Bp[bodyPartIndex].Weapons[weaponIndex].MagazineCapacity = awi.MagazineCapacity;
                Bp[bodyPartIndex].Weapons[weaponIndex].WeaponType = weapon.Type;
                Bp[bodyPartIndex].Weapons[weaponIndex].FireGroup = fireGroup;

                //set weapons

                //how much weapon can hold ammo
                item.AmmoCapacity = awi.Ammo;
                //How much ammo is left
                item.CurrentAmmoCount = item.AmmoCapacity - awi.MagazineCapacity;
                //magazine capacity
                item.MagazineCapacity = awi.MagazineCapacity;
                //how much ammo in the magazine
                item.CurrentMagazineCount = awi.MagazineCapacity;
                //weapons class
                item.FireGroup = fireGroup;

                Gun gun = amo.MapObject as Gun;
                if (gun != null)
                {
                    item.AttachedObject = amo;
                    gun.AddBullets(gun.Type.NormalMode.BulletType, awi.Ammo);
                    item.Weapon = gun;
                }

                if (clientData != string.Empty)
                    clientData += ":";

                clientData += string.Format("{0}:{1}:{2}:{3}:{4}", bodyPartIndex, weaponIndex,
                    altWeaponIndex, weapon.NetworkUIN, fireGroup);
            }

            if (EntitySystemWorld.Instance.IsServer())
                Server_SendVariantToAllClients(EntitySystemWorld.Instance.RemoteEntityWorlds,
                    clientData);
        }

        public void Client_SetVariant(int[] varFile)
        {
            for (int i = 0; i < varFile.Length; i += 5)
            {
                int bodyPartIndex = varFile[i];
                int weaponIndex = varFile[i + 1];
                int altWeaponIndex = varFile[i + 2];
                uint alternateUIN = (uint)varFile[i + 3];
                int fireGroup = varFile[i + 4];

                if (fireGroup > maxFireGroup) maxFireGroup = fireGroup;

                int index = 0;

                //count to where we should be in the weaponslist
                for (int a = 0; a < bodyPartIndex; a++)
                    index += Bp[a].Weapons.Count;

                index += (weaponIndex);

                AKunitType.WeaponItem wi = Bp[bodyPartIndex].Weapons[weaponIndex];
                AKunitType.AlternateWeaponItem awi = wi.Alternates[altWeaponIndex];
                WeaponItem item = weapons[index];

                MapObjectAttachedMapObject mo = item.AttachedObject;

                Weapon weapon = Entities.Instance.GetByNetworkUIN(alternateUIN) as Weapon;
                MapObjectAttachedMapObject amo = new MapObjectAttachedMapObject();
                amo.MapObject = weapon;
                amo.PositionOffset = mo.PositionOffset;
                amo.Alias = mo.Alias;
                amo.Body = mo.Body;
                amo.BoneSlot = mo.BoneSlot;
                amo.RotationOffset = mo.RotationOffset;
                amo.ScaleOffset = mo.ScaleOffset;

                Detach(item.AttachedObject);
                Attach(amo);

                //set the weapon info
                Bp[bodyPartIndex].Weapons[weaponIndex].Ammo = awi.Ammo; ;
                Bp[bodyPartIndex].Weapons[weaponIndex].MagazineCapacity = awi.MagazineCapacity;
                Bp[bodyPartIndex].Weapons[weaponIndex].WeaponType = weapon.Type;
                Bp[bodyPartIndex].Weapons[weaponIndex].FireGroup = fireGroup;

                //set weapons

                //how much weapon can hold ammo
                item.AmmoCapacity = awi.Ammo;
                //How much ammo is left
                item.CurrentAmmoCount = item.AmmoCapacity - awi.MagazineCapacity;
                //magazine capacity
                item.MagazineCapacity = awi.MagazineCapacity;
                //how much ammo in the magazine
                item.CurrentMagazineCount = awi.MagazineCapacity;
                //weapons class
                item.FireGroup = fireGroup;

                Gun gun = amo.MapObject as Gun;
                if (gun != null)
                {
                    item.AttachedObject = amo;
                    gun.AddBullets(gun.Type.NormalMode.BulletType, awi.Ammo);
                    item.Weapon = gun;
                }
            }
        }

        /*public void SetWeaponsFromVariant(TextBlock varFile)
        {
            //foreach (TextBlock bodyPartBlock in varFile.Children)
            for (int b = 0; b < varFile.Children.Count; b++)
            {
                TextBlock bodyPartBlock = varFile.Children[b];

                for (int w = 0; w < bodyPartBlock.Children.Count; w++)
                {
                    TextBlock weaponBlock = bodyPartBlock.Children[w];
                    //change attached mesh
                    string weaponAlias = weaponBlock.Name;
                    string weaponType = weaponBlock.Attributes[2].Value;

                    MapObjectAttachedObject o = GetFirstAttachedObjectByAlias(weaponAlias);
                    MapObjectAttachedMapObject mo = o as MapObjectAttachedMapObject;

                    Weapon weap = mo.MapObject as Weapon;

                    //only change what we have to
                    if (weap.Type.Name != weaponType)
                    {
                        Detach(mo);

                        Weapon weapon = Entities.Instance.Create(weaponType, Map.Instance) as Weapon;
                        weapon.PostCreate();
                        MapObjectAttachedMapObject amo = new MapObjectAttachedMapObject();
                        amo.MapObject = weapon;
                        amo.PositionOffset = mo.PositionOffset;
                        amo.Alias = mo.Alias;
                        amo.Body = mo.Body;
                        amo.BoneSlot = mo.BoneSlot;
                        amo.RotationOffset = mo.RotationOffset;
                        amo.ScaleOffset = mo.ScaleOffset;

                        Attach(amo);

                        //set the weapon info
                        int ammo = int.Parse(weaponBlock.Attributes[0].Value);
                        int magazineCapacity = int.Parse(weaponBlock.Attributes[1].Value);

                        Bp[b].Weapons[w].Ammo = ammo;
                        Bp[b].Weapons[w].MagazineCapacity = magazineCapacity;
                        Bp[b].Weapons[w].WeaponType = weapon.Type;

                        //set weapons
                        int index = b + w;
                        WeaponItem item = weapons[index];
                        //how much weapon can hold ammo
                        item.AmmoCapacity = ammo;
                        //How much ammo is left
                        item.CurrentAmmoCount = item.AmmoCapacity - magazineCapacity;
                        //magazine capacity
                        item.MagazineCapacity = magazineCapacity;
                        //how much ammo in the magazine
                        item.CurrentMagazineCount = magazineCapacity;
                        //weapons class

                        Gun gun = amo.MapObject as Gun;
                        if (gun != null)
                        {
                            item.AttachedObject = amo;
                            gun.AddBullets(gun.Type.NormalMode.BulletType, ammo);
                            item.Weapon = gun;
                        }
                    }
                }
            }
        }*/

        protected VirtualChannel soundOnChannel;

        protected override void OnTick()
        {
            base.OnTick();

            CalculateTracksSpeed();
            if (EntitySystemWorld.Instance.IsServer())
                Server_SendTracksSpeedToClients(EntitySystemWorld.Instance.RemoteEntityWorlds);

            if (towerBody != null)
            {
                towerBody.Static = false;
            }

            ////check outside Map position
            //Bounds checkBounds = Map.Instance.InitialCollisionBounds;
            //checkBounds.Expand(new Vec3(300, 300, 10000));
            //if (!checkBounds.IsContainsPoint(Position))
            //    SetDeleted();

            if (Intellect != null)
            {
                if (Intellect.IsControlKeyPressed(GameControlKeys.Fire1))
                {
                    GunsTryFire(false);
                }
                else if (Intellect.IsControlKeyPressed(GameControlKeys.Fire2))
                {
                    GunsTryFire(true);
                }
            }

            if (soundOnChannel != null)
            {
                if (soundOnChannel.IsStopped())
                {
                    if (!entrySoundsPlayed || !entrySounds2Played || !entrySounds3Played)
                    {
                        EntrySoundsOnTick();
                    }
                }
            }

            //else
            //{
            //    if (soundOnChannel.IsTotalPaused() && (!entrySoundsPlayed || !entrySounds2Played || !entrySounds3Played))
            //    {
            //    }
            //}

            BodyPartManagment();
            HeatManagment();
            GUIManagment();

            PlayHeatSounds();

            Damnweapons(false, false);

            if (Intellect != null)
            {
                TickIntellect();
            }

            TickTowerTurn();
            UpdateTowerTransform();

            //send tower local direction to clients
            if (EntitySystemWorld.Instance.IsServer())
                Server_TickSendTowerLocalDirection();

            //!!!!!should use for disabled renderer
            if (EntitySystemWorld.Instance.IsDedicatedServer())
                UpdateTowerTransform();
        }

        private Unit GetPlayerUnit()
        {
            if (PlayerIntellect.Instance == null)
                return null;
            return PlayerIntellect.Instance.ControlledObject;
        }

        private Intellect GetPlayerIntellect()
        {
            if (PlayerIntellect.Instance == null)
                return null;
            return PlayerIntellect.Instance;
        }

        protected override void Client_OnTick()
        {
            base.Client_OnTick();

            Damnweapons(false, false);

            if (EntitySystemWorld.Instance.IsClientOnly() &&
             Type.NetworkType == EntityNetworkTypes.Synchronized)
            {
                if ((Server_currentMissileTarget != CurrentMissileTarget && CurrentMissileTarget != null)
                    || (Server_currentMissileTarget != null && CurrentMissileTarget == null))
                {
                    Client_SendAKunitUpdateCurrentMissileTarget();
                }
            }

            PlayHeatSounds();

            if (soundOnChannel != null && soundOnChannel.IsStopped() && (!entrySoundsPlayed || !entrySounds2Played || !entrySounds3Played))
            {
                EntrySoundsOnTick();
            }
        }

        private bool HitPointAdded;

        private void BodyPartManagment()
        {
            float Hmax = 0;
            float H = 0;
            if (!HitPointAdded)
            {
                for (int i = 0; i < Bp.Count; )
                {
                    BP BP = Bp[i];

                    BP.HitPoints = BP.HitpointsMax;
                    i++;
                    if (i == Type.BodyParts.Count)
                    {
                        HitPointAdded = true;
                    }
                }
            }

            foreach (AKunit.BP BP in Bp)
            {
                if (BP.HitPoints < 0 && HitPointAdded == true)
                {
                    BP.HitPoints = 0;
                    BP.Damaged = true;
                }

                if (BP.Damaged == true)
                {
                    for (int i = 0; i < BP.Weapons.Count; i++)
                    {
                        Gun gun = null;
                        MapObjectAttachedMapObject GunObject = GetFirstAttachedObjectByAlias(BP.Weapons[i].MapObjectAlias.ToString()) as MapObjectAttachedMapObject;

                        if (GunObject != null)
                            gun = GunObject.MapObject as Gun;

                        if (gun != null)
                        {
                            gun.Damaged = true;
                            if (!string.IsNullOrEmpty(Type.SoundWeaponDestroyed) && !gun.DestroyedSoundPlayed)
                            {
                                gun.DestroyedSoundPlayed = true;
                                VirtualChannel destroyedChannel;
                                Sound sound = SoundWorld.Instance.SoundCreate(Type.SoundWeaponDestroyed, SoundMode.Mode3D);
                                if (sound != null)
                                {
                                    destroyedChannel = SoundWorld.Instance.SoundPlay(sound, EngineApp.Instance.DefaultSoundChannelGroup, .3f, true);
                                    if (destroyedChannel != null)
                                    {
                                        destroyedChannel.Position = Position;
                                        destroyedChannel.Pause = false;
                                    }
                                }
                            }
                        }
                    }
                }
                Hmax += BP.HitpointsMax;
                H += BP.HitPoints;
            }

            this.Type.HealthMax = Hmax;
            this.Health = H;

            if (this.Type.HealthMax > 0 && this.Health < 1)
            {
                Die();
            }
        }

        protected override void OnRender(Camera camera)
        {
            //not very true update in the OnRender.
            //it is here because need update after all Ticks and before update attached objects.
            UpdateTowerTransform();

            base.OnRender(camera);
        }

        public void SetMomentaryTurnToPosition(Vec3 pos)
        {
            if (towerBody == null)
                return;

            Vec3 direction = pos - towerBody.Position;
            towerLocalDirection = SphereDir.FromVector(Rotation.GetInverse() * direction);
            needTowerLocalDirection = towerLocalDirection;
        }

        public virtual void SetNeedTurnToPosition(Vec3 pos)
        {
            if (towerBody == null)
                return;
            lookto = pos;

            if (Type.TowerTurnSpeed != 0)
            {
                Vec3 direction = pos - towerBody.Position;
                needTowerLocalDirection = SphereDir.FromVector(Rotation.GetInverse() * direction);
            }
            else
                SetMomentaryTurnToPosition(pos);
        }

        protected virtual void UpdateTowerTransform()
        {
            if (towerBody == null || mainGunAttachedObject == null)
                return;

            Radian horizontalAngle = towerLocalDirection.Horizontal;
            Radian verticalAngle = towerLocalDirection.Vertical;

            Range torsoRotationRange = Type.TorsoRotationAngleRange * MathFunctions.PI / 180.0f;
            if (horizontalAngle < torsoRotationRange.Minimum)
                horizontalAngle = torsoRotationRange.Minimum;
            if (horizontalAngle > torsoRotationRange.Maximum)
                horizontalAngle = torsoRotationRange.Maximum;

            Range gunRotationRange = Type.GunRotationAngleRange * MathFunctions.PI / 180.0f;
            if (verticalAngle < gunRotationRange.Minimum)
                verticalAngle = gunRotationRange.Minimum;
            if (verticalAngle > gunRotationRange.Maximum)
                verticalAngle = gunRotationRange.Maximum;

            //update tower body
            towerBody.Position = GetInterpolatedPosition() +
                GetInterpolatedRotation() * towerBodyLocalPosition;
            towerBody.Rotation = GetInterpolatedRotation() *
                new Angles(0, 0, -horizontalAngle.InDegrees()).ToQuat();
            towerBody.Sleeping = true;

            foreach (Body body in PhysicsModel.Bodies)
            {
                if (body.Name == "Gun1" || body.Name == "Gun2" || body.Name == "Gun3" || body.Name == "Gun4" || body.Name == "Gun6")
                {
                    body.Rotation = GetInterpolatedRotation() *
                new Angles(0, verticalAngle.InDegrees(), -horizontalAngle.InDegrees()).ToQuat();
                }
            }

            Quat verticalRotation = new Angles(0, verticalAngle.InDegrees(), 0).ToQuat();

            bool DontUpdate = !towerLocalDirection.Equals(needTowerLocalDirection,
                    new Degree(20).InRadians());

            foreach (AKunit.WeaponItem item in weapons)
            {
                if (Intellect != null && DontUpdate == false)
                {
                    Vec3 diff = lookto - (Position + item.AttachedObject.PositionOffset * towerBody.Rotation);

                    Radian horizontalAngle2 = MathFunctions.ATan(diff.Y, diff.X);
                    Radian verticalAngle2 = MathFunctions.ATan(diff.Z, diff.ToVec2().Length());

                    item.AttachedObject.RotationOffset = new Angles(0, 0, -horizontalAngle2.InDegrees()).ToQuat();

                    Quat rot = towerBody.Rotation.GetInverse() * item.AttachedObject.RotationOffset;
                    Quat verticalRot = new Angles(0, verticalAngle2.InDegrees(), 0).ToQuat();

                    //item.AttachedObject.PositionOffset = rot * mainGunOffsetPosition;
                    item.AttachedObject.RotationOffset = rot * verticalRot;
                }
                else
                {
                    item.AttachedObject.RotationOffset = new Quat(0, 0, 0, 1);
                }
            }
        }

        private bool entrySoundsCreated = false;
        private bool entrySounds2Created = false;
        private bool entrySounds3Created = false;

        private bool startupSequenceChannelPlayed = false;
        private bool weaponsActivatedChannelPlayed = false;
        private bool systemsNominalChannelPlayed = false;

        private VirtualChannel startupSequenceChannel = null;
        private VirtualChannel weaponsActivatedChannel = null;
        private VirtualChannel systemsNominalChannel = null;

        protected void EntrySoundsOnTick()
        {
            Unit unit = PlayerIntellect.Instance.ControlledObject;
            if (unit != null)
            {
                MechUnitAI unitAI = unit.Intellect as MechUnitAI;
                if (unitAI != null)
                {
                    return;// don't play sounds if mech ai
                }
            }

            //EngineConsole.Instance.Print("Mech Name to check" + this.Type.Name.ToString());
            //if (GameNetworkClient.Instance == null) //TODO : Incin test sound settings
            //   return;

            //if (entrySoundsPlayed)
            //    return;

            if (!entrySoundsCreated || !entrySounds2Created || !entrySounds3Created)
            {
                Sound s1 = null;
                Sound s2 = null;
                Sound s3 = null;

                if (!string.IsNullOrEmpty(Type.VehicleEntrySounds.StartupSequenceActivated))
                {
                    s1 = SoundWorld.Instance.SoundCreate(Type.VehicleEntrySounds.StartupSequenceActivated, SoundMode.Mode3D);
                    entrySoundsCreated = true;

                    if (s1 != null)
                    {
                        startupSequenceChannel = SoundWorld.Instance.SoundPlay(s1, EngineApp.Instance.DefaultSoundChannelGroup, .33f, true); //.3
                        if (startupSequenceChannel != null)
                        {
                            startupSequenceChannel.Position = Position;
                            //entrySoundsCreated = true;
                            entrySoundsPlayed = false;
                        }
                    }
                }
                if (!string.IsNullOrEmpty(Type.VehicleEntrySounds.WeaponSystemsActivated))
                {
                    s2 = SoundWorld.Instance.SoundCreate(Type.VehicleEntrySounds.WeaponSystemsActivated, SoundMode.Mode3D);
                    entrySounds2Created = true;

                    if (s2 != null)
                    {
                        weaponsActivatedChannel = SoundWorld.Instance.SoundPlay(s2, EngineApp.Instance.DefaultSoundChannelGroup, .66f, true); //.3
                        if (weaponsActivatedChannel != null)
                        {
                            weaponsActivatedChannel.Position = Position;
                            //entrySounds2Created = true;
                            entrySounds2Played = false;
                        }
                    }
                }

                if (!string.IsNullOrEmpty(Type.VehicleEntrySounds.AllSystemsNominal))
                {
                    s3 = SoundWorld.Instance.SoundCreate(Type.VehicleEntrySounds.AllSystemsNominal, SoundMode.Mode3D);
                    entrySounds3Created = true;

                    if (s3 != null)
                    {
                        systemsNominalChannel = SoundWorld.Instance.SoundPlay(s3, EngineApp.Instance.DefaultSoundChannelGroup, .99f, true); //.3
                        if (systemsNominalChannel != null)
                        {
                            systemsNominalChannel.Position = Position;
                            //entrySounds3Created = true;
                            entrySounds3Played = false;
                        }
                    }
                }
            }
            if (!startupSequenceChannelPlayed && startupSequenceChannel != null && entrySoundsPlayed == false)
            {
                if (startupSequenceChannel.Pause)
                {
                    startupSequenceChannel.Pause = false;
                }
                else
                {
                    if (startupSequenceChannel.IsStopped())
                    {
                        startupSequenceChannelPlayed = true;
                        entrySoundsPlayed = true;
                    }
                }

                return;
            }

            if (!weaponsActivatedChannelPlayed && weaponsActivatedChannel != null && entrySounds2Played == false)
            {
                if (weaponsActivatedChannel.Pause)
                {
                    weaponsActivatedChannel.Pause = false;
                }
                else
                {
                    if (weaponsActivatedChannel.IsStopped())
                    {
                        weaponsActivatedChannelPlayed = true;
                        entrySounds2Played = true;
                    }
                }

                return;
            }

            if (!systemsNominalChannelPlayed && systemsNominalChannel != null && entrySounds3Played == false)
            {
                if (systemsNominalChannel.Pause)
                {
                    systemsNominalChannel.Pause = false;
                }
                else
                {
                    if (systemsNominalChannel.IsStopped())
                    {
                        systemsNominalChannelPlayed = true;
                        entrySounds3Played = true;
                    }
                }

                return;
            }
            else
            {
                //reset all values so they will play again when leaving vehicle
                if (entrySoundsPlayed && entrySounds2Played && entrySounds3Played)
                {
                    entrySoundsCreated = false;
                    startupSequenceChannelPlayed = false;
                    startupSequenceChannel = null;

                    entrySounds2Created = false;
                    weaponsActivatedChannelPlayed = false;
                    weaponsActivatedChannel = null;

                    entrySounds3Created = false;
                    systemsNominalChannelPlayed = false;
                    systemsNominalChannel = null;
                }
            }
        }

        private void TickTowerTurn()
        {
            //update direction
            if (towerLocalDirection != needTowerLocalDirection)
            {
                Radian turnSpeed = Type.TowerTurnSpeed;

                SphereDir needDirection = needTowerLocalDirection;
                SphereDir direction = towerLocalDirection;

                //update horizontal direction
                float diffHorizontalAngle = needDirection.Horizontal - direction.Horizontal;
                while (diffHorizontalAngle < -MathFunctions.PI)
                    diffHorizontalAngle += MathFunctions.PI * 2;
                while (diffHorizontalAngle > MathFunctions.PI)
                    diffHorizontalAngle -= MathFunctions.PI * 2;

                if (diffHorizontalAngle > 0)
                {
                    if (direction.Horizontal > needDirection.Horizontal)
                        direction.Horizontal -= MathFunctions.PI * 2;
                    direction.Horizontal += turnSpeed * TickDelta;
                    if (direction.Horizontal > needDirection.Horizontal)
                        direction.Horizontal = needDirection.Horizontal;
                }
                else
                {
                    if (direction.Horizontal < needDirection.Horizontal)
                        direction.Horizontal += MathFunctions.PI * 2;
                    direction.Horizontal -= turnSpeed * TickDelta;
                    if (direction.Horizontal < needDirection.Horizontal)
                        direction.Horizontal = needDirection.Horizontal;
                }

                //update vertical direction
                if (direction.Vertical < needDirection.Vertical)
                {
                    direction.Vertical += turnSpeed * TickDelta;
                    if (direction.Vertical > needDirection.Vertical)
                        direction.Vertical = needDirection.Vertical;
                }
                else
                {
                    direction.Vertical -= turnSpeed * TickDelta;
                    if (direction.Vertical < needDirection.Vertical)
                        direction.Vertical = needDirection.Vertical;
                }

                if (direction.Equals(needTowerLocalDirection, .001f))
                    towerLocalDirection = direction;

                towerLocalDirection = direction;
            }

            //update tower turn sound
            {
                bool needSound = !towerLocalDirection.Equals(needTowerLocalDirection,
                    new Degree(2).InRadians());

                if (needSound)
                {
                    if (towerTurnChannel == null && !string.IsNullOrEmpty(Type.SoundTowerTurn))
                    {
                        Sound sound = SoundWorld.Instance.SoundCreate(Type.SoundTowerTurn,
                            SoundMode.Mode3D | SoundMode.Loop);

                        if (sound != null)
                        {
                            towerTurnChannel = SoundWorld.Instance.SoundPlay(
                                sound, EngineApp.Instance.DefaultSoundChannelGroup, .3f, true);
                            towerTurnChannel.Position = Position;
                            towerTurnChannel.Pause = false;
                        }
                    }

                    if (towerTurnChannel != null)
                        towerTurnChannel.Position = Position;
                }
                else
                {
                    if (towerTurnChannel != null)
                    {
                        towerTurnChannel.Stop();
                        towerTurnChannel = null;
                    }
                }
            }
        }

        protected override void OnIntellectCommand(Intellect.Command command)
        {
            base.OnIntellectCommand(command);
            if (command.KeyPressed)
            {
                if (command.Key == GameControlKeys.Weapon1)
                {
                    Damnweapons(true, false);
                }

                if (command.Key == GameControlKeys.Weapon2)
                {
                    Damnweapons(false, true);
                }

                if (command.Key == GameControlKeys.Weapon3)
                {
                    currentFireMode = (FireModes)((int)currentFireMode + 1);

                    if (currentFireMode == FireModes.Count) currentFireMode = (FireModes)0;
                }

                if (command.Key == GameControlKeys.Target)
                {
                    nextUnit();
                }

                if (command.Key == GameControlKeys.Light)
                {
                    lightsOn = !lightsOn;
                    foreach (MapObjectAttachedLight light in lights)
                        light.Visible = lightsOn;
                }
            }
        }

        public int i = 0;

        private void Damnweapons(bool One, bool Two)
        {
            if (weapons.Count == 0)
                return;

            if (currentFireMode == FireModes.Alpha)
                return;

            if (currentFireMode == FireModes.Group)
            {
                if (One)
                {
                    if (currentFireGroup == 0) currentFireGroup = maxFireGroup;
                    else currentFireGroup--;
                }

                if (Two)
                {
                    if (currentFireGroup == maxFireGroup) currentFireGroup = 0;
                    else currentFireGroup++;
                }
            }

            if (currentFireMode == FireModes.Link)
            {
                if (One)
                {
                    i++;
                    if (i == weapons.Count)
                        i = 0;
                }
                if (Two)
                {
                    i--;
                    if (i == -1)
                        i = weapons.Count - 1;
                }

                Gun gun = (Gun)weapons[i].Weapon;
                if (gun != null)
                {
                    mainGun = gun;
                }
                else
                {
                    Log.Error("AKunit: Weapon dose not exists.");
                }
            }
        }

        private void CalculateTracksSpeed()
        {
            tracksSpeed = 0;
            Body chassisBody = PhysicsModel.GetBody("mainBody");
            if (chassisBody == null) return;

            if (chassisBody.Sleeping)
                return;

            Vec3 linearVelocity = chassisBody.LinearVelocity;
            Vec3 angularVelocity = chassisBody.AngularVelocity;

            //optimization
            if (linearVelocity.Equals(Vec3.Zero, .1f) && angularVelocity.Equals(Vec3.Zero, .1f))
                return;

            Vec3 localLinearVelocity = linearVelocity * chassisBody.Rotation.GetInverse();
            tracksSpeed = localLinearVelocity.X + Math.Abs(angularVelocity.Z) * 2;
        }

        public bool GunsTryFire(bool alternative)
        {
            bool fire = false;

            if (currentFireMode == FireModes.Alpha)
            {
                foreach (WeaponItem item in weapons)
                {
                    if (item.Weapon != null)
                    {
                        item.Weapon.SetForceFireRotationLookTo(lookto);
                        item.Weapon.TryFire(alternative);
                        fire = true;
                    }
                }
            }
            else if (currentFireMode == FireModes.Group)
            {
                foreach (WeaponItem item in weapons)
                {
                    if (item.FireGroup == CurrentFireGroup)
                    {
                        if (item.Weapon != null)
                        {
                            item.Weapon.SetForceFireRotationLookTo(lookto);
                            item.Weapon.TryFire(alternative);
                            fire = true;
                        }
                    }
                }
            }
            else if (currentFireMode == FireModes.Link)
            {
                if (mainGun != null)
                {
                    mainGun.SetForceFireRotationLookTo(lookto);
                    mainGun.TryFire(alternative);
                    fire = true;
                }
            }
            return fire;
        }

        public float GetRemainingLockTime()
        {
            return 1f;
        }

        public float GetLockingCompletionPercentage()
        {
            //return the how much time has elapsed to gain missile lock 0 - 1.00f

            float completion = 0f;
            float divider;

            if (MissileLockCounter == 0.0f) return completion;

            if (MainGun.NormalMode.typeMode.BulletType is MissileType)
            {
                divider = ((MissileType)MainGun.NormalMode.typeMode.BulletType).LockingTime;
                if (divider == 0f) return 1f;
                completion = MissileLockCounter / divider;
                return completion;
            }

            foreach (WeaponItem item in Weapons)
            {
                //go through weapons and select the first missile
                if (item.Weapon.Type as MissileLauncherType != null)
                {
                    Gun launcher = ((Gun)item.Weapon);
                    divider = ((MissileType)launcher.NormalMode.typeMode.BulletType).LockingTime;

                    //if no locking time is set it means instant lock
                    if (divider == 0f) return 1f;

                    completion = MissileLockCounter / divider;

                    //find only the first missile
                    break;
                }
            }

            return completion;
        }

        public virtual bool IsTargetLocked(Unit target)
        {
            if (target == CurrentMissileTarget) // don't count same target twice
            {
                LastTickTarget = target;
                MissileLockCounter = 0f;
                return true;
            }
            else if (target != LastTickTarget) //can't be locked then if it has been changed!
            {
                LastTickTarget = target;
                MissileLockCounter = 0f;
                return false;
            }
            else if (target == lastTickTarget) //still same target on this tick
            {
                //if we still have a misile active add more lock time
                MissileLockCounter = MissileLockCounter + TickDelta;

                if (GetLockingCompletionPercentage() >= 1.0f)
                {
                    missileLockCounter = 0f;
                    CurrentMissileTarget = target;

                    return true;
                }
            }

            return false;
        }

        public float unlockcounter = 5;

        public bool UnlockP(bool Stop)
        {
            if (Stop)
                unlockcounter -= TickDelta;

            if (unlockcounter < 0)
            {
                CurrentMissileTarget = null;
                unlockcounter = 5;
                return true;
            }
            return false;
        }

        public void PlayLockingSound()
        {
            //nothing Yet
            Sound soundi = Engine.SoundSystem.SoundWorld.Instance.SoundCreate("Sounds\\Beep.ogg", SoundMode.Mode3D);
            //SoundWorld.Instance.SoundPlay(soundi, SoundWorld.Instance.MasterChannelGroup,0.5f, false);

            SoundWorld.Instance.SoundPlay(soundi, EngineApp.Instance.DefaultSoundChannelGroup, 0.5f, false);
        }

        public bool IsMissileActive()
        {
            bool returnValue = false;

            foreach (WeaponItem item in GetActiveWeapons())
            {
                if (item.Weapon.Type is MissileLauncherType) returnValue = true;
            }

            return returnValue;
        }

        public Unit nextUnit()
        {
            //switch next unit in visible units to have red reticule

            if (visibleUnits == null || visibleUnits.Count == 1) return null;

            if (visibleUnits.Count == 1)
            {
                reticuleTarget = visibleUnits[0];
                //return reticuleTarget;
            }
            else
            {
                if (reticuleTarget != null && visibleUnits.Contains(reticuleTarget))
                {
                    int index = visibleUnits.IndexOf(reticuleTarget);

                    if (index == visibleUnits.Count - 1) reticuleTarget = visibleUnits[0];
                    else
                    {
                        reticuleTarget = visibleUnits[index + 1];
                    }
                }
                //else reticuleTarget = visibleUnits[0];
            }

            if (CurrentMissileTarget != null && reticuleTarget != CurrentMissileTarget) CurrentMissileTarget = null;

            return reticuleTarget;
        }

        private bool isHeatedByFlamer;

        public bool IsHeatedByFlamer
        {
            get { return isHeatedByFlamer; }
            set { isHeatedByFlamer = value; }
        }

        protected int heattoadd = 0;
        private float keyTime = 0;

        public void GunHeat(int gunheatadd)
        {
            heattoadd += gunheatadd;
        }

        public int GetHeatLevel()
        {
            return AKunitHeat;
        }

        private VirtualChannel criticalChannel;

        public void PlayHeatSounds()
        {
            if (!playedHeatLevelCriticalSound)
            {
                if (AKunitHeat > Type.AKunitShutDownHeat)
                {
                    Sound sound = SoundWorld.Instance.SoundCreate(Type.HeatLevelCriticalSound, SoundMode.Mode3D);
                    if (sound != null)
                    {
                        criticalChannel = SoundWorld.Instance.SoundPlay(sound, EngineApp.Instance.DefaultSoundChannelGroup, .7f, true);
                        if (criticalChannel != null)
                        {
                            criticalChannel.Position = Position;
                            criticalChannel.Pause = false;
                        }
                    }
                    playedHeatLevelCriticalSound = true;
                }
            }
            else
            {
                if (!playedShutdownSound && criticalChannel != null && criticalChannel.IsStopped())
                {
                    if (AKunitHeat >= Type.AKunitHeatMax)
                    {
                        SoundPlay3D(Type.ShutdownSound, .7f, false);
                        playedShutdownSound = true;
                    }
                }
            }

            if (AKunitHeat < Type.AKunitShutDownHeat)
            {
                playedHeatLevelCriticalSound = false;
                playedShutdownSound = false;
            }
        }

        public void SoundPlay3D(string name, float priority, bool needAttach, out VirtualChannel channel)
        {
            channel = null;
            if (string.IsNullOrEmpty(name))
                return;

            if (EngineApp.Instance.DefaultSoundChannelGroup != null &&
                EngineApp.Instance.DefaultSoundChannelGroup.Volume == 0)
                return;

            //2d sound mode for FPS Camera Player
            PlayerIntellect playerIntellect = PlayerIntellect.Instance;

            if (playerIntellect != null && playerIntellect.FPSCamera &&
                 playerIntellect.ControlledObject != null &&
                 playerIntellect.ControlledObject == GetParentUnit())
            {
                Sound sound = SoundWorld.Instance.SoundCreate(name, 0);
                if (sound != null)
                {
                    SoundWorld.Instance.SoundPlay(sound, EngineApp.Instance.DefaultSoundChannelGroup,
                        priority);
                }
                return;
            }

            //Default 3d mode
            {
                if (!needAttach)
                {
                    Sound sound = SoundWorld.Instance.SoundCreate(name, SoundMode.Mode3D);
                    if (sound == null)
                        return;

                    channel = SoundWorld.Instance.SoundPlay(sound,
                        EngineApp.Instance.DefaultSoundChannelGroup, priority, true);
                    if (channel != null)
                    {
                        channel.Position = Position;
                        channel.Pause = false;
                    }
                }
                else
                {
                    MapObjectAttachedSound attachedSound = new MapObjectAttachedSound();
                    attachedSound.SetSoundName(name, false);
                    Attach(attachedSound);
                }
            }
        }

        private void Body_Collision(ref CollisionEvent collisionEvent)
        {
            //Die on Collision
            Body thisBody = collisionEvent.ThisShape.Body;
            Body otherBody = collisionEvent.OtherShape.Body;
            float otherMass = otherBody.Mass;

            float impulse = 0;
            impulse += thisBody.LastStepLinearVelocity.Length() * thisBody.Mass;
            if (otherMass != 0)
                impulse += otherBody.LastStepLinearVelocity.Length() * otherMass;

            float damage = impulse * Type.ImpulseDamageCoefficient;
            if (damage >= Type.ImpulseMinimalDamage)
                OnDamage(null, collisionEvent.Position, collisionEvent.ThisShape, damage, true);
        }

        private bool shutdownDlay;
        private bool used;
        public bool colantflush;

        private void HeatManagment()
        {
            bool Override = false;
            if (!isHeatedByFlamer)
            {
                float OverrideTime = 0;

                if (Intellect != null)
                {
                    if (Intellect.IsControlKeyPressed(GameControlKeys.Shut_Down_Override))
                    {
                        Override = true;
                        keyTime += Intellect.GetControlKeyStrength(GameControlKeys.Shut_Down_Override);
                    }
                    else
                    {
                        OverrideTime = 0;
                        keyTime = 0;
                    }
                    //colant flush
                    if (Intellect.IsControlKeyPressed(GameControlKeys.CoolantFlush) && colantflush == false)
                    {
                        AKunitHeat -= Type.AKunitCoolantFlash;
                        colantflush = true;
                    }
                }
                OverrideTime += keyTime;

                if (OverrideTime > 100)
                {
                    used = true;
                }

                if (Type.AKunitHeatGenUpdateSpeed == 0)
                    Type.AKunitHeatGenUpdateSpeed = 1;
                if (heattoadd < 1) heattoadd = 0;

                AKunitHeat += heattoadd / Type.AKunitHeatGenUpdateSpeed;
                heattoadd -= heattoadd / Type.AKunitHeatGenUpdateSpeed;

                if (AKunitHeat > 0)
                {
                    if (heattoadd == 0)
                    {
                        AKunitHeat -= Type.AKunitDefultHeatSink;
                    }
                    else
                    {
                        AKunitHeat -= Type.AKunitDefultHeatSink / 3;
                    }
                }
            }
            else
            {
                if (Type.AKunitHeatGenUpdateSpeed == 0)
                    Type.AKunitHeatGenUpdateSpeed = 1;
                if (heattoadd < 1) heattoadd = 0;

                AKunitHeat += heattoadd / Type.AKunitHeatGenUpdateSpeed;
                heattoadd -= heattoadd / Type.AKunitHeatGenUpdateSpeed;
            }

            if (AKunitHeat > Type.AKunitExplodeHeat)
            {
                Die();
            }

            if (AKunitHeat < 0)
                AKunitHeat = 0;

            if (AKunitHeat == 0)
            {
                used = false;
            }

            if (AKunitHeat > Type.AKunitShutDownHeat && (Override == false || used == true))
            {
                MechShutDown = true;
                if (!shutdownDlay)
                {
                    shutdownDlay = true;
                }
            }
            else if (AKunitHeat < (Type.AKunitShutDownHeat - 200))
            {
                MechShutDown = false;
                used = false;
            }
        }

        public List<WeaponItem> GetActiveWeapons()
        {
            //go trough every weapon. Check if it's active and return them

            List<WeaponItem> activeOnes = new List<WeaponItem>();

            for (int i = 0; i < weapons.Count; i++)
            {
                if (CurrentFireMode == AKunit.FireModes.Alpha ||
                (CurrentFireMode == AKunit.FireModes.Group && Weapons[i].FireGroup == CurrentFireGroup)
                || (MainGun == Weapons[i].Weapon && CurrentFireMode == AKunit.FireModes.Link))
                {
                    activeOnes.Add(Weapons[i]);
                }
            }

            return activeOnes;
        }

        //W

        protected override void OnDamage(MapObject prejudicial, Vec3 pos, Shape shape, float damage, bool allowMoveDamageToParent)
        {
            base.OnDamage(prejudicial, pos, shape, damage, allowMoveDamageToParent);

            bool DamageDone = false;

            if (shape != null)
            {
                foreach (AKunit.BP BP in Bp)
                {
                    if (BP.PhysicsShape.ToString() == shape.Name.ToString() && BP.Damaged != true)
                    {
                        BP.HitPoints -= damage;
                        DamageDone = true;
                    }
                }
            }

            if (!DamageDone)
            {
                foreach (AKunit.BP BP in Bp)
                {
                    float bodycounts = Type.BodyParts.Count;
                    if (bodycounts == 0) bodycounts = 1;
                    float toallDamage = damage / Type.BodyParts.Count;
                    BP.HitPoints -= toallDamage;
                }
            }

            if (EntitySystemWorld.Instance.IsServer() && Type.NetworkType == EntityNetworkTypes.Synchronized)
            {
                Server_SendUpdateDamage(EntitySystemWorld.Instance.RemoteEntityWorlds);
            }
        }

        /* public void AKunitDoDamage(Shape shape, float damage, AKunit akunit)
         {
             if (akunit.UIN.ToString() == this.UIN.ToString())
             {
                 bool DamageDone = false;
                 foreach (AKunit.BP BP in Bp)
                 {
                     if (BP.PhysicsShape.ToString() == shape.Name.ToString() && BP.Damaged != true)
                     {
                         BP.HitPoints -= damage;
                         DamageDone = true;
                     }
                 }
                 if (!DamageDone)
                 {
                     foreach (AKunit.BP BP in Bp)
                     {
                         float bodycounts = Type.BodyParts.Count;
                         if (bodycounts == 0) bodycounts = 1;
                         float toallDamage = damage / Type.BodyParts.Count;
                         BP.HitPoints -= toallDamage;
                     }
                 }
             }

             if (EntitySystemWorld.Instance.IsServer() &&
               Type.NetworkType == EntityNetworkTypes.Synchronized)
             {
                 Server_SendUpdateDamage(
                             EntitySystemWorld.Instance.RemoteEntityWorlds);
             }
         }*/

        public float TorsoR;
        public float GunA;
        public Quat TowerRot;

        private void GUIManagment()
        {
            TowerRot = towerBody.Rotation;

            Body chassisBody = PhysicsModel.GetBody("mainBody");
            if (chassisBody == null) return;

            AKunit.WeaponItem WI = Weapons[0];
            if (WI != null)
            {
                GunA = (towerBody.Rotation * WI.AttachedObject.RotationOffset.GetInverse()).ToAngles().Roll / 180;
            }

            {
                float shuit = (chassisBody.Rotation * towerBody.Rotation.GetInverse()).ToAngles().Yaw;
                TorsoR = -shuit / 180;
            }
        }

        private enum NetworkMessages
        {
            UpdateAKunit,
            TowerLocalDirectionToClient,
            TracksSpeedToClient,
            UpdateDamage,
            UpdateAKunitHeat,
            UpdateAKunitColant,
            UpdateAKunitWeaponInt,
            UpdateAKunitWeaponMode,
            AKunitUpdateCurrentMissileTarget,
            VariantToClient,
        }

        protected override void Server_OnClientConnectedAfterPostCreate(
            RemoteEntityWorld remoteEntityWorld)
        {
            base.Server_OnClientConnectedAfterPostCreate(remoteEntityWorld);

            RemoteEntityWorld[] worlds = new RemoteEntityWorld[] { remoteEntityWorld };

            Server_SendTowerLocalDirectionToClients(worlds);
            Server_SendTracksSpeedToClients(worlds);
        }

        private void Server_TickSendTowerLocalDirection()
        {
            float epsilon = new Degree(.5f).InRadians();
            if (!towerLocalDirection.Equals(server_sentTowerLocalDirection, epsilon))
            {
                Server_SendTowerLocalDirectionToClients(EntitySystemWorld.Instance.RemoteEntityWorlds);
                server_sentTowerLocalDirection = towerLocalDirection;
            }
        }

        private void Server_SendTowerLocalDirectionToClients(IList<RemoteEntityWorld> remoteEntityWorlds)
        {
            SendDataWriter writer = BeginNetworkMessage(remoteEntityWorlds, typeof(AKunit),
                (ushort)NetworkMessages.TowerLocalDirectionToClient);
            writer.Write(towerLocalDirection);
            EndNetworkMessage();
        }

        [NetworkReceive(NetworkDirections.ToClient, (ushort)NetworkMessages.TowerLocalDirectionToClient)]
        private void Client_ReceiveTowerLocalDirection(RemoteEntityWorld sender, ReceiveDataReader reader)
        {
            SphereDir value = reader.ReadSphereDir();
            if (!reader.Complete())
                return;
            towerLocalDirection = value;
        }

        private void Server_SendTracksSpeedToClients(IList<RemoteEntityWorld> remoteEntityWorlds)
        {
            const float epsilon = .5f;
            if (Math.Abs(tracksSpeed - server_sendTracksSpeed) > epsilon)
            {
                SendDataWriter writer = BeginNetworkMessage(remoteEntityWorlds, typeof(AKunit),
                    (ushort)NetworkMessages.TracksSpeedToClient);
                writer.Write(tracksSpeed);
                EndNetworkMessage();

                server_sendTracksSpeed = tracksSpeed;
            }
        }

        [NetworkReceive(NetworkDirections.ToClient, (ushort)NetworkMessages.TracksSpeedToClient)]
        private void Client_ReceiveTracksSpeed(RemoteEntityWorld sender, ReceiveDataReader reader)
        {
            float value = reader.ReadSingle();
            if (!reader.Complete())
                return;
            tracksSpeed = value;
        }

        ////////////////Network AKunit//////////////////////
        public Unit Server_currentMissileTarget;

        private void TickIntellect()
        {
            //////////////Networking////////////////
            if (EntitySystemWorld.Instance.IsServer() &&
             Type.NetworkType == EntityNetworkTypes.Synchronized)
            {
                Server_SendUpdateAKunitColant(EntitySystemWorld.Instance.RemoteEntityWorlds);
                Server_SendUpdateAKunitHeat(EntitySystemWorld.Instance.RemoteEntityWorlds);
                Server_SendUpdateAKunit(EntitySystemWorld.Instance.RemoteEntityWorlds);
                Server_SendUpdateAKunitWeaponInt(EntitySystemWorld.Instance.RemoteEntityWorlds);
                Server_SendUpdateAKunitWeaponMode(EntitySystemWorld.Instance.RemoteEntityWorlds);
            }
        }

        private void Server_SendUpdateAKunit(IList<RemoteEntityWorld> remoteEntityWorlds)
        {
            if (MechShutDown != Server_MechShutDown)
            {
                SendDataWriter writer = BeginNetworkMessage(remoteEntityWorlds, typeof(AKunit),
                (ushort)NetworkMessages.UpdateAKunit);

                writer.Write(MechShutDown);

                EndNetworkMessage();
                Server_MechShutDown = MechShutDown;
            }
        }

        [NetworkReceive(NetworkDirections.ToClient, (ushort)NetworkMessages.UpdateAKunit)]
        private void Client_ReceiveUpdateAKunit(RemoteEntityWorld sender, ReceiveDataReader reader)
        {
            bool value = reader.ReadBoolean();
            if (!reader.Complete())
                return;

            MechShutDown = value;
        }

        //updateDamage
        private void Server_SendUpdateDamage(IList<RemoteEntityWorld> remoteEntityWorlds)
        {
            SendDataWriter writer = BeginNetworkMessage(remoteEntityWorlds, typeof(AKunit),
                (ushort)NetworkMessages.UpdateDamage);

            foreach (AKunit.BP BP in Bp)
            {
                writer.Write(BP.HitPoints);
            }
            EndNetworkMessage();
        }

        [NetworkReceive(NetworkDirections.ToClient, (ushort)NetworkMessages.UpdateDamage)]
        private void Client_ReceiveUpdateDamage(RemoteEntityWorld sender, ReceiveDataReader reader)
        {
            foreach (AKunit.BP BP in Bp)
            {
                BP.HitPoints = reader.ReadSingle();
                if (BP.HitPoints < 0)
                {
                    for (int i = 0; i < BP.Weapons.Count; i++)
                    {
                        Gun gun = null;
                        MapObjectAttachedMapObject GunObject = GetFirstAttachedObjectByAlias(BP.Weapons[i].MapObjectAlias.ToString()) as MapObjectAttachedMapObject;

                        if (GunObject != null)
                            gun = GunObject.MapObject as Gun;

                        if (gun != null)
                        {
                            gun.Damaged = true;
                            if (!string.IsNullOrEmpty(Type.SoundWeaponDestroyed) && !gun.DestroyedSoundPlayed)
                            {
                                gun.DestroyedSoundPlayed = true;
                                VirtualChannel destroyedChannel;
                                Sound sound = SoundWorld.Instance.SoundCreate(Type.SoundWeaponDestroyed, SoundMode.Mode3D);
                                if (sound != null)
                                {
                                    destroyedChannel = SoundWorld.Instance.SoundPlay(sound, EngineApp.Instance.DefaultSoundChannelGroup, .3f, true);
                                    if (destroyedChannel != null)
                                    {
                                        destroyedChannel.Position = Position;
                                        destroyedChannel.Pause = false;
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        private int Server_akheat;

        private void Server_SendUpdateAKunitHeat(IList<RemoteEntityWorld> remoteEntityWorlds)
        {
            if (AKunitHeat > 0)
            {
            }
            const int epsilon = 3;
            if (Math.Abs(AKunitHeat - Server_akheat) > epsilon)
            {
                SendDataWriter writer = BeginNetworkMessage(remoteEntityWorlds, typeof(AKunit),
                    (ushort)NetworkMessages.UpdateAKunitHeat);

                writer.WriteVariableInt32(AKunitHeat);
                EndNetworkMessage();

                Server_akheat = AKunitHeat;
            }
        }

        [NetworkReceive(NetworkDirections.ToClient, (ushort)NetworkMessages.UpdateAKunitHeat)]
        private void Client_ReceiveUpdateAKunitHeat(RemoteEntityWorld sender, ReceiveDataReader reader)
        {
            int value = reader.ReadVariableInt32();
            if (!reader.Complete())
                return;
            AKunitHeat = value;
        }

        private void Server_SendUpdateAKunitColant(IList<RemoteEntityWorld> remoteEntityWorlds)
        {
            if (colantflush != Server_colantflush)
            {
                SendDataWriter writer = BeginNetworkMessage(remoteEntityWorlds, typeof(AKunit),
                    (ushort)NetworkMessages.UpdateAKunitColant);

                writer.Write(colantflush);
                EndNetworkMessage();

                Server_colantflush = colantflush;
            }
        }

        [NetworkReceive(NetworkDirections.ToClient, (ushort)NetworkMessages.UpdateAKunitColant)]
        private void Client_ReceiveUpdateAKunitColant(RemoteEntityWorld sender, ReceiveDataReader reader)
        {
            bool value = reader.ReadBoolean();
            if (!reader.Complete())
                return;
            colantflush = value;
        }

        private int Server_Weaponint;

        private void Server_SendUpdateAKunitWeaponInt(IList<RemoteEntityWorld> remoteEntityWorlds)
        {
            const int epsilon = 1;
            if (Math.Abs(i - Server_Weaponint) > epsilon)
            {
                SendDataWriter writer = BeginNetworkMessage(remoteEntityWorlds, typeof(AKunit),
                    (ushort)NetworkMessages.UpdateAKunitWeaponInt);

                writer.WriteVariableInt32(i);
                EndNetworkMessage();

                Server_Weaponint = i;
            }
        }

        [NetworkReceive(NetworkDirections.ToClient, (ushort)NetworkMessages.UpdateAKunitWeaponInt)]
        private void Client_ReceiveUpdateAKunitWeaponInt(RemoteEntityWorld sender, ReceiveDataReader reader)
        {
            int value = reader.ReadVariableInt32();
            if (!reader.Complete())
                return;
            i = value;
        }

        private int Server_WeaponMode = 0;

        private void Server_SendUpdateAKunitWeaponMode(IList<RemoteEntityWorld> remoteEntityWorlds)
        {
            int Mode = (int)CurrentFireMode;
            if (Server_WeaponMode != Mode)
            {
                SendDataWriter writer = BeginNetworkMessage(remoteEntityWorlds, typeof(AKunit),
                    (ushort)NetworkMessages.UpdateAKunitWeaponMode);

                writer.WriteVariableInt32(Mode);
                EndNetworkMessage();

                Server_WeaponMode = Mode;
            }
        }

        [NetworkReceive(NetworkDirections.ToClient, (ushort)NetworkMessages.UpdateAKunitWeaponMode)]
        private void Client_ReceiveUpdateAKunitWeaponMode(RemoteEntityWorld sender, ReceiveDataReader reader)
        {
            int value = reader.ReadVariableInt32();
            if (!reader.Complete())
                return;

            currentFireMode = (FireModes)((int)value);
        }

        //Missiles
        private void Client_SendAKunitUpdateCurrentMissileTarget()
        {
            SendDataWriter writer = BeginNetworkMessage(typeof(AKunit),
               (ushort)NetworkMessages.AKunitUpdateCurrentMissileTarget);

            if (CurrentMissileTarget != null)
            {
                writer.WriteVariableUInt32(CurrentMissileTarget.NetworkUIN);
            }
            else
            {
                writer.WriteVariableUInt32(0);
            }

            EndNetworkMessage();

            Server_currentMissileTarget = CurrentMissileTarget;
        }

        [NetworkReceive(NetworkDirections.ToServer, (ushort)NetworkMessages.AKunitUpdateCurrentMissileTarget)]
        private void Client_ReceiveAKunitUpdateCurrentMissileTarget(RemoteEntityWorld sender, ReceiveDataReader reader)
        {
            uint networkUIN = reader.ReadVariableUInt32();

            if (!reader.Complete())
                return;

            PlayerIntellect intellect1 = this.Intellect as PlayerIntellect;

            if (intellect1 == null)
                return;

            //check to ensure that other players can not send messages to another player
            if (!intellect1.Server_CheckRemoteEntityWorldAssociatedWithThisIntellect(sender))
                return;

            Unit value = (Unit)Entities.Instance.GetByNetworkUIN(networkUIN);
            CurrentMissileTarget = value;
        }

        private void Server_SendVariantToAllClients(IList<RemoteEntityWorld> remoteEntityWorlds, string variant)
        {
            SendDataWriter writer = BeginNetworkMessage(remoteEntityWorlds, typeof(AKunit),
                (ushort)NetworkMessages.VariantToClient);

            writer.Write(variant);

            EndNetworkMessage();
        }

        [NetworkReceive(NetworkDirections.ToClient, (ushort)NetworkMessages.VariantToClient)]
        private void Client_ReceiveVariant(RemoteEntityWorld sender, ReceiveDataReader reader)
        {
            string variantText = reader.ReadString();

            if (!reader.Complete())
                return;

            string[] broken = variantText.Split(new char[] { ':' }, StringSplitOptions.RemoveEmptyEntries);

            int[] finalData = new int[broken.Length];

            for (int i = 0; i < broken.Length; i++)
                finalData[i] = int.Parse(broken[i]);

            Client_SetVariant(finalData);
        }
    }
}