﻿using System;
using System.Collections.Generic;
using Server.Factions;
using Server.Mobiles;
using Server.Spells.Fifth;
using Server.Spells.Ninjitsu;
using Server.Spells.Seventh;

namespace Server.Spells.Mysticism
{
  public class StoneFormSpell : MysticSpell
  {
    private static SpellInfo m_Info = new SpellInfo(
      "Stone Form", "In Rel Ylem",
      -1,
      9002,
      Reagent.Bloodmoss,
      Reagent.FertileDirt,
      Reagent.Garlic
    );

    private static Dictionary<Mobile, ResistanceMod[]> m_Table = new Dictionary<Mobile, ResistanceMod[]>();

    public StoneFormSpell(Mobile caster, Item scroll = null)
      : base(caster, scroll, m_Info)
    {
    }

    public override TimeSpan CastDelayBase => TimeSpan.FromSeconds(1.5);

    public override double RequiredSkill => 33.0;
    public override int RequiredMana => 11;

    public static void Initialize()
    {
      EventSink.PlayerDeath += OnPlayerDeath;
    }

    public static bool UnderEffect(Mobile m) => m_Table.ContainsKey(m);

    public override bool CheckCast()
    {
      if (Sigil.ExistsOn(Caster))
      {
        Caster.SendLocalizedMessage(1061632); // You can't do that while carrying the sigil.
        return false;
      }

      if (!Caster.CanBeginAction<PolymorphSpell>())
      {
        Caster.SendLocalizedMessage(1061628); // You can't do that while polymorphed.
        return false;
      }

      if (AnimalForm.UnderTransformation(Caster))
      {
        Caster.SendLocalizedMessage(1063218); // You cannot use that ability in this form.
        return false;
      }

      if (Caster.Flying)
      {
        Caster.SendLocalizedMessage(1113415); // You cannot use this ability while flying.
        return false;
      }

      return base.CheckCast();
    }

    public override void OnCast()
    {
      if (Sigil.ExistsOn(Caster))
      {
        Caster.SendLocalizedMessage(1061632); // You can't do that while carrying the sigil.
      }
      else if (!Caster.CanBeginAction<PolymorphSpell>())
      {
        Caster.SendLocalizedMessage(1061628); // You can't do that while polymorphed.
      }
      else if (!Caster.CanBeginAction<IncognitoSpell>() || Caster.IsBodyMod && !UnderEffect(Caster))
      {
        Caster.SendLocalizedMessage(1063218); // You cannot use that ability in this form.
      }
      else if (CheckSequence())
      {
        if (UnderEffect(Caster))
        {
          RemoveEffects(Caster);

          Caster.PlaySound(0xFA);
          Caster.Delta(MobileDelta.Resistances);
        }
        else
        {
          IMount mount = Caster.Mount;

          if (mount != null)
            mount.Rider = null;

          Caster.BodyMod = 0x2C1;
          Caster.HueMod = 0;

          int offset = (int)((GetBaseSkill(Caster) + GetBoostSkill(Caster)) / 24.0);

          ResistanceMod[] mods = {
            new ResistanceMod(ResistanceType.Physical, offset),
            new ResistanceMod(ResistanceType.Fire, offset),
            new ResistanceMod(ResistanceType.Cold, offset),
            new ResistanceMod(ResistanceType.Poison, offset),
            new ResistanceMod(ResistanceType.Energy, offset)
          };

          for (int i = 0; i < mods.Length; ++i)
            Caster.AddResistanceMod(mods[i]);

          m_Table[Caster] = mods;

          Caster.PlaySound(0x65A);
          Caster.Delta(MobileDelta.Resistances);

          BuffInfo.AddBuff(Caster, new BuffInfo(BuffIcon.StoneForm, 1080145, 1080146,
            $"-10\t-2\t{offset}\t{GetResistCapBonus(Caster)}\t{GetDIBonus(Caster)}", false));
        }
      }

      FinishSequence();
    }

    public static int GetDIBonus(Mobile m) => (int)((GetBaseSkill(m) + GetBoostSkill(m)) / 12.0);

    public static int GetResistCapBonus(Mobile m) => (int)((GetBaseSkill(m) + GetBoostSkill(m)) / 48.0);

    public static void RemoveEffects(Mobile m)
    {
      if (!m_Table.TryGetValue(m, out ResistanceMod[] mods))
        return;

      for (int i = 0; i < mods.Length; ++i)
        m.RemoveResistanceMod(mods[i]);

      m.BodyMod = 0;
      m.HueMod = -1;

      m_Table.Remove(m);

      BuffInfo.RemoveBuff(m, BuffIcon.StoneForm);
    }

    private static void OnPlayerDeath(Mobile m)
    {
      RemoveEffects(m);
    }
  }
}
