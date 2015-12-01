#region Assembly Renderer.dll, v2.0.50727
// F:\NeoAxis1.32\NeoAxis Engine Unlimited SDK 2.1\Project\Bin\Renderer.dll
#endregion

using Engine.FileSystem;
using Engine.MathEx;
using System;
using System.Collections.Generic;

namespace Engine.Renderer
{
    public class CopyofAnimationTree
    {
        public MeshObject.AnimationState[] animationstates;
        
        public MeshObject.AnimationState[] AnimationStates 
        { 
            get {return animationstates[] };
        }
        public IList<AnimationTree.Block> Blocks { get; }
        public MeshObject MeshObject { get; }

        public void ActivateTrigger(string triggerName);
        public void BreakTrigger(string triggerName);
        public AnimationTree.Block CreateBlock(Type type);
        public void DeleteBlock(AnimationTree.Block block);
        public void DoTick(float delta);
        public IList<AnimationTree.TriggerBlock> GetActiveTriggers();
        public AnimationTree.Block GetBlockByUIN(int uin);
        public AnimationTree.OutputBlock GetOutputBlock();
        public bool Load(TextBlock block, out string error);
        public void ResetItemsTimePosition(AnimationTree.AnimationListOutput output);
        public void SaveToFile(string realFileName);
        public void SaveToTextBlock(TextBlock textBlock);
        public void SetParameterValue(string parameterName, float value);
        public void StopAllAnimationStates();
        public void UpdateMeshObjectAnimationStates(float delta);
        public bool WorldLoad(TextBlock textBlock, out string error);
        public void WorldSave(TextBlock textBlock);

        public abstract class AnimationListBlock : AnimationTree.Block
        {
            protected AnimationListBlock();

            public AnimationTree.AnimationListOutput Calculate();
            protected abstract AnimationTree.AnimationListOutput OnCalculate();
        }

        public class AnimationListOutput
        {
            public AnimationListOutput(AnimationTree tree);

            // Summary:
            //     Don't modify list.
            public List<AnimationTree.AnimationListOutput.Item> Items { get; }

            public AnimationTree.AnimationListOutput.Item AddItem(int animationIndex, float speed, float weight);
            public AnimationTree.AnimationListOutput.Item GetItemByAnimationIndex(int animationIndex);
            public void RemoveItemsWithZeroWeight();

            public class Item
            {
                public Item(int animationIndex, float speed, float weight);

                public int AnimationIndex { get; set; }
                public float Speed { get; set; }
                public float Weight { get; set; }

                public override string ToString();
            }
        }

        public class AnimationSourceBlock : AnimationTree.AnimationListBlock
        {
            public AnimationSourceBlock();

            public string AnimationName { get; set; }
            public AnimationTree.ValueBlock SpeedSource { get; set; }
            public float SpeedValue { get; set; }

            public float CalculateSpeed();
            protected override AnimationTree.AnimationListOutput OnCalculate();
            protected override void OnClone(AnimationTree.Block source);
            protected override void OnDeleteBlock(AnimationTree.Block block);
            protected override bool OnLoad(TextBlock textBlock, out string error);
            protected override void OnSave(TextBlock textBlock);
        }

        public class BlendBlock : AnimationTree.AnimationListBlock
        {
            public BlendBlock();

            public IList<AnimationTree.AnimationListBlock> AnimationSources { get; }
            public AnimationTree.ValueBlock WeightSource { get; set; }
            public float WeightValue { get; set; }

            public float CalculateWeight();
            public AnimationTree.AnimationListBlock GetAnimationSource(int index);
            protected override AnimationTree.AnimationListOutput OnCalculate();
            protected override void OnClone(AnimationTree.Block source);
            protected override void OnDeleteBlock(AnimationTree.Block block);
            protected override bool OnLoad(TextBlock textBlock, out string error);
            protected override void OnSave(TextBlock textBlock);
            public void SetAnimationSource(int index, AnimationTree.AnimationListBlock block);
        }

        public class Block
        {
            public Block();

            public AnimationTree Tree { get; }
            public int UIN { get; }

            protected virtual void OnClone(AnimationTree.Block source);
            protected virtual void OnDeleteBlock(AnimationTree.Block block);
            protected virtual bool OnLoad(TextBlock textBlock, out string error);
            protected virtual void OnSave(TextBlock textBlock);
            protected virtual void OnTick(float delta);
            protected virtual bool OnWorldLoad(TextBlock textBlock, out string error);
            protected virtual void OnWorldSave(TextBlock textBlock);
        }

        public class Direction8SidesBlock : AnimationTree.AnimationListBlock
        {
            public Direction8SidesBlock();

            public AnimationTree.ValueBlock AngleSource { get; set; }
            public Degree AngleValue { get; set; }
            public AnimationTree.AnimationListBlock[] AnimationSources { get; }
            public AnimationTree.ValueBlock SpeedMultiplierSource { get; set; }
            public float SpeedMultiplierValue { get; set; }
            public AnimationTree.Direction8SidesBlock.Directions TransitionFrom { get; set; }
            public float TransitionProgress { get; set; }
            public AnimationTree.ValueBlock TransitionTimeSource { get; set; }
            public float TransitionTimeValue { get; set; }
            public AnimationTree.Direction8SidesBlock.Directions TransitionTo { get; set; }

            public Degree CalculateAngle();
            public AnimationTree.Direction8SidesBlock.Directions CalculateDirection();
            public float CalculateSpeedMultiplier();
            public float CalculateTransitionTime();
            protected override AnimationTree.AnimationListOutput OnCalculate();
            protected override void OnClone(AnimationTree.Block source);
            protected override void OnDeleteBlock(AnimationTree.Block block);
            protected override bool OnLoad(TextBlock textBlock, out string error);
            protected override void OnSave(TextBlock textBlock);
            protected override void OnTick(float delta);

            public enum Directions
            {
                Forward = 0,
                ForwardLeft = 1,
                ForwardRight = 2,
                Left = 3,
                Right = 4,
                Backward = 5,
                BackwardLeft = 6,
                BackwardRight = 7,
            }
        }

        public class LookAtBlock : AnimationTree.AnimationListBlock
        {
            public LookAtBlock();

            public AnimationTree.AnimationListBlock[] AnimationSources { get; }
            public AnimationTree.ValueBlock HorizontalAngleSource { get; set; }
            public AnimationTree.ValueBlock HorizontalAngleSpeedSource { get; set; }
            public float HorizontalAngleSpeedValue { get; set; }
            public Degree HorizontalAngleValue { get; set; }
            public AnimationTree.ValueBlock SpeedMultiplierSource { get; set; }
            public float SpeedMultiplierValue { get; set; }
            public Degree TransitionHorizontalAngle { get; set; }
            public Degree TransitionVerticalAngle { get; set; }
            public AnimationTree.ValueBlock VerticalAngleSource { get; set; }
            public AnimationTree.ValueBlock VerticalAngleSpeedSource { get; set; }
            public float VerticalAngleSpeedValue { get; set; }
            public Degree VerticalAngleValue { get; set; }

            public Degree CalculateHorizontalAngle();
            public float CalculateHorizontalAngleSpeed();
            public float CalculateSpeedMultiplier();
            public Degree CalculateVerticalAngle();
            public float CalculateVerticalAngleSpeed();
            protected override AnimationTree.AnimationListOutput OnCalculate();
            protected override void OnClone(AnimationTree.Block source);
            protected override void OnDeleteBlock(AnimationTree.Block block);
            protected override bool OnLoad(TextBlock textBlock, out string error);
            protected override void OnSave(TextBlock textBlock);
            protected override void OnTick(float delta);
            protected override bool OnWorldLoad(TextBlock textBlock, out string error);
            protected override void OnWorldSave(TextBlock textBlock);

            public enum Directions
            {
                LeftUp = 0,
                LeftCenter = 1,
                LeftDown = 2,
                CenterUp = 3,
                CenterCenter = 4,
                CenterDown = 5,
                RightUp = 6,
                RightCenter = 7,
                RightDown = 8,
            }
        }

        public class OutputBlock : AnimationTree.AnimationListBlock
        {
            public OutputBlock();

            public AnimationTree.AnimationListBlock InSource { get; set; }

            protected override AnimationTree.AnimationListOutput OnCalculate();
            protected override void OnClone(AnimationTree.Block source);
            protected override void OnDeleteBlock(AnimationTree.Block block);
            protected override bool OnLoad(TextBlock textBlock, out string error);
            protected override void OnSave(TextBlock textBlock);
        }

        public class ParameterBlock : AnimationTree.ValueBlock
        {
            public ParameterBlock();

            public string ParameterName { get; set; }
            public float Value { get; set; }

            protected override float OnCalculate();
            protected override void OnClone(AnimationTree.Block source);
            protected override bool OnLoad(TextBlock textBlock, out string error);
            protected override void OnSave(TextBlock textBlock);
            protected override bool OnWorldLoad(TextBlock textBlock, out string error);
            protected override void OnWorldSave(TextBlock textBlock);
        }

        public class SumBlock : AnimationTree.AnimationListBlock
        {
            public SumBlock();

            public IList<AnimationTree.AnimationListBlock> AnimationSources { get; }

            public AnimationTree.AnimationListBlock GetAnimationSource(int index);
            protected override AnimationTree.AnimationListOutput OnCalculate();
            protected override void OnClone(AnimationTree.Block source);
            protected override void OnDeleteBlock(AnimationTree.Block block);
            protected override bool OnLoad(TextBlock textBlock, out string error);
            protected override void OnSave(TextBlock textBlock);
            public void SetAnimationSource(int index, AnimationTree.AnimationListBlock block);
        }

        public class TransitionBlock : AnimationTree.AnimationListBlock
        {
            public TransitionBlock();

            public IList<AnimationTree.AnimationListBlock> AnimationSources { get; }
            public AnimationTree.ValueBlock SelectedIndexSource { get; set; }
            public int SelectedIndexValue { get; set; }
            public int TransitionFrom { get; set; }
            public float TransitionProgress { get; set; }
            public AnimationTree.ValueBlock TransitionTimeSource { get; set; }
            public float TransitionTimeValue { get; set; }
            public int TransitionTo { get; set; }

            public int CalculateSelectedIndex();
            public float CalculateTransitionTime();
            public AnimationTree.AnimationListBlock GetAnimationSource(int index);
            protected override AnimationTree.AnimationListOutput OnCalculate();
            protected override void OnClone(AnimationTree.Block source);
            protected override void OnDeleteBlock(AnimationTree.Block block);
            protected override bool OnLoad(TextBlock textBlock, out string error);
            protected override void OnSave(TextBlock textBlock);
            protected override void OnTick(float delta);
            public void SetAnimationSource(int index, AnimationTree.AnimationListBlock block);
        }

        public class TriggerBlock : AnimationTree.AnimationListBlock
        {
            public TriggerBlock();

            public bool Active { get; }
            public float ActiveTime { get; set; }
            public float ActiveTimeLength { get; set; }
            public AnimationTree.AnimationListBlock OffSource { get; set; }
            public AnimationTree.AnimationListBlock OnSource { get; set; }
            public AnimationTree.ValueBlock TransitionTimeSource { get; set; }
            public float TransitionTimeValue { get; set; }
            public float TransitionWeight { get; set; }
            public string TriggerName { get; set; }

            public void Activate();
            public void Break();
            public float CalculateTransitionTime();
            protected override AnimationTree.AnimationListOutput OnCalculate();
            protected override void OnClone(AnimationTree.Block source);
            protected override void OnDeleteBlock(AnimationTree.Block block);
            protected override bool OnLoad(TextBlock textBlock, out string error);
            protected override void OnSave(TextBlock textBlock);
            protected override void OnTick(float delta);
            protected override bool OnWorldLoad(TextBlock textBlock, out string error);
            protected override void OnWorldSave(TextBlock textBlock);
        }

        public abstract class ValueBlock : AnimationTree.Block
        {
            protected ValueBlock();

            public float Calculate();
            protected abstract float OnCalculate();
        }
    }
}