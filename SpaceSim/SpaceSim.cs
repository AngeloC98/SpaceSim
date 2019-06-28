#region Using Statements
using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
#endregion

namespace SpaceSim
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class SpaceSim : Game
    {
        GraphicsDeviceManager graphDev;
        Color background = new Color(2, 0, 6);
        public static SpaceSim World;
        Vector3 cameraPosition = new Vector3(0f, 30f, 80f);
        Vector3 cameraLookAt = new Vector3(0f, 0f, 0f);
        Matrix cameraOrientationMatrix = Matrix.Identity;
        public Matrix View;
        public Matrix Projection;
        public static GraphicsDevice Graphics;

        List<Sphere> spheres;

        Sphere sun, earth, mars, jupiter, saturn, uranus, moon;
        double moonRotation = 0;
        float rollVelocity, forwardVelocity;

        Spaceship spaceship;
        Vector3 spaceshipPosition = new Vector3(0f, 28f, 77f);
        Matrix spaceshipOrientationMatrix = Matrix.CreateFromYawPitchRoll(0f, -0.17f, 0f);
        Vector3 spaceshipFollowPoint = new Vector3(0f, 0.09f, 0.2f);
        Vector3 spaceshipLookAtPoint = new Vector3(0f, 0.05f, 0f);
        Vector3 bulletSpawnPosition = new Vector3(0f, 0f, -0.1f);

        Skybox skybox;

        SpriteBatch spriteBatch;
        Texture2D reticle, controls;
        Point mousePosition;
        bool wKeyDown, aKeyDown, sKeyDown, dKeyDown;
        bool mouseButton, mouseDown, lastMouseButton;
        float reticleHalfWidth, reticleHalfHeight;

        Vector2 screenCenter;

        public SpaceSim()
            : base()
        {
            Content.RootDirectory = "Content";

            World = this;
            graphDev = new GraphicsDeviceManager(this);
        }

        protected override void Initialize()
        {
            Graphics = GraphicsDevice;

#if DEBUG
            graphDev.PreferredBackBufferWidth = 1600;
            graphDev.PreferredBackBufferHeight = 900;
            graphDev.IsFullScreen = false;
#else
            graphDev.PreferredBackBufferWidth = GraphicsDevice.DisplayMode.Width;
            graphDev.PreferredBackBufferHeight = GraphicsDevice.DisplayMode.Height;
            graphDev.IsFullScreen = true;
#endif
            graphDev.ApplyChanges();

            SetupCamera(true);
            Window.Title = "HvA - Simulation & Physics - Opdracht 6 - SpaceSim";
            spriteBatch = new SpriteBatch(Graphics);

            spheres = new List<Sphere>();

            // Planets

            spheres.Add(sun = new Sphere(Matrix.Identity, Color.Yellow, 30));
            sun.Transform = Matrix.CreateScale(2);

            spheres.Add(earth = new Sphere(Matrix.Identity, Color.DeepSkyBlue, 30));
            earth.Transform = Matrix.CreateScale(1);
            earth.Transform *= Matrix.CreateTranslation(16, 0, 0);

            spheres.Add(mars = new Sphere(Matrix.Identity, Color.Red, 30));
            mars.Transform = Matrix.CreateScale(0.6f);
            mars.Transform *= Matrix.CreateTranslation(21, 0, 0);

            spheres.Add(jupiter = new Sphere(Matrix.Identity, Color.Orange, 30));
            jupiter.Transform = Matrix.CreateScale(1.7f);
            jupiter.Transform *= Matrix.CreateTranslation(27, 0, 0);

            spheres.Add(saturn = new Sphere(Matrix.Identity, Color.Khaki, 30));
            saturn.Transform = Matrix.CreateScale(1.7f);
            saturn.Transform *= Matrix.CreateTranslation(36, 0, 0);

            spheres.Add(uranus = new Sphere(Matrix.Identity, Color.Cyan, 30));
            uranus.Transform = Matrix.CreateScale(1.5f);
            uranus.Transform *= Matrix.CreateTranslation(43, 0, 0);

            spheres.Add(moon = new Sphere(Matrix.Identity, Color.LightGray, 30));

            //  Random Y-rotation
            Random random = new Random();
            foreach (Sphere planet in spheres)
            {
                planet.Transform *= Matrix.CreateRotationY((float)(random.NextDouble() * Math.PI * 2));
            }

            base.Initialize();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            base.LoadContent();

            spaceship = new Spaceship(spaceshipOrientationMatrix * Matrix.CreateTranslation(spaceshipPosition), Content);
            skybox = new Skybox(Matrix.CreateScale(1000f) * Matrix.CreateTranslation(cameraPosition), Content);
            reticle = Content.Load<Texture2D>("Reticle");
            reticleHalfWidth = reticle.Width / 2f;
            reticleHalfHeight = reticle.Height / 2f;
            controls = Content.Load<Texture2D>("Controls");

            IsMouseVisible = false;
        }

        private void SetupCamera(bool initialize = false)
        {
            View = Matrix.CreateLookAt(cameraPosition, cameraLookAt, cameraOrientationMatrix.Up);
            if (initialize) Projection = Matrix.CreatePerspectiveFieldOfView(MathHelper.PiOver4, SpaceSim.World.GraphicsDevice.Viewport.AspectRatio, 0.1f, 2000.0f);
        }

        int i = 0;
        protected override void Draw(GameTime gameTime)
        {
            base.Draw(gameTime);

            GraphicsDevice.Clear(background);

            SetupCamera();

            skybox.Draw();

            foreach (Sphere sphere in spheres)
            {
                sphere.Draw();
            }

            spaceship.Draw();



            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearWrap, DepthStencilState.Default, RasterizerState.CullCounterClockwise);
            spriteBatch.Draw(reticle, new Vector2(mousePosition.X - reticleHalfWidth, mousePosition.Y - reticleHalfHeight), Color.White);
            spriteBatch.Draw(controls, new Vector2(10f, 10f), Color.White);
            spriteBatch.End();
        }

        protected override void Update(GameTime gameTime)
        {
            screenCenter = new Vector2((Window.ClientBounds.Width / 2), (Window.ClientBounds.Height / 2));

            cameraPosition = Vector3.Transform(spaceshipFollowPoint, spaceship.Transform);
            cameraLookAt = Vector3.Transform(spaceshipLookAtPoint, spaceship.Transform);
            cameraOrientationMatrix = spaceshipOrientationMatrix;

            // Helpers for input
            KeyboardState keyboard = Keyboard.GetState();
            wKeyDown = keyboard.IsKeyDown(Keys.W);
            aKeyDown = keyboard.IsKeyDown(Keys.A);
            sKeyDown = keyboard.IsKeyDown(Keys.S);
            dKeyDown = keyboard.IsKeyDown(Keys.D);
            if (keyboard.IsKeyDown(Keys.Escape)) Exit();
            MouseState mouse = Mouse.GetState();
            mousePosition = mouse.Position;
            mouseButton = mouse.LeftButton == ButtonState.Pressed;
            mouseDown = mouseButton && !lastMouseButton;
            lastMouseButton = mouseButton;

            skybox.Transform = Matrix.CreateScale(1000f) * Matrix.CreateTranslation(cameraPosition);

            //  Planet rotation
            Matrix earthRotation = Matrix.CreateRotationY((float)(gameTime.ElapsedGameTime.TotalSeconds * 0.15f));
            earth.Transform = earth.Transform * earthRotation;

            Matrix marsRotation = Matrix.CreateRotationY((float)(gameTime.ElapsedGameTime.TotalSeconds * 0.20f));
            mars.Transform = mars.Transform * marsRotation;

            Matrix jupiterRotation = Matrix.CreateRotationY((float)(gameTime.ElapsedGameTime.TotalSeconds * 0.25f));
            jupiter.Transform = jupiter.Transform * jupiterRotation;

            Matrix saturnRotation = Matrix.CreateRotationY((float)(gameTime.ElapsedGameTime.TotalSeconds * 0.35f));
            saturn.Transform = saturn.Transform * saturnRotation;

            Matrix uranusRotation = Matrix.CreateRotationY((float)(gameTime.ElapsedGameTime.TotalSeconds * 0.5f));
            uranus.Transform = uranus.Transform * uranusRotation;

            //  Moon rotation
            moonRotation += gameTime.ElapsedGameTime.TotalSeconds * 1.5;

            //  Moon scale and position
            moon.Transform = Matrix.CreateScale(0.5f);
            moon.Transform *= Matrix.CreateTranslation(2, 0, 0);

            //  Position relative to earth
            moon.Transform *= Matrix.CreateRotationY((float)(moonRotation));
            moon.Transform *= Matrix.CreateRotationX((float)(Math.PI / 4));
            moon.Transform *= Matrix.CreateTranslation(Vector3.Transform(Vector3.Zero, earth.Transform));

            //  Distance between mouse pos and screen center
            float mouseXPosition = -(mousePosition.X - screenCenter.X) / screenCenter.X;
            float mouseYPosition = -(mousePosition.Y - screenCenter.Y) / screenCenter.Y;

            TimeSpan elapsedGameTime = gameTime.ElapsedGameTime;

            //  Change yaw/pitch based on distance between mouse and screen center position
            float yawChange = (float)(mouseXPosition * (double)elapsedGameTime.TotalSeconds);
            float pitchChange = (float)(mouseYPosition * (double)elapsedGameTime.TotalSeconds);

            double LFSpeed = ((aKeyDown ? -1 : 0) + (dKeyDown ? 1 : 0)) * 5;
            rollVelocity = (float)(rollVelocity + (LFSpeed * elapsedGameTime.TotalSeconds));

            //  sets max speed
            if (rollVelocity > 10) rollVelocity = 10;
            else if (rollVelocity < -10) rollVelocity = -10;
            
            //  slows down velocity if keys aren't pressed
            if (!aKeyDown && !dKeyDown) rollVelocity *= 0.99f;
            float rollChange = (float)(rollVelocity * elapsedGameTime.TotalSeconds);

            RotateOrientationMatrixByYawPitchRoll(ref spaceshipOrientationMatrix, yawChange, pitchChange, rollChange);
            spaceship.Transform = spaceshipOrientationMatrix * Matrix.CreateTranslation(spaceshipPosition);

            double UDSpeed = ((wKeyDown ? 1 : 0) + (sKeyDown ? -0.5 : 0)) * 1;
            forwardVelocity = (float)(forwardVelocity + (UDSpeed * elapsedGameTime.TotalSeconds));

            //  sets max speed
            if (forwardVelocity > 10) forwardVelocity = 10;
            else if (forwardVelocity < -10) forwardVelocity = -10;

            //  slows down velocity if keys aren't pressed
            if (!wKeyDown && !sKeyDown) forwardVelocity *= 0.99f;

            Vector3 sPosition = spaceshipPosition;
            Vector3 fVelocity = (forwardVelocity * spaceshipOrientationMatrix.Forward) * (float)elapsedGameTime.TotalSeconds;
            spaceshipPosition = sPosition + fVelocity;
            Console.WriteLine(rollVelocity);

            base.Update(gameTime);
        }

        static void RotateOrientationMatrixByYawPitchRoll(ref Matrix matrix, float yawChange, float pitchChange, float rollChange)
        {
            if (rollChange != 0f || yawChange != 0f || pitchChange != 0f)
            {
                Vector3 pitch = matrix.Right * pitchChange;
                Vector3 yaw = matrix.Up * yawChange;
                Vector3 roll = matrix.Forward * rollChange;

                Vector3 overallOrientationChange = pitch + yaw + roll;
                float overallAngularChange = overallOrientationChange.Length();
                Vector3 overallRotationAxis = Vector3.Normalize(overallOrientationChange);
                Matrix orientationChange = Matrix.CreateFromAxisAngle(overallRotationAxis, overallAngularChange);
                matrix *= orientationChange;
            }
        }
    }
}
