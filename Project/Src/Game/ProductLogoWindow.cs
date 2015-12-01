// Copyright (C) NeoAxis Group Ltd. This is part of NeoAxis 3D Engine SDK.
using Engine;
using Engine.MathEx;
using Engine.Renderer;
using Engine.SoundSystem;
using Engine.UISystem;

/*
namespace Game
{
    /// <summary>
    /// Defines a your project logo window.
    /// </summary>
    public class ProductLogoWindow : Control
    {
        private const float lifeTime = 10;

        private Texture productTexture;

        protected override void OnAttach()
        {
            base.OnAttach();

            productTexture = TextureManager.Instance.Load("GUI\\Textures\\Product.png");

            EngineApp.Instance.MouseRelativeMode = true;

            SoundWorld.Instance.SetListener(new Vec3(1000, 1000, 1000),
                Vec3.Zero, new Vec3(1, 0, 0), new Vec3(0, 0, 1));

            ResetTime();
        }

        protected override void OnDetach()
        {
            EngineApp.Instance.MouseRelativeMode = false;
            EngineApp.Instance.MousePosition = new Vec2(.9f, .8f);

            base.OnDetach();
        }

        protected override bool OnKeyDown(KeyEvent e)
        {
            if (base.OnKeyDown(e))
                return true;
            if (e.Key == EKeys.Escape || e.Key == EKeys.Return || e.Key == EKeys.Space)
            {
                Destroy();
                return true;
            }
            return false;
        }

        protected override bool OnMouseDown(EMouseButtons button)
        {
            if (button == EMouseButtons.Left || button == EMouseButtons.Right)
            {
                Destroy();
                return true;
            }
            return base.OnMouseDown(button);
        }

        protected override void OnTick(float delta)
        {
            base.OnTick(delta);
            if (Time > lifeTime)
                Destroy();
        }

        private void Destroy()
        {
            SetShouldDetach();

            //go to main menu
            GameEngineApp.Instance.ControlManager.Controls.Add(new MainMenuWindow());
        }

        protected override void OnRenderUI(GuiRenderer renderer)
        {
            base.OnRenderUI(renderer);

            Vec2 size = new Vec2(232, 335);
            //size *= 1.0f + Time * .015f;
            size /= new Vec2(768.0f * renderer.AspectRatio, 768.0f);

            Rect rectangle = new Rect(-size / 2, size / 2) + new Vec2(.3f, .5f);

            float alpha = 0;

            if (Time > 1 && Time <= 2)
                alpha = Time - 1;
            else if (Time > 2 && Time <= lifeTime - 2 - 2)
                alpha = 1;
            else if (Time >= lifeTime - 2 - 2 && Time < lifeTime - 1)
                alpha = 1 - (Time - (lifeTime - 2 - 2)) / 3;

            if (alpha != 0)
            {
                renderer.AddQuad(rectangle, new Rect(0, 0, 1, 1), productTexture,
                    new ColorValue(1, 1, 1, alpha));
            }
        }
    }
}
*/
// Copyright (C) 2006-2013 NeoAxis Group Ltd.
//using Engine;
//using Engine.MathEx;
//using Engine.Renderer;
//using Engine.SoundSystem;
//using Engine.UISystem;

namespace Game
{
    /// <summary>
    /// Defines a your project logo window.
    /// </summary>
    public class ProductLogoWindow : Control
    {
        private Control window;
        private const float lifeTime = 10;

        //Texture productTexture;

        protected override void OnAttach()
        {
            base.OnAttach();

            //create window
            window = ControlDeclarationManager.Instance.CreateControl("Gui\\ProductLogoWindow.gui");
            Controls.Add(window);

            //productTexture = TextureManager.Instance.Load("Gui\\Various\\AKLogoNew.png", Texture.Type.Type2D);
            VideoBox videoBox = window.Controls["VideoBox"] as VideoBox;

            if (string.IsNullOrEmpty(videoBox.FileName) || videoBox.IsEndOfFile())
            {
                if (string.IsNullOrEmpty(videoBox.FileName))
                    videoBox.FileName = "Gui\\Various\\akvideo2.ogv";
                if (videoBox.IsEndOfFile())
                    videoBox.Rewind();
            }
            else
                videoBox.FileName = "";

            EngineApp.Instance.MouseRelativeMode = true;

            SoundWorld.Instance.SetListener(new Vec3(1000, 1000, 1000),
                Vec3.Zero, new Vec3(1, 0, 0), new Vec3(0, 0, 1));

            ResetTime();
        }

        protected override void OnDetach()
        {
            EngineApp.Instance.MouseRelativeMode = false;
            EngineApp.Instance.MousePosition = new Vec2(.9f, .8f);

            base.OnDetach();
        }

        protected override bool OnKeyDown(KeyEvent e)
        {
            if (base.OnKeyDown(e))
                return true;
            if (e.Key == EKeys.Escape || e.Key == EKeys.Return || e.Key == EKeys.Space)
            {
                Destroy();
                return true;
            }
            return false;
        }

        protected override bool OnMouseDown(EMouseButtons button)
        {
            if (button == EMouseButtons.Left || button == EMouseButtons.Right)
            {
                Destroy();
                return true;
            }
            return base.OnMouseDown(button);
        }

        protected override void OnTick(float delta)
        {
            base.OnTick(delta);
            if (Time > lifeTime)
                Destroy();
        }

        private void Destroy()
        {
            SetShouldDetach();

            //go to main menu
            GameEngineApp.Instance.ControlManager.Controls.Add(new MainMenuWindow());
        }

        protected override void OnRenderUI(GuiRenderer renderer)
        {
            base.OnRenderUI(renderer);
            //productTexture.SourceSize = new Vec2(235, 523);
            //Vec2 size = new Vec2( 523, 235 );
            //size *= 1.0f + Time * .015f;
            //size /= new Vec2( 768.0f * renderer.AspectRatio, 768.0f );

            //Rect rectangle = new Rect(-size, size) + new Vec2(.5f, .5f);

            //float alpha = 0;

            //if( Time > 1 && Time <= 2 )
            //    alpha = Time - 1;
            //else if( Time > 2 && Time <= lifeTime - 2 - 2 )
            //    alpha = 1;
            //else if( Time >= lifeTime - 2 - 2 && Time < lifeTime - 1 )
            //    alpha = 1 - ( Time - ( lifeTime - 2 - 2 ) ) / 3;

            //if( alpha != 0 )
            //{
            //    renderer.AddQuad( rectangle, new Rect( 0, 0, 1, 1 ), productTexture,
            //        new ColorValue( 1, 1, 1, alpha ) );
            //}
        }
    }
}