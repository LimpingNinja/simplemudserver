using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Linq;


namespace SimpleMud;


[Serializable]
public class Room
{
    public int Uid { get; set; }
    public DateTime Birth { get; set; }
    public int Terrain { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public BitVector32 Bits { get; set; }
    public Dictionary<Direction, ExitData> Exits { get; set; }
    public List<object> Contents { get; set; }
    public bool Extracted { get; set; }
    // Other properties can be added here
    
    public Room()
    {
        Uid = -1;
        Birth = DateTime.Now;
        Terrain = (int)TerrainType.None;
        Name = "";
        Description = "";
        Bits = new BitVector32(0);
        Exits = new Dictionary<Direction, ExitData>();
        Contents = new List<object>();
        Extracted = false;
    }

    public void Serialize(string filename)
    {
        var pickle = Utilities.ObjectToByteArray(this);
        if(pickle == null) {
            Console.WriteLine($"Error in Serializing room {this.Uid}.");
            return;
        }
        File.WriteAllBytes(filename, pickle);
    }

    public Room Deserialize(string filename)
    {
        var filebytes = File.ReadAllBytes(filename);
        var cucumber = Utilities.ByteArrayToObject<Room>(filebytes);
        if(cucumber==null) {
            Console.WriteLine($"Error in Deserializing from file {filename}.");
            return new Room();
        }
        return cucumber;
    }

    [Serializable]
    public class ExitData
    {
        public Direction ExitDirection { get; set; }
        // Other properties can be added here
    }
    
    // Additional methods can be added here
    public enum Direction
    {
        None = -1,
        North,
        East,
        South,
        West,
        Up,
        Down,
        Northeast,
        Southeast,
        Southwest,
        Northwest
    }

    public enum TerrainType
    {
        None = -1,
        Indoors,
        City,
        Road,
        Alley,
        Bridge,
        ShallowWater,
        DeepWater,
        Ocean,
        Underwater,
        Field,
        Plains,
        Meadow,
        Forest,
        DeepForest,
        Hills,
        HighHills,
        Mountain,
        Swamp,
        DeepSwamp,
        Sand,
        Desert,
        Ice,
        Glacier,
        Cavern,
        // Assuming NUM_TERRAINS is used as a count, not an actual terrain type
    }
}

// // Placeholder for CMD_DATA structure
// [Serializable]
// public class CmdData
// {
//     // Assuming the structure of CMD_DATA based on the C code provided
//     // ... properties and methods will be translated as needed
// }

// public static Room NewRoom()
// {
//     Room room = new Room();

//     room.Uid = NextUid(); // This will be another method to generate the next unique ID
//     room.Birth = DateTime.Now; // current_time in C# would be DateTime.Now
//     room.Name = ""; // strdup("") is equivalent to assigning an empty string in C#
//     room.Description = ""; // newBuffer(1) would be an empty string or possibly a StringBuilder in C#
//     room.Terrain = (int)TerrainType.Indoors; // Direct translation of TERRAIN_INDOORS

//     // bitvectorInstanceOf("room_bits") would be a new instance of a BitVector class or similar structure in C#
//     room.Bits = new BitVector();

//     // newAuxiliaryData(AUXILIARY_TYPE_ROOM) would be a new instance of an AuxiliaryData class or similar structure in C#
//     room.AuxiliaryData = new AuxiliaryData();

//     // newHashtable() could be a new Dictionary or similar key-value pair structure in C#
//     room.Exits = new Dictionary<Direction, ExitData>();

//     // newEdescSet() would be a new instance of an EdescSet class or similar structure in C#
//     room.Edescs = new EdescSet();

//     // newList() would translate to new List<T>() in C#
//     room.Contents = new List<object>(); // Assuming contents can be of any object type
//     room.Characters = new List<Character>(); // Assuming a Character class exists

//     room.Extracted = false; // FALSE in C translates to false in C#
//     room.CmdTable = null; // cmd_table being NULL translates to null in C#

//     return room;
// }


