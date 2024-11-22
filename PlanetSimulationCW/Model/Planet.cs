using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace PlanetSimulationCW.Model
{
    public class Planet
    {
        public Guid Id { get; private set; }
        public Vector3D Position { get; private set; }
        public Vector3D Velocity { get; private set; }
        public double Mass { get; private set; }
        public double Radius { get; private set; }
        public Color Color { get; private set; }

        public bool Destroyed { get; private set; }

        public PlanetDBEntry PlanetDBEntry
        {
            get
            {
                PlanetDBEntry dbEntry = new PlanetDBEntry();

                dbEntry.Id = Id;
                dbEntry.PositionX = Position.X;
                dbEntry.PositionY = Position.Y;
                dbEntry.PositionZ = Position.Z;
                dbEntry.Mass = Mass;
                dbEntry.Radius = Radius;
                dbEntry.VelocityX = Velocity.X;
                dbEntry.VelocityY = Velocity.Y;
                dbEntry.VelocityZ = Velocity.Z;
                dbEntry.ColorR = Color.R;
                dbEntry.ColorG = Color.G;
                dbEntry.ColorB = Color.B;

                return dbEntry;
            }
        }

        public Planet(Vector3D position, double mass, double radius, Color color)
        {
            Id = Guid.NewGuid();
            Position = position;
            Mass = mass;
            Radius = radius;
            Velocity = new Vector3D(0, 0, 0);
            Color = color;
        }

        // Инициализация при помощи загруженных из бд данных
        public Planet(PlanetDBEntry dbEntry)
        {
            Id = dbEntry.Id;
            Position = new Vector3D(dbEntry.PositionX, dbEntry.PositionY, dbEntry.PositionZ);
            Mass = dbEntry.Mass;
            Radius = dbEntry.Radius;
            Velocity = new Vector3D(dbEntry.VelocityX, dbEntry.VelocityY, dbEntry.VelocityZ);
            Color = Color.FromArgb(255, dbEntry.ColorR, dbEntry.ColorG, dbEntry.ColorB);
        }

        public void Collide(Planet otherPlanet)
        {
            Mass += otherPlanet.Mass;

            // Объем планеты после столкновения
            double V = (4d / 3d * Math.PI * Math.Pow(Radius, 3) + 4d / 3d * Math.PI * Math.Pow(otherPlanet.Radius, 3));
            Radius = Math.Pow(3d / 4d * V / Math.PI, 1d / 3d);
            Velocity = Velocity + otherPlanet.Velocity * otherPlanet.Mass / Mass;
            otherPlanet.Destroyed = true;
        }

        public void AddVelocity(Vector3D addVelocity)
        {
            Velocity += addVelocity;
        }

        public void SetVelocity(Vector3D setVelocity)
        {
            Velocity = setVelocity;
        }

        public void Move(double deltaTime)
        {
            Position += Velocity * deltaTime;
        }
    }
}
