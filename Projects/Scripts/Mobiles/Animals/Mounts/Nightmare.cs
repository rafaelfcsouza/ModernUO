using Server.Items;

namespace Server.Mobiles
{
  public class Nightmare : BaseMount
  {
    [Constructible]
    public Nightmare(string name = "a nightmare") : base(name, 0x74, 0x3EA7, AIType.AI_Mage, FightMode.Closest, 10, 1, 0.2, 0.4)
    {
      BaseSoundID = Core.AOS ? 0xA8 : 0x16A;

      SetStr(496, 525);
      SetDex(86, 105);
      SetInt(86, 125);

      SetHits(298, 315);

      SetDamage(16, 22);

      SetDamageType(ResistanceType.Physical, 40);
      SetDamageType(ResistanceType.Fire, 40);
      SetDamageType(ResistanceType.Energy, 20);

      SetResistance(ResistanceType.Physical, 55, 65);
      SetResistance(ResistanceType.Fire, 30, 40);
      SetResistance(ResistanceType.Cold, 30, 40);
      SetResistance(ResistanceType.Poison, 30, 40);
      SetResistance(ResistanceType.Energy, 20, 30);

      SetSkill(SkillName.EvalInt, 10.4, 50.0);
      SetSkill(SkillName.Magery, 10.4, 50.0);
      SetSkill(SkillName.MagicResist, 85.3, 100.0);
      SetSkill(SkillName.Tactics, 97.6, 100.0);
      SetSkill(SkillName.Wrestling, 80.5, 92.5);

      Fame = 14000;
      Karma = -14000;

      VirtualArmor = 60;

      Tamable = true;
      ControlSlots = 2;
      MinTameSkill = 95.1;

      switch (Utility.Random(3))
      {
        case 0:
          {
            BodyValue = 116;
            ItemID = 16039;
            break;
          }
        case 1:
          {
            BodyValue = 178;
            ItemID = 16041;
            break;
          }
        case 2:
          {
            BodyValue = 179;
            ItemID = 16055;
            break;
          }
      }

      PackItem(new SulfurousAsh(Utility.RandomMinMax(3, 5)));
    }

    public Nightmare(Serial serial) : base(serial)
    {
    }

    public override string CorpseName => "a nightmare corpse";

    public override bool HasBreath => true; // fire breath enabled
    public override int Meat => 5;
    public override int Hides => 10;
    public override HideType HideType => HideType.Barbed;
    public override FoodType FavoriteFood => FoodType.Meat;
    public override bool CanAngerOnTame => true;

    public override void GenerateLoot()
    {
      AddLoot(LootPack.Rich);
      AddLoot(LootPack.Average);
      AddLoot(LootPack.LowScrolls);
      AddLoot(LootPack.Potions);
    }

    public override int GetAngerSound()
    {
      if (!Controlled)
        return 0x16A;

      return base.GetAngerSound();
    }

    public override void Serialize(IGenericWriter writer)
    {
      base.Serialize(writer);

      writer.Write(0); // version
    }

    public override void Deserialize(IGenericReader reader)
    {
      base.Deserialize(reader);

      int version = reader.ReadInt();

      if (Core.AOS && BaseSoundID == 0x16A)
        BaseSoundID = 0xA8;
      else if (!Core.AOS && BaseSoundID == 0xA8)
        BaseSoundID = 0x16A;
    }
  }
}
