using System;
using System.Collections.Generic;
using System.Linq;
using Server.Engines.CannedEvil;
using Server.Engines.PartySystem;
using Server.Factions;
using Server.Guilds;
using Server.Items;
using Server.Regions;

namespace Server.Spells.Necromancy
{
  public class ExorcismSpell : NecromancerSpell
  {
    private static SpellInfo m_Info = new SpellInfo(
      "Exorcism", "Ort Corp Grav",
      203,
      9031,
      Reagent.NoxCrystal,
      Reagent.GraveDust
    );

    private static readonly int Range = Core.ML ? 48 : 18;

    private static readonly Point3D[] m_BritanniaLocs =
    {
      new Point3D(1470, 843, 0),
      new Point3D(1857, 865, -1),
      new Point3D(4220, 563, 36),
      new Point3D(1732, 3528, 0),
      new Point3D(1300, 644, 8),
      new Point3D(3355, 302, 9),
      new Point3D(1606, 2490, 5),
      new Point3D(2500, 3931, 3),
      new Point3D(4264, 3707, 0)
    };

    private static readonly Point3D[] m_IllshLocs =
    {
      new Point3D(1222, 474, -17),
      new Point3D(718, 1360, -60),
      new Point3D(297, 1014, -19),
      new Point3D(986, 1006, -36),
      new Point3D(1180, 1288, -30),
      new Point3D(1538, 1341, -3),
      new Point3D(528, 223, -38)
    };

    private static readonly Point3D[] m_MalasLocs =
    {
      new Point3D(976, 517, -30)
    };

    private static readonly Point3D[] m_TokunoLocs =
    {
      new Point3D(710, 1162, 25),
      new Point3D(1034, 515, 18),
      new Point3D(295, 712, 55)
    };

    public ExorcismSpell(Mobile caster, Item scroll = null) : base(caster, scroll, m_Info)
    {
    }

    public override TimeSpan CastDelayBase => TimeSpan.FromSeconds(2.0);

    public override double RequiredSkill => 80.0;
    public override int RequiredMana => 40;


    public override bool DelayedDamage => false;

    public override bool CheckCast()
    {
      if (Caster.Skills.SpiritSpeak.Value < 100.0)
      {
        Caster.SendLocalizedMessage(1072112); // You must have GM Spirit Speak to use this spell
        return false;
      }

      return base.CheckCast();
    }

    public override int ComputeKarmaAward()
    {
      return 0; //no karma lost from this spell!
    }

    public override void OnCast()
    {
      ChampionSpawnRegion r = Caster.Region.GetRegion<ChampionSpawnRegion>();
      if (r == null || !Caster.InRange(r.ChampionSpawn, Range))
      {
        Caster.SendLocalizedMessage(1072111); // You are not in a valid exorcism region.
      }
      else if (CheckSequence())
      {
        Map map = Caster.Map;

        if (map != null)
        {
          IEnumerable<Mobile> targets = r.ChampionSpawn.GetMobilesInRange(Range).Where(IsValidTarget);

          foreach (Mobile m in targets)
          {
            //Surprisingly, no sparkle type effects

            m.Location = GetNearestShrine(m);
          }
        }
      }

      FinishSequence();
    }

    private bool IsValidTarget(Mobile m)
    {
      if (!m.Player || m.Alive)
        return false;

      Corpse c = m.Corpse as Corpse;
      Map map = m.Map;

      if (c?.Deleted == false && map != null && c.Map == map)
      {
        if (SpellHelper.IsAnyT2A(map, c.Location) && SpellHelper.IsAnyT2A(map, m.Location))
          return false; //Same Map, both in T2A, ie, same 'sub server'.

        if (m.Region.IsPartOf<DungeonRegion>() == Region.Find(c.Location, map).IsPartOf<DungeonRegion>())
          return false; //Same Map, both in Dungeon region OR They're both NOT in a dungeon region.

        //Just an approximation cause RunUO doesn't divide up the world the same way OSI does ;p
      }

      if (Party.Get(m)?.Contains(Caster) == true)
        return false;

      if (m.Guild != null && Caster.Guild != null)
      {
        Guild mGuild = m.Guild as Guild;
        Guild cGuild = Caster.Guild as Guild;

        if (mGuild.IsAlly(cGuild))
          return false;

        if (mGuild == cGuild)
          return false;
      }

      Faction f = Faction.Find(m);

      return Faction.Facet != m.Map || f == null || f != Faction.Find(Caster);
    }

    private static Point3D GetNearestShrine(Mobile m)
    {
      Map map = m.Map;

      Point3D[] locList;


      if (map == Map.Felucca || map == Map.Trammel)
        locList = m_BritanniaLocs;
      else if (map == Map.Ilshenar)
        locList = m_IllshLocs;
      else if (map == Map.Tokuno)
        locList = m_TokunoLocs;
      else if (map == Map.Malas)
        locList = m_MalasLocs;
      else
        locList = new Point3D[0];

      Point3D closest = Point3D.Zero;
      double minDist = double.MaxValue;

      for (int i = 0; i < locList.Length; i++)
      {
        Point3D p = locList[i];

        double dist = m.GetDistanceToSqrt(p);
        if (minDist > dist)
        {
          closest = p;
          minDist = dist;
        }
      }

      return closest;
    }
  }
}