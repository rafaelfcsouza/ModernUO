using System;
using System.Collections.Generic;
using Server.Engines.Quests;
using Server.Engines.Quests.Necro;
using Server.Items;
using Server.Mobiles;
using Server.Targeting;
using Server.Utilities;

namespace Server.Spells.Necromancy
{
  public class AnimateDeadSpell : NecromancerSpell, ISpellTargetingItem
  {
    private static SpellInfo m_Info = new SpellInfo(
      "Animate Dead", "Uus Corp",
      203,
      9031,
      Reagent.GraveDust,
      Reagent.DaemonBlood
    );

    private static CreatureGroup[] m_Groups =
    {
      // Undead group--empty
      new CreatureGroup(SlayerGroup.GetEntryByName(SlayerName.Silver).Types, new SummonEntry[0]),
      // Insects
      new CreatureGroup(new[]
        {
          typeof(DreadSpider), typeof(FrostSpider), typeof(GiantSpider), typeof(GiantBlackWidow),
          typeof(BlackSolenInfiltratorQueen), typeof(BlackSolenInfiltratorWarrior),
          typeof(BlackSolenQueen), typeof(BlackSolenWarrior), typeof(BlackSolenWorker),
          typeof(RedSolenInfiltratorQueen), typeof(RedSolenInfiltratorWarrior),
          typeof(RedSolenQueen), typeof(RedSolenWarrior), typeof(RedSolenWorker),
          typeof(TerathanAvenger), typeof(TerathanDrone), typeof(TerathanMatriarch),
          typeof(TerathanWarrior)
          // TODO: Giant beetle? Ant lion? Ophidians?
        },
        new[]
        {
          new SummonEntry(0, typeof(MoundOfMaggots))
        }),
      // Mounts
      new CreatureGroup(new[]
      {
        typeof(Horse), typeof(Nightmare), typeof(FireSteed),
        typeof(Kirin), typeof(Unicorn)
      }, new[]
      {
        new SummonEntry(10000, typeof(HellSteed)),
        new SummonEntry(0, typeof(SkeletalMount))
      }),
      // Elementals
      new CreatureGroup(new[]
      {
        typeof(BloodElemental), typeof(EarthElemental), typeof(SummonedEarthElemental),
        typeof(AgapiteElemental), typeof(BronzeElemental), typeof(CopperElemental),
        typeof(DullCopperElemental), typeof(GoldenElemental), typeof(ShadowIronElemental),
        typeof(ValoriteElemental), typeof(VeriteElemental), typeof(PoisonElemental),
        typeof(FireElemental), typeof(SummonedFireElemental), typeof(SnowElemental),
        typeof(AirElemental), typeof(SummonedAirElemental), typeof(WaterElemental),
        typeof(SummonedAirElemental), typeof(AcidElemental)
      }, new[]
      {
        new SummonEntry(5000, typeof(WailingBanshee)),
        new SummonEntry(0, typeof(Wraith))
      }),
      // Dragons
      new CreatureGroup(new[]
      {
        typeof(AncientWyrm), typeof(Dragon), typeof(GreaterDragon), typeof(SerpentineDragon),
        typeof(ShadowWyrm), typeof(SkeletalDragon), typeof(WhiteWyrm),
        typeof(Drake), typeof(Wyvern), typeof(LesserHiryu), typeof(Hiryu)
      }, new[]
      {
        new SummonEntry(18000, typeof(SkeletalDragon)),
        new SummonEntry(10000, typeof(FleshGolem)),
        new SummonEntry(5000, typeof(Lich)),
        new SummonEntry(3000, typeof(SkeletalKnight), typeof(BoneKnight)),
        new SummonEntry(2000, typeof(Mummy)),
        new SummonEntry(1000, typeof(SkeletalMage), typeof(BoneMagi)),
        new SummonEntry(0, typeof(PatchworkSkeleton))
      }),
      // Default group
      new CreatureGroup(new Type[0], new[]
      {
        new SummonEntry(18000, typeof(LichLord)),
        new SummonEntry(10000, typeof(FleshGolem)),
        new SummonEntry(5000, typeof(Lich)),
        new SummonEntry(3000, typeof(SkeletalKnight), typeof(BoneKnight)),
        new SummonEntry(2000, typeof(Mummy)),
        new SummonEntry(1000, typeof(SkeletalMage), typeof(BoneMagi)),
        new SummonEntry(0, typeof(PatchworkSkeleton))
      })
    };

    private static Dictionary<Mobile, List<Mobile>> m_Table = new Dictionary<Mobile, List<Mobile>>();

    public AnimateDeadSpell(Mobile caster, Item scroll = null) : base(caster, scroll, m_Info)
    {
    }

    public override TimeSpan CastDelayBase => TimeSpan.FromSeconds(1.5);

    public override double RequiredSkill => 40.0;
    public override int RequiredMana => 23;

    public override void OnCast()
    {
      Caster.Target = new SpellTargetItem(this, TargetFlags.None, Core.ML ? 10 : 12);
      Caster.SendLocalizedMessage(1061083); // Animate what corpse?
    }

    private static CreatureGroup FindGroup(Type type)
    {
      for (int i = 0; i < m_Groups.Length; ++i)
      {
        CreatureGroup group = m_Groups[i];
        Type[] types = group.m_Types;

        bool contains = types.Length == 0;

        for (int j = 0; !contains && j < types.Length; ++j)
          contains = types[j].IsAssignableFrom(type);

        if (contains)
          return group;
      }

      return null;
    }

    public void Target(Item item)
    {
      MaabusCoffinComponent comp = item as MaabusCoffinComponent;

      if (comp?.Addon is MaabusCoffin addon)
      {
        PlayerMobile pm = Caster as PlayerMobile;

        QuestSystem qs = pm?.Quest;

        if (qs is DarkTidesQuest)
        {
          QuestObjective objective = qs.FindObjective<AnimateMaabusCorpseObjective>();

          if (objective?.Completed == false)
          {
            addon.Awake(Caster);
            objective.Complete();
          }
        }

        return;
      }

      if (!(item is Corpse c))
      {
        Caster.SendLocalizedMessage(1061084); // You cannot animate that.
      }
      else
      {
        Type type = null;

        if (c.Owner != null) type = c.Owner.GetType();

        if (c.ItemID != 0x2006 || c.Animated || type == typeof(PlayerMobile) || type == null ||
            c.Owner != null && c.Owner.Fame < 100 || c.Owner is BaseCreature creature &&
            (creature.Summoned || creature.IsBonded))
        {
          Caster.SendLocalizedMessage(1061085); // There's not enough life force there to animate.
        }
        else
        {
          CreatureGroup group = FindGroup(type);

          if (group != null)
          {
            if (group.m_Entries.Length == 0 || type == typeof(DemonKnight))
            {
              Caster.SendLocalizedMessage(1061086); // You cannot animate undead remains.
            }
            else if (CheckSequence())
            {
              Point3D p = c.GetWorldLocation();
              Map map = c.Map;

              if (map != null)
              {
                Effects.PlaySound(p, map, 0x1FB);
                Effects.SendLocationParticles(EffectItem.Create(p, map, EffectItem.DefaultDuration), 0x3789,
                  1, 40, 0x3F, 3, 9907, 0);

                Timer.DelayCall(TimeSpan.FromSeconds(2.0),
                  () => SummonDelay_Callback(Caster, c, p, map, group));
              }
            }
          }
        }
      }

      FinishSequence();
    }

    public static void Unregister(Mobile master, Mobile summoned)
    {
      if (master == null)
        return;

      if (!m_Table.TryGetValue(master, out List<Mobile> list))
        return;

      list.Remove(summoned);

      if (list.Count == 0)
        m_Table.Remove(master);
    }

    public static void Register(Mobile master, Mobile summoned)
    {
      if (master == null)
        return;

      if (!m_Table.TryGetValue(master, out List<Mobile> list))
        m_Table[master] = list = new List<Mobile>();

      for (int i = list.Count - 1; i >= 0; --i)
      {
        if (i >= list.Count)
          continue;

        Mobile mob = list[i];

        if (mob.Deleted)
          list.RemoveAt(i--);
      }

      list.Add(summoned);

      if (list.Count > 3)
        Timer.DelayCall(TimeSpan.Zero, list[0].Kill);

      Timer.DelayCall(TimeSpan.FromSeconds(2.0), TimeSpan.FromSeconds(2.0), Summoned_Damage, summoned);
    }

    private static void Summoned_Damage(Mobile mob)
    {
      if (mob.Hits > 0)
        --mob.Hits;
      else
        mob.Kill();
    }

    private static void SummonDelay_Callback(Mobile caster, Corpse corpse, Point3D loc, Map map, CreatureGroup group)
    {
      if (corpse.Animated)
        return;

      Mobile owner = corpse.Owner;

      if (owner == null)
        return;

      double necromancy = caster.Skills.Necromancy.Value;
      double spiritSpeak = caster.Skills.SpiritSpeak.Value;

      int casterAbility = 0;

      casterAbility += (int)(necromancy * 30);
      casterAbility += (int)(spiritSpeak * 70);
      casterAbility /= 10;
      casterAbility *= 18;

      if (casterAbility > owner.Fame)
        casterAbility = owner.Fame;

      if (casterAbility < 0)
        casterAbility = 0;

      Type toSummon = null;
      SummonEntry[] entries = group.m_Entries;

      for (int i = 0; toSummon == null && i < entries.Length; ++i)
      {
        SummonEntry entry = entries[i];

        if (casterAbility < entry.m_Requirement)
          continue;

        Type[] animates = entry.m_ToSummon;

        if (animates.Length >= 0)
          toSummon = animates[Utility.Random(animates.Length)];
      }

      if (toSummon == null)
        return;

      Mobile summoned = null;

      try
      {
        summoned = ActivatorUtil.CreateInstance(toSummon) as Mobile;
      }
      catch
      {
        // ignored
      }

      if (summoned == null)
        return;

      if (summoned is BaseCreature bc)
      {
        // to be sure
        bc.Tamable = false;

        bc.ControlSlots = bc is BaseMount ? 1 : 0;

        Effects.PlaySound(loc, map, bc.GetAngerSound());

        BaseCreature.Summon(bc, false, caster, loc, 0x28, TimeSpan.FromDays(1.0));
      }

      if (summoned is SkeletalDragon dragon)
        Scale(dragon, 50); // lose 50% hp and strength

      summoned.Fame = 0;
      summoned.Karma = -1500;

      summoned.MoveToWorld(loc, map);

      corpse.Hue = 1109;
      corpse.Animated = true;

      Register(caster, summoned);
    }

    public static void Scale(BaseCreature bc, int scalar)
    {
      int toScale = bc.RawStr;
      bc.RawStr = AOS.Scale(toScale, scalar);

      toScale = bc.HitsMaxSeed;

      if (toScale > 0)
        bc.HitsMaxSeed = AOS.Scale(toScale, scalar);

      bc.Hits = bc.Hits; // refresh hits
    }

    private class CreatureGroup
    {
      public SummonEntry[] m_Entries;
      public Type[] m_Types;

      public CreatureGroup(Type[] types, SummonEntry[] entries)
      {
        m_Types = types;
        m_Entries = entries;
      }
    }

    private class SummonEntry
    {
      public int m_Requirement;
      public Type[] m_ToSummon;

      public SummonEntry(int requirement, params Type[] toSummon)
      {
        m_ToSummon = toSummon;
        m_Requirement = requirement;
      }
    }
  }
}
