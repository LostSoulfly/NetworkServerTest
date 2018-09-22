using Network.Packets;

namespace Packets
{
    public class TestPacket : RequestPacket
    {

        public string test;

        public TestPacket(string test)
        {
            this.test = test;
        }

        public TestPacket()
        {

        }
    }
}