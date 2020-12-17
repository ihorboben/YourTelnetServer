using System.Collections.Generic;
using System.Linq;
using System.Net;

namespace YourTelnetServer
{
    class Client
    {
        private readonly int _id;
        private readonly IPEndPoint _remoteAddr;
        private string receivedData;
        private List<int> list;

        public Client(int id, IPEndPoint remoteAddr)
        {
            _id = id;
            _remoteAddr = remoteAddr;
            list = new List<int>();
        }

        public void AddNumber(int num)
        {
            list.Add(num);
        }

        public long GetSum()
        {
            return list.Sum();
        }

        public int GetClientID()
        {
            return _id;
        }

        public void ResetReceivedData()
        {
            receivedData = string.Empty;
        }

        public string GetReceivedData()
        {
            return receivedData;
        }

        public void AppendReceivedData(string dataToAppend)
        {
            this.receivedData += dataToAppend;
        }

        public void RemoveLastCharacterReceived()
        {
            receivedData = receivedData.Substring(0, receivedData.Length - 1);
        }

        public override string ToString()
        {
            string ip = $"{_remoteAddr.Address}:{_remoteAddr.Port}";

            string res = $"Client #{_id} (From: {ip})";

            return res;
        }
    }
}
