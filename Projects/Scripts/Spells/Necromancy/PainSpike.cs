using System;
using System.Collections.Generic;
using Server.Misc;
using Server.Targeting;

namespace Server.Spells.Necromancy
{
  public class PainSpikeSpell : NecromancerSpell, ISpellTargetingMobile
  {
    private static readonly SpellInfo m_Info = new SpellInfo(
      "Pain Spike", "In Sar",
      203,
      9031,
      Reagent.GraveDust,
      Reagent.PigIron);

    private static readonly Dictionary<Mobile, InternalTimer> m_Table = new Dictionary<Mobile, InternalTimer>();

    public PainSpikeSpell(Mobile caster, Item scroll = null) : base(caster, scroll, m_Info)
    {
    }

    public override TimeSpan CastDelayBase => TimeSpan.FromSeconds(1.0);

    public override double RequiredSkill => 20.0;
    public override int RequiredMana => 5;

    public override bool DelayedDamage => false;

    public override void OnCast()
    {
      Caster.Target = new SpellTargetMobile(this, TargetFlags.Harmful, Core.ML ? 10 : 12);
    }

    public void Target(Mobile m)
    {
      if (m == null)
        return;

      if (CheckHSequence(m))
      {
        SpellHelper.Turn(Caster, m);

        // SpellHelper.CheckReflect( (int)this.Circle, Caster, ref m ); //Irrelevent after AoS

        /* Temporarily causes intense physical pain to the target, dealing direct damage.
         * After 10 seconds the spell wears off, and if the target is still alive,
         * some of the Hit Points lost through Pain Spike are restored.
         */

        m.FixedParticles(0x37C4, 1, 8, 9916, 39, 3, EffectLayer.Head);
        m.FixedParticles(0x37C4, 1, 8, 9502, 39, 4, EffectLayer.Head);
        m.PlaySound(0x210);

        double damage = (GetDamageSkill(Caster) - GetResistSkill(m)) / 10 + (m.Player ? 18 : 30);
        m.CheckSkill(SkillName.MagicResist, 0.0, 120.0); // Skill check for gain

        if (damage < 1)
          damage = 1;

        TimeSpan buffTime = TimeSpan.FromSeconds(10.0);

        if (!m_Table.TryGetValue(m, out InternalTimer timer))
        {
          m_Table[m] = timer = new InternalTimer(m, damage);
          timer.Start();
        }
        else
        {
          damage = Utility.RandomMinMax(3, 7);
          timer.Delay += TimeSpan.FromSeconds(2.0);
          buffTime = timer.Next - DateTime.UtcNow;
        }

        BuffInfo.AddBuff(m, new BuffInfo(BuffIcon.PainSpike, 1075667, buffTime, m, Convert.ToString((int)damage)));

        // TODO: Find a better way to do this
        WeightOverloading.DFA = DFAlgorithm.PainSpike;
        m.Damage((int)damage, Caster);
        SpellHelper.DoLeech((int)damage, Caster, m);
        WeightOverloading.DFA = DFAlgorithm.Standard;

        // SpellHelper.Damage( this, m, damage, 100, 0, 0, 0, 0, Misc.DFAlgorithm.PainSpike );
        HarmfulSpell(m);
      }

      FinishSequence();
    }

    private class InternalTimer : Timer
    {
      private readonly Mobile m_Mobile;
      private readonly int m_ToRestore;

      public InternalTimer(Mobile m, double toRestore) : base(TimeSpan.FromSeconds(10.0))
      {
        Priority = TimerPriority.OneSecond;

        m_Mobile = m;
        m_ToRestore = (int)toRestore;
      }

      protected override void OnTick()
      {
        m_Table.Remove(m_Mobile);

        if (m_Mobile.Alive && !m_Mobile.IsDeadBondedPet)
          m_Mobile.Hits += m_ToRestore;

        BuffInfo.RemoveBuff(m_Mobile, BuffIcon.PainSpike);
      }
    }
  }
}
