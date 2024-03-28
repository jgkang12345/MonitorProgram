using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonitorProgram
{
    public class Type 
    {
        public enum PacketProtocol : Int16
        {
            C2S_PLAYERINIT,
            S2C_PLAYERINIT,
            C2S_PLAYERSYNC,
            S2C_PLAYERSYNC,
            S2C_PLAYERLIST,
            S2C_PLAYERREMOVELIST,
            S2C_PLAYERENTER,
            S2C_PLAYEROUT,
            C2S_LATENCY,
            S2C_LATENCY,
            C2S_MAPSYNC,
            S2C_MAPSYNC,
            S2C_PLAYERNEW,
            S2C_PLAYERDESTORY,
            C2S_PLAYERATTACK,
            S2C_PLAYERATTACKED,
            C2S_PLAYERCHAT,
            S2C_PLAYERCHAT,
            S2C_PLAYERDETH,
            C2S_PLAYERESPAWN,
            S2C_PLAYERESPAWN,
            S2C_MONSTERSPAWN,
            S2C_MONSTERREMOVELIST,
            S2C_MONSTERRENEWLIST,
            C2S_MONSTERATTACKED,
            S2C_MONSTERATTACKED,
            S2C_MONSTERDEAD,
            S2C_MONSTERSYNC,
            S2C_NEWMONSTER,
            S2C_DELETEMONSTER,
            S2C_MONSTERDEADCLIENT,
            S2C_MONSTERINFO,
            S2C_PLAYEREXP,
            C2S_PLAYERSTATINFO,
            S2C_PLAYERSTATINFO,
            C2S_UPSTAT,
            C2S_LOGIN,
            S2C_LOGIN,
            C2S_CREATECHARACTER,
            S2C_CREATECHARACTER,
            S2C_CHARACTERLIST,
            C2S_CHARACTERLIST,
            C2S_DELETECHARACTER,
            C2S_GAMEPLAY,
            S2C_SERVERMOVE,
            C2S_SERVER_MOVE,
            C2S_PLAYERSKILLSYNC,
            S2C_HEARTBIT,
            C2S_HEARTBIT,
            S2C_SERVERLIST,
            C2S_LATECY,
            S2C_MONITORINIT,
            S2C_LATECY,
            S2C_CONNECTIONLIST,
        }
    }
}
