// Copyright (C) NeoAxis Group Ltd. This is part of NeoAxis 3D Engine SDK.

namespace WinFormsMultiViewAppExample
{
    public class EmptyOptionsLeaf : OptionsLeaf
    {
        private string displayName;

        public EmptyOptionsLeaf(string displayName)
        {
            this.displayName = displayName;
        }

        public override string ToString()
        {
            return displayName;
        }
    }
}