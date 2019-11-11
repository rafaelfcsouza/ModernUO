using System.Collections.Generic;
using Server.Items;
using Server.Misc;
using Server.Mobiles;
using Server.Network;

namespace Server.Engines.Quests.Haven
{
  public class MilitiaFighter : BaseCreature
  {
    [Constructible]
    public MilitiaFighter() : base(AIType.AI_Melee, FightMode.Closest, 10, 1, 0.2, 0.4)
    {
      InitStats(40, 30, 5);
      Title = "the Militia Fighter";

      SpeechHue = Utility.RandomDyedHue();

      Hue = Race.Human.RandomSkinHue();

      Female = false;
      Body = 0x190;
      Name = NameList.RandomName("male");

      Utility.AssignRandomHair(this);
      Utility.AssignRandomFacialHair(this, HairHue);

      AddItem(new ThighBoots(0x1BB));
      AddItem(new LeatherChest());
      AddItem(new LeatherArms());
      AddItem(new LeatherLegs());
      AddItem(new LeatherCap());
      AddItem(new LeatherGloves());
      AddItem(new LeatherGorget());

      var weapon = Utility.Random(6) switch
      {
        0 => (Item)new Broadsword(),
        1 => new Cutlass(),
        2 => new Katana(),
        3 => new Longsword(),
        4 => new Scimitar(),
        _ => new VikingSword()
      };

      weapon.Movable = false;
      AddItem(weapon);

      Item shield = new BronzeShield();
      shield.Movable = false;
      AddItem(shield);

      SetSkill(SkillName.Swords, 20.0);
    }

    public MilitiaFighter(Serial serial) : base(serial)
    {
    }

    public override bool ClickTitle => false;

    public override bool IsEnemy(Mobile m)
    {
      if (m.Player || m is BaseVendor)
        return false;

      if (m is BaseCreature bc)
      {
        Mobile master = bc.GetMaster();
        if (master != null)
          return IsEnemy(master);
      }

      return m.Karma < 0;
    }

    public override void Serialize(GenericWriter writer)
    {
      base.Serialize(writer);

      writer.Write(0); // version
    }

    public override void Deserialize(GenericReader reader)
    {
      base.Deserialize(reader);

      int version = reader.ReadInt();
    }
  }

  public class MilitiaFighterCorpse : Corpse
  {
    public MilitiaFighterCorpse(Mobile owner, HairInfo hair, FacialHairInfo facialhair, List<Item> equipItems) : base(
      owner, hair, facialhair, equipItems)
    {
    }

    public MilitiaFighterCorpse(Serial serial) : base(serial)
    {
    }

    public override void AddNameProperty(ObjectPropertyList list)
    {
      if (ItemID == 0x2006) // Corpse form
      {
        list.Add("a human corpse");
        list.Add(1049318, Name); // the remains of ~1_NAME~ the militia fighter
      }
      else
      {
        list.Add(1049319); // the remains of a militia fighter
      }
    }

    public override void OnSingleClick(Mobile from)
    {
      int hue = Notoriety.GetHue(NotorietyHandlers.CorpseNotoriety(from, this));

      if (ItemID == 0x2006) // Corpse form
        Packets.SendMessageLocalized(from.NetState, Serial, ItemID, MessageType.Label, hue, 3, 1049318, "",
          Name); // the remains of ~1_NAME~ the militia fighter
      else
        Packets.SendMessageLocalized(from.NetState, Serial, ItemID, MessageType.Label, hue, 3, 1049318); // the remains of a militia fighters
    }

    public override void Open(Mobile from, bool checkSelfLoot)
    {
      if (from.InRange(GetWorldLocation(), 2))
        from.SendLocalizedMessage(1049661, "",
          0x22); // Thinking about his sacrifice, you can't bring yourself to loot the body of this militia fighter.
    }

    public override void Serialize(GenericWriter writer)
    {
      base.Serialize(writer);

      writer.Write(0); // version
    }

    public override void Deserialize(GenericReader reader)
    {
      base.Deserialize(reader);

      int version = reader.ReadInt();
    }
  }
}
