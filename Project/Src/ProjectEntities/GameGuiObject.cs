// Copyright (C) NeoAxis Group Ltd. This is part of NeoAxis 3D Engine SDK.
using System;
using System.ComponentModel;
using System.Drawing.Design;
using Engine;
using Engine.MapSystem;
using Engine.UISystem;

namespace ProjectEntities
{
    /// <summary>
    /// Defines the <see cref="GameGuiObject"/> entity type.
    /// </summary>
    public class GameGuiObjectType : DynamicType
    {
    }

    public class GameGuiObject : Dynamic
    {
        private GameGuiObjectType _type = null; public new GameGuiObjectType Type { get { return _type; } }

        [FieldSerialize]
        private string initialControl = "";

        private MapObjectAttachedGui attachedGuiObject;
        private In3dControlManager controlManager;
        private Control mainControl;

        [Editor(typeof(EditorGuiUITypeEditor), typeof(UITypeEditor))]
        public string InitialControl
        {
            get { return initialControl; }
            set
            {
                initialControl = value;
                CreateMainControl();
            }
        }

        [Browsable(false)]
        [LogicSystemBrowsable(true)]
        public In3dControlManager ControlManager
        {
            get { return controlManager; }
        }

        [Browsable(false)]
        [LogicSystemBrowsable(true)]
        public Control MainControl
        {
            get { return mainControl; }
            set { mainControl = value; }
        }

        /// <summary>Overridden from <see cref="Engine.EntitySystem.Entity.OnPostCreate(Boolean)"/>.</summary>
        protected override void OnPostCreate(bool loaded)
        {
            base.OnPostCreate(loaded);

            foreach (MapObjectAttachedObject attachedObject in AttachedObjects)
            {
                attachedGuiObject = attachedObject as MapObjectAttachedGui;
                if (attachedGuiObject != null)
                {
                    controlManager = attachedGuiObject.ControlManager;
                    break;
                }
            }

            CreateMainControl();
        }

        /// <summary>Overridden from <see cref="Engine.EntitySystem.Entity.OnDestroy()"/>.</summary>
        protected override void OnDestroy()
        {
            mainControl = null;
            controlManager = null;
            base.OnDestroy();
        }

        private void CreateMainControl()
        {
            if (mainControl != null)
            {
                mainControl.Parent.Controls.Remove(mainControl);
                mainControl = null;
            }

            if (controlManager != null && !string.IsNullOrEmpty(initialControl))
            {
                mainControl = ControlDeclarationManager.Instance.CreateControl(initialControl);
                if (mainControl != null)
                    controlManager.Controls.Add(mainControl);
            }

            //update MapBounds
            SetTransform(Position, Rotation, Scale);
        }
    }
}