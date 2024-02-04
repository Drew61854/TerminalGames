using UnityEngine;
using static TerminalApi.TerminalApi;
using static SimpleCommand.API.SimpleCommand;
using System.Collections.Generic;
using static TerminalGames.CardClasses;
using System.Linq;
using static ConsoleGames.DiceClasses;
using Steamworks;
using System.Data;
using DunGen;
using static UnityEngine.UIElements.UxmlAttributeDescription;
using UnityEngine.InputSystem.EnhancedTouch;
using UnityEngine.UIElements.Experimental;
using UnityEngine.InputSystem.Layouts;
using JetBrains.Annotations;
using System.Linq.Expressions;

namespace TerminalGames
{
    internal class AdventureMethods 
    {
        static string returnString = "";
        static bool flashlightOn = false;
        static int gameState;
        static int area;
        static int cameFrom;
        static List<string> inventory = new List<string>() {"Empty", "Empty", "Empty", "Empty"};
        static int door0;
        static int door3;
        static int door5;
        static int door12;
        static int door12Locked;
        static int flashlight;
        static int ladder;
        static int remote;
        static int landmine;
        static int turret;
        static int key;
        static int keyGiven;
        static int arm;
        static int fuse1;
        static int fuse2;
        static int fuse3;
        static int fuse4;
        static int bodyMoved;
        static List<int> insertedFuses = new List<int>() { 0, 0, 0, 0 };
        public static TerminalNode LoadGame(Terminal __terminal)
        {
            ConsoleGamesMain.playingAdventure = true;
            gameState = PlayerPrefs.GetInt("GameState", 0);
            area = PlayerPrefs.GetInt("Area", 0);
            inventory[0] = PlayerPrefs.GetString("InvOne", "Empty");
            inventory[1] = PlayerPrefs.GetString("InvTwo", "Empty");
            inventory[2] = PlayerPrefs.GetString("InvThree", "Empty");
            inventory[3] = PlayerPrefs.GetString("InvFour", "Empty");
            door0 = PlayerPrefs.GetInt("Door0", 0);
            door3 = PlayerPrefs.GetInt("Door3", 0);
            door5 = PlayerPrefs.GetInt("Door5", 0);
            door12 = PlayerPrefs.GetInt("Door12", 0);
            door12Locked = PlayerPrefs.GetInt("Door12Locked", 1);
            flashlight = PlayerPrefs.GetInt("Flashlight", 0);
            ladder = PlayerPrefs.GetInt("Ladder", 0);
            remote = PlayerPrefs.GetInt("Remote", 0);
            landmine = PlayerPrefs.GetInt("Landmine", 0);
            turret = PlayerPrefs.GetInt("Turret", 0);
            key = PlayerPrefs.GetInt("Key", 0);
            keyGiven = PlayerPrefs.GetInt("KeyGiven", 0);
            arm = PlayerPrefs.GetInt("Arm", 0);
            fuse1 = PlayerPrefs.GetInt("Fuse1", 0);
            fuse2 = PlayerPrefs.GetInt("Fuse2", 0);
            fuse3 = PlayerPrefs.GetInt("Fuse3", 0);
            fuse4 = PlayerPrefs.GetInt("Fuse4", 0);
            bodyMoved = PlayerPrefs.GetInt("BodyMoved", 0);
            insertedFuses[0] = PlayerPrefs.GetInt("FuseOne", 0);
            insertedFuses[1] = PlayerPrefs.GetInt("FuseTwo", 0);
            insertedFuses[2] = PlayerPrefs.GetInt("FuseThree", 0);
            insertedFuses[3] = PlayerPrefs.GetInt("FuseFour", 0);
            returnString = "";  //Clear previous messages that may have been sent.
            if (area == 0)
            {
                returnString += "<size=25>Salvage Synergy: The Company's Lost Legacy\u2122</size>\n\nYou find yourself waking up on the cold, hard ground. You don't know where you are or how you got here.\nEnter an action, or type 'Actions' for a list of actions.\n\n";
            }
            else
            {
                returnString += "Your save data was loaded.\n\n" + LoadIntro(area);
            }
            TerminalNode returnNode = CreateTerminalNode(returnString);
            returnNode.clearPreviousText = true;
            return returnNode;
        }
        public static string LoadIntro(int area)
        {
            string returnString = "";
            switch (area)
            {
                case 1:
                     returnString = "You're on a grated walkway above a large hole. Don't fall. To your west there is a corridor. To the south there is an open door, leading to a dark hall.";
                     break;
                case 2:
                    returnString = "You're in a dark corridor with pipes and valves jutting out of the wall. The smell of iron fills your nose. To the west there is a ";
                    if (door3 == 0)
                        returnString += "closed door.";
                    else
                        returnString += "open door leading to a blood stain.";
                    returnString += " To the east there is a dark hallway with a faint light at the end of it. To the south is a staircase leading into the dark. To the north is a grate pathway.";
                    break;
                case 3:
                    returnString = "You stand in a small room with a large blood stain adorning the center of the floor. To the north is a ";
                    if (door0 == 0)
                        returnString += "closed door,";
                    else
                        returnString += "open door leading to a dark turn,";
                    if (door3 == 0)
                        returnString += "and to the east is a closed door.";
                    else
                        returnString += "and to the east is an open door leading to a faint light.";
                    returnString += "\nTo the west is a trail of blood leading into the darkness. You think you're alone.";
                    break;
                case 4:
                    returnString = "You're in a small electrical room. To the east is a bloodtrail leading down the hall.";
                    break;
                case 5:
                    returnString = "You've made your way to a four-way intersection. To the north is a room with a lot of light in it. To the east is a hallway bending out of sight. To the south is a long corridor with something lying at it's end, and to the west is a door.";
                    break;
                case 6:
                    returnString = "You stand before the main entrance to the facility. Light filters in from above you, a sight you sorely missed. To the south is the only exit from this room.";
                    break;
                case 7:
                    returnString = "You're on a landing platform between two sets of stairs, one ascending to the north and one descending to the south. To the east is a twisting hallway which elevates. It's so terribly dark down here, you can't see even three feet in front of you. Watch your step.";
                    break;
                case 8:
                    returnString = "You're beside a dead body which has been shot quite a few times. The scent of sulfur and blood is suffocating. To the south is a small room, to the east is a conjoining hall, to the north you can faintly see light, and to the west is a snaking hallway.";
                    break;
                case 9:
                    returnString = "You're right in front of a turret!\nA deactivated one, that is. Bullet casings litter the ground, and the only way out is to the north.";
                    break;
                case 10:
                    returnString = "You're at the foot of stairs which pierce the heavens. Well, close to it, anyway. To the west is a passage, and to the south is a corridor. To the north are the stairs. This room is the darkest room you've ever been in, maybe because of how deep into the ground it is.";
                    break;
                case 11:
                    returnString = "You are in a small, dark hallway. To the north you can continue to follow the hall, or to the east you can exit the hall. A shiver runs down your spine as you hear a clicking sound from far-away, but oh-so-close.";
                    break;
                case 12:
                    if (keyGiven == 0)
                        returnString = "You're in a mexican stand off with a giant bug. It stares at you intently, unbudging. You could go south, and leave this thing be.";
                    else
                        returnString = "You're standing in a dark room where a bug once was. The only exit is to the south.";
                    break;
                case 13:
                    returnString = "You're standing in front of a landmine. It's too wide to get around, so your forward progress has been blocked. To the west is the only way to leave, unless you want to get blown up, that is.";
                    cameFrom = 10;
                    break;
                case 14:
                    returnString = "You're in a hall connecting a small nook to the east, a corridor to the west, and a door to the south. While you were glancing around in this room you swore you saw two white lights in the distance, but they vanished as soon as you saw them.";
                    break;
                case 15:
                    returnString = "You're at a dead-end. To the west is the way out. The back wall of this room is scratched up badly, and the scratches are far too deep to be human-made.";
                    break;
                case 16:
                    returnString = "You're standing on the opposite end of a hole in the ground to a giant spring-headed monster. You stare at it unblinking, as you know that if you break eye-contact, it will move. What will happen if it gets to you? You don't wanna find out. To the south is the only exit.";
                    break;
                default:
                    returnString = "You find yourself waking up on the cold, hard ground. You don't know where you are or how you got here.\nEnter an action, or type 'Actions' for a list of actions.\n\n";
                    break;
            }
            returnString += "\nEnter an action, or type 'Actions' for a list of actions.\n\n";
            return returnString;
        }

        public static void Save()
        {
            PlayerPrefs.SetInt("GameState", gameState);
            PlayerPrefs.SetInt("Area", area);
            PlayerPrefs.SetString("InvOne", inventory[0]);
            PlayerPrefs.SetString("InvTwo", inventory[1]);
            PlayerPrefs.SetString("InvThree", inventory[2]);
            PlayerPrefs.SetString("InvFour", inventory[3]);
            PlayerPrefs.SetInt("Door0", door0);
            PlayerPrefs.SetInt("Door3", door3);
            PlayerPrefs.SetInt("Door5", door5);
            PlayerPrefs.SetInt("Door12", door12);
            PlayerPrefs.SetInt("Door12Locked", door12Locked);
            PlayerPrefs.SetInt("Flashlight", flashlight);
            PlayerPrefs.SetInt("Ladder", ladder);
            PlayerPrefs.SetInt("Remote", remote);
            PlayerPrefs.SetInt("Landmine", landmine);
            PlayerPrefs.SetInt("Turret", turret);
            PlayerPrefs.SetInt("Key", key);
            PlayerPrefs.SetInt("KeyGiven", keyGiven);
            PlayerPrefs.SetInt("Arm", arm);
            PlayerPrefs.SetInt("Fuse1", fuse1);
            PlayerPrefs.SetInt("Fuse2", fuse2);
            PlayerPrefs.SetInt("Fuse3", fuse3);
            PlayerPrefs.SetInt("Fuse4", fuse4);
            PlayerPrefs.SetInt("FuseOne", insertedFuses[0]);
            PlayerPrefs.SetInt("FuseTwo", insertedFuses[1]);
            PlayerPrefs.SetInt("FuseThree", insertedFuses[2]);
            PlayerPrefs.SetInt("FuseFour", insertedFuses[3]);
            PlayerPrefs.SetInt("BodyMoved", bodyMoved);
            PlayerPrefs.Save();
        }

        public static int InputParse(string input)
        {
            int newInt;
            try
            {
                input = input.Substring(5);  //After walk
                if (input.Substring(0, 2).Equals("to")) //After to if included
                    input = input.Substring(3);
                switch (input)
                {
                    case "north":
                        newInt = 0;
                        break;
                    case "east":
                        newInt = 1;
                        break;
                    case "west":
                        newInt = 2;
                        break;
                    case "south":
                        newInt = 3;
                        break;
                    default:
                        newInt = -1;
                        break;

                }
            }
            catch
            {
                return -1;
            }
            return newInt;
        }

        public static bool AreasConnected(int area, string input, out int translatedInput) //Think this is a pretty good way to do this.
        {
            translatedInput = InputParse(input);
            if (translatedInput == -1)
                return false;
            if (area == 0)
            {
                switch (translatedInput)
                {
                    case 0: //North
                        return false;
                    case 1: //East
                        return true;
                    case 2: //West
                        return false;
                    default: //South
                        return true;
                }
            }
            else if (area == 1)
            {
                switch (translatedInput)
                {
                    case 0: //North
                        return false;
                    case 1: //East
                        return false;
                    case 2: //West
                        return true;
                    default: //South
                        return true;
                }
            }
            else if (area == 14)
            {
                if (translatedInput == 0)
                    return false;
                return true;
            }
            else if (area == 3)
            {
                if (translatedInput == 3)
                    return false;
                return true;
            }
            else if (area == 4)
            {
                if (translatedInput == 1)
                    return true;
                return false;
            }
            else if (area == 2|| area == 5 || area == 8)
                return true;
            else if (area == 6)
            {
                switch (translatedInput)
                {
                    case 0: //North
                        return true;
                    case 1: //East
                        return false;
                    case 2: //West
                        return false;
                    default: //South
                        return true;
                }
            }
            else if (area == 7)
            {
                if (translatedInput == 2)
                    return false;
                return true;
            }
            else if (area == 9)
            {
                if (translatedInput == 0)
                    return true;
                return false;
            }
            else if (area == 10)
            {
                switch (translatedInput)
                {
                    case 0: //North
                        return true;
                    case 1: //East
                        return false;
                    case 2: //West
                        return true;
                    default: //South
                        return true;
                }
            }
            else if (area == 11)
            {
                switch (translatedInput)
                {
                    case 0: //North
                        return true;
                    case 1: //East
                        return true;
                    case 2: //West
                        return false;
                    default: //South
                        return false;
                }
            }
            else if (area == 12 || area == 16)
            {
                if (translatedInput == 3)
                    return true;
                return false;
            }
            else if (area == 13)
            {
                switch (translatedInput)
                {
                    case 0: //North
                        return false;
                    case 1: //East
                        return true;
                    case 2: //West
                        return true;
                    default: //South
                        return false;
                }
            }
            else if (area == 15)
            {
                if (translatedInput == 2)
                    return true;
                return false;
            }
            return true;
        }
        public static TerminalNode Actions(Terminal __terminal)
        {
            TerminalNode returnNode;
            if (ConsoleGamesMain.playingAdventure)
            {
                returnNode = CreateTerminalNode("Inspect (optional: object)\n<size=13>Get a description of your general surroundings or of a specific object</size>\nWalk 'direction'\n<size=13>Travel to another location (Ex. walk east)</size>\nGrab 'item'\n<size=13>Pick up an item. (Ex. grab shovel)</size>\nInventory\n<size=13>List what you are holding</size>\nUse 'item'\n<size=13>Use an item. (Ex. use key)</size>\nRestart\n<size=13>If you find yourself wanting to start a new game, use this command.</size>\n\n");
            }
            else
            {
                returnNode = CreateTerminalNode("There was an issue with your request.\n\n");
            }
            returnNode.clearPreviousText = true;
            return returnNode;
        }

        public static TerminalNode Restart(Terminal __terminal) //Reset all vars to default and display intro text.
        {
            TerminalNode returnNode = CreateTerminalNode("Data wiped.\n\n<size=25>Salvage Synergy: The Company's Lost Legacy\u2122</size>\n\nYou find yourself waking up on the cold, hard ground. You don't know where you are or how you got here.\nEnter an action, or type 'Actions' for a list of actions.\n\n");
            returnNode.clearPreviousText = true;
            ConsoleGamesMain.playingAdventure = true;
            gameState = 0;
            area = 0;
            for (int i = 0; i < 4; i++)
            {
                inventory[i] = "Empty";
                insertedFuses[i] = 0;
            }
            door0 = 0;
            door3 = 0;
            door5 = 0;
            door12 = 0;
            door12Locked = 1;
            flashlight = 0;
            ladder = 0;
            remote = 0;
            landmine = 0;
            turret = 0;
            key = 0;
            keyGiven = 0;
            arm = 0;
            fuse1 = 0;
            fuse2 = 0;
            fuse3 = 0;
            fuse4 = 0;
            bodyMoved = 0;
            try
            {
                PlayerPrefs.DeleteKey("Cheater");
            }
            catch
            { 
            }
            Save();
            return returnNode;
        }
        public static TerminalNode Inspect(Terminal __terminal)
        {
            string finalDisplay = "";
            if (ConsoleGamesMain.playingAdventure)
            {
                string input = SimpleCommand.API.SimpleCommand.GetInputValue(__terminal);
                input = input.ToLower();
                if (input.Length > 8)
                {
                    input = input.Substring(8);
                    switch (area)
                    {

                        case 1:
                            if (input.Equals("ladder") && ladder == 0)
                                finalDisplay = "The ladder is in decent condition. Dried blood runs along the main chasis.";
                            else
                                finalDisplay = "No such object exists in this room.";
                            break;
                        case 2:
                            if (input.Equals("door"))
                                finalDisplay = "You cannot see through either door's window, but both doors are unlocked.";
                            else
                                finalDisplay = "No such object exists in this room.";
                            break;
                        case 3:
                            if (input.Equals("door"))
                                finalDisplay = "You cannot see through either door's window, but both doors are unlocked.";
                            else if (input.Equals("blood") || input.Equals("blood stain"))
                            {
                                finalDisplay = "The blood is fresh, and it leads to the west.";
                            }
                            else
                                finalDisplay = "No such object exists in this room.";
                            break;
                        case 4:
                            int numFuses = 4;
                            for (int i = 0; i < 4; i++)
                                if (insertedFuses[i] == 1)
                                    numFuses--;
                            if ((input.Equals("breaker") || input.Equals("breaker box")) && numFuses != 0)
                                finalDisplay = "Opening the breaker, you notice that it's missing\n" + numFuses + " fuses. With them, you bet the power would come on.";
                            else if (numFuses == 0 && (input.Equals("breaker") || input.Equals("breaker box")))
                            {
                                finalDisplay = "You've placed the fuses in the breaker.";
                            }
                            else if (input.Equals("arm") || input.Equals("severed arm") && arm == 0)
                            {
                                finalDisplay = "The blood is fresh.";
                                if (key == 0)
                                    finalDisplay += " You notice a key in the arm's hand.";
                            }
                            else
                                finalDisplay = "No such object exists in this room.";
                            break;
                        case 5:
                            finalDisplay = "No such object exists in this room.";
                            break;
                        case 6:
                            if (input.Equals("door") && insertedFuses[3] == 0)
                                finalDisplay = "The door is hydraulically shut. It's opened by the scanner, but it has no power.";
                            else if (input.Equals("door") && gameState != 150)
                                finalDisplay = "The door is hydraulically shut. It's opened by the scanner.";
                            else if (input.Equals("door") && gameState == 150)
                                finalDisplay = "The door is open!";
                            else if ((input.Equals("scanner") || input.Equals("the scanner")) && insertedFuses[3] == 0)
                                finalDisplay = "The scanner is powered off";
                            else if (input.Equals("scanner") || input.Equals("the scanner"))
                                finalDisplay = "The scanner reads 'enter biometric data'.";
                            else
                                finalDisplay = "No such object exists in this room.";
                            break;
                        case 7:
                            finalDisplay = "No such object exists in this room.";
                            break;
                        case 8:
                            if ((input.Equals("body") || input.Equals("the body")) && bodyMoved == 0)
                                finalDisplay = "There are a lot of bullet holes in the body. It's very cold. It seems like there's something under it. You could get it if you pushed the body out of the way.";
                            else if (input.Equals("body") || input.Equals("the body"))
                                finalDisplay = "There are a lot of bullet holes in the body. It's very cold.";
                            else
                                finalDisplay = "No such object exists in this room.";
                            break;
                        case 9:
                            if ((input.Equals("turret") || input.Equals("the turret")) && fuse2 == 0)
                                finalDisplay = "The turret's been deactivated. On it's left side there's a fuse embedded in it. You could take it out.";
                            else if (input.Equals("turret") || input.Equals("the turret"))
                                finalDisplay = "The turret's been deactivated. You took it's fuse.";
                            else
                                finalDisplay = "No such object exists in this room.";
                            break;
                        case 10:
                            finalDisplay = "No such object exists in this room.";
                            break;
                        case 11:
                            finalDisplay = "No such object exists in this room.";
                            break;
                        case 12:
                            if (input.Equals("bug") || input.Equals("loot bug"))
                            {
                                finalDisplay = "The bug has beady eyes, and it clicks loudly at you. It seems to be guarding whatever's behind it.";
                                for (int i = 0; i < 4; i++)
                                {
                                    if (inventory[i].Equals("Key") || inventory[i].Equals("Flashlight"))
                                        finalDisplay += "\nIt looks like it's fixated on some item you have.\n\n";
                                }
                            }
                            else
                                finalDisplay = "No such object exists in this room.";
                            break;
                        case 13:
                            if ((input.Equals("mine") || input.Equals("landmine")) && landmine == 0)
                                finalDisplay = "There's a mine on the ground with a flashing red light. It's blocking the way forward from the west.";
                            else if ((input.Equals("device") || input.Equals("the device")) && remote == 0)
                            {
                                if (landmine == 0)
                                    finalDisplay = "There's some sort of device on the ground, but there's a landmine blocking the way from the west. Maybe you could get it from the east, or maybe you could get rid of the landmine.";
                                else
                                    finalDisplay = "There's a device laying on the ground. It looks like a television remote, but more scientific.";
                            }
                            else
                                finalDisplay = "No such object exists in this room.";
                            break;
                        case 14:
                            if (input.Equals("door") || input.Equals("the door"))
                            {
                                if (door12Locked == 0)
                                    finalDisplay = "The door is locked!";
                                else
                                    finalDisplay = "The door leads to a long hallway. It's unlocked";
                            }
                            else
                                finalDisplay = "No such object exists in this room.";
                            break;
                        case 15:
                            if (input.Equals("flashlight"))
                                finalDisplay = "There's a flashlight on the ground. Looks like it still works, too.";
                            else
                                finalDisplay = "No such object exists in this room.";
                            break;
                        case 16:
                            if (input.Equals("coil head") || input.Equals("coilhead"))
                                finalDisplay = "The thing is disturbing. It doesn't seem to move so long as you look at it. Good thing you have this flashlight.";
                            else
                                finalDisplay = "No such object exists in this room.";
                            break;
                        default: //Area 0
                            if (input.Equals("door"))
                                finalDisplay = "It's too dark to see through the door's window, but you notice that the door is unlocked.";
                            else
                                finalDisplay = "No such object exists in this room.";
                            break;
                    }
                }
                else
                {
                    switch (area)
                    {
                        case 1:
                            finalDisplay = "You're standing on a grate platform hovering above a chasm which you cannot see the bottom of. To be fair, it's so dark you can't see much of anything. To your west there is a corridor. To the south there is an open door, leading to a dark hall.";
                                if (ladder == 0)
                                    finalDisplay += "\nYou notice a ladder leaning against the wall. It has blood on it.";
                            break;
                        case 2:
                            finalDisplay =  "You're in a dark corridor with pipes and valves jutting out of the wall. The smell of iron fills your nose. To the west there is a ";
                            if (door3 == 0)
                                finalDisplay += "closed door.";
                            else
                                finalDisplay += "open door leading to a blood stain.";
                            finalDisplay += " To the east there is a dark hallway with a faint light at the end of it. To the south is a staircase leading into the dark. To the north is a grate pathway.";
                            break;
                        case 3:
                            finalDisplay = "You stand in a small room with a large blood stain adorning the center of the floor. To the north is a ";
                            if (door0 == 0)
                                finalDisplay += "closed door,";
                            else
                                finalDisplay += "open door leading to a dark turn,";
                            if (door3 == 0)
                                finalDisplay += "and to the east is a closed door.";
                            else
                                finalDisplay += "and to the east is an open door leading to a faint light.";
                            finalDisplay += "\nTo the west is a trail of blood leading into the darkness. You think you're alone.";
                            break;
                        case 4:
                            finalDisplay = "You're in a small electrical room. To the east is a hallway with a bloodtrail leading into it. There's a breakerbox on the wall.";
                            if (arm == 0)
                                finalDisplay += "\nThere's a severed arm on the ground sitting in a pool of blood.";
                            break;
                        case 5:
                            finalDisplay = "The hall you're in is illuminated by the light filtering in from the room to the north. There's some sort of blueish-green muck scattered across the floor. To the west is a dark hall. To the east is a curved hallway. To the south is a hallway with a body at the end of it. To the north is a bright room.";
                            break;
                        case 6:
                            finalDisplay = "You're in the lobby of the facility, and light filters through a large fan in the ceiling. The north wall has a door. It has some sort of scanner next to it. To the south is a dark hallway.";
                            break;
                        case 7:
                            finalDisplay = "To the east is a long hallway that seems to turn. To the south are more stairs leading down, and to the north are more stairs leading up.\nIt's eerily quiet here, and the air is stale.";
                            break;
                        case 8:
                            finalDisplay = "In the middle of the walkway there is a body riddled with bullet holes. The smell is awful. To the north there is a light. To the east there looks to be a T-Junction. There's an open doorway to the west.";
                            if (bodyMoved == 1 && fuse3 == 0)
                                finalDisplay += "There's also a fuse on the ground.";
                            break;
                        case 9:
                            finalDisplay = "The turret is deactivated. There are a lot of bullet casings on the ground. This seems to be a storage room, there are boxes and bits on shelves all around. Nothing catches your eye.";
                            if (fuse2 == 0)
                                finalDisplay += "There's a fuse in the turret. ";
                            finalDisplay += "To the north is the ony exit.";
                            break;
                        case 10:
                            finalDisplay = "To the north are stairs. To the south the path curves out of sight. To the west stretches a long walkway.\nYou hear something far away, but you don't know what. After a moment silence returns.";
                            break;
                        case 11:
                            finalDisplay = "To the north the walkway continues. To the east is a corridor. It's cold in this room, much colder than other parts of the facility. You swear that you heard something behind you, but nothing was there.";
                            break;
                        case 12:
                            if (keyGiven == 0)
                                finalDisplay = "There's a weird bug at the end of the hallway. It looks at you from afar. It seems like there's something behind it. To the south is the way back.";
                            else if (fuse1 == 0)
                                finalDisplay = "There's a fuse sitting on the ground. To the south is the way back.";
                            else
                                finalDisplay = "The room is empty. To the south is the way back.\nYou don't want to stick around for long.";
                            break;
                        case 13:
                            if (landmine == 0 && remote == 0)
                            {
                                if (cameFrom == 10)
                                    finalDisplay = "There's a device laying on the ground, but in front of it is a landmine. To the east is a door. To the west is an open passage. Through the blinking of the red light on the landmine you see that this room is very red. You hope it's paint.";
                                else
                                    finalDisplay = "There's a device laying on the ground in front of the landmine. To the east is an open door. Through the blinking of the red light on the landmine you see that this room is very red. You hope it's paint.";
                            }
                            else if (landmine == 0)
                                finalDisplay = "There's a landmine on the ground. To the east is a door. To the west is an open passage. Through the blinking of the red light on the landmine you see that this room is very red. You hope it's paint.";
                            else
                                finalDisplay = "To the east is a door. To the west is an open passage way.";
                            break;
                        case 14:
                            if (door12 == 0)
                                finalDisplay = "To the south is a closed door. ";
                            else
                                finalDisplay = "To the south is an open door. ";
                            finalDisplay += "To the east is a small room. To the west is a hallway with a body in it.";
                            break;
                        case 15:
                            if (flashlight == 0)
                                finalDisplay = "There's a flashlight on the ground. To the west is the way back.";
                            else
                                finalDisplay = "To the west is the way back.";
                            break;
                        case 16:
                            if (fuse3 == 0)
                                finalDisplay = "There's a fuse on the ground in the middle of the room. A giant mannequin with a spring for a head is at the back of the room. To the south is the way back.";
                            else
                                finalDisplay = "There's a giant mannequin with a spring for a head in the back of the room. To the south is the way back.";
                            break;
                        default:    //Area 0
                            finalDisplay = "As your eyes slowly adjust to the darkness you realize you are in the middle of a hallway. Jagged metal embroiders the walls, and soot covers the floor. To the south is a ";
                            if (door0 == 0)
                                finalDisplay += "closed door";
                            else
                                finalDisplay += "open door leading to a blood stain";
                            finalDisplay += ", and to the east is an open doorway. It appears you are alone.";
                            break;
                    }
                }
                finalDisplay += "\nEnter an action, or type 'Actions' for a list of actions.\n\n";
            }
            else
            {
                finalDisplay = "There was an issue with your request.\n\n";
            }
            TerminalNode returnNode = CreateTerminalNode(finalDisplay);
            returnNode.clearPreviousText = true;
            return returnNode;
        }
        public static TerminalNode GrabItem(Terminal __terminal)
        {
            TerminalNode returnNode = CreateTerminalNode("");
            returnNode.clearPreviousText = true;
            int firstOpenSlot = -1;
            for (int i = 0; i < 4; i++)
            {
                if (inventory[i].Equals("Empty"))
                {
                    firstOpenSlot = i;
                    break;
                }
            }
            string item;
            string input = SimpleCommand.API.SimpleCommand.GetInputValue(__terminal);
            input = input.ToLower();
            try
            {
                item = input.Substring(5); 
            }
            catch
            {
                returnNode.displayText = "You have to enter an item. (Ex. Grab 'boombox')\n\n";
                return returnNode;
            }
            if (firstOpenSlot == -1) 
            {
                returnNode.displayText = "Your inventory is full!\n\n";
                return returnNode;
            }
            if (item.Equals("ladder"))
            {
                if (area == 1 && ladder == 0)
                {
                    ladder = 1;
                    inventory[firstOpenSlot] = "Ladder";
                    returnNode.displayText = "You picked up the ladder. How did you store it, you ask? Don't worry about it.\n\n";
                    return returnNode;
                }
                else
                {
                    returnNode.displayText = "There isn't a ladder in this room.\n\n";
                    return returnNode;
                }
            }
            else if (item.Equals("key"))
            {
                if (area == 4 && key == 0)
                {
                    key = 1;
                    inventory[firstOpenSlot] = "Key";
                    returnNode.displayText = "You took the key and put it on your phone's keychain...sorry, wrong game.\n\n";
                    return returnNode;
                }
                else
                {
                    returnNode.displayText = "There isn't a key in this room.\n\n";
                    return returnNode;
                }
            }
            else if (item.Equals("arm"))
            {
                if (area == 4 && arm == 0)
                {
                    arm = 1;
                    inventory[firstOpenSlot] = "Arm";
                    returnNode.displayText = "You shoved the bloody arm into your pocket. Surely it won't attract any beasts.\n\n";
                    return returnNode;
                }
                else
                { 
                    returnNode.displayText = "The only arm in this room is yours.\n\n";
                    return returnNode;
                }
            }
            else if (item.Equals("flashlight") || item.Equals("flash light"))
            {
                if (area == 15 && flashlight == 0)
                {
                    flashlight = 1;
                    inventory[firstOpenSlot] = "Flashlight";
                    returnNode.displayText = "You got the flashlight. Good thing the batteries are still working.\n\n";
                    return returnNode;
                }
                else
                {
                    returnNode.displayText = "There isn't a flashlight in this room.\n\n";
                    return returnNode;
                }
            }
            else if (item.Equals("remote") || item.Equals("device"))
            {
                if (area == 13 && remote == 0)
                {
                    remote = 1;
                    inventory[firstOpenSlot] = "Device";
                    returnNode.displayText = "You picked up the device. Let's just hope it's not to the garage.\n\n";
                    return returnNode;
                }
                else 
                {
                    returnNode.displayText = "There isn't any doohickey of any sort in this room.\n\n";
                    return returnNode;
                }
            }
            else if (item.Equals("fuse"))
            {
                if (area == 12)
                {
                    if (keyGiven == 0)
                    {
                        returnNode.displayText = "You can't get past the bug, and you're not gonna risk it. You need the bug out of the way first.\n\n";
                        return returnNode;
                    }
                    else if (fuse1 == 0)
                    {
                        fuse1 = 1;
                        inventory[firstOpenSlot] = "Fuse";
                        returnNode.displayText = "You picked up the fuse. Don't break it.\n\n";
                        return returnNode;
                    }
                    else
                    {
                        returnNode.displayText = "You already got this fuse, silly.\n\n";
                        return returnNode;
                    }
                }
                else if (area == 9) //The turret neccecarily must be off to even be in area 9, so no check is needed.
                {
                    if (fuse2 == 0)
                    {
                        fuse2 = 1;
                        inventory[firstOpenSlot] = "Fuse";
                        returnNode.displayText = "You aquired this fuse. Good job.\n\n";
                        return returnNode;
                    }
                    else
                    {
                        returnNode.displayText = "This turret only had one fuse on it. Sorry.\n\n";
                        return returnNode;
                    }
                }
                else if (area == 8)
                {
                    if (bodyMoved == 0)
                    {
                        returnNode.displayText = "You're a cheater. You should have no way to know a fuse is under the body, but you somehow did. How'd you do that, huh? You gonna lie to me? You know how much time I put into planning where all the fuses would be, how you got them, the puzzle aspect of all of this? It took me like 10 minutes! I didn't spend all that time just for you to cheat your way through it. Inspect the damn body, learn there's something under it, move the body, and THEN grab the fuse. Until you do that, you aren't getting anything from me.\n\n";
                        PlayerPrefs.SetInt("Cheater", 1);
                        return returnNode;
                    }
                    else if (fuse3 == 0)
                    {
                        fuse3 = 1;
                        inventory[firstOpenSlot] = "Fuse";
                        if (PlayerPrefs.GetInt("Cheater", 0) == 1) //Lecturing the speedrunners. If even one person sees this I will be happy.
                            returnNode.displayText = "See? How hard was that? Was that so bad, taking the twenty, maybe thirty seconds to just play the game as it was intended to be played? Did you suffer? Was it agonizing? Did I even spell that word right? I'm not spell checking it, that's how disgruntled I am about this whole situation, it's just gonna be wrong most likely. " +
                                "Screw you, I hate you...I didn't mean that. I don't hate you. Thank you for downloading my mod. Truly. As of writing this I have over 7,000 downloads. That's crazy, so thank you. I'm still upset you tried to cheat, though. " +
                                "That's why I'm hopefully wasting your time with this unending paragraph I'm typing up. You could just not read it, but if you care enough about this game to play it for long enough to get to this fuse, you probably would take the time to read this. " +
                                "Although, if you're cheating, maybe not. Maybe you accidentally soft-locked yourself by using the ladder on the landmine, using the key in the door, and dropping the flashlight for the bug, leaving you with no way to get the final fuse from the coil-head. Did you do that? And then, trying to get back as soon as possible, you just try and pick the fuse in this room up? " +
                                "I feel bad for you. I included that on purpose, but I still feel bad. It sucks, I know, but I had to give you a chance to lose. Imagine an adventure game where you can't lose, wouldn't that be silly? And besides, I gave you so many options to do the bug room, if you somehow messed it up I think that's on you. Okay, this is long enough. I hope you read all of this and then " +
                                "reflected on your choices up to this point. Hopefully your future adventures will be within the confines of the rules. Anyway, you got the fuse.\n\n";
                        else
                            returnNode.displayText = "You picked the fuse up. It's very bloody, but it might still work.\n\n";
                        return returnNode;
                    }
                    else
                    {
                        returnNode.displayText = "He didn't have two fuses, sadly.\n\n";
                        return returnNode;
                    }
                }
                else if (area == 16)
                {
                    if (flashlightOn == false && fuse4 == 0)
                    {
                        returnNode.displayText = "You would get yourself killed trying to get it, it's far too dark.\n\n";
                        return returnNode;
                    }
                    else if (fuse4 == 1)
                    {
                        returnNode.displayText = "You need to work on your short term memory, because you already have this fuse.\n\n";
                        return returnNode;
                    }
                    else if (flashlightOn == true && fuse4 == 0)
                    {
                        fuse4 = 1;
                        inventory[firstOpenSlot] = "Fuse";
                        returnNode.displayText = "Very carefully and very slowly, with your flashlight fixated on the deadly monolith, you grab the fuse and back towards the door. Absolutely finnesed.\n\n";
                        return returnNode;
                    }
                }
                else
                {
                    returnNode.displayText = "There are many fuses in this facility, but there are none in this room.\n\n";
                    return returnNode;
                }
            }
            returnNode.displayText = "Not only is that item not in this room, it isn't even in the entire facility.\n\n";
            return returnNode;
        }

        public static TerminalNode Push(Terminal __terminal)
        {
            TerminalNode returnNode = CreateTerminalNode("");
            returnNode.clearPreviousText = true;
            string item;
            string input = SimpleCommand.API.SimpleCommand.GetInputValue(__terminal);
            input = input.ToLower();
            try
            {
                item = input.Substring(5);
            }
            catch
            {
                returnNode.displayText = "You have to reference what you're pushing. (Ex. push body)\n\n";
                return returnNode;
            }
            if (item.Equals("body") || item.Equals("the body"))
            {
                if (area == 8)
                {
                    bodyMoved = 1;
                    returnNode.displayText = "You roll the body over and find underneath an in-tact fuse!\n\n";
                    return returnNode;
                }
                else
                {
                    returnNode.displayText = "The only body in this room is your body. And you can push that one by walking.\n\n";
                    return returnNode;
                }
            }
            else
            {
                returnNode.displayText = "To keep it a buck with you, the 'push' command is for one very specific scenario, and nothing else. Unless you find yourself in such a scenario, don't use it. You have nothing to push.\n\n";
                return returnNode;
            }
        }
        public static TerminalNode Inventory(Terminal __terminal)
        {
            string finalDisplay = "";
            for (int i = 0; i < inventory.Count; i++)
            {
                finalDisplay += "Slot " + (i + 1) + ": " + inventory[i] + "\n";
            }
            finalDisplay += "\n\n";
            TerminalNode returnNode = CreateTerminalNode(finalDisplay);
            returnNode.clearPreviousText = true;
            return returnNode;
        }

        public static TerminalNode UseItem(Terminal __terminal)
        {
            TerminalNode returnNode = CreateTerminalNode("");
            returnNode.clearPreviousText = true;
            bool hasKey = false;
            bool hasFlash = false;
            bool hasArm = false;
            bool hasLadder = false;
            bool hasDevice = false;
            int keyIndex = -1;
            int ladderIndex = -1;
            int flashIndex = -1;
            string item;
            string input = SimpleCommand.API.SimpleCommand.GetInputValue(__terminal);
            input = input.ToLower();
            try
            {
                item = input.Substring(4);
            }
            catch
            {
                returnNode.displayText = "You have to enter an item. (Ex. Use 'key')\n\n";
                return returnNode;
            }
            for (int i = 0; i < 4; i++)
            {
                if (inventory[i].Equals("Key"))
                {
                    hasKey = true;
                    keyIndex = i;
                }
                else if (inventory[i].Equals("Flashlight"))
                {       
                    hasFlash = true;
                    flashIndex = i;
                }
                else if (inventory[i].Equals("Arm"))
                    hasArm = true;
                else if (inventory[i].Equals("Ladder"))
                {       hasLadder = true;
                        ladderIndex = i;
                }
                else if (inventory[i].Equals("Device"))
                    hasDevice = true;
            }
            if (item.Equals("fuse"))
            {
                int numFuses = 0;
                for (int i = 0; i < 4; i++)
                {
                    if (inventory[i].Equals("Fuse"))
                        numFuses++;
                }
                if (numFuses == 0)
                {
                    returnNode.displayText = "You don't have that item.";
                    return returnNode;
                }
                else if (area == 4)
                {
                    string finalDisplay = "You opened the box and inserted " + numFuses + " fuses.";
                    for (int x = 0; x < 4; x++)
                    {
                        if (inventory[x].Equals("Fuse"))
                            inventory[x] = "Empty";
                    }
                    int index = 0;
                    for (int i = 0; i < 4; i++)
                    {
                        if (insertedFuses[i] == 0)
                        {
                            insertedFuses[i] = 1;
                            numFuses--;
                            if (numFuses == 0)
                                break;
                        }
                    }
                    if (insertedFuses[3] == 1)
                    {
                        gameState = 100;
                        finalDisplay += " With the final fuse in place the lights come on, illuminating the facility. Power has been restored.\n\n";
                        returnNode.displayText = finalDisplay;
                        return returnNode;
                    }
                    else
                    {

                        int fusesLeft = 0;
                        for (int i = 0; i < 4; i++)
                        {
                            if (insertedFuses[i] == 0)
                                fusesLeft++;
                        }
                        finalDisplay += " Alas, it's not enough. There are still " + fusesLeft + " slots empty in the breaker.\n\n";
                        returnNode.displayText = finalDisplay;
                        return returnNode;
                    }
                }
                else
                {
                    returnNode.displayText = "I don't know how that could help here.\n\n";
                    return returnNode;
                }
            }
            else if (item.Equals("scanner") || item.Equals("panel"))
            {
                if (area == 6)
                {
                    if (gameState == 100)
                    {
                        returnNode.displayText = "You place your hand on the panel and the screen lights up with 'Access Denied'.\n\n";
                        return returnNode;
                    }
                    else if (gameState < 100)
                    {
                        returnNode.displayText = "You try to use the panel, but the power is off.\n\n";
                        return returnNode;
                    }
                    else
                    {
                        returnNode.displayText = "Instead of sprinting out of the open door, you instead play with the panel some more. You giggle to yourself as the panel lights up 'Access Denied' over and over. After a few minutes, you get bored and stop.\n\n";
                        return returnNode;
                    }
                }
                else
                {
                    returnNode.displayText = "There's no such thing in this room.\n\n";
                    return returnNode;
                }
            }
            else if (item.Equals("arm"))
            {
                if (hasArm)
                {
                    if (area == 6)
                    {
                        if (gameState == 100)
                        {
                            gameState = 150;
                            returnNode.displayText = "You place the severed hand onto the panel, and it lights up 'Access Granted'! The door next to you opens.\n\n";
                            return returnNode;
                        }
                        else if (gameState < 100)
                        {
                            returnNode.displayText = "You place the hand on the panel, but the power's off. What did you expect?\n\n";
                            return returnNode;
                        }
                        else
                        {
                            returnNode.displayText = "Although you could leave, you instead play around with your dead coworker's severed arm. Look at you.\n\n";
                            return returnNode;
                        }
                    }
                    else
                    {
                        returnNode.displayText = "How would you even use this here?\n\n";
                        return returnNode;
                    }
                }
                else
                {
                    returnNode.displayText = "You don't have this item.\n\n";
                    return returnNode;
                }
            }
            else if (item.Equals("key"))
            {
                if (hasKey)
                {
                    if (area == 12)
                    {
                        if (keyGiven == 0)
                        {
                            keyGiven = 1;
                            inventory[keyIndex] = "Empty";
                            returnNode.displayText = "You slowly place the key on the ground. The bug scutters over, takes it, and crawls away. Looking where the bug used to stand, you see a fuse on the ground!\n\n";
                            return returnNode;
                        }
                        else
                        {
                            returnNode.displayText = "That doesn't work here.\n\n";
                            return returnNode;
                        }
                    }
                    else if (area == 13 || area == 14)
                    {
                        door12Locked = 0;
                        inventory[keyIndex] = "Empty";
                        returnNode.displayText = "You put the key into the slot and turn it. As the door clicks open, the key breaks into two pieces!\n\n";
                        return returnNode;
                    }
                    else if (area == 6)
                    {
                        returnNode.displayText = "Nice try.\n\n";
                        return returnNode;
                    }
                    else
                    {
                        returnNode.displayText = "You try to think of a way to use a key in this scenario, but you fail.\n\n";
                        return returnNode;
                    }
                }
                else
                {
                    returnNode.displayText = "You don't have this item.\n\n";
                    return returnNode;
                }
            }
            else if (item.Equals("flashlight"))
            {
                if (hasFlash)
                {
                    if (area == 16)
                    {
                        flashlightOn = true;
                        returnNode.displayText = "You turn the flashlight on. Now you can see clearly what's in the room, and you see that there's a fuse on the ground next to the spring...thing.\n\n";
                        return returnNode;
                    }
                    else if (area == 12)
                    {
                        inventory[flashIndex] = "Empty";
                        keyGiven = 1;
                        returnNode.displayText = "You slowly lower the flashlight to the ground. The bug scutters over and takes it, quickly leaving after. Looking back at where it used to be, you see that there's a fuse on the ground! I just hope you didn't need that light.\n\n";
                        return returnNode;
                    }
                    else
                    {
                        returnNode.displayText = "You turn the flashlight on. You're now able to see...not a lot. Debris, mostly. You turn it back off.\n\n";
                        return returnNode;
                    }
                }
                else
                {
                    returnNode.displayText = "You don't have this item.\n\n";
                    return returnNode;
                }
            }
            else if (item.Equals("ladder"))
            {
                if (hasLadder)
                {
                    if (area == 12)
                    {
                        keyGiven = 1;
                        returnNode.displayText = "You set the ladder up in front of the bug. It doesn't move, content with watching you. You tip the ladder over, and it smashes the bug, killing it instantly. Why did that work? You pick up the ladder and continue. Looking past the bug's corpse, you see that it was guarding a fuse!\n\n";
                        return returnNode;
                    }
                    else if (area == 13)
                    {
                        inventory[ladderIndex] = "Empty";
                        landmine = 1;
                        returnNode.displayText = "You set the ladder up in front of the landmine. You tip it over and quickly exit the room. A second later you hear an ear-splitting explosion. You peek your head back into the room, and the landmine, as well as the ladder, is gone. Stepping into the room, you see a device lying on the ground. How did it survive the explosion?\n\n";
                        return returnNode;
                    }
                    else
                    {
                        returnNode.displayText = "Contrary to popular opinion, a ladder is not always useful. This is one such rare instance.\n\n";
                        return returnNode;
                    }
                }
                else
                {
                    returnNode.displayText = "You don't have this item.\n\n";
                    return returnNode;
                }
            }
            else if (item.Equals("device") || item.Equals("remote"))
            {
                if (hasDevice)
                {
                    if (area == 8)
                    {
                        turret = 1;
                        returnNode.displayText = "You press the button on the middle of the device.\nSuddenly, you hear a whirring sound in the room to the south. Peaking your head inside, you see that the turret in the room has been deactivated!\n\n";
                        return returnNode;
                    }
                    else
                    {
                        returnNode.displayText = "You press the button on the middle of the device. It seems nothing happened.";
                        return returnNode;
                    }
                }
                else
                {
                    returnNode.displayText = "You don't have this item.\n\n";
                    return returnNode;
                }
            }
            returnNode.displayText = "You cannot use that item.\n\n";
            return returnNode;
        }

        public static string GameOver()
        {
            PlayerPrefs.DeleteKey("GameState");
            PlayerPrefs.DeleteKey("Area");
            PlayerPrefs.DeleteKey("InvOne");
            PlayerPrefs.DeleteKey("InvTwo");
            PlayerPrefs.DeleteKey("InvThree");
            PlayerPrefs.DeleteKey("InvFour");
            PlayerPrefs.DeleteKey("Door0");
            PlayerPrefs.DeleteKey("Door3");
            PlayerPrefs.DeleteKey("Door5");
            PlayerPrefs.DeleteKey("Door12");
            PlayerPrefs.DeleteKey("Door12Locked");
            PlayerPrefs.DeleteKey("Flashlight");
            PlayerPrefs.DeleteKey("Ladder");
            PlayerPrefs.DeleteKey("Remote");
            PlayerPrefs.DeleteKey("Landmine");
            PlayerPrefs.DeleteKey("Turret");
            PlayerPrefs.DeleteKey("Key");
            PlayerPrefs.DeleteKey("KeyGiven");
            PlayerPrefs.DeleteKey("Arm");
            PlayerPrefs.DeleteKey("Fuse1");
            PlayerPrefs.DeleteKey("Fuse2");
            PlayerPrefs.DeleteKey("Fuse3");
            PlayerPrefs.DeleteKey("Fuse4");
            PlayerPrefs.DeleteKey("BodyMoved");
            PlayerPrefs.DeleteKey("FuseOne");
            PlayerPrefs.DeleteKey("FuseTwo");
            PlayerPrefs.DeleteKey("FuseThree");
            PlayerPrefs.DeleteKey("FuseFour");
            ConsoleGamesMain.playingAdventure = false;
            return "You step through the open door, sunlight beaming down on your face. After a moment basking in the sun, you begin your trek back to...wherever you came from; but that's a story for another game, and for a different day.\n\n<size=28>You win!</size>\n\n";
        }
        public static string GetWalkMessage(int currentArea, int direction)
        {
            switch (currentArea)
            {
                case 1:
                    if (direction == 2) //West
                    {
                        area = 0;
                        return "You walk through the archway and into the small room. It smells awful in here, so you'd like to leave as soon as possible. To the south is a door, and to the east is the way you came.";
                    }
                    if (direction == 3) //South
                    {
                        area = 2;
                        return "With how structurally sound this place seems, you're happy to get off that walkway. You step into a large hallway with a door to the east, a door to the west, and a staircase to the south. You can't see the end of it.";
                    }
                    break;
                case 2:
                    if (direction == 0) //North
                    {
                        area = 1;
                        return "You step out onto the platform, praying that it holds your weight. You don't know why there's a giant hole here, but you don't want to find out what's on the bottom. To the west is a doorway, and to the south is where you just were.";
                    }
                    if (direction == 1) //East
                    {
                        area = 5;
                        if (door5 == 0)
                        {
                            door5 = 1;
                            return "You open the door and step into the hallway. This room is much brighter than the others, because there's light coming from the passageway to the north. To the east is a hallway that turns away, and to the south is a hall with something big at the end of it on the ground.";
                        }
                        return "You step into the hallway. The light filtering through the passage to the north is a welcome sight. To the east is a twisting hallway, and to the south is a corridor with something large laying on the ground at the end.";
                    }
                    if (direction == 2) //West
                    {
                        area = 3;
                        if (door3 == 0)
                        {
                            door3 = 1;
                            return "You twist the doorknob and the door swings open, revealing a large pool of blood on the ground, with a trail leading off to the west. The smell is putrid. To the north is another door, and to the east is the way you came.";
                        }
                        return "You step into the room, making sure not to step on the blood. To the west is where the bloodtrail leads, to the north is a door and to the east is back the way you came.";
                    }
                    if (direction == 3) //South
                    {
                        area = 7;
                        return "You slowly make your way down the stairs, watching your step. After what seems like an eternity you reach a landing platform with a long, long hallway to the east. It seems to raise slightly, effectively undoing all the stairs you just walked down. Alternatively, you could continue south down the stairs.";
                    }
                    break;
                case 3:
                    if (direction == 0) //North
                    {
                        area = 0;
                        if (door0 == 0)
                        {
                            door0 = 1;
                            return "You step around the blood and to the door. You open it slowly, and realize you've made your way back to where you began. You step into the room. To the east is a doorway, and to the south is the way you came.";
                        }
                        return "Carefully avoiding the blood you step into the room. To the east is an open doorway, and to the south is the way you came.";
                    }
                    else if (direction == 1) //East
                    {
                        area = 2;
                        if (door3 == 0)
                        {
                            door3 = 1;
                            return "You pull the door open and step through. Looking around, you see stairs leading down into a dark abyss to the south, a path leading to the north and a door to the east. To the west is back the way you came.";
                        }
                        return "You enter the hallway. There are stairs leading down to the south, a door to the east and a passage to the north.";
                    }
                    else if (direction == 2) //West
                    {
                        area = 4;
                        if (gameState < 2)
                        {
                            gameState = 2;
                            return "You follow the bloodtrail (against your better judgement) and find yourself at a dead end. Two things stand out to you: a breakerbox on the wall and the severed arm laying right below it. Luckily, whatever did this has left. You're alone in this room. To the east is the way back.";
                        }
                        if (arm == 0)
                            return "You walk into the breaker room. The arm is still there, and the box is still on the wall. Good to see some things don't change. To the east is the way back.";
                        return "You walk into the breaker room. The box is still on the wall. Good to see some things don't change. To the east is the way back.";                   
                    }
                    break;
                case 4:
                    area = 3;
                    return "You follow the bloodtrail back, finding yourself back in the room with the large pool of blood. To the north and east are doors, to the west is the way back.";
                    break;
                case 5:
                    if (direction == 0) //North
                    {
                        area = 6;
                        return "You step into the illuminated entrance. On the back wall to the north is a door and an electronic panel. The only other exit is the way you came, to the south.";
                    }
                    else if (direction == 1) //East
                    {
                        area = 16;
                        return "You traverse the winding corridor and find yourself in a large room with a hole in the middle. You take a few steps in when you hear a loud screeching coming from the back of the room. You snap your head to the sound and see a large manequein with a spring for a head standing motionless in the center of the room. You remember hearing about this thing, it moves when you don't look at it. The room is far too dark to see anything in without breaking eye contact with the thing, so you can't properly explore it. The only exit from this room is the way you came, to the south.";
                    }
                    else if (direction == 2) //West
                    {
                        area = 2;
                        if (door5 == 0)
                        {
                            door5 = 1;
                            return "You crack the door open and step through. To the north is a walkway, to the south are stairs leading into pitch black darkness, and to the west is another door. To the east is the door you just opened.";
                        }
                        return "You enter the open door. To the north is a walkway, to the south are stairs leading into pitch black darkness, and to the west is another door. To the east is the way you came";
                    }
                    else if (direction == 3) //South
                    {
                        area = 8;
                        return "You go down the hall and end up standing right next to a body with what has to be at least fifty bullet holes. To the east is small passage and to the south is an enclosed room. To the west is a twisting hallway, and to the north is the way you came from.";
                    }
                    break;
                case 6:
                    if (direction == 0) //North
                    {
                        if (gameState < 100)
                        {
                            return "You cannot go north, as the door is closed.";
                        }
                        else if (gameState > 100)
                        {
                            return GameOver();
                        }
                        else
                        {
                            return "The power might be on, but the door is still closed. Try the panel, perhaps.";
                        }
                    }
                    else if (direction == 3) //South
                    {
                        area = 5;
                        return "You step back into the hall. To the east is a winding hallway, to the west is a door, and to the south is a corridor with something large on the ground at the end of it. To the north is the lobby you just came from.";
                    }
                    break;
                case 7:
                    if (direction == 0) //North
                    {
                        area = 2;
                        return "You climb the seemingly infinite stairs and reach the summit. To the east and west are doorways. To the north is a grate platform, and to the south are the stairs you just climbed for the past six minutes.";
                    }
                    else if (direction == 1) //East
                    {
                        area = 8;
                        return "You make your way slowly up the inclined hall. After a series of twists and turns you find yourself in a small passageway with a body on the ground. It's quite...perforated. To the north is a tunnel with a faint light at the end of it. To the east is a walkway and to the south is a small room. To the east is the way you came. ";
                    }
                    else if (direction == 3) //South
                    {
                        area = 10;
                        return "You climb the stairs even further down, finally reaching their end. It's extremely dark, but you can make out a doorway to your west and a path leading to the south. You could also climb the stairs to the north that you just came down. But why would you?";
                    }
                    break;

                case 8:
                    if (direction == 0) //North
                    {
                        area = 5;
                        return "You walk towards the light, but you're not dead (yet). You find yourself in at a four-way intersection, with the lightsource to the north in a small room, a winding hallway to the east, a door to your west, and the way you came to the south.";
                    }
                    else if (direction == 1) //East
                    {
                        area = 14;
                        return "You continue down the walkway. You reach a T-junction with a small nook to the east and a further path to the south. Both ways are quite dark.";
                    }
                    else if (direction == 2) //West
                    {
                        area = 7;
                        return "You walk through the bending halls and reach a landing platform where stairs extend upwards to the north and stretch downwards to the south. To the east is back where you came. Something about this place makes you want to leave, and quickly.";
                    }
                    else if (direction == 3)
                    {
                        if (turret == 0)
                        {
                            return "You take one step into the room and a bright red light appears and focuses on you. You quickly duck out of the doorway, and light dissapears. Whatever's in there, you don't think you can enter without turning it off.";
                        }
                        area = 9;
                        return "This room is much nicer with the turret deactivated. To the north is the only way back.";
                    }
                    break;
                case 9:
                    area = 8;
                    return "You walk back into the room with the body. To the east is a bending hallway, to the north is a faint light at the end of a corridor, and to the east is a T-junction. To the south is the way you came from.";
                    break;
                case 10:
                    if (direction == 0) //North
                    {
                        area = 7;
                        return "You climb the stairs back up. After a while you reach a small platform where a hallway extends to the east. To the north the stairs continue, and to the south is where you came from.";
                    }
                    else if (direction == 2) //West
                    {
                        area = 11;
                        if (keyGiven == 0)
                            return "You walk through the hallway and after a long while find yourself in front of an open doorway. In the other room you head scittering and what sounds like bird calls, but you know it's probably not from birds. You can go north, towards the sound, or east back where you came.";
                        return "You walk through the hallway and after a long while find yourself in front of an open doorway to your north. To the east is the way back.";
                    }
                    else if (direction == 3) //South
                    {
                        area = 13;
                        cameFrom = 10;
                        if (landmine == 0)
                            return "You follow the hallway but are stopped by a landmine in the middle of the hall. The hallway is very narrow, so you doubt you could get around it. To the west is the way back.";
                        return "You follow the hallway, unfettered by an explosive device. To your east is a doorway, and to the wets is the way you came.";
                    }
                    break;
                case 11:
                    if (direction == 0) //North
                    {
                        area = 12;
                        if (keyGiven == 0)
                            return "You walk through the door and find yourself face-to-face with a giant bug thing. It doesn't move, it simply stares at you from a far, making noises. To the south is the way back.";
                        return "You slowly step into the room. No bugs. Good. To the south is the way back.";
                    }
                    else if (direction == 1) //East
                    {
                        area = 10;
                        return "You get into the hallway. It's very dark. To the south is a hallway that bends out of sight. To the north is a staircase which seems to go up forever. To the west is where you came from.";
                    }
                    break;
                case 12:
                    area = 11;
                    return "You exit the dead-end and walk back into the corridor. To the east is the bottom of the stairs, and to the north is where you came from.";
                    break;
                case 13:
                    if (direction == 1) //East
                    {
                        if (landmine == 0 && cameFrom == 10)
                            return "You're not going that way if you want to keep your legs on your body. The only available exit is to the west.";
                        else if (landmine == 0 && cameFrom == 14)
                        {
                            area = 14;
                            return "You turn around and go back. To the east is a small nook, and to the west is a corridor. To the south is where you just came from.";
                        }
                        else if (door12Locked == 1)
                            return "You went to the door and tried to open it, but it was locked! Nothing can be easy.";
                        else if (door12 == 0)
                        {
                            door12 = 1;
                            area = 14;
                            return "Now that the door's unlocked you push it open and step through. You walk down a brief hall and find yourself between a small corner to the east and an open path to the west. You could also return south to where you just were.";
                        }
                        else
                        {
                            area = 14;
                            return "You go through the open door. You walk down a brief hall and find yourself between a small corner to the east and an open path to the west. You could also return south to where you just were.";
                        }
                    }
                    else if (direction == 2) //West
                    {
                        if (landmine == 0 && cameFrom == 14)
                            return "You're not going that way if you want to keep your legs on your body. The only available exit is to the east.";
                        else if (landmine == 0 && cameFrom == 10)
                        {
                            area = 10;
                            return "You turn around and go back. To the north are interminable stairs, to the west is a twisting pathway and to the south is where you just were.";
                        }
                        else
                        {
                            area = 10;
                            return "You traverse the corridor and find yourself at the foot of some stairs to the north. To the west is an offshooting path, and to the south is where you just were.";
                        }
                    }
                    break;
                case 14:
                    if (direction == 1) //East
                    {
                        area = 15;
                        return "The room is so small that you can see all of it, even in the darkness. There's a flashlight on the ground in front of you. The only exit is back where you came, to the west.";
                    }
                    else if (direction == 2) //West
                    {
                        area = 8;
                        return "Exiting the path you find yourself standing near a body filled with bullets. To the north you can see light, to the south is a small room and to the west is a twisting path. You could also go east, where you just were.";
                    }
                    else if (direction == 3) //South
                    {
                        if (door12Locked == 1)
                            return "You try to open the door and step through, but the door's locked! To the east is a small room, to the west is a pathway, and to the south is the locked door.";
                        else if (door12 == 0 && landmine == 0)
                        {
                            cameFrom = 14;
                            door12 = 1;
                            area = 13;
                            return "Having unlocked the door you swing it open and enter. You take a few steps in when you see a landmine right in the middle of the path. You doubt you could get around it, so your only way to go is back to the east.";
                        }
                        else if (door12 == 0 && landmine == 1)
                        {
                            door12 = 1;
                            area = 13;
                            return "Having unlocked the door you swing it open and enter. You find yourself in the middle of a hallway, with an open passage to the west and the way you came to the east.";
                        }
                        else if (landmine == 0)
                        {
                            cameFrom = 14;
                            area = 13;
                            return "You enter the hallway but are stopped by a landmine in the middle of the path. Getting around it isn't going to happen, so the only way to go is east, where you came from.";
                        }
                        else
                        {
                            area = 13;
                            return "You enter the hallway. To the west is an open doorway, and to the east is the way you came from.";
                        }
                    }
                    break;
                case 15:
                    area = 14;
                    return "You go back to the pathway. To the south is a door, and to the west is a corridor. To the east is where you just were.";
                    break;
                case 16:
                    area = 5;
                    return "Keeping your eyes on the spring thing, you back out of the room. It clinks up against the doorway as you turn, but it's too large to get through the door. So that's good. To the north is a room with a lot of light, to the south is a hallway with something at the end of it, and to the west is a door. To the east is where you just were.";
                    break;
                default: //Area 0
                    if (direction == 1) //East
                    {
                        area = 1;
                        if (gameState == 0)
                        {
                            gameState = 1;
                            return "You walk through the open doorway, still unsure where you are. You step out onto a grate walkway. Through the holes you can see a long, long way down. Better hold on to the guardrails. To the west is the way you came, and to the south is a dark corridor."; 
                            
                        }
                        return "You step out onto the grated platform. Don't look down. To the west is an open doorway and to the south is a dark corridor.";
                    }
                    else if (direction == 3) //South
                    {
                        area = 3;
                        if (door0 == 0)
                        {
                            door0 = 1;
                            if (gameState == 0)
                            {
                                gameState = 0;
                                return "You turn the doorknob and it clicks open. Pulling the door open you're hit by the smell of iron, and looking to the large red mark on the ground you think you know what the iron's from. Leading away from the bloodstain is a trail off to the west. It's dark. To the east is a door, and to the north is the way you came.";
                            }
                            return "You turn the doorknob and it clicks open. Pulling the door open you see a large bloodstain on the ground. Best to avoid that. To the west a trail of blood leads into the darkness. To the east is a door, and to the north is the way back.";
                        }
                        else
                        {
                            if (gameState == 0)
                            {
                                gameState = 0;
                                return "Stepping into the room you're hit by the smell of iron, and looking to the large red mark on the ground you think you know what the iron's from. Leading away from the bloodstain is a trail off to the west. It's dark. To the east is a door, and to the north is the way you came.";
                            }
                            return "Stepping into the room you see a large bloodstain on the ground. Best to avoid that. To the west a trail of blood leads into the darkness. To the east is a door, and to the north is the way back.";
                        }
                    }
                    break;
            }
            return "Something went wrong in the WalkMessage method. Tell Garbage Mammal to hurry up and fix it.";
        }
        public static TerminalNode Walk(Terminal __terminal)
        {
            string input = SimpleCommand.API.SimpleCommand.GetInputValue(__terminal);
            input = input.ToLower();
            string finalDisplay;
            int direction;
            if (ConsoleGamesMain.playingAdventure)
            {
                if (AreasConnected(area, input, out direction)) // 0 is North, 1 is East, 2 is West, 3 is South
                { 
                    finalDisplay = GetWalkMessage(area, direction) + "\n\n";
                }
                else
                {
                    finalDisplay = "There's nothing over that way.\n\n";
                }
            }
            else
            {
                finalDisplay = "There was an issue with your request.\n\n";
            }
            TerminalNode returnNode = CreateTerminalNode(finalDisplay);
            returnNode.clearPreviousText = true;
            return returnNode;
        }
    }
}
