namespace FPSGame.Health
{
    public interface IHealable
    {
        void Heal(int amount);
        void FullHeal();
    }
}