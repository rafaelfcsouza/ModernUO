using System.Collections.Generic;

namespace Server.Spells.First
{
  public class ReactiveArmorSpell : MagerySpell
  {
    private static SpellInfo m_Info = new SpellInfo(
      "Reactive Armor", "Flam Sanct",
      236,
      9011,
      Reagent.Garlic,
      Reagent.SpidersSilk,
      Reagent.SulfurousAsh
    );

    private static Dictionary<Mobile, ResistanceMod[]> m_Table = new Dictionary<Mobile, ResistanceMod[]>();

    public ReactiveArmorSpell(Mobile caster, Item scroll = null) : base(caster, scroll, m_Info)
    {
    }

    public override SpellCircle Circle => SpellCircle.First;

    public override bool CheckCast()
    {
      if (Core.AOS)
        return true;

      if (Caster.MeleeDamageAbsorb > 0)
      {
        Caster.SendLocalizedMessage(1005559); // This spell is already in effect.
        return false;
      }

      if (!Caster.CanBeginAction<DefensiveSpell>())
      {
        Caster.SendLocalizedMessage(1005385); // The spell will not adhere to you at this time.
        return false;
      }

      return true;
    }

    public override void OnCast()
    {
      if (Core.AOS)
      {
        /* The reactive armor spell increases the caster's physical resistance, while lowering the caster's elemental resistances.
         * 15 + (Inscription/20) Physcial bonus
         * -5 Elemental
         * The reactive armor spell has an indefinite duration, becoming active when cast, and deactivated when re-cast.
         * Reactive Armor, Protection, and Magic Reflection will stay on�even after logging out, even after dying�until you �turn them off� by casting them again.
         * (+20 physical -5 elemental at 100 Inscription)
         */

        if (CheckSequence())
        {
          Mobile targ = Caster;

          if (!m_Table.TryGetValue(targ, out ResistanceMod[] mods))
          {
            targ.PlaySound(0x1E9);
            targ.FixedParticles(0x376A, 9, 32, 5008, EffectLayer.Waist);

            mods = new []
            {
              new ResistanceMod(ResistanceType.Physical,
                15 + (int)(targ.Skills.Inscribe.Value / 20)),
              new ResistanceMod(ResistanceType.Fire, -5),
              new ResistanceMod(ResistanceType.Cold, -5),
              new ResistanceMod(ResistanceType.Poison, -5),
              new ResistanceMod(ResistanceType.Energy, -5)
            };

            m_Table[targ] = mods;

            for (int i = 0; i < mods.Length; ++i)
              targ.AddResistanceMod(mods[i]);

            int physresist = 15 + (int)(targ.Skills.Inscribe.Value / 20);
            string args = $"{physresist}\t{5}\t{5}\t{5}\t{5}";

            BuffInfo.AddBuff(Caster, new BuffInfo(BuffIcon.ReactiveArmor, 1075812, 1075813, args));
          }
          else
          {
            targ.PlaySound(0x1ED);
            targ.FixedParticles(0x376A, 9, 32, 5008, EffectLayer.Waist);

            m_Table.Remove(targ);

            for (int i = 0; i < mods.Length; ++i)
              targ.RemoveResistanceMod(mods[i]);

            BuffInfo.RemoveBuff(Caster, BuffIcon.ReactiveArmor);
          }
        }

        FinishSequence();
      }
      else
      {
        if (Caster.MeleeDamageAbsorb > 0)
        {
          Caster.SendLocalizedMessage(1005559); // This spell is already in effect.
        }
        else if (!Caster.CanBeginAction<DefensiveSpell>())
        {
          Caster.SendLocalizedMessage(1005385); // The spell will not adhere to you at this time.
        }
        else if (CheckSequence())
        {
          if (Caster.BeginAction<DefensiveSpell>())
          {
            int value = (int)(Caster.Skills.Magery.Value + Caster.Skills.Meditation.Value +
                              Caster.Skills.Inscribe.Value);
            value /= 3;

            if (value < 0)
              value = 1;
            else if (value > 75)
              value = 75;

            Caster.MeleeDamageAbsorb = value;

            Caster.FixedParticles(0x376A, 9, 32, 5008, EffectLayer.Waist);
            Caster.PlaySound(0x1F2);
          }
          else
          {
            Caster.SendLocalizedMessage(1005385); // The spell will not adhere to you at this time.
          }
        }

        FinishSequence();
      }
    }

    public static void EndArmor(Mobile m)
    {
      if (!m_Table.TryGetValue(m, out ResistanceMod[] mods))
        return;

      for (int i = 0; i < mods?.Length; ++i)
        m.RemoveResistanceMod(mods[i]);

      m_Table.Remove(m);
      BuffInfo.RemoveBuff(m, BuffIcon.ReactiveArmor);
    }
  }
}