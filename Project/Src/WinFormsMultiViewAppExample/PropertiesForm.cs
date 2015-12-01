// Copyright (C) NeoAxis Group Ltd. This is part of NeoAxis 3D Engine SDK.
using System.Windows.Forms;
using WeifenLuo.WinFormsUI.Docking;

namespace WinFormsMultiViewAppExample
{
    public partial class PropertiesForm : DockContent
    {
        public PropertiesForm()
        {
            InitializeComponent();
        }

        public void OnClose()
        {
        }

        public object[] GetSelectedObjects()
        {
            return propertyGrid1.SelectedObjects;
        }

        public void SelectObjects(object[] objects)
        {
            if (objects != null && objects.Length != 0)
                propertyGrid1.SelectedObjects = objects;
            else
                propertyGrid1.SelectedObject = null;
        }

        public PropertyGrid GetPropertyGrid()
        {
            return propertyGrid1;
        }
    }
}