using Microsoft.Xna.Framework;

namespace SpaceSim
{
    class Bullet : Sphere
    {
        public Vector3 position, velocity;

        public Bullet(Vector3 position, Vector3 velocity) : base(Matrix.CreateScale(0.005f) * Matrix.CreateTranslation(position), Color.White, 30)
        {
            this.position = position;
            this.velocity = velocity;
        }

        public void Update(float timeElapsed)
        {
            //  Change position over time
            position += velocity * timeElapsed;
            this.Transform = Matrix.CreateScale(0.005f) * Matrix.CreateTranslation(position);
        }
    }
}
