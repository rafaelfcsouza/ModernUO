using Server.Items;
using Server.Misc;

namespace Server.Mobiles
{
  public class Orc : BaseCreature
  {
    [Constructible]
    public Orc() : base(AIType.AI_Melee, FightMode.Closest, 10, 1, 0.2, 0.4)
    {
      Name = NameList.RandomName("orc");
      Body = 17;
      BaseSoundID = 0x45A;

      SetStr(96, 120);
      SetDex(81, 105);
      SetInt(36, 60);

      SetHits(58, 72);

      SetDamage(5, 7);

      SetDamageType(ResistanceType.Physical, 100);

      SetResistance(ResistanceType.Physical, 25, 30);
      SetResistance(ResistanceType.Fire, 20, 30);
      SetResistance(ResistanceType.Cold, 10, 20);
      SetResistance(ResistanceType.Poison, 10, 20);
      SetResistance(ResistanceType.Energy, 20, 30);

      SetSkill(SkillName.MagicResist, 50.1, 75.0);
      SetSkill(SkillName.Tactics, 55.1, 80.0);
      SetSkill(SkillName.Wrestling, 50.1, 70.0);

      Fame = 1500;
      Karma = -1500;

      VirtualArmor = 28;

      switch (Utility.Random(20))
      {
        case 0:
          PackItem(new Scimitar());
          break;
        case 1:
          PackItem(new Katana());
          break;
        case 2:
          PackItem(new WarMace());
          break;
        case 3:
          PackItem(new WarHammer());
          break;
        case 4:
          PackItem(new Kryss());
          break;
        case 5:
          PackItem(new Pitchfork());
          break;
      }

      PackItem(new ThighBoots());

      switch (Utility.Random(3))
      {
        case 0:
          PackItem(new Ribs());
          break;
        case 1:
          PackItem(new Shaft());
          break;
        case 2:
          PackItem(new Candle());
          break;
      }

      if (Utility.RandomDouble() < 0.2)
        PackItem(new BolaBall());
    }

    public Orc(Serial serial) : base(serial)
    {
    }

    public override string CorpseName => "an orcish corpse";
    public override InhumanSpeech SpeechType => InhumanSpeech.Orc;

    public override bool CanRummageCorpses => true;
    public override int TreasureMapLevel => 1;
    public override int Meat => 1;

    public override OppositionGroup OppositionGroup => OppositionGroup.SavagesAndOrcs;

    public override void GenerateLoot()
    {
      AddLoot(LootPack.Meager);
    }

    public override bool IsEnemy(Mobile m)
    {
      if (m.Player && m.FindItemOnLayer(Layer.Helm) is OrcishKinMask)
        return false;

      return base.IsEnemy(m);
    }

    public override void AggressiveAction(Mobile aggressor, bool criminal)
    {
      base.AggressiveAction(aggressor, criminal);

      Item item = aggressor.FindItemOnLayer(Layer.Helm);

      if (item is OrcishKinMask)
      {
        AOS.Damage(aggressor, 50, 0, 100, 0, 0, 0);
        item.Delete();
        aggressor.FixedParticles(0x36BD, 20, 10, 5044, EffectLayer.Head);
        aggressor.PlaySound(0x307);
      }
    }

    public override void Serialize(IGenericWriter writer)
    {
      base.Serialize(writer);
      writer.Write(0);
    }

    public override void Deserialize(IGenericReader reader)
    {
      base.Deserialize(reader);
      int version = reader.ReadInt();
    }
  }
}