namespace Sagittaras.Progression
{
    public readonly partial struct Experience
    {
        public static Experience operator +(Experience left, Experience right)
        {
            return new Experience(left.Value + right.Value);
        }
        
        public static Experience operator -(Experience left, Experience right)
        {
            return new Experience(left.Value - right.Value);
        }
    }
}