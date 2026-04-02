using UnityEngine;

public abstract class SkillBase
{
    protected float cooldown;
    protected float timer;

    public virtual void Init(float cooldown)
    {
        this.cooldown = cooldown;
    }

    public virtual void Tick(Transform player)
    {
        timer += Time.deltaTime;

        if (timer >= cooldown)
        {
            Execute(player);
            timer = 0;
        }
    }

    protected abstract void Execute(Transform player);
}